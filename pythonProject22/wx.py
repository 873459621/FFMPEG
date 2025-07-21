# 导入
from wxauto import WeChat
import time
import subprocess

# 获取微信窗口对象
wx = WeChat()
# 输出 > 初始化成功，获取到已登录窗口：xxxx

# 设置监听列表
listen_list = [
    '文件传输助手',
]
# 循环添加监听对象
for i in listen_list:
    wx.AddListenChat(who=i, savepic=False)

# 持续监听消息，并且收到消息后回复“收到”
wait = 3  # 设置1秒查看一次是否有新消息
while True:
    msgs = wx.GetListenMessage()
    file = open('C:\\msg.txt', mode='a', encoding='utf-8')
    for chat in msgs:
        who = chat.who              # 获取聊天窗口名（人或群名）
        one_msgs = msgs.get(chat)   # 获取消息内容
        # 回复收到
        for msg in one_msgs:
            msgtype = msg.type       # 获取消息类型
            content = msg.content    # 获取消息内容，字符串类型的消息内容
            print(f'【{who}】：{content}')
            if "magnet" in content:
                # subprocess.call(["C:\\Program Files\\qBittorrent\\qbittorrent.exe", "", content])
                file.writelines(content)
                file.write('\n')
    file.close()
    time.sleep(wait)