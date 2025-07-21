import tushare as ts

ts.set_token('e6b85cc9abb9c59e815c54ef92be54bc6671ff55d60af0c2820864e2')

from flask import Flask, request, jsonify
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

    return data_str.split(',')

app = Flask(__name__)

@app.route('/unity', methods=['POST'])
def handle_unity():
    data = request.json
    print(f"Unity发送: {data}")

    if data['code'].split('.')[1] == 'HK':
        df = get_hk_realtime_quote(data['code'])
        result = f"{df[1]},{df[6]}"
    else:
        df = ts.realtime_quote(ts_code=data['code'])
        result = f"{df['NAME'].iloc[0]},{df['PRICE'].iloc[0]}"

    print(f"返回数据: {result}")
    return jsonify({"response": result})

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)