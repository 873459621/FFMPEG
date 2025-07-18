using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StockItem : UIBase
{
    private StockData stockData;
    
    void Start()
    {
        AddListener("btn_apply", () =>
        {
            StockUI.Instance.ModifyData(stockData);
        });
        
        AddListener("btn_sell", () =>
        {
            StockUI.Instance.Sell(stockData);
        });
    }

    public void Init(StockData stockData)
    {
        double exchange = 1;
        
        if (StockUI.Instance.CurStockType == StockType.All && stockData.Type == StockType.US)
        {
            exchange = StockDataManager.Instance.US_Exchange;
        }
        else if (StockUI.Instance.CurStockType == StockType.All && stockData.Type == StockType.HK)
        {
            exchange = StockDataManager.Instance.HK_Exchange;
        }
        else
        {
            exchange = 1;
        }
        
        this.stockData = stockData;
        GetText("code").text = stockData.Code;
        GetText("name").text = stockData.Name;
        GetText("sum").text = (stockData.Sum * exchange).ToString("f2");
        GetText("num").text = stockData.Num.ToString();
        GetText("unit").text = stockData.Unit.ToString("f4");
        GetText("buy").text = stockData.BuyDate.ToShortDateString();
        GetButton("btn_apply").gameObject.SetActive(true);

        if (stockData.SellType == SellType.Sold)
        {
            GetText("profit").text = (stockData.Profit * exchange).ToString("f2");
            GetText("rate").text = (stockData.Rate * 100).ToString("f2") + "%";
            GetButton("btn_sell").gameObject.SetActive(false);
            GetText("sell").text = stockData.SellDate.ToShortDateString();
            GetText("fsum").text = "";
            GetText("funit").text = "";
            GetText("day").text = (stockData.SellDate - stockData.BuyDate).Days.ToString();
        }
        else
        {
            GetText("profit").text = (stockData.FloatingProfit * exchange).ToString("f2");
            GetText("rate").text = (stockData.FloatingRate * 100).ToString("f2") + "%";
            GetButton("btn_sell").gameObject.SetActive(true);
            GetText("sell").text = "";
            GetText("fsum").text = ((stockData.Sum + stockData.FloatingProfit) * exchange).ToString("f2");
            GetText("funit").text = stockData.CurUnit.ToString("f4");
            GetText("day").text = (DateTime.Now - stockData.BuyDate).Days.ToString();
        }
    }
}
