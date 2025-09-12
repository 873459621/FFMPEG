using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum StockType
{
    All = 0,
    Short,
    Mid,
    Long,
    US,
    HK,
    A,
}

public enum SellType
{
    All = 0,
    Hold,
    Sold,
}

public enum MergeType
{
    No = 0,
    Hold,
    Sold,
    Mix,
    All,
}

public enum GroupType
{
    No = 0,
    DaiMi,
    BB,
    MoDa,
    BenZong,
    HaiXia,
    QiJiaYi,
}

public class StockData
{
    //股票分类
    public StockType Type;
    //股票代码
    public string Code;
    //股票名称
    public string Name;
    //买入数量
    public int Num;
    //买入单价
    public double Unit;
    //收益
    public double Profit;

    //买入日期
    public DateTime BuyDate;
    //卖出日期
    public DateTime SellDate;
    
    //------------------------------------------------
    
    //唯一ID
    public int Id;
    //卖出类型
    public SellType SellType;
    //买入金额
    public double Sum;
    //收益率
    public double Rate;
    
    //当前价格
    public double CurUnit;
    //浮盈
    public double FloatingProfit;
    //浮动收益率
    public double FloatingRate;
    //浮动仓位
    public double FloatingSum;
    //人民币仓位
    public double ExchangeSum;
    //人民币浮动仓位
    public double ExchangeFloatingSum;
    //人民币收益
    public double ExchangeProfit;
    //人民币浮动收益
    public double ExchangeFloatingProfit;
    //持有天数
    public int HoldDays;

    public StockData()
    {
    }

    public StockData(string data)
    {
        var ss = data.Split(',');
        int i = 0;
        
        Type = (StockType)int.Parse(ss[i++]);
        Code = ss[i++];
        Name = ss[i++];
        Num = int.Parse(ss[i++]);
        Unit = double.Parse(ss[i++]);
        Profit = double.Parse(ss[i++]);
        BuyDate = DateTime.Parse(ss[i++]);
        SellDate = DateTime.Parse(ss[i++]);

        Init();
    }

    public void Init()
    {
        Id = StockDataManager.Instance.Index++;
        Calc();
    }

    public void Calc()
    {
        SellType = SellDate >= BuyDate ? SellType.Sold : SellType.Hold;
        Sum = Unit * Num;
        Rate = Profit / Sum;

        if (StockDataManager.Instance.CurUnits.TryGetValue(Code, out double d) && SellType == SellType.Hold)
        {
            CurUnit = d;
        }

        if (CurUnit > 0)
        {
            FloatingProfit = (CurUnit - Unit) * Num;
            FloatingRate = FloatingProfit / Sum;
        }

        FloatingSum = Sum + FloatingProfit;
        
        double exchange = 1;
        
        if (Type == StockType.US)
        {
            exchange = StockDataManager.Instance.US_Exchange;
        }
        else if (Type == StockType.HK)
        {
            exchange = StockDataManager.Instance.HK_Exchange;
        }

        ExchangeSum = exchange * Sum;
        ExchangeProfit = exchange * Profit;
        ExchangeFloatingSum = exchange * FloatingSum;
        ExchangeFloatingProfit = exchange * FloatingProfit;

        HoldDays = SellDate >= BuyDate ? (SellDate - BuyDate).Days : (DateTime.Now - BuyDate).Days;
    }
    
    public override string ToString()
    {
        return $"{(int)Type},{Code},{Name},{Num},{Unit:f4},{Profit:f2},{BuyDate.ToShortDateString()},{SellDate.ToShortDateString()}";
    }
}
