using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StockDataManager : MonoBehaviour
{
    public static StockDataManager Instance;

    private Dictionary<string, string> CodeName = new Dictionary<string, string>();
    private Dictionary<int, StockData> StockDatas = new Dictionary<int, StockData>();

    public int Index = 0;

    private void Awake()
    {
        Instance = this;
        LoadCodeName();
        LoadAll();
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
        if (AddCodeName(stockData.Code, stockData.Name))
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
                continue;
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
}
