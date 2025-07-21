import os
from collections import defaultdict

import chardet
import re

def remove_after_space(s, p):
    index = s.find(p)
    return s[:index] if index != -1 else s

def process_string(s):
    s_no_space = remove_after_space(s, ' ')
    s_no_space = remove_after_space(s_no_space, '(')
    s_no_space = remove_after_space(s_no_space, '（')
    s_no_space = remove_after_space(s_no_space, ':')
    s_no_space = remove_after_space(s_no_space, '：')
    s_no_space = remove_after_space(s_no_space, '—')
    s_no_space = s_no_space.rstrip('之')
    return s_no_space

def chinese_to_arabic(chinese_num):
    chinese_num_map = {
        '零': 0,
        '一': 1,
        '二': 2,
        '三': 3,
        '四': 4,
        '五': 5,
        '六': 6,
        '七': 7,
        '八': 8,
        '九': 9,
        '十': 10,
        '百': 100,
        '千': 1000,
        '万': 10000,
        '亿': 100000000,
    }
    if not chinese_num:
        return 0
    total = 0
    current = 0
    prev_value = 0
    for c in chinese_num:
        value = chinese_num_map.get(c, 0)
        total += value
        # if value >= 10:
        #     if current == 0:
        #         current = 1
        #     prev_value += current
        #     total += prev_value * value
        #     prev_value = 0
        #     current = 0
        # else:
        #     current = value
        #     prev_value += current
    # total += prev_value + current
    return total

pattern = re.compile(
    r'(\d+)|([上下中])|([零一二三四五六七八九十百千万亿]+)|(.)',
    re.UNICODE
)

def get_sort_key(filename):
    key_parts = []
    for match in pattern.finditer(filename):
        arabic_num, upmidlow, chinese_num, other_char = match.groups()
        if arabic_num is not None:
            num = int(arabic_num)
            key_part = f"{num:08d}"
        elif upmidlow is not None:
            trans = {'上': 1, '中': 2, '下': 3}.get(upmidlow, 0)
            key_part = f"{trans:08d}"
        elif chinese_num is not None:
            num = chinese_to_arabic(chinese_num)
            key_part = f"{num:08d}"
        else:
            key_part = other_char
        key_parts.append(key_part)
    return ''.join(key_parts)

def sort_filenames(filenames):
    return sorted(filenames, key=get_sort_key)


def longest_common_prefix(a, b):
    prefix = []
    for char_a, char_b in zip(a, b):
        if char_a == char_b:
            prefix.append(char_a)
        else:
            break
    return ''.join(prefix)

def safe_read_file(file_path):
    """安全读取文件内容的加强版"""
    try:
        # 二次编码检测机制
        with open(file_path, 'rb') as f:
            raw_data = f.read()

        # 第一次检测（快速检测）
        encoding = chardet.detect(raw_data[:1000])['encoding']
        try:
            return raw_data.decode(encoding or 'utf-8')
        except UnicodeDecodeError:
            # 第二次检测（完整检测）
            encoding = chardet.detect(raw_data)['encoding']
            return raw_data.decode(encoding or 'utf-8', errors='replace')

    except Exception as e:
        print(f"无法读取文件 {os.path.basename(file_path)}: {str(e)}")
        return f"【文件内容损坏或无法解码】\n"


def merge_files_with_encoding_fix(src_dir, output_dir='merged'):
    """带编码修正的合并函数"""
    # 创建输出目录
    os.makedirs(output_dir, exist_ok=True)

    # 收集所有txt文件并按智能分组
    file_groups = defaultdict(list)
    all_files = [f for f in os.listdir(src_dir) if f.endswith('.txt')]

    # 智能分组算法（改进版）
    for filename in sorted(all_files):
        if '34gc' in filename:
            continue

        # 提取有效前缀（过滤单字符）
        base_name = os.path.splitext(filename)[0]
        clean_name = re.sub(r'[_\-—\d]', '', base_name)
        if len(clean_name) < 2:
            continue

        # 寻找最佳匹配组
        matched = False
        for prefix in list(file_groups.keys()):
            if filename.startswith(prefix):
                file_groups[prefix].append(filename)
                matched = True
                break
            elif prefix.startswith(clean_name[:2]):
                file_groups[clean_name[:2]].append(filename)
                matched = True
                break
        if not matched:
            file_groups[clean_name[:2]].append(filename)

    # 合并处理
    merge_report = []
    for prefix, files in file_groups.items():
        if len(files) < 2:
            continue

        # 生成合并文件名
        files = sort_filenames(files)
        pp = longest_common_prefix(files[0], files[1])
        pp = process_string(pp)

        merged_name = f"{pp}——合集.txt"
        merged_path = os.path.join(output_dir, merged_name)

        # 记录合并信息
        merge_report.append(f"合并组：{pp}")

        nf = []

        for f in files:
            if pp in f:
                nf.append(f)
            else:
                merge_report.append(f"过滤：---------------------------------------------《{f}》")

        files = nf

        merge_report.extend([f"  ├ {f}" for f in files])

        # 写入内容（统一使用UTF-8 with BOM）
        with open(merged_path, 'w', encoding='utf-8-sig') as outfile:
            outfile.write("\ufeff")  # 添加BOM头
            for filename in files:
                file_path = os.path.join(src_dir, filename)
                content = safe_read_file(file_path)
                outfile.write(f"\n▼▼▼ {filename} ▼▼▼\n")
                outfile.write(content)
                outfile.write("\n▲▲▲\n")

    # 打印报告
    if merge_report:
        print("成功合并组：")
        print("\n".join(merge_report))
    else:
        print("没有需要合并的文件组")

#堕落天使咒

# 使用示例
if __name__ == '__main__':
    folder = 'C:/proj/txt_files/413'
    merge_files_with_encoding_fix(folder, folder + '_m')
