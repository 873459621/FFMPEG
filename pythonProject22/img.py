import matplotlib.pyplot as plt
import numpy as np

# 设置中文字体，防止乱码
plt.rcParams['font.sans-serif'] = ['SimHei']  # 使用黑体
plt.rcParams['axes.unicode_minus'] = False    # 解决负号显示问题

# 数据准备
years = np.arange(1985, 2025) # 从1985年到2024年
birth_population = [
    2227, 2411, 2550, 2307, 2432, # 1985-1989
    2391, 2258, 2119, 2126, 2104, # 1990-1994
    2063, 2067, 2038, 1991, 1909, # 1995-1999
    1771, 1702, 1647, 1599, 1593, # 2000-2004
    1617, 1584, 1594, 1608, 1615, # 2005-2009
    1588, 1604, 1635, 1640, 1687, # 2010-2014
    1655, 1883, 1765, 1523, 1465, # 2015-2019
    1200, 1062, 956, 902, 954      # 2020-2024
]

# 创建图表和坐标轴
fig, ax = plt.subplots(figsize=(15, 8))

# 绘制柱状图
bars = ax.bar(years, birth_population, color='skyblue', edgecolor='black', alpha=0.7)

# 设置标题和标签
ax.set_title('中国近40年出生人口变化 (1985-2024)', fontsize=16, fontweight='bold')
ax.set_xlabel('年份', fontsize=12)
ax.set_ylabel('出生人口 (万人)', fontsize=12)

# 调整X轴刻度，每5年显示一次
ax.set_xticks(years[::1])
ax.set_xticklabels(years[::1], rotation=45)

# 在柱子上方显示数值
for bar, value in zip(bars, birth_population):
    height = bar.get_height()
    ax.text(bar.get_x() + bar.get_width()/2., height + 10,
            f'{value}', ha='center', va='bottom', fontsize=9)

# 添加网格线以便更好地读取数值
ax.grid(axis='y', linestyle='--', alpha=0.7)

# 优化布局
plt.tight_layout()

# 显示图表
plt.show()