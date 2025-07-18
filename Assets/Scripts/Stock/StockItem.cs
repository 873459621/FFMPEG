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
        this.stockData = stockData;
        GetText("code").text = stockData.Code;
        GetText("name").text = stockData.Name;
        GetText("sum").text = stockData.Sum.ToString("f2");
        GetText("num").text = stockData.Num.ToString();
        GetText("unit").text = stockData.Unit.ToString("f4");
        GetText("buy").text = stockData.BuyDate.ToShortDateString();
        GetButton("btn_apply").gameObject.SetActive(true);

        if (stockData.SellType == SellType.Sold)
        {
            GetText("profit").text = stockData.Profit.ToString("f2");
            GetText("rate").text = (stockData.Rate * 100).ToString("f2") + "%";
            GetButton("btn_sell").gameObject.SetActive(false);
            GetText("sell").text = stockData.SellDate.ToShortDateString();
        }
        else
        {
            GetText("profit").text = stockData.FloatingProfit.ToString("f2");
            GetText("rate").text = (stockData.FloatingRate * 100).ToString("f2") + "%";
            GetButton("btn_sell").gameObject.SetActive(true);
            GetText("sell").text = $"{stockData.CurUnit.ToString("f4")}";
        }
    }
}
