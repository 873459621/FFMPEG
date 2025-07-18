using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using System.Text;
using Newtonsoft.Json.Linq;

public class StockDataManager : MonoBehaviour
{
    public static StockDataManager Instance;

    private Dictionary<string, string> CodeName = new Dictionary<string, string>();
    public Dictionary<int, StockData> StockDatas = new Dictionary<int, StockData>();
    public Dictionary<string, double> CurUnits = new Dictionary<string, double>();

    public Dictionary<string, StockData> HoldStockDatas = new Dictionary<string, StockData>();

    public int Index = 0;

    public double US_Exchange {
        get
        {
            return double.Parse(PlayerPrefs.GetString("US_Exchange", "7.2"));
        }

        set
        {
            PlayerPrefs.SetString("US_Exchange", value.ToString());
            Debug.Log($"美元汇率更新：{US_Exchange}");
        }
    }
    
    public double HK_Exchange {
        get
        {
            return double.Parse(PlayerPrefs.GetString("HK_Exchange", "0.9"));
        }

        set
        {
            PlayerPrefs.SetString("HK_Exchange", value.ToString());
            Debug.Log($"港元汇率更新：{HK_Exchange}");
        }
    }

    private void Awake()
    {
        Instance = this;
        LoadCodeName();
        LoadAll();
        
        Debug.Log($"当前美元汇率：{US_Exchange}");
        Debug.Log($"当前港元汇率：{HK_Exchange}");

        StartCoroutine(UpdateStockPrice());
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

    public void CalcHoldStockDatas()
    {
        HoldStockDatas.Clear();
        
        foreach (var kv in StockDatas)
        {
            if (kv.Value.SellType == SellType.Hold && !HoldStockDatas.TryAdd(kv.Value.Code, kv.Value))
            {
                var oldData = HoldStockDatas[kv.Value.Code];
                StockData stockData = new StockData()
                {
                    Type = oldData.Type,
                    Code = oldData.Code,
                    Name = oldData.Name,
                    Num = oldData.Num + kv.Value.Num,
                    Unit = (oldData.Unit * oldData.Num + kv.Value.Unit * kv.Value.Num) / (oldData.Num + kv.Value.Num),
                    Profit = 0,
                    BuyDate = oldData.BuyDate > kv.Value.BuyDate ? kv.Value.BuyDate : oldData.BuyDate,
                    SellDate = oldData.SellDate,
                };
                stockData.Init();
                HoldStockDatas[kv.Value.Code] = stockData;
            }
        }
    }
    
    public List<StockData> GetHoldStockDatas(StockType stockType = StockType.All, SellType sellType = SellType.All)
    {
        List<StockData> stockDatas = new List<StockData>();

        foreach (var kv in HoldStockDatas)
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
        
        stockDatas.Sort((a, b) => a.Sum.CompareTo(b.Sum));

        return stockDatas;
    }

    IEnumerator UpdateStockPrice()
    {
        yield return new WaitForSeconds(2.5f);
        
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

            if (date.ToShortDateString().Equals(now.ToShortDateString()) && date.Hour == now.Hour)
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
