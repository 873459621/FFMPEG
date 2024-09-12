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
        Texts.Add(name, transform.Find(name).GetComponent<Text>());
    }
    
    private Dictionary<string, int> MsgCache = new Dictionary<string, int>();
    private string url;

    private void Start()
    {
        url = PlayerPrefs.GetString("url", "https://1a17-116-51-23-163.ngrok-free.app/post_endpoint");

        AddText("url");
        AddText("hint");
        AddInput("msg");

        Texts["url"].text = url;
        Texts["hint"].text = "";
        
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
                ShowHint("更新成功！");
            }
        });
        
        AddListener("btn_find", () =>
        {
            SceneManager.LoadScene(1);
        });
    }

    private void ShowHint(string hint)
    {
        StartCoroutine(Hint(hint));
    }

    private IEnumerator Hint(string hint)
    {
        Texts["hint"].text = hint;
        yield return new WaitForSeconds(2);
        Texts["hint"].text = "";
    }

    private string lastMsg;
    
    private void SaveMsg(string msg)
    {
        if (string.IsNullOrEmpty(msg) || MsgCache.ContainsKey(msg))
        {
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
                    ShowHint($"消息发送成功：{lastMsg}");
                }
                Debug.Log("Response: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }
        };
    }
}