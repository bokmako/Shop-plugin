using System.Collections.Generic;

namespace ShopPlugin
{
    public class ItemInfo
    {
        public int Id = 0;
        public string Name = "";
        public bool Infinity = false;
        public int Amount = 0;
        public byte modifierId = 0;

        /// <summary>Цена купли у игрока</summary>
        public string SelPrice = "0";

        /// <summary>Цена продажи игроку</summary>
        public string BuyPrice = "0";
    }
    public class Config
    {
        public string BuyCommand = "";
        public string ListShopItemsCommand = "";
        public string BuyItemPermission = "";
        public string SellCommand = "";
        public string SellItemPermission = "";
        public string ShopCommand = "";
        public string ShopPermission = "";
        public string SearchCommand = "";
        public string SearchPermission = "";
        public string ShopRegionName = "";
        public string Tip = "";

        public List<ItemInfo> Items = new List<ItemInfo>();

        public Dictionary<string, string> Messages = new Dictionary<string, string>();
    }

}
