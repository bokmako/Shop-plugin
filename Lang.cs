using System.Collections.Generic;

namespace ShopPlugin
{
    class Lang
    {
        private static string _buyCommand = "buy";
        private static string _buyItemPermission = "shop.buy";
        private static string _sellCommand = "sell";
        private static string _sellItemPermission = "shop.sell";
        private static string _shopCommand = "shop";
        private static string _shopPermission = "shop.shop";    
        private static string _searchCommand = "search";
        private static string _searchPermission = "shop.search";
        private static string _shopRegionName = "Shop";

        private static Dictionary<string, string> _message = new Dictionary<string, string>
        {
            {"Load", "[Shop] Loaded {0} items in shop"},
            {"NoPerms", "[Shop] Вы не можете использовать эту команду."},
            {"BuyInfo", "[Shop] /{0} {1} (Id предмета) [Кол-во]"},
            {"SellInfo", "[Shop] /{0} {1}"},
            {"SearchInfo", "[Shop] /{0} {1} (Id предмета)"},
            {"NoSlot", "[Shop] В вашем рюкзаке нет места!" },
            {"TooMany", "[Shop] У нас есть только {0} {1}"},
            {"OnBuy", "[Shop] Вы купили {0} {1} за {2}"},
            {"OnSell", "[Shop] Вы продали {0} {1} за {2}"},
            {"EmptyHand", "[Shop] Возьмите предмет в руку, затем напишите /{0} {1}"},
            {"WrongSellItem", "[Shop] Мы не покупаем данный предмет" },
            {"Error", "[Shop] Что-то пошло не так!" },
            {"NoMatch", "[Shop] У нас нет такого в ассортименте!" },
            {"SearchItem", "[Shop] {0} | Цена продажи: {1} | Цена покупки: {2} | Кол-во на складе: {3}" },
            {"NotInShop", "[Shop] Чтобы пользоваться магазином вы должны находится на территории прилавка." },
            {"ShopCommands", "[Shop] Комманды магазина:" },
            {"NoMoney", "У вас недостаточно виты! Нужно {0}"}
        };

        public static Config DefaultConfig()
        {
            Config vConf = new Config
            {
                BuyCommand = _buyCommand,
                BuyItemPermission = _buyItemPermission,
                SellCommand = _sellCommand,
                SellItemPermission = _sellItemPermission,
                ShopCommand = _shopCommand,
                ShopPermission = _shopPermission,
                SearchCommand = _searchCommand,
                SearchPermission = _searchPermission,
                ShopRegionName = _shopRegionName,
                Tip = "You can find Id and Names off all terraria items there: https://terraria.fandom.com/wiki/Item_IDs",
                Messages = _message,

                //for example
                Items = new List<ItemInfo>()
                {
                    new ItemInfo
                    {
                        Id = 19,
                        Name = Terraria.Lang.GetItemNameValue(19),
                        Infinity = false,
                        Amount = 500,
                        SelPrice = "1000",
                        BuyPrice = "2000"
                    },
                    new ItemInfo
                    {
                        Id = 20,
                        Name = Terraria.Lang.GetItemNameValue(20),
                        Infinity = false,
                        Amount = 500,
                        SelPrice = "500",
                        BuyPrice = "1000"
                    },
                    new ItemInfo
                    {
                        Id = 21,
                        Name = Terraria.Lang.GetItemNameValue(21),
                        Infinity = false,
                        Amount = 500,
                        SelPrice = "750",
                        BuyPrice = "1500"
                    }
                }
            };
            return vConf;
        }
    }
}
