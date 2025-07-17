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
        var stockDatas = StockDataManager.Instance.GetStockDatas((StockType)GetDropdownId("stocktype"), (SellType)GetDropdownId("selltype"));
        
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
        }

        for (int i = stockDatas.Count; i < items.Count; i++)
        {
            items[i].gameObject.SetActive(false);
        }
    }

    public void ModifyData(StockData stockData)
    {
        curStockData = stockData;

        GetDropdown("type").value = (int)stockData.Type;
        GetInput("code").text = stockData.Code;
        GetInput("name").text = stockData.Name;
        GetInput("num").text = stockData.Num.ToString();
        GetInput("unit").text = stockData.Unit.ToString("f3");
        GetInput("profit").text = stockData.Profit.ToString("f2");
        GetInput("buy").text = stockData.BuyDate.ToShortDateString();
        GetInput("sell").text = stockData.SellDate.ToShortDateString();
    }
}
