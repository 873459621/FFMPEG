using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;

public class StockDataManager : MonoBehaviour
{
    public static StockDataManager Instance;

    private Dictionary<string, string> CodeName = new Dictionary<string, string>();
    public Dictionary<int, StockData> StockDatas = new Dictionary<int, StockData>();
    public Dictionary<string, double> CurUnits = new Dictionary<string, double>();

    public int Index = 0;

    private double us_exchange = 0;
    
    public double US_Exchange {
        get
        {
            if (us_exchange == 0)
            {
                us_exchange = double.Parse(PlayerPrefs.GetString("US_Exchange", "7.2"));
            }
            
            return us_exchange;
        }

        set
        {
            us_exchange = value;
            PlayerPrefs.SetString("US_Exchange", value.ToString());
            Debug.Log($"美元汇率更新：{US_Exchange}");
        }
    }
    
    private double hk_exchange = 0;
    
    public double HK_Exchange {
        get
        {
            if (hk_exchange == 0)
            {
                hk_exchange = double.Parse(PlayerPrefs.GetString("HK_Exchange", "0.9"));
            }
            
            return hk_exchange;
        }

        set
        {
            hk_exchange = value;
            PlayerPrefs.SetString("HK_Exchange", value.ToString());
            Debug.Log($"港元汇率更新：{HK_Exchange}");
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(Init());
    }

    IEnumerator Init()
    {
        yield return ExchangeRateFetcher.Instance.ExchangeRateFetch();
        
        LoadCodeName();
        LoadAll();
        
        Debug.Log($"当前美元汇率：{US_Exchange}");
        Debug.Log($"当前港元汇率：{HK_Exchange}");

        yield return UpdateStockPrice();
    }

    public void LoadAll()
    {
        var reader = File.OpenText("Assets/Resources/Stock/stock.txt");

        while (!reader.EndOfStream)
        {
            AddStockData(new StockData(reader.ReadLine().Trim()));
        }

        reader.Close();
        reader.Dispose();
    }
    
    public void SaveAll()
    {
        StreamWriter sw = new FileInfo("Assets/Resources/Stock/stock.txt").CreateText();

        foreach (var kv in StockDatas)
        {
            sw.WriteLine(kv.Value);
        }

        sw.Close();
        sw.Dispose();
    }
    
    public void LoadCodeName()
    {
        var reader = File.OpenText("Assets/Resources/Stock/codename.txt");

        while (!reader.EndOfStream)
        {
            var ss = reader.ReadLine().Trim().Split(',');
            CodeName.Add(ss[0], ss[1]);
        }

        reader.Close();
        reader.Dispose();
    }

    public bool AddCodeName(string code, string n)
    {
        return CodeName.TryAdd(code, n);
    }

    public string GetCodeName(string code)
    {
        CodeName.TryGetValue(code, out string name);
        return name;
    }

    public void SaveCodeName()
    {
        StreamWriter sw = new FileInfo("Assets/Resources/Stock/codename.txt").CreateText();

        foreach (var kv in CodeName)
        {
            sw.WriteLine($"{kv.Key},{kv.Value}");
        }

        sw.Close();
        sw.Dispose();
    }

    public void AddStockData(StockData stockData, bool isNew = false)
    {
        if (!string.IsNullOrEmpty(stockData.Name) && AddCodeName(stockData.Code, stockData.Name))
        {
            SaveCodeName();
        }

        StockDatas.Add(stockData.Id, stockData);

        if (isNew)
        {
            SaveAll();
        }
    }

    public void DeleteStockData(int id)
    {
        StockDatas.Remove(id);
        SaveAll();
    }

    public List<StockData> GetStockDatas(StockType stockType = StockType.All, SellType sellType = SellType.All)
    {
        List<StockData> stockDatas = new List<StockData>();

        foreach (var kv in StockDatas)
        {
            if (stockType != StockType.All && kv.Value.Type != stockType)
            {
                if (stockType == StockType.A && (kv.Value.Type == StockType.Short || kv.Value.Type == StockType.Mid || kv.Value.Type == StockType.Long))
                {
                    
                }
                else
                {
                    continue;
                }
            }
            
            if (sellType != SellType.All && kv.Value.SellType != sellType)
            {
                continue;
            }

            stockDatas.Add(kv.Value);
        }
        
        stockDatas.Sort((a, b) => a.BuyDate.CompareTo(b.BuyDate));

        return stockDatas;
    }

    public List<StockData> CalcStockDatas(List<StockData> stockDatas, MergeType mergeType)
    {
        if (mergeType == MergeType.No)
        {
            return stockDatas;
        }
        
        Dictionary<string, StockData> holdStockDatas = new Dictionary<string, StockData>();
        Dictionary<string, StockData> soldStockDatas = new Dictionary<string, StockData>();
        Dictionary<string, StockData> allStockDatas = new Dictionary<string, StockData>();
        List<StockData> newStockDatas = new List<StockData>();

        if (mergeType == MergeType.All)
        {
            foreach (var stockData in stockDatas)
            {
                if (allStockDatas.TryAdd(stockData.Code, stockData))
                {
                    continue;
                }

                var oldData = allStockDatas[stockData.Code];

                DateTime buyDate;
                DateTime sellDate;

                if (oldData.SellType == stockData.SellType)
                {
                    buyDate = oldData.BuyDate > stockData.BuyDate ? stockData.BuyDate : oldData.BuyDate;
                    sellDate = oldData.SellDate > stockData.SellDate ? oldData.SellDate : stockData.SellDate;
                }
                else
                {
                    buyDate = oldData.BuyDate > stockData.BuyDate ? stockData.BuyDate : oldData.BuyDate;
                    sellDate = DateTime.MinValue;
                }

                StockData newData = new StockData()
                {
                    Type = oldData.Type,
                    Code = oldData.Code,
                    Name = oldData.Name,
                    Num = oldData.Num + stockData.Num,
                    Unit = (oldData.Sum + stockData.Sum) / (oldData.Num + stockData.Num),
                    Profit = oldData.Profit + stockData.Profit,
                    BuyDate = buyDate,
                    SellDate = sellDate,
                };

                newData.Init();

                allStockDatas[stockData.Code] = newData;
            }
        }
        else
        {
            if (mergeType == MergeType.Hold || mergeType == MergeType.Mix)
            {
                foreach (var stockData in stockDatas)
                {
                    if (stockData.SellType != SellType.Hold)
                    {
                        newStockDatas.Add(stockData);
                        continue;
                    }
                    
                    if (holdStockDatas.TryAdd(stockData.Code, stockData))
                    {
                        continue;
                    }
                    
                    var oldData = holdStockDatas[stockData.Code];
                
                    StockData newData = new StockData()
                    {
                        Type = oldData.Type,
                        Code = oldData.Code,
                        Name = oldData.Name,
                        Num = oldData.Num + stockData.Num,
                        Unit = (oldData.Sum + stockData.Sum) / (oldData.Num + stockData.Num),
                        Profit = oldData.Profit + stockData.Profit,
                        BuyDate = oldData.BuyDate > stockData.BuyDate ? stockData.BuyDate : oldData.BuyDate,
                        SellDate = oldData.SellDate,
                    };
                
                    newData.Init();
                
                    holdStockDatas[stockData.Code] = newData;
                }
            }
            
            if (newStockDatas.Count > 0)
            {
                stockDatas = newStockDatas;
            }
            
            if (mergeType == MergeType.Sold || mergeType == MergeType.Mix)
            {
                foreach (var stockData in stockDatas)
                {
                    if (stockData.SellType != SellType.Sold)
                    {
                        if (mergeType != MergeType.Mix)
                        {
                            newStockDatas.Add(stockData);
                        }
                        
                        continue;
                    }
                    
                    if (soldStockDatas.TryAdd(stockData.Code, stockData))
                    {
                        continue;
                    }
                    
                    var oldData = soldStockDatas[stockData.Code];
                
                    StockData newData = new StockData()
                    {
                        Type = oldData.Type,
                        Code = oldData.Code,
                        Name = oldData.Name,
                        Num = oldData.Num + stockData.Num,
                        Unit = (oldData.Sum + stockData.Sum) / (oldData.Num + stockData.Num),
                        Profit = oldData.Profit + stockData.Profit,
                        BuyDate = oldData.BuyDate > stockData.BuyDate ? stockData.BuyDate : oldData.BuyDate,
                        SellDate = oldData.SellDate > stockData.SellDate ? oldData.SellDate : stockData.SellDate,
                    };
                
                    newData.Init();
                
                    soldStockDatas[stockData.Code] = newData;
                }
            }
            
            if (newStockDatas.Count > 0 && mergeType == MergeType.Mix)
            {
                newStockDatas.Clear();
            }
        }

        newStockDatas.AddRange(holdStockDatas.Values.ToList());
        newStockDatas.AddRange(soldStockDatas.Values.ToList());
        newStockDatas.AddRange(allStockDatas.Values.ToList());
        
        return newStockDatas;
    }

    IEnumerator UpdateStockPrice()
    {
        foreach (var kv in StockDatas)
        {
            if (kv.Value.SellType == SellType.Sold)
            {
                continue;
            }

            if (kv.Value.Type == StockType.US)
            {
                yield return GetStockPrice(kv.Value);
            }
            else
            {
                yield return SendRequest(kv.Value);
            }
        }
        
        StockUI.Instance.RefreshList();
    }
    
    IEnumerator SendRequest(StockData stockData)
    {
        var s = PlayerPrefs.GetString("CurUnit_" + stockData.Code);

        if (!string.IsNullOrEmpty(s))
        {
            var ss = s.Split(',');
            var date = new DateTime(long.Parse(ss[1]));
            var now = DateTime.Now;

            if (date.ToShortDateString().Equals(now.ToShortDateString()) && (date.Hour == now.Hour || ((date.Hour >= 16 && stockData.Type == StockType.HK) || (date.Hour >= 15 && stockData.Type != StockType.HK))))
            {
                CurUnits.TryAdd(stockData.Code, double.Parse(ss[0]));
                stockData.Calc();
                yield break;
            }
        }
        
        string symbol = stockData.Code;

        if (stockData.Type == StockType.HK)
        {
            symbol += ".HK";
        }
        else if (stockData.Code.StartsWith("5") || stockData.Code.StartsWith("6"))
        {
            symbol += ".SH";
        }
        else if (stockData.Code.StartsWith("0") || stockData.Code.StartsWith("1") || stockData.Code.StartsWith("3"))
        {
            symbol += ".SZ";
        }
        else
        {
            symbol += ".BJ";
        }
        
        string url = "http://localhost:5000/unity";
        string jsonBody = "{\"code\": \"" + symbol + "\"}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Python响应: " + request.downloadHandler.text);
                JObject data = JObject.Parse(request.downloadHandler.text);
                string result = (string)data["response"];
                var ss = result.Split(',');
                
                Debug.Log($"<{stockData.Name}>当前价格: {ss[1]}");

                if (string.IsNullOrEmpty(stockData.Name))
                {
                    stockData.Name = ss[0];
                    AddCodeName(stockData.Code, stockData.Name);
                    SaveCodeName();
                    SaveAll();
                }
                
                CurUnits.TryAdd(stockData.Code, double.Parse(ss[1]));
                stockData.Calc();
                
                PlayerPrefs.SetString("CurUnit_" + stockData.Code, $"{stockData.CurUnit},{DateTime.Now.Ticks}");
            }
            else
                Debug.LogError("请求失败: " + request.error);
        }
        
        yield return new WaitForSeconds(0.5f);
    }
    
    IEnumerator GetStockPrice(StockData stockData)
    {
        var s = PlayerPrefs.GetString("CurUnit_" + stockData.Code);

        if (!string.IsNullOrEmpty(s))
        {
            var ss = s.Split(',');
            var date = new DateTime(long.Parse(ss[1]));
            var now = DateTime.Now;

            if (date.ToShortDateString().Equals(now.ToShortDateString()))
            {
                CurUnits.TryAdd(stockData.Code, double.Parse(ss[0]));
                stockData.Calc();
                yield break;
            }
        }
        
        string symbol = stockData.Code;
        
        // 构建API请求URL
        string url = $"https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol={symbol}&apikey=E2I268LB8XIN5OXG";
        
        Debug.Log($"请求URL：{url}");

        UnityWebRequest request = UnityWebRequest.Get(url);
        
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || 
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            
            Regex priceRegex = new Regex("\"05\\. price\": \"(\\d+\\.\\d+)\"");
            Match match = priceRegex.Match(jsonResponse);

            if (match.Success)
            {
                string price = match.Groups[1].Value;
                Debug.Log($"{stockData.Name} 更新当前价格: ${price}");
                
                CurUnits.TryAdd(stockData.Code, double.Parse(price));
                stockData.Calc();
                
                PlayerPrefs.SetString("CurUnit_" + stockData.Code, $"{stockData.CurUnit},{DateTime.Now.Ticks}");
            }
            else
            {
                Debug.LogError("解析失败！原始响应: " + jsonResponse);
            }
        }
        
        yield return new WaitForSeconds(0.5f);
    }
}
