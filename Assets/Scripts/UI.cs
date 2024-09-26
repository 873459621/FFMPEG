using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public static UI Instance;
    
    public InputField Input;
    public InputField Output;
    public Button btn_scan;
    public Button btn_load;
    public Button btn_encode;
    public Button btn_move;
    public Button btn_del;
    
    public RectTransform Content;
    public GameObject Item;

    private List<Item> items = new List<Item>();

    private void Start()
    {
        Instance = this;
        
        btn_scan.onClick.AddListener(() =>
        {
            FFMPEGUtil.Instance.Scan(Input.text);
        });
        
        btn_load.onClick.AddListener(() =>
        {
            FFMPEGUtil.Instance.Load(Input.text);
        });
        
        btn_encode.onClick.AddListener(() =>
        {
            FFMPEGUtil.Instance.GenSH(int.Parse(Output.text));
        });
        
        btn_move.onClick.AddListener(() =>
        {
            FFMPEGUtil.Instance.GenMoveSH(int.Parse(Output.text));
        });
        
        AddListener("btn_move2", () =>
        {
            FFMPEGUtil.Instance.GenMoveSH(int.Parse(Output.text), true);
        });
        
        btn_del.onClick.AddListener(() =>
        {
            StartCoroutine(FFMPEGUtil.Instance.GenDelSH());
        });
    }
    
    private void AddListener(string name, Action a)
    {
        transform.Find(name).GetComponent<Button>().onClick.AddListener(() =>
        {
            a?.Invoke();
        });
    }

    public void ShowVideo(List<VideoInfo> videoInfos)
    {
        int max = Mathf.Min(400, videoInfos.Count);
        Content.sizeDelta = new Vector2(Content.sizeDelta.x, (max + 1) * 64);

        for (int i = 0; i < max; i++)
        {
            if (i < items.Count)
            {
                items[i].SetVideo(videoInfos[i]);
            }
            else
            {
                var item = Instantiate(Item, Content).GetComponent<Item>();
                item.SetVideo(videoInfos[i]);
                (item.transform as RectTransform).anchoredPosition = new Vector2(0, (i + 1) * -64);
                items.Add(item);
            }
        }
    }
}
