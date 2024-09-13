using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class Web : MonoBehaviour
{
    private Dictionary<string, InputField> InputFields = new Dictionary<string, InputField>();
    private Dictionary<string, Text> Texts = new Dictionary<string, Text>();
    
    private void AddListener(string name, Action a)
    {
        transform.Find(name).GetComponent<Button>().onClick.AddListener(() =>
        {
            a?.Invoke();
        });
    }

    private void AddInput(string name)
    {
        InputFields.Add(name, transform.Find(name).GetComponent<InputField>());
    }
    
    private void AddText(string name)
    {
        var text = transform.Find(name).GetComponent<Text>();
        text.text = "";
        Texts.Add(name, text);
    }
    
    private void ShowHint(string key, string hint)
    {
        StartCoroutine(Hint(key, hint));
    }

    private IEnumerator Hint(string key, string hint)
    {
        Texts[key].text = hint;
        yield return new WaitForSeconds(2);
        Texts[key].text = "";
    }
    
    public string DefaultUrl = "https://a0cb-116-51-23-163.ngrok-free.app/post_endpoint";
    
    private Dictionary<string, int> MsgCache = new Dictionary<string, int>();
    private string url;

    private void Start()
    {
        // ngrok http http://localhost:8989
        url = PlayerPrefs.GetString("url", DefaultUrl);

        AddText("url");
        AddText("hint");
        AddText("hint2");
        AddInput("msg");

        Texts["url"].text = url;
        
        AddListener("btn_send", () =>
        {
            SaveMsg(InputFields["msg"].text);
            InputFields["msg"].text = "";
        });
        
        AddListener("btn_url", () =>
        {
            var text = InputFields["msg"].text;

            if (!string.IsNullOrEmpty(text))
            {
                url = text;
                Texts["url"].text = url;
                PlayerPrefs.SetString("url", url);
                InputFields["msg"].text = "";
                ShowHint("hint","更新成功！");
            }
        });
        
        AddListener("btn_find", () =>
        {
            SceneManager.LoadScene(1);
        });
        
        AddListener("btn_clip", () =>
        {
            SendClipboard();
        });
    }

    private string lastMsg;
    
    private void SaveMsg(string msg)
    {
        if (string.IsNullOrEmpty(msg) || MsgCache.ContainsKey(msg))
        {
            ShowHint("hint2","链接为空或重复！");
            return;
        }
        
        MsgCache.Add(msg, 1);
        lastMsg = msg;
        
        WWWForm form = new WWWForm();
        form.AddField("key", "hhw_1995");
        form.AddField("msg", msg);

        UnityWebRequest request = UnityWebRequest.Post(url, form);
        request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        SendRequest(request);
    }

    private void SendRequest(UnityWebRequest request)
    {
        // 异步发送请求
        var asyncOperation = request.SendWebRequest();
        asyncOperation.completed += operation =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                if (request.downloadHandler.text.Equals("Success"))
                {
                    ShowHint("hint",$"消息发送成功：{lastMsg}");
                }
                ShowHint("hint2",$"回复：{request.downloadHandler.text}");
            }
            else
            {
                ShowHint("hint2",$"错误：{request.error}");
            }
        };
    }

    private void SendClipboard()
    {
        using (var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (var currentActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity"))
        {
            string clipboardText = currentActivity.CallStatic<string>("getClipboardText", currentActivity);
            
            SaveMsg(clipboardText);
        }
    }
}