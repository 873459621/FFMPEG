# 计算需要多少钱

def calculate_yearly_breakdown(title, pp, r, dd, verbose=False):
    per = pp # 一次价格
    ra = r # 生活费系数
    day = dd # 一年多少次

    house = 2_000  # 房租
    water = 400 # 水电气网
    food = 30 * 2.5 * 30 # 吃饭
    traffic = 50 * 30 # 交通费
    other = 4_000 # 杂费

    life = (house + water + food + traffic + other) * ra # 每月生活费
    C = per * day + life * 12  # 首年支出金额
    g = 0.02  # 通货膨胀率
    r = 0.1  # 投资回报率
    n = 35  # 总年数

    # 计算初始本金（根据等比增长年金公式）
    factor = (1 + g) / (1 + r)
    initial_principal = C * (1 - factor ** n) / (r - g)

    # 初始化跟踪变量
    current_principal = initial_principal
    yearly_data = []

    # 逐年模拟本金变化
    for year in range(1, n + 1):
        # 计算当年支出（考虑通胀）
        withdrawal = C * (1 + g) ** (year - 1)
        # 计算投资收益
        investment_return = current_principal * r
        # 更新本金（先获得收益，后支出）
        current_principal = current_principal + investment_return - withdrawal
        # 记录数据
        yearly_data.append({
            "year": year,
            "begin_principal": initial_principal if year == 1 else yearly_data[-1]["end_principal"],
            "withdrawal": withdrawal,
            "investment_return": investment_return,
            "end_principal": current_principal
        })

    # 打印详细过程
    if verbose:
        print(f"{'年份':<5}{'年初本金':<15}{'投资收益':<15}{'当年支出':<15}{'年末本金':<15}")
        for data in yearly_data:
            print(
                f"{data['year']:<5}{data['begin_principal']:>15,.2f}{data['investment_return']:>15,.2f}{data['withdrawal']:>15,.2f}{data['end_principal']:>15,.2f}")

    print(f"\n{title}")
    print(f"每月花费: {per * day / 12:,.2f}元（每次{per}元，{dd / 52:,.1f}次/周，{dd / 12:,.0f}次/月）")
    print(f"生活费: {life:,.2f}元")
    print(f"初始每年花费: {C:,.2f}元")
    print(f"初始本金需求: {initial_principal:,.2f}元")

    return yearly_data


# 执行计算并打印结果
calculate_yearly_breakdown("低端", 400, 1, 360)
calculate_yearly_breakdown("中端", 600, 1.2, 360)
calculate_yearly_breakdown("高端", 800, 1.4, 360)
calculate_yearly_breakdown("超高", 1_000, 1.6, 360)
calculate_yearly_breakdown("特高", 1_500, 2.0, 360)
calculate_yearly_breakdown("终极", 2_000, 2.5, 360)
calculate_yearly_breakdown("无敌", 3_000, 3, 360)