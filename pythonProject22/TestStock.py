import pandas as pd
import requests
import re


def get_hk_realtime_quote(stock_code):
    # Convert stock code to Sina's format (e.g., '00882.HK' -> 'hk00882')
    symbol = f"hk{stock_code.split('.')[0]}"
    url = f"http://hq.sinajs.cn/list={symbol}"
    headers = {'Referer': 'http://finance.sina.com.cn'}  # Required for access
    response = requests.get(url, headers=headers)
    response.encoding = 'gbk'  # Decode with GBK

    # Extract the data portion from the response
    data_str = re.search(r'"(.*?)"', response.text).group(1)
    data_list = data_str.split(',')

    # Use columns directly from Tushare or define custom ones
    columns = [
                  'name', 'open', 'pre_close', 'price', 'high', 'low',
                  'bid', 'ask', 'volume', 'amount', '_', '_', '_',
                  '_', '_', '_', '_', '_', 'date', 'time'
              ][:len(data_list)]  # Only take relevant columns

    return pd.DataFrame([data_list], columns=columns)


# Example usage
df = get_hk_realtime_quote('00882.HK')
print(df)