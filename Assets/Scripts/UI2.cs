using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI2 : MonoBehaviour
{
    public static UI2 Instance;
    
    public InputField Input1;
    public InputField Input2;
    public Button btn_read;
    public Button btn_gen;
    public Button btn_add;
    public Button btn_export;
    public Button btn_find;
    public Button btn_total;
    public Button btn_name;
    public Button btn_pre;
    public Button btn_sub;
    public Button btn_non;
    public Button btn_findall;
    public Button btn_back;
    
    public RectTransform Content;
    public GameObject Item;

    private List<Item2> items = new List<Item2>();

    private List<List<MP4>> cache_list;
    private List<MP4> cache_mp4;
    private ShowType cache_type;
    private bool isMp4 = false;

    private void Start()
    {
        Instance = this;

        btn_read.onClick.AddListener(() =>
        {
            FFMPEGUtil.Instance.ReadTotal();
            ShowList(FFMPEGUtil.Instance.GetTotalList());
        });

        btn_gen.onClick.AddListener(() =>
        {
            FFMPEGUtil.Instance.GenTotal();
            ShowList(FFMPEGUtil.Instance.GetTotalList());
        });

        btn_add.onClick.AddListener(() =>
        {
            FFMPEGUtil.Instance.AddToTotal(Input1.text);
            ShowList(FFMPEGUtil.Instance.GetTotalList());
        });

        btn_export.onClick.AddListener(() => { FFMPEGUtil.Instance.ExportTotal(); });

        btn_find.onClick.AddListener(() => { ShowList(FFMPEGUtil.Instance.FindMp4(Input1.text, Input2.text)); });

        btn_total.onClick.AddListener(() => { ShowList(FFMPEGUtil.Instance.GetTotalList()); });

        btn_name.onClick.AddListener(() => { ShowList(FFMPEGUtil.Instance.GetSameNameList()); });

        btn_pre.onClick.AddListener(() => { ShowList(FFMPEGUtil.Instance.GetPreList(), ShowType.Pre); });

        btn_sub.onClick.AddListener(() => { ShowList(FFMPEGUtil.Instance.GetSubList(), ShowType.Sub); });

        btn_non.onClick.AddListener(() => { ShowList(FFMPEGUtil.Instance.GetNonList()); });

        btn_findall.onClick.AddListener(() => { ShowList(FFMPEGUtil.Instance.FindAllMp4(Input1.text)); });

        btn_back.onClick.AddListener(() =>
        {
            if (cache_list != null)
            {
                ShowList(cache_list, cache_type);
            }
        });

        AddListener("btn_match", () =>
        {
            FFMPEGUtil.Instance.TestFile(Input1.text);
        });
        
        AddListener("btn_move", () =>
        {
            if (isMp4 && cache_mp4 != null)
            {
                FFMPEGUtil.Instance.GenMoveMp4SH(cache_mp4);
            }
            else if (!isMp4 && cache_list != null)
            {
                FFMPEGUtil.Instance.GenMoveMp4SH(cache_list);
            }
        });
        
        AddListener("btn_web", () =>
        {
            SceneManager.LoadScene(0);
        });
        
        AddListener("btn_img", () =>
        {
            FFMPEGUtil.Instance.ScanImage(Input1.text);
        });
        
        AddListener("btn_test", () =>
        {
            FFMPEGUtil.Instance.TestDir(Input1.text);
        });
    }

    private void AddListener(string name, Action a)
    {
        transform.Find(name).GetComponent<Button>().onClick.AddListener(() =>
        {
            a?.Invoke();
        });
    }

    public void ShowList(List<List<MP4>> list, ShowType type = ShowType.Name)
    {
        isMp4 = false;
        cache_list = list;
        cache_type = type;
        
        int max = Mathf.Min(400, list.Count);
        Content.sizeDelta = new Vector2(Content.sizeDelta.x, (max + 1) * 64);
        
        for (int i = 0; i < max; i++)
        {
            if (i < items.Count)
            {
                items[i].gameObject.SetActive(true);
                items[i].ShowList(list[i], type);
            }
            else
            {
                var item = Instantiate(Item, Content).GetComponent<Item2>();
                item.ShowList(list[i], type);
                (item.transform as RectTransform).anchoredPosition = new Vector2(0, (i + 1) * -64);
                items.Add(item);
            }
        }

        for (int i = max; i < items.Count; i++)
        {
            items[i].gameObject.SetActive(false);
        }
    }
    
    public void ShowList(List<MP4> list)
    {
        isMp4 = true;
        cache_mp4 = list;
        
        int max = Mathf.Min(400, list.Count);
        Content.sizeDelta = new Vector2(Content.sizeDelta.x, (max + 1) * 64);
        
        for (int i = 0; i < max; i++)
        {
            if (i < items.Count)
            {
                items[i].gameObject.SetActive(true);
                items[i].ShowMp4(list[i]);
            }
            else
            {
                var item = Instantiate(Item, Content).GetComponent<Item2>();
                item.ShowMp4(list[i]);
                (item.transform as RectTransform).anchoredPosition = new Vector2(0, (i + 1) * -64);
                items.Add(item);
            }
        }

        for (int i = max; i < items.Count; i++)
        {
            items[i].gameObject.SetActive(false);
        }
    }
}