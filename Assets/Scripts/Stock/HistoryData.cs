using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HistoryData
{
    //股票分类
    public StockType Type;
    //日期
    public DateTime Date;
    //仓位
    public double Sum;
    //收益
    public double Profit;
    //浮盈
    public double FloatingProfit;
    
    public HistoryData()
    {
    }

    public HistoryData(string data)
    {
        var ss = data.Split(',');
        int i = 0;
        
        Type = (StockType)int.Parse(ss[i++]);
        Date = DateTime.Parse(ss[i++]);
        Sum = double.Parse(ss[i++]);
        Profit = double.Parse(ss[i++]);
        FloatingProfit = double.Parse(ss[i++]);
    }
    
    public override string ToString()
    {
        return $"{(int)Type},{Date.ToShortDateString()},{Sum:f2},{Profit:f2},{FloatingProfit:f2}";
    }
}
