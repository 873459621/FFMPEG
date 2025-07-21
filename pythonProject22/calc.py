# 计算每年可以花多少
# 参数设置
initial = 5_320_000  # 初始本金(元)
r = 0.05  # 年投资回报率
g = 0.02  # 通货膨胀率
spend = 250_000  # 首年实际支出(元)
years = 35  # 计算年限

# 初始化参数
remaining = initial
print(f"{'年份':<5}{'年初本金':<15}{'投资收益':<15}{'当年支出':<15}{'年末剩余':<15}")
print("-" * 60)

for year in range(1, years + 1):
    # 计算通胀调整后的当年支出
    actual_spend = spend * (1 + g) ** (year - 1)

    # 计算投资收益
    interest = remaining * r

    # 计算年末剩余金额
    remaining = remaining * (1 + r) - actual_spend

    # 输出当年结果(四舍五入到整数)
    print(f"{year:<5}{(remaining + actual_spend) / (1 + r):>15,.2f}{interest:>15,.2f}"
          f"{actual_spend:>15,.2f}{remaining:>15,.2f}")

# 最终结果输出
print("\n30年后的精确计算结果：")
print(f"剩余金额: {remaining:,.2f} 元")
print(f"等效公式验证: {initial * (1 + r) ** years - spend * ((1 + r) ** years - (1 + g) ** years) / (r - g):,.2f} 元")