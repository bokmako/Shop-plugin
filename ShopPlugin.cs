using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using Wolfje.Plugins.SEconomy;

namespace ShopPlugin
{
    [ApiVersion(2, 1)]
    public class ShopPlugin : TerrariaPlugin
    {
        public List<ItemInfo> SellItems = new List<ItemInfo>();
        internal static string filepath { get { return Path.Combine(TShock.SavePath, "shop.json"); } }
        public ShopPlugin(Main game) : base(game)
        {
        }
        private Config config;
        public override string Author => "Bokmako";
        public override string Name => "ShopPlugin";
        public override Version Version => new Version(0, 0, 0, 1);
        public override string Description => "A shop plugin.";

        public override void Initialize()
        {
            ReadConfig(filepath, Lang.DefaultConfig(), out config);
            int Lot = 0;
            foreach (var item in config.Items)
            {
                ItemInfo vItem = new ItemInfo()
                {
                    Id = item.Id,
                    Name = Terraria.Lang.GetItemNameValue(item.Id),
                    Infinity = item.Infinity,
                    Amount = item.Amount,
                    SelPrice = item.SelPrice,
                    BuyPrice = item.BuyPrice
                };
                SellItems.Add(vItem);
                Lot++;
            }
            Console.WriteLine(config.Messages["Load"], Lot);
            Commands.ChatCommands.Add(new Command(config.BuyItemPermission, Shop,
                !config.ShopCommand.Contains(" ")
                ? config.ShopCommand
                : throw new ArgumentException("Command contains space")));
        }
        private static void ReadConfig<TConfig>(string path, TConfig defaultConfig, out TConfig config)
        {
            if (!File.Exists(path))
            {
                config = defaultConfig;
                File.WriteAllText(path, JsonConvert.SerializeObject(config, Formatting.Indented));
            }
            else
            {
                config = JsonConvert.DeserializeObject<TConfig>(File.ReadAllText(path));
            }
        }
        private bool IsInShop(CommandArgs args)
        {
            var rg = TShock.Regions.InAreaRegionName(args.Player.TileX, args.Player.TileY);
            if(rg == null || rg.Count() == 0)
                return false;
            foreach(var reg in rg)
            {
                if (reg == "Shop")
                    return true;
            }
            return false;
        }
        private void Shop(CommandArgs args)
        {
            if (!IsInShop(args))
            {
                args.Player.SendInfoMessage(config.Messages["NotInShop"]);
                return;
            }
            if (!args.Player.HasPermission(config.ShopPermission))
            {
                args.Player.SendInfoMessage(config.Messages["NoPerms"]);
                return;
            }
            if (args.Parameters.Count == 0)
            {
                args.Player.SendInfoMessage(config.Messages["ShopCommands"]);
                args.Player.SendInfoMessage(config.Messages["BuyInfo"], config.ShopCommand, config.BuyCommand);
                args.Player.SendInfoMessage(config.Messages["SellInfo"], config.ShopCommand, config.SellCommand);
                args.Player.SendInfoMessage(config.Messages["SearchInfo"], config.ShopCommand, config.SearchCommand);
                return;
            }
            string cmd = args.Parameters[0].ToLower();
            #region Buy scenario
            if (cmd == config.BuyCommand)
            {
                if (args.Player.HasPermission(config.BuyItemPermission))
                {
                    if (args.Parameters.Count != 3)
                    {
                        args.Player.SendInfoMessage(config.Messages["BuyInfo"], config.ShopCommand, config.BuyCommand);
                    }
                    if (args.Parameters.Count == 3 || args.Parameters.Count == 2)
                    {
                        int id;
                        int amount = 1;
                        if (!int.TryParse(args.Parameters[1], out id))
                        {
                            args.Player.SendErrorMessage("Error: Invalid Id entered!");
                            return;
                        }
                        if (args.Parameters.Count == 3)
                        {
                            if (!int.TryParse(args.Parameters[2], out amount))
                            {
                                args.Player.SendErrorMessage("Error: Invalid amount entered!");
                                return;
                            }
                        }
                        int ItemNumber = 0;
                        ItemInfo Item = null;
                        ReadConfig(filepath, Lang.DefaultConfig(), out config);
                        for (int i = 0; i <= config.Items.Count - 1; i++)
                        {
                            if (config.Items[i].Id == id)
                            {
                                Item = config.Items[i];
                                ItemNumber = i;
                                break;
                            }
                        }
                        if (Item == null)
                        {
                            args.Player.SendErrorMessage(config.Messages["NoMatch"]);
                            return;
                        }
                        if (amount > Item.Amount && Item.Infinity != true)
                        {
                            args.Player.SendInfoMessage(config.Messages["TooMany"], Item.Amount, Terraria.Lang.GetItemNameValue(Item.Id));
                            return;
                        }
                        if (!args.Player.InventorySlotAvailable)
                        {
                            args.Player.SendInfoMessage(config.Messages["NoSlot"]);
                        }
                        if (args.Player.InventorySlotAvailable)
                        {
                            TryToBuy(args.Player, Item, amount, ItemNumber);
                        }
                    }
                }
            }
            #endregion
            #region Sell scenario
            if (cmd == config.SellCommand)
            {
                if (!args.Player.HasPermission(config.SellItemPermission))
                {
                    args.Player.SendInfoMessage(config.Messages["NoPerms"]);
                    return;
                }

                TryToSell(args);
            }
            #endregion
            #region Search scenario
            if (cmd == config.SearchCommand)
            {
                if (!args.Player.HasPermission(config.SearchPermission))
                {
                    args.Player.SendInfoMessage(config.Messages["NoPerms"]);
                    return;
                }
                if (args.Parameters.Count > 2)
                {
                    args.Player.SendInfoMessage(config.Messages["SearchInfo"], config.ShopCommand, config.SearchCommand);
                    return;
                }

                ItemInfo vItem = new ItemInfo();
                bool vFound = false;
                int IdItem = 0;
                if (args.Parameters.Count == 1)
                {
                    if (args.Player.SelectedItem.netID == 0)
                    {
                        args.Player.SendInfoMessage(config.Messages["SearchInfo"], config.ShopCommand, config.SearchCommand);
                        return;
                    }
                    if (args.Player.SelectedItem.netID != 0)
                    {
                        IdItem = args.Player.SelectedItem.netID;
                    }
                }
                if (args.Parameters.Count == 2)
                {
                    if (!int.TryParse(args.Parameters[1], out int _idItem))
                    {
                        args.Player.SendInfoMessage(config.Messages["SearchInfo"], config.ShopCommand, config.SearchCommand);
                        return;
                    }
                    IdItem = _idItem;
                }
                ReadConfig(filepath, Lang.DefaultConfig(), out config);
                for (int i = 0; i < config.Items.Count; i++)
                {
                    if (IdItem == config.Items[i].Id)
                    {
                        vFound = true;
                        vItem = config.Items[i];
                        break;
                    }
                }
                if (!vFound)
                {
                    args.Player.SendInfoMessage(config.Messages["NoMatch"]);
                    return;
                }
                Money.TryParse(vItem.SelPrice, out var _sellPrice);
                Money.TryParse(vItem.BuyPrice, out var _buyPrice);
                args.Player.SendInfoMessage(config.Messages["SearchItem"], Terraria.Lang.GetItemNameValue(vItem.Id), _sellPrice.ToString(), _buyPrice.ToString(), vItem.Amount);

            }

        }
        #endregion
        private bool TryToBuy(TSPlayer pPlayer, ItemInfo pItem, int pStack, int pNumber)
        {
            var Bank = SEconomyPlugin.Instance.GetBankAccount(pPlayer);
            if(Int32.TryParse(pItem.BuyPrice, out var x) && x < 0)
            {
                pPlayer.SendInfoMessage("[Shop] Мы не продаем {0}. Только покупаем!", pItem.Name);
                return false;
            }
            int _itemCost = Convert.ToInt32(pItem.BuyPrice) * pStack;
            if (Bank == null || Bank.IsAccountEnabled == false || !Money.TryParse(Convert.ToString(_itemCost), out var itemCost))
            {
                return false;
            }
            if (Bank.Balance < itemCost)
            {
                pPlayer.SendInfoMessage(config.Messages["NoMoney"], _itemCost);
                return false;
            }
            if(!pItem.Infinity)
                ShopManager(pStack, pNumber, true);
            Bank.TransferTo(SEconomyPlugin.Instance.WorldAccount, itemCost, Wolfje.Plugins.SEconomy.Journal.BankAccountTransferOptions.AnnounceToSender | Wolfje.Plugins.SEconomy.Journal.BankAccountTransferOptions.IsPayment,
                                        "Purchase", string.Format("For buying {0} {1}", pStack, Terraria.Lang.GetItemNameValue(pItem.Id)));
            OnBuySell(pPlayer, pStack, Terraria.Lang.GetItemNameValue(pItem.Id), itemCost.ToString(), true);
            TShock.Log.Write(string.Format("{0} купил(а) {1} за {2}", pPlayer.Name, pItem.Name, _itemCost.ToString()), System.Diagnostics.TraceLevel.Info);
            pPlayer.GiveItem(pItem.Id, pStack);
            return true;
        }
        private bool TryToSell(CommandArgs args)
        {
            var Bank = SEconomyPlugin.Instance.GetBankAccount(args.Player);
            ItemInfo vItem = null;
            bool vFound = false;

            if (args.Player.SelectedItem.netID == 0)
            {
                args.Player.SendInfoMessage(string.Format(config.Messages["EmptyHand"], config.ShopCommand, config.SellCommand));
                return false;
            }
            int Number = 0;
            ReadConfig(filepath, Lang.DefaultConfig(), out config);
            for (int i = 0; i <= config.Items.Count - 1; i++)
                if (config.Items[i].Id == args.Player.SelectedItem.netID)
                {
                    vItem = config.Items[i];
                    vFound = true;
                    Number = i;
                    break;
                }
            if (!vFound)
            {
                args.Player.SendInfoMessage(string.Format(config.Messages["WrongSellItem"], config.ShopCommand, config.SellCommand));
                return false;
            }
            if (Int32.TryParse(vItem.SelPrice, out var x) && x < 0)
            {
                args.Player.SendInfoMessage("[Shop] Мы не покупаем {0}. Только продаем!", vItem.Name);
                return false;
            }

            int amount = args.Player.SelectedItem.stack;

            int _totalCost = amount * Convert.ToInt32(vItem.SelPrice);

            if (Bank == null || Bank.IsAccountEnabled == false || !Money.TryParse(Convert.ToString(_totalCost), out var itemCost))
            {
                args.Player.SendErrorMessage(config.Messages["Error"]);
                return false;
            }
            int InvItem = Array.IndexOf(args.Player.TPlayer.inventory, args.Player.SelectedItem);
            args.Player.TPlayer.inventory[InvItem].SetDefaults(0);
            NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, args.Player.Index, InvItem);
            ShopManager(amount, Number, false);
            SEconomyPlugin.Instance.WorldAccount.TransferTo(Bank, itemCost, Wolfje.Plugins.SEconomy.Journal.BankAccountTransferOptions.AnnounceToReceiver,
                                                            "Sell item", string.Format("[Shop] Sold {0} {1} for {2} money",
                                                                                        amount, Terraria.Lang.GetItemNameValue(vItem.Id), _totalCost));
            OnBuySell(args.Player, amount, Terraria.Lang.GetItemNameValue(vItem.Id), itemCost.ToString(), false);
            TShock.Log.Write(string.Format("{0} продал(а) {1} за {2}", args.Player.Name, vItem.Name, _totalCost.ToString()), System.Diagnostics.TraceLevel.Info);
            return true;
        }
        public void ShopManager(int pStack, int pNumber, bool isBuy)
        {
            if (isBuy)
                config.Items[pNumber].Amount -= pStack;
            else
                config.Items[pNumber].Amount += pStack;
            File.WriteAllText(filepath, JsonConvert.SerializeObject(config, Formatting.Indented));
        }
        private void OnBuySell(TSPlayer pPlayer, int pAmount, string pName, string TotalCost, bool isBuy)
        {
            if (isBuy)
                pPlayer.SendInfoMessage(config.Messages["OnBuy"], pAmount, pName, TotalCost);
            else
                pPlayer.SendInfoMessage(config.Messages["OnSell"], pAmount, pName, TotalCost);


        }
    }
}


