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

        bool showExchange = StockUI.Instance.ShowExchange;

        Color color = (stockData.Profit > 0 || this.stockData.FloatingRate > 0) ? new Color(1, 0.2f, 0.2f) : Color.green;
        
        GetText("code").text = stockData.Code;
        GetText("name").text = stockData.Name;
        GetText("sum").text = (showExchange ? stockData.ExchangeSum : stockData.Sum).ToPrice();
        GetText("num").text = stockData.Num.ToString();
        GetText("unit").text = stockData.Unit.ToUnit();
        GetText("buy").text = stockData.BuyDate.ToShortDateString();
        GetButton("btn_apply").gameObject.SetActive(StockDataManager.Instance.StockDatas.ContainsKey(stockData.Id));
        GetText("day").text = stockData.HoldDays.ToString();

        GetText("profit").color = color;
        GetText("rate").color = color;
        // GetText("fsum").color = color;
        // GetText("funit").color = color;

        if (stockData.SellType == SellType.Sold)
        {
            GetText("profit").text = (showExchange ? stockData.ExchangeProfit : stockData.Profit).ToPrice();
            GetText("rate").text = stockData.Rate.ToPercent();
            GetButton("btn_sell").gameObject.SetActive(false);
            GetText("sell").text = stockData.SellDate.ToShortDateString();
            GetText("fsum").text = "";
            GetText("funit").text = (stockData.Profit / stockData.Num + stockData.Unit).ToUnit();
        }
        else
        {
            GetText("profit").text = (showExchange ? stockData.ExchangeFloatingProfit : stockData.FloatingProfit).ToPrice();
            GetText("rate").text = stockData.FloatingRate.ToPercent();
            GetButton("btn_sell").gameObject.SetActive(StockDataManager.Instance.StockDatas.ContainsKey(stockData.Id));
            GetText("sell").text = "";
            GetText("fsum").text = (showExchange ? stockData.ExchangeFloatingSum : stockData.FloatingSum).ToPrice();
            GetText("funit").text = stockData.CurUnit.ToUnit();
        }
    }
}
