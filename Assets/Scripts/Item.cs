using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    public Text Name;
    public Text Bit;
    public Text Time;
    public Button btn_jump;
    public Button btn_encode;

    public VideoInfo VideoInfo;

    private void Start()
    {
        btn_jump.onClick.AddListener(() =>
        {
            FFMPEGUtil.OpenFolderAndSelectedFile($"{VideoInfo.Path}\\{VideoInfo.Name}");
        });
        
        btn_encode.onClick.AddListener(() =>
        {
            FFMPEGUtil.OpenFolder(VideoInfo.Path);
        });
    }

    public void SetVideo(VideoInfo info)
    {
        VideoInfo = info;
        Name.text = info.Name;
        Bit.text = info.Bitrate.ToString();
        Time.text = info.Duration.ToString();
        btn_jump.gameObject.SetActive(true);
        btn_encode.gameObject.SetActive(true);
    }
}
