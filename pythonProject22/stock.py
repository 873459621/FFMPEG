import tushare as ts

ts.set_token('e6b85cc9abb9c59e815c54ef92be54bc6671ff55d60af0c2820864e2')

from flask import Flask, request, jsonify

app = Flask(__name__)

@app.route('/unity', methods=['POST'])
def handle_unity():
    data = request.json
    print(f"Unity发送: {data}")

    df = ts.realtime_quote(ts_code=data['code'])
    result = f"{df['NAME'].iloc[0]},{df['PRICE'].iloc[0]}"

    print(f"返回数据: {result}")
    return jsonify({"response": result})

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)