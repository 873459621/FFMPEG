# ngrok http http://localhost:8989

from flask import Flask, request

app = Flask(__name__)

@app.route('/post_endpoint', methods=['POST'])
def save_string():
    data = None
    if request.content_type == 'application/x-www-form-urlencoded':
        data = request.form
    if data:
        print(f"Received data: {data}")
    else:
        return "No data", 200
    if 'key' in data and data['key'] == 'hhw_1995':
        if "magnet" in data['msg']:
            file = open('C:\\proj\\FFMPEG\\Assets\\Resources\\msg.txt', mode='a', encoding='utf-8')
            file.writelines(data['msg'])
            file.write('\n')
            file.close()
            return "Success", 200
        else:
            return "No magnet", 200
    else:
        return "Key error", 200

if __name__ == '__main__':
    app.run(port=8989)