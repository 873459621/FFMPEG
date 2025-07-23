using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using Random = UnityEngine.Random;

public class ExchangeRateFetcher : MonoBehaviour
{
    // 从Alpha Vantage获取实时汇率（免费API密钥）
    private string apiURL = "https://www.alphavantage.co/query?function=CURRENCY_EXCHANGE_RATE&from_currency=USD&to_currency=CNH&apikey=IWVNVTWTNUIGGC7Y";
    private string apiURL2 = "https://www.alphavantage.co/query?function=CURRENCY_EXCHANGE_RATE&from_currency=HKD&to_currency=CNH&apikey=IWVNVTWTNUIGGC7Y";

    public static ExchangeRateFetcher Instance;

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator ExchangeRateFetch()
    {
        var date = PlayerPrefs.GetString("ExchangeRateFetcher");

        if (string.IsNullOrEmpty(date) || !date.Equals(DateTime.Now.ToShortDateString()))
        {
            PlayerPrefs.SetString("ExchangeRateFetcher", DateTime.Now.ToShortDateString());
        }
        else
        {
            yield break;
        }

        yield return GetExchangeRate(apiURL);
        yield return GetExchangeRate(apiURL2);
    }

    IEnumerator GetExchangeRate(string url)
    {
        yield return new WaitForSeconds(Random.value * 2);
        
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || 
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {webRequest.error}");
            }
            else
            {
                JObject data = JObject.Parse(webRequest.downloadHandler.text);
                string exchangeRate = (string)data["Realtime Currency Exchange Rate"]["5. Exchange Rate"];

                if (url == apiURL)
                {
                    StockDataManager.Instance.US_Exchange = double.Parse(exchangeRate);
                }
                else
                {
                    StockDataManager.Instance.HK_Exchange = double.Parse(exchangeRate);
                }
            }
        }
    }
}