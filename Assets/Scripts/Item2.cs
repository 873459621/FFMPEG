using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ShowType
{
    Name,
    Pre,
    Sub,
}

public class Item2 : MonoBehaviour
{
    public Text Name;
    public Text File;
    public Button btn_file;
    public Button btn_folder;
    public Button btn_expand;

    public List<MP4> List;
    public MP4 Mp4;

    private void Start()
    {
        btn_file.onClick.AddListener(() =>
        {
            FFMPEGUtil.OpenFolderAndSelectedFile(Mp4.Path);
        });
        
        btn_folder.onClick.AddListener(() =>
        {
            FFMPEGUtil.OpenFolder(Mp4.Path.Substring(0, Mp4.Path.LastIndexOf('\\')));
        });
        
        btn_expand.onClick.AddListener(() =>
        {
            UI2.Instance.ShowList(List);
        });
    }

    public void ShowMp4(MP4 mp4)
    {
        Mp4 = mp4;
        Name.text = mp4.Name;
        File.text = mp4.RealName;
        btn_file.gameObject.SetActive(true);
        btn_folder.gameObject.SetActive(true);
        btn_expand.gameObject.SetActive(false);
    }
    
    public void ShowList(List<MP4> list, ShowType type = ShowType.Name)
    {
        List = list;

        switch (type)
        {
            case ShowType.Name:
                Name.text = list[0].Name;
                break;
            case ShowType.Pre:
                Name.text = list[0].Pre;
                break;
            case ShowType.Sub:
                Name.text = list[0].Sub;
                break;
        }

        File.text = "";
        btn_file.gameObject.SetActive(false);
        btn_folder.gameObject.SetActive(false);
        btn_expand.gameObject.SetActive(true);
    }
}