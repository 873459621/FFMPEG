import numpy as np
import matplotlib.pyplot as plt
from typing import List


def generate_random_prices(
        days: int = 30,
        initial_price: float = 100.0,
        daily_volatility: float = 0.03,
        drift: float = 0.0
) -> List[float]:
    """
    生成随机价格序列（几何布朗运动模型）
    :param days: 模拟天数
    :param initial_price: 初始价格
    :param daily_volatility: 日波动率（例如0.03表示3%）
    :param drift: 日漂移率（长期趋势）
    :return: 生成的价格序列
    """
    # 生成随机收益率（对数收益率服从正态分布）
    log_returns = np.random.normal(
        loc=drift - 0.5 * daily_volatility ** 2,
        scale=daily_volatility,
        size=days - 1
    )

    # 计算价格序列
    prices = [initial_price]
    for r in log_returns:
        prices.append(prices[-1] * np.exp(r))
    return prices


def simulate_3x_short_etf(prices):
    """
    模拟3倍做空ETF的净值走势
    :param prices: 标的指数的每日价格列表（按时间顺序排列）
    :return: (benchmark_net, etf_net) 分别为标的和ETF的净值列表
    """
    # 计算标的指数的净值（标准化到初始1）
    initial_price = prices[0]
    benchmark_net = [p / initial_price for p in prices]

    # 计算3倍做空ETF的净值
    etf_net = [1.0]  # 初始净值为1
    for i in range(1, len(prices)):
        daily_return = (prices[i] - prices[i - 1]) / prices[i - 1]
        etf_daily_return = -3 * daily_return
        etf_net.append(etf_net[i - 1] * (1 + etf_daily_return))

    return benchmark_net, etf_net


# 参数设置
# np.random.seed(42)  # 固定随机种子确保结果可复现
sim_days = 30  # 模拟30天
volatility = 0.01  # 设置4%的日波动率
drift = -0.01  # 标的指数每天有0.1%的上涨趋势

# 生成随机价格
prices = generate_random_prices(
    days=sim_days,
    daily_volatility=volatility,
    drift=drift
)

# 运行模拟
benchmark, etf = simulate_3x_short_etf(prices)

# 增强型结果展示
plt.figure(figsize=(12, 6))
ax1 = plt.subplot()
ax2 = ax1.twinx()  # 双坐标轴

# 绘制价格序列（左轴）
ax1.plot(prices, color='blue', label='Index Price', marker='o')
ax1.set_xlabel('Trading Days')
ax1.set_ylabel('Price', color='blue')

# 绘制净值走势（右轴）
ax2.plot(benchmark, color='green', linestyle='--', label='Benchmark Net Value')
ax2.plot(etf, color='red', linestyle='-.', label='3x Short ETF Net Value')
ax2.set_ylabel('Net Value', color='red')

# 合并图例
lines, labels = ax1.get_legend_handles_labels()
lines2, labels2 = ax2.get_legend_handles_labels()
ax1.legend(lines + lines2, labels + labels2, loc='upper left')

plt.title(f'3x Short ETF Simulation ({sim_days} Days)\n'
          f'Volatility: {volatility * 100:.1f}% | Drift: {drift * 100:.1f}%')
plt.grid(True)
plt.show()

# 关键指标统计
final_return = (prices[-1] / prices[0] - 1) * 100
etf_final_return = (etf[-1] / etf[0] - 1) * 100

print(f"标的指数 {final_return:+.1f}%")
print(f"3倍做空ETF {etf_final_return:+.1f}%")
print(f"最大回撤: {min(etf) / max(etf) * 100 - 100:.1f}%")