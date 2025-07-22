using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StockUI : UIBase
{
    public static StockUI Instance;
    
    public GameObject Item;
    
    private List<StockItem> items = new List<StockItem>();

    private StockData curStockData;

    public StockType CurStockType;
    public bool ShowExchange = false;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        AddListener("btn_save", () =>
        {
            var code = GetInputText("code");

            if (string.IsNullOrEmpty(code) 
                || (StockType)GetDropdownId("type") == StockType.All 
                || GetInputDouble("unit") == 0 
                || GetInputInt("num") == 0)
            {
                return;
            }
            
            var name = StockDataManager.Instance.GetCodeName(code);

            if (string.IsNullOrEmpty(name))
            {
                name = GetInputText("name");
            }

            if (curStockData != null)
            {
                curStockData.Type = (StockType)GetDropdownId("type");
                curStockData.Code = code;
                curStockData.Name = name;
                curStockData.Num = GetInputInt("num");
                curStockData.Unit = GetInputDouble("unit");
                curStockData.Profit = GetInputDouble("profit");
                curStockData.BuyDate = GetInputDate("buy", true);
                curStockData.SellDate = GetInputDate("sell");
                
                curStockData.Calc();
                
                curStockData = null;
                
                StockDataManager.Instance.SaveAll();
            }
            else
            {
                StockData data = new StockData()
                {
                    Type = (StockType)GetDropdownId("type"),
                    Code = code,
                    Name = name,
                    Num = GetInputInt("num"),
                    Unit = GetInputDouble("unit"),
                    Profit = GetInputDouble("profit"),
                    BuyDate = GetInputDate("buy", true),
                    SellDate = GetInputDate("sell"),
                };
            
                data.Init();
            
                StockDataManager.Instance.AddStockData(data, true);
            }

            GetInput("code").text = "";
            GetInput("name").text = "";
            GetInput("num").text = "";
            GetInput("unit").text = "";
            GetInput("profit").text = "";
            GetInput("buy").text = "";
            GetInput("sell").text = "";
            
            RefreshList();
        });
        
        AddListener("btn_show", RefreshList);
        
        GetDropdown("type").value = (int)StockType.Short;

        GetDropdown("stocktype").value = (int)StockType.All;
        GetDropdown("selltype").value = (int)SellType.Hold;
        
        RefreshList();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            RefreshList();
        }
        
        if (Input.GetKeyDown(KeyCode.F1))
        {
            curStockData = null;

            GetDropdown("type").value = (int)StockType.Short;
            GetInput("code").text = "";
            GetInput("name").text = "";
            GetInput("num").text = "";
            GetInput("unit").text = "";
            GetInput("profit").text = "";
            GetInput("buy").text = "";
            GetInput("sell").text = "";
            
            RefreshList();
        }
        
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            GetDropdown("stocktype").value = (GetDropdownId("stocktype") + 1) % ((int)StockType.A + 1);
            RefreshList();
        }
    }

    public void RefreshList()
    {
        var stockType = (StockType)GetDropdownId("stocktype");
        var sellType = (SellType)GetDropdownId("selltype");
        var mergeType = (MergeType)GetDropdownId("mergetype");

        CurStockType = stockType;
        ShowExchange = GetToggle("exchange").isOn || CurStockType == StockType.All;

        List<StockData> stockDatas = StockDataManager.Instance.GetStockDatas(stockType, sellType);
        
        var code = GetInputText("code");

        if (!string.IsNullOrEmpty(code))
        {
            stockDatas.RemoveAll(x => !x.Code.Equals(code));
        }

        stockDatas = StockDataManager.Instance.CalcStockDatas(stockDatas, mergeType);

        if (GetToggle("dsum").isOn)
        {
            if (ShowExchange)
            {
                stockDatas.Sort((a, b) => a.ExchangeSum.CompareTo(b.ExchangeSum));
            }
            else
            {
                stockDatas.Sort((a, b) => a.Sum.CompareTo(b.Sum));
            }
        }
        
        if (GetToggle("dprofit").isOn)
        {
            if (ShowExchange)
            {
                if (sellType == SellType.Hold)
                {
                    stockDatas.Sort((a, b) => a.ExchangeFloatingProfit.CompareTo(b.ExchangeFloatingProfit));
                }
                else
                {
                    stockDatas.Sort((a, b) => a.ExchangeProfit.CompareTo(b.ExchangeProfit));
                }
            }
            else
            {
                if (sellType == SellType.Hold)
                {
                    stockDatas.Sort((a, b) => a.FloatingProfit.CompareTo(b.FloatingProfit));
                }
                else
                {
                    stockDatas.Sort((a, b) => a.Profit.CompareTo(b.Profit));
                }
            }
        }
        
        if (GetToggle("drate").isOn)
        {
            if (sellType == SellType.Hold)
            {
                stockDatas.Sort((a, b) => a.FloatingRate.CompareTo(b.FloatingRate));
            }
            else
            {
                stockDatas.Sort((a, b) => a.Rate.CompareTo(b.Rate));
            }
        }
        
        if (GetToggle("dbuy").isOn)
        {
            stockDatas.Sort((a, b) => a.BuyDate.CompareTo(b.BuyDate));
        }
        
        if (GetToggle("dsell").isOn && sellType == SellType.Sold)
        {
            stockDatas.Sort((a, b) => a.SellDate.CompareTo(b.SellDate));
        }
        
        if (GetToggle("dfsum").isOn && sellType == SellType.Hold)
        {
            if (ShowExchange)
            {
                stockDatas.Sort((a, b) => a.ExchangeFloatingSum.CompareTo(b.ExchangeFloatingSum));
            }
            else
            {
                stockDatas.Sort((a, b) => a.FloatingSum.CompareTo(b.FloatingSum));
            }
        }
        
        if (GetToggle("dday").isOn)
        {
            stockDatas.Sort((a, b) => a.HoldDays.CompareTo(b.HoldDays));
        }
        
        if (!GetToggle("dir").isOn)
        {
            stockDatas.Reverse();
        }

        double totalSum = 0;
        double historyProfit = 0;
        double totalFloatingProfit = 0;
        
        for (int i = 0; i < stockDatas.Count; i++)
        {
            if (i < items.Count)
            {
                items[i].gameObject.SetActive(true);
                items[i].Init(stockDatas[i]);
            }
            else
            {
                var item = Instantiate(Item, Item.transform.parent).GetComponent<StockItem>();
                item.Init(stockDatas[i]);
                item.gameObject.SetActive(true);
                items.Add(item);
            }

            if (ShowExchange)
            {
                if (stockDatas[i].SellType != SellType.Sold)
                {
                    totalSum += stockDatas[i].ExchangeSum;
                    totalFloatingProfit += stockDatas[i].ExchangeFloatingProfit;
                }
                else
                {
                    historyProfit += stockDatas[i].ExchangeProfit;
                }
            }
            else
            {
                if (stockDatas[i].SellType != SellType.Sold)
                {
                    totalSum += stockDatas[i].Sum;
                    totalFloatingProfit += stockDatas[i].FloatingProfit;
                }
                else
                {
                    historyProfit += stockDatas[i].Profit;
                }
            }
        }

        for (int i = stockDatas.Count; i < items.Count; i++)
        {
            items[i].gameObject.SetActive(false);
        }

        if (sellType == SellType.Sold)
        {
            GetText("msg").text = $"历史盈亏：{historyProfit.ToPrice()}";
        }
        else
        {
            GetText("msg").text = $"总仓位：{totalSum.ToPrice()}    浮盈：{totalFloatingProfit.ToPrice()}    浮盈率：{(totalFloatingProfit / totalSum).ToPercent()}\n总盈利：{historyProfit.ToPrice()}    总盈利（浮动）：{(totalFloatingProfit + historyProfit).ToPrice()}";
        }
    }

    public void ModifyData(StockData stockData)
    {
        curStockData = stockData;

        GetDropdown("type").value = (int)stockData.Type;
        GetInput("code").text = stockData.Code;
        GetInput("name").text = stockData.Name;
        GetInput("num").text = stockData.Num.ToString();
        GetInput("unit").text = stockData.Unit.ToUnit();
        GetInput("profit").text = stockData.Profit.ToPrice();
        GetInput("buy").text = stockData.BuyDate.ToShortDateString();
        GetInput("sell").text = stockData.SellDate.ToShortDateString();
    }

    public void Sell(StockData stockData)
    {
        double d = GetInputDouble("profit");
        int num = GetInputInt("num");
        double unit = GetInputDouble("unit");

        var sellData = GetInputDate("sell");

        if (!StockDataManager.Instance.StockDatas.ContainsKey(stockData.Id))
        {
            return;
        }

        if (d != 0)
        {
            stockData.SellDate = sellData;
            stockData.Profit = d;
            stockData.Calc();
        
            StockDataManager.Instance.SaveAll();
        
            RefreshList();
        }
        else if (num != 0 && unit != 0 && num < stockData.Num)
        {
            var newData = new StockData()
            {
                Type = stockData.Type,
                Code = stockData.Code,
                Name = stockData.Name,
                Num = stockData.Num - num,
                Unit = stockData.Unit,
                Profit = stockData.Profit,
                BuyDate = stockData.BuyDate,
                SellDate = stockData.SellDate,
            };
            
            newData.Init();
            StockDataManager.Instance.AddStockData(newData);
            
            stockData.SellDate = sellData;
            stockData.Num = num;
            stockData.Profit = (unit - stockData.Unit) * num;
            stockData.Calc();
        
            StockDataManager.Instance.SaveAll();
        
            RefreshList();
        }

        GetInput("profit").text = "";
        GetInput("num").text = "";
        GetInput("unit").text = "";
        GetInput("sell").text = "";
    }
}
