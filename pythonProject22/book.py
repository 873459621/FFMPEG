from operator import truediv
import random

import requests
from bs4 import BeautifulSoup
from urllib.parse import urljoin
import time
import re

# 配置参数
IDS = []

#听雪谱
#########################################################################################################################

# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")
# IDS.append("8888888888888")

BASE_URL = "https://crxs.me/fiction/id-{}/{}.html"  # 替换为实际小说目录页
NAME = ""
OUTPUT_FILE = "C:/proj/txt_files/417/{}.txt"
c_num = 9999

# HEADERS = {
#     "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36"
# }


# def get_chapter_links():
#     """获取所有章节链接"""
#     response = requests.get(BASE_URL, headers=HEADERS)
#     soup = BeautifulSoup(response.text, 'html.parser')
#
#     # 根据实际网站结构修改选择器
#     links = soup.select('.chapter-list a')
#     return [a['href'] for a in links]


# def get_chapter_content2(url):
#     """处理AJAX动态加载的分页内容"""
#     session = requests.Session()
#     full_content = []
#     current_params = {
#         "fictionid": None,
#         "page": None,
#         "t": None,
#         "hash": None
#     }
#
#     # 首次请求获取初始参数
#     response = session.get(url, headers=HEADERS)
#     soup = BeautifulSoup(response.text, 'html.parser')
#     challenge_btn = soup.find('a', {'name': 'challenge'})
#
#     if not challenge_btn:
#         print(f"没有按钮")
#         return get_chapter_content2(url)
#
#     title = soup.find('div', class_='chapter').text.strip()
#
#     # 提取关键参数
#     current_params.update({
#         "fictionid": challenge_btn['fictionid'],
#         "page": challenge_btn['page'],
#         "t": challenge_btn['t'],
#         "hash": challenge_btn['hash']
#     })
#
#     while True:
#         # 构造AJAX请求
#         ajax_url = "https://crxs.me/ajax.php"  # 替换为实际接口地址
#         payload = {
#             "action": "fictionChallenge",
#             "id": current_params["fictionid"],
#             "page": current_params["page"],
#             "t": current_params["t"],
#             "hash": current_params["hash"]
#         }
#
#         response = session.post(ajax_url, data=payload)
#         result = response.json()
#
#         if result['code'] != 0:
#             print(f"下载失败 {result['code']}")
#             break
#
#         # 解析新内容
#         new_content = BeautifulSoup(result['content'], 'html.parser')
#         paragraphs = [p.text.strip() for p in new_content.find_all('p')]
#         full_content.extend(paragraphs)
#
#         # 查找新分页参数
#         new_btn = new_content.find('a', {'name': 'challenge'})
#         if not new_btn:
#             break
#
#         # 更新参数
#         current_params = {
#             "fictionid": new_btn['fictionid'],
#             "page": new_btn['page'],
#             "t": new_btn['t'],
#             "hash": new_btn['hash']
#         }
#
#         time.sleep(1.5)  # 控制请求频率
#
#     content = '\n\n'.join(full_content).replace("（看精彩成人小说上《成人小说网》：https://crxs.me）", "")
#     content = content.replace("（成人APP精选（https://xchina.app），每款都经过站长人工审核）", "")

    # full_content = []
    # current_url = url
    #
    # while True:
    #     # 请求当前页
    #     response = requests.get(current_url, headers=HEADERS)
    #     soup = BeautifulSoup(response.text, 'html.parser')
    #
    #     title = soup.find('div', class_='chapter').text.strip()
    #
    #     # 提取当前页内容（使用之前的分段优化逻辑）
    #     content_div = soup.find('div', class_='content')
    #     paragraphs = [p.text for p in content_div.find_all('p') if p.text.strip()]
    #     full_content.extend(paragraphs)
    #
    #     # 查找下一页按钮（根据目标网站结构调整）
    #     next_page = soup.find('a', text=['下一页', '继续阅读', '>', '请点击这里继续阅读本文'])
    #
    #     # 处理特殊按钮（如JS触发按钮）
    #     # next_page = soup.find('a', onclick=lambda x: x and 'nextpage' in x.lower())
    #
    #     if not next_page or 'disabled' in next_page.get('class', []):
    #         break
    #
    #     # 拼接完整URL（相对路径转绝对路径）
    #     current_url = urljoin(current_url, next_page['href'])
    #
    #     # 防止请求过快
    #     time.sleep(1)
    #
    # content = '\n\n'.join(full_content).replace("（看精彩成人小说上《成人小说网》：https://crxs.me）", "")
    # content = content.replace("（成人APP精选（https://xchina.app），每款都经过站长人工审核）", "")

    # """获取单章内容"""
    # response = requests.get(url, headers=HEADERS)
    # soup = BeautifulSoup(response.text, 'html.parser')
    #
    # # 提取章节标题
    # # title = soup.find('h1').text.strip()
    # title = soup.find('div', class_='chapter').text.strip()
    #
    # # 提取正文内容（根据实际网站结构调整选择器）
    # content_div = soup.find('div', class_='content')
    #
    # paragraphs = [p.text for p in content_div.find_all('p') if p.text.strip()]
    #
    # content = '\n\n'.join([p.strip() for p in paragraphs if p.strip()])
    #
    # # 清理多余空白
    # content = re.sub(r'\s+', '\n', content).strip()
    #
    # content = content.replace("（看精彩成人小说上《成人小说网》：https://crxs.me）", "")
    # content = content.replace("（成人APP精选（https://xchina.app），每款都经过站长人工审核）", "")

    # return title, content


