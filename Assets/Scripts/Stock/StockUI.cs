using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StockUI : UIBase
{
    public static StockUI Instance;
    
    public GameObject Item;
    
    private List<StockItem> items = new List<StockItem>();

    private StockData curStockData;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        AddListener("btn_save", () =>
        {
            var code = GetInputText("code");
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
            
            RefreshList();

            GetInput("code").text = "";
            GetInput("name").text = "";
            GetInput("num").text = "";
            GetInput("unit").text = "";
            GetInput("profit").text = "";
            GetInput("buy").text = "";
            GetInput("sell").text = "";
        });
        
        AddListener("btn_show", RefreshList);
        
        RefreshList();
    }

    public void RefreshList()
    {
        var stockType = (StockType)GetDropdownId("stocktype");
        var sellType = (SellType)GetDropdownId("selltype");
        
        var stockDatas = StockDataManager.Instance.GetStockDatas(stockType, sellType);

        double totalSum = 0;
        double historyProfit = 0;
        double totalFloatingProfit = 0;

        double exchange = 1;
        
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
                items.Add(item);
            }

            if (stockType == StockType.All && stockDatas[i].Type == StockType.US)
            {
                exchange = StockDataManager.Instance.US_Exchange;
            }
            else if (stockType == StockType.All && stockDatas[i].Type == StockType.HK)
            {
                exchange = StockDataManager.Instance.HK_Exchange;
            }
            else
            {
                exchange = 1;
            }

            if (stockDatas[i].SellType != SellType.Sold)
            {
                totalSum += stockDatas[i].Sum * exchange;
                totalFloatingProfit += stockDatas[i].FloatingProfit * exchange;
            }
            else
            {
                historyProfit += stockDatas[i].Profit * exchange;
            }
        }

        for (int i = stockDatas.Count; i < items.Count; i++)
        {
            items[i].gameObject.SetActive(false);
        }

        if (sellType == SellType.Sold)
        {
            GetText("msg").text = $"历史盈亏：{historyProfit.ToString("f2")}";
        }
        else
        {
            GetText("msg").text = $"总仓位：{totalSum.ToString("f2")}    浮盈：{totalFloatingProfit.ToString("f2")}    浮盈率：{(totalFloatingProfit / totalSum * 100).ToString("f2")}%\n总盈利：{historyProfit.ToString("f2")}    总盈利（浮动）：{(totalFloatingProfit + historyProfit).ToString("f2")}";
        }
    }

    public void ModifyData(StockData stockData)
    {
        curStockData = stockData;

        GetDropdown("type").value = (int)stockData.Type;
        GetInput("code").text = stockData.Code;
        GetInput("name").text = stockData.Name;
        GetInput("num").text = stockData.Num.ToString();
        GetInput("unit").text = stockData.Unit.ToString("f4");
        GetInput("profit").text = stockData.Profit.ToString("f2");
        GetInput("buy").text = stockData.BuyDate.ToShortDateString();
        GetInput("sell").text = stockData.SellDate.ToShortDateString();
    }

    public void Sell(StockData stockData)
    {
        double d = GetInputDouble("profit");

        if (d == 0)
        {
            return;
        }
        
        stockData.SellDate = DateTime.Now;
        stockData.Profit = d;
        stockData.Calc();
        
        StockDataManager.Instance.SaveAll();
        
        RefreshList();
    }
}
