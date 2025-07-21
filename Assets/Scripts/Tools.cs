using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Tools
{
    public static string ToPercent(this double d)
    {
        return (d * 100).ToString("f2") + "%";
    }
    
    public static string ToUnit(this double d)
    {
        return d.ToString("0.####");
    }
    
    public static string ToPrice(this double d)
    {
        return d.ToString("N2");
    }
}
