#炉石礼包价格计算

pricePer = 3.127
wildPer = 0.8
card = 5
hero = 10

dustPer = 112.35

gPkg = 467.2 / dustPer
gLegend = 1600 / dustPer
gEpic = 400 / dustPer
gRare = 100 / dustPer
gNormal = 50 / dustPer
legend = gEpic
epic = gRare
rare = 20 / dustPer
normal = 5 / dustPer
ticket = 1.5

def calculate_hearthstone_value(
        oriPrice = 1,
        num = 0,
        wildNum = 0,
        goldNum = 0,
        wildGoldNum = 0,
        gLegendNum = 0,
        legendNum = 0,
        ticketNum = 0,
        cardNum = 0,
        heroNum = 0,
        gEpicNum = 0,
        epicNum = 0,
        gRareNum = 0,
        rareNum = 0,
        gNormalNum = 0,
        normalNum = 0,
):

    price = (num
             + wildNum * wildPer
             + goldNum * gPkg
             + wildGoldNum * gPkg * wildPer
             + gLegendNum * gLegend
             + legendNum * legend
             + ticketNum * ticket
             + gEpicNum * gEpic
             + epicNum * epic
             + gRareNum * gRare
             + rareNum * rare
             + gNormalNum * gNormal
             + normalNum * normal) * pricePer + cardNum * card + heroNum * hero

    dust = (num
             + wildNum
             + goldNum * gPkg
             + wildGoldNum * gPkg
             + gLegendNum * gLegend
             + legendNum * legend
             + ticketNum * ticket
             + gEpicNum * gEpic
             + epicNum * epic
             + gRareNum * gRare
             + rareNum * rare
             + gNormalNum * gNormal
             + normalNum * normal) * dustPer

    dustPrice = dust / dustPer * pricePer

    print(f"原价：{oriPrice}\n实际价值：{round(price * 100) / 100}\n打折：{round(oriPrice / price * 10000) / 100}%\n")
    print(f"可分解尘：{round(dust * 100) / 100}\n尘价值：{round(dustPrice * 100) / 100}\n打折：{round(oriPrice / dustPrice * 10000) / 100}%\n")

calculate_hearthstone_value(oriPrice=488,num=80,goldNum=10,gLegendNum=2,ticketNum=4)
# calculate_hearthstone_value(oriPrice=60,num=30,legendNum=1)
# calculate_hearthstone_value(oriPrice=198,num=90,legendNum=3)

# calculate_hearthstone_value(oriPrice=60,legendNum=2,epicNum=4,rareNum=10,normalNum=14)
# calculate_hearthstone_value(oriPrice=488,num=100,gLegendNum=2)
calculate_hearthstone_value(oriPrice=888,num=200,goldNum=40,gLegendNum=4)