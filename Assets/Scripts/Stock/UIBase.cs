using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBase : MonoBehaviour
{
    protected void AddListener(string name, Action a)
    {
        GetButton(name).onClick.AddListener(() =>
        {
            a?.Invoke();
        });
    }

    protected Button GetButton(string name)
    {
        return transform.Find(name).GetComponent<Button>();
    }

    protected InputField GetInput(string name)
    {
        return transform.Find(name).GetComponent<InputField>();
    }
    
    protected Dropdown GetDropdown(string name)
    {
        return transform.Find(name).GetComponent<Dropdown>();
    }

    protected Text GetText(string name)
    {
        return transform.Find(name).GetComponent<Text>();
    }

    protected string GetInputText(string name)
    {
        return GetInput(name).text;
    }
    
    protected int GetInputInt(string name)
    {
        return int.Parse(GetInput(name).text);
    }
    
    protected double GetInputDouble(string name)
    {
        double.TryParse(GetInput(name).text, out double d);
        return d;
    }
    
    protected DateTime GetInputDate(string name, bool isBuy = false)
    {
        var s = GetInput(name).text;

        if (string.IsNullOrEmpty(s))
        {
            return isBuy ? DateTime.Now : DateTime.MinValue;
        }

        if (int.TryParse(s, out int i))
        {
            var now = DateTime.Now;
            
            if (i >= 10000)
            {
                return new DateTime(i / 10000, i % 10000 / 100, i % 100);
            }
            else if (i > 100)
            {
                return new DateTime(now.Year, i / 100, i % 100);
            }
            else if (i > 31)
            {
                return new DateTime(now.Year, i / 10, i % 10);
            }
            else if (i <= 0)
            {
                return isBuy ? DateTime.Now : DateTime.MinValue;
            }
            else
            {
                return new DateTime(now.Year, now.Month, i);
            }
        }

        var ss = s.Split('/');
        return new DateTime(int.Parse(ss[0]), int.Parse(ss[1]), int.Parse(ss[2]));
    }
    
    protected int GetDropdownId(string name)
    {
        return GetDropdown(name).value;
    }
}