# 免费代理抓取示例
# def get_free_proxies():
#     proxies = []
#     url = "https://www.free-proxy-list.net/"
#     response = requests.get(url)
#     soup = BeautifulSoup(response.text, 'html.parser')
#
#     for row in soup.select('table#proxylisttable tbody tr'):
#         cells = row.find_all('td')
#         if cells[4].text == 'elite proxy' and cells[6].text == 'yes':
#             proxies.append(f"{cells[0].text}:{cells[1].text}")
#
#     return proxies
#
# def validate_proxy(proxy):
#     try:
#         test_url = "https://httpbin.org/ip"
#         response = requests.get(test_url, proxies={'http': proxy}, timeout=10)
#         if response.status_code == 200:
#             print(f"有效代理: {proxy} 返回IP: {response.json()['origin']}")
#             return True
#     except Exception as e:
#         print(f"无效代理: {proxy} 错误: {str(e)}")
#     return False

def get_chapter_content(url):
    """获取单章内容"""
    # response = requests.get(url, headers=HEADERS)
    # soup = BeautifulSoup(response.text, 'html.parser')
    #
    # challenge_btn = soup.find('a', {'name': 'challenge'})
    #
    # if challenge_btn:
    #     print(f"有按钮")

    start_time = 0

    while True:
        headers = {
            "User-Agent": random.choice([
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15",
                "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36"
            ])
        }

        response = requests.get(url, headers=headers)
        soup = BeautifulSoup(response.text, 'html.parser')

        challenge_btn = soup.find('a', {'name': 'challenge'})

        if challenge_btn:
            if start_time == 0:
                start_time = time.time()
                n = random.randint(80, 84)
            else:
                n = random.randint(2, 4)
            print(f"有按钮，等{n}秒重试")
            time.sleep(n)
        else:
            if start_time != 0:
                print(f"重试等待时间：{time.time() - start_time:.2f}秒")
            break

    global NAME
    NAME = soup.find('div', class_='title').text.strip()

    # 提取章节标题
    title = soup.find('div', class_='chapter').text.strip()

    # 提取正文内容（根据实际网站结构调整选择器）
    content_div = soup.find('div', class_='content')

    paragraphs = [p.text for p in content_div.find_all('p') if p.text.strip()]

    content = '\n\n'.join([p.strip() for p in paragraphs if p.strip()])

    # 清理多余空白
    content = re.sub(r'\s+', '\n', content).strip()

    content = content.replace("（看精彩成人小说上《成人小说网》：https://crxs.me）", "")
    content = content.replace("（成人APP精选（https://xchina.app），每款都经过站长人工审核）", "")

    return title, content


def save_novel(id):
    # chapters = get_chapter_links()
    # with open(OUTPUT_FILE, 'w', encoding='utf-8') as f:

    f = 1
    start_time = time.time()

    for idx in range(1, c_num + 1):
        try:
            # 添加延迟防止被封（建议3-10秒）
            time.sleep(1)

            title, content = get_chapter_content(BASE_URL.format(id, idx))

            if f == 1:
                f = open(OUTPUT_FILE.format(NAME), 'w', encoding='utf-8')
                print(f"《{NAME}》开始下载...")

            f.write(f"第{idx}章 {title}\n\n")
            f.write(content + "\n\n")
            print(f"已下载: 第{idx}章 {title}")

        except Exception as e:
            print(f"下载失败 {BASE_URL.format(id, idx)}，错误：{str(e)}")
            break

    print(f"《{NAME}》下载完成！\n总耗时：{time.time() - start_time:.2f}秒\n")


if __name__ == "__main__":
    for ii in IDS:
        save_novel(ii)
