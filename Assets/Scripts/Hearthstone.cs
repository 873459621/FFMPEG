using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Hearthstone : MonoBehaviour
{
    private List<List<int>> allRecord = new List<List<int>>();

    void Start()
    {
        StartCoroutine(StartLushi(3, 4, 2, 0.5f));
    }
    
    private IEnumerator StartLushi(int lv, int num, int star, float winRate)
    {
        int calcCount = 10000;
        
        for (int i = 0; i < calcCount; i++)
        {
            yield return LushiTest(lv, num, star, winRate);
        }

        int max = 0;
        int min = int.MaxValue;
        int total = 0;

        foreach (var record in allRecord)
        {
            if (record.Count > max)
            {
                max = record.Count;
            }

            if (record.Count < min)
            {
                min = record.Count;
            }

            total += record.Count;
        }
        
        allRecord.Sort((a, b) => a.Count.CompareTo(b.Count));
        
        Debug.LogError($"模拟次数：{calcCount}，最多对局数：{max}，最少对局数：{min}，平均对局数：{total / calcCount}，中位数：{allRecord[allRecord.Count / 2].Count}");
    }

    private IEnumerator LushiTest(int lv, int num, int star, float winRate)
    {
        List<int> record = new List<int>();
        int calcCount = 1000;
            
        int winCount = 0;
        
        while (true)
        {
            for (int i = 0; i < calcCount; i++)
            {
                if (Random.value <= winRate)
                {
                    record.Add(1);
                    winCount++;
                
                    if (winCount > 2 && !(lv == 5 && num <= 5))
                    {
                        star += 2;
                    }
                    else
                    {
                        star++;
                    }

                    if (star > 3)
                    {
                        star -= 3;
                        num--;

                        if (num <= 0)
                        {
                            lv++;
                            num = 10;
                        }
                    }
                }
                else
                {
                    record.Add(0);
                    winCount = 0;

                    star--;

                    if (star < 0)
                    {
                        if (num != 5 && num != 10)
                        {
                            star += 3;
                            num++;
                        }
                        else
                        {
                            star = 0;
                        }
                    }
                }
                
                if (lv > 5)
                {
                    Debug.LogError($"到达传说，对局数：{record.Count}，对局记录：{string.Join(',', record)}");
                    allRecord.Add(record);
                    yield break;
                }
            }
            yield return null;
        }
    }
}
