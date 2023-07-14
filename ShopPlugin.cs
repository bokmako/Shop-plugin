using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using Wolfje.Plugins.SEconomy;
using Color = Microsoft.Xna.Framework.Color;

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
        public override string Author => "Bokmako|Maxthegreat99";
        public override string Name => "ShopPlugin";
        public override Version Version => new Version(0, 1, 0, 0);
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
            Commands.ChatCommands.Add(new Command( Shop,
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
                if (reg == config.ShopRegionName)
                    return true;
            }
            return false;
        }
        private void Shop(CommandArgs args)
        {
            string cmd = "";
            if (!SEconomyPlugin.Instance.GetBankAccount(args.Player).IsAccountEnabled)
            {
                args.Player.SendErrorMessage("[Shop] Error: Your bank account is not enabled! you cannot access the shop.");
                return;
            }
            if (!args.Player.HasPermission(config.ShopPermission))
            {
                args.Player.SendInfoMessage(config.Messages["NoPerms"]);
                return;
            }
            if(args.Parameters.Count > 0)
                cmd = args.Parameters[0].ToLower();

            #region list senario
            if (!cmd.Equals("") && cmd.Equals(config.ListShopItemsCommand))
            {
                args.Player.SendMessage("[Shop] List of items present in the global shop:", Microsoft.Xna.Framework.Color.Green);
                int i = 0;
                foreach (ItemInfo item in config.Items)
                {
                    i++;
                    Item tItem = TShock.Utils.GetItemById(item.Id);
                    tItem.Prefix(item.modifierId);
                    Money buyingPriceToShow;
                    Money.TryParse(item.BuyPrice, out buyingPriceToShow);
                    Money sellingPriceToShow;
                    Money.TryParse(item.SelPrice, out sellingPriceToShow);
                    string itemShown = string.Format((item.modifierId != 0) ? "[i/p{0}:{1}]" : "[i:{1}]", tItem.prefix, item.Id);
                    args.Player.SendInfoMessage("- {0} [{1}] - [c/{2}:{3}]([c/{4}:{5}]) [c/{6}:B:] {7} {8}", i, itemShown,
                                                Microsoft.Xna.Framework.Color.Gold.Hex3(), tItem.HoverName,
                                                (item.Infinity || item.Amount != 0) ? Microsoft.Xna.Framework.Color.Green.Hex3() : Microsoft.Xna.Framework.Color.Red.Hex3(),
                                                item.Infinity ? "+" : item.Amount.ToString(),
                                                Microsoft.Xna.Framework.Color.Magenta.Hex3(),
                                                buyingPriceToShow.ToString(),
                                                (sellingPriceToShow.Value < 0) ? "" : string.Format("[c/{0}:S:] {1}",
                                                                                       Microsoft.Xna.Framework.Color.Magenta.Hex3(),
                                                                                       sellingPriceToShow.ToString()));
                }
                args.Player.SendInfoMessage("You have: {0}",SEconomyPlugin.Instance.GetBankAccount(args.Player).Balance.ToString());
                if (!IsInShop(args))
                    args.Player.SendErrorMessage("[Shop] You have to be in the shop to buy items!");

                return;
            }
            #endregion
            if (!IsInShop(args))
            {
                args.Player.SendInfoMessage(config.Messages["NotInShop"]);
                return;
            }

            if (args.Parameters.Count == 0)
            {
                args.Player.SendInfoMessage(config.Messages["ShopCommands"]);
                args.Player.SendInfoMessage(config.Messages["ListItems"], config.ShopCommand, config.ListShopItemsCommand);
                args.Player.SendInfoMessage(config.Messages["BuyInfo"], config.ShopCommand, config.BuyCommand);
                args.Player.SendInfoMessage(config.Messages["SellInfo"], config.ShopCommand, config.SellCommand);
                args.Player.SendInfoMessage(config.Messages["SearchInfo"], config.ShopCommand, config.SearchCommand);
                return;
            }

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
                        int index = 0;
                        string name;

                        int amount = 1;
                        if (int.TryParse(args.Parameters[1], out index) && ( index < 1 || index > config.Items.Count ) )
                        {
                            args.Player.SendErrorMessage("[Shop] Error: Invalid index entered!");
                            return;
                        }
                        else
                        {
                            name = args.Parameters[1];
                            if(TShock.Utils.GetItemByIdOrName(name) == null)
                            {
                                args.Player.SendErrorMessage("[Shop] Error: Invalid item name entered!");
                                return;
                            }
                        }
                        if (args.Parameters.Count == 3)
                        {
                            if (!int.TryParse(args.Parameters[2], out amount) || amount < 1)
                            {
                                args.Player.SendErrorMessage("[Shop] Error: Invalid amount entered!");
                                return;
                            }
                        }
                        int ItemNumber = 0;
                        ItemInfo Item = null;
                        int maxStack = 0;
                        ReadConfig(filepath, Lang.DefaultConfig(), out config);
                        for (int i = 0; i <= config.Items.Count - 1; i++)
                        {
                            Item tItem = TShock.Utils.GetItemByName(config.Items[i].Name)[0];
                            tItem.prefix = config.Items[i].modifierId;
                            if (i == index - 1 || tItem.HoverName.StartsWith(name) || tItem.Name.StartsWith(name) )
                            {
                                Item = config.Items[i];
                                ItemNumber = i;
                                maxStack = tItem.maxStack;
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
                        int slotsAvailable = 0;
                        //gets how many empty slots player has
                        for(int i = 0; i < NetItem.InventorySlots; i++)
                        {
                            if (args.TPlayer.inventory[i] == null || !args.TPlayer.inventory[i].active || args.TPlayer.inventory[i].Name == "")
                            {
                                slotsAvailable++;
                            }
                        }
                        if (!args.Player.InventorySlotAvailable || slotsAvailable < (int) Math.Ceiling((double)amount / maxStack) )
                        {
                            args.Player.SendInfoMessage(config.Messages["NoSlot"]);
                            return;
                        }
                        if (args.Player.InventorySlotAvailable)
                        {
                            TryToBuy(args.Player, Item, amount, ItemNumber);
                            return;
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
                return;
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
                int itemIndex = 0;
                Item tItem = new Item();
                string itemName = "null";
                if (args.Parameters.Count == 1)
                {
                    if (args.Player.SelectedItem.netID == 0 || TShock.Utils.GetItemById(args.Player.SelectedItem.netID) == null)
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
                    itemName = args.Parameters[2];
                    if (int.TryParse(args.Parameters[1], out IdItem) && IdItem < 1 || IdItem > Terraria.ID.ItemID.Count - 1)
                    {
                        args.Player.SendInfoMessage(config.Messages["SearchInfo"], config.ShopCommand, config.SearchCommand);
                        return;
                    }
                    else
                    {

                        if (TShock.Utils.GetItemByName(itemName) == null)
                        {
                            args.Player.SendInfoMessage(config.Messages["SearchInfo"], config.ShopCommand, config.SearchCommand);
                            return;
                        }
                    }
                }
                ReadConfig(filepath, Lang.DefaultConfig(), out config);
                for (int i = 0; i < config.Items.Count; i++)
                {
                    Item _tItem = TShock.Utils.GetItemByName(config.Items[i].Name)[0];
                    _tItem.prefix = config.Items[i].modifierId;
                    if (IdItem == config.Items[i].Id || config.Items[i].Name.StartsWith(itemName) || _tItem.HoverName.StartsWith(itemName) )
                    {
                        vFound = true;
                        vItem = config.Items[i];
                        tItem = _tItem;
                        itemIndex = i + 1;
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
                string itemShown = string.Format((vItem.modifierId != 0) ? "[i/p{0}:{1}]" : "[i:{1}]", tItem.prefix, tItem.netID);
                args.Player.SendInfoMessage(config.Messages["SearchItem"], itemIndex, itemShown,
                                                Microsoft.Xna.Framework.Color.Gold.Hex3(), tItem.HoverName,
                                                (vItem.Infinity || vItem.Amount != 0) ? Microsoft.Xna.Framework.Color.Green.Hex3() : Microsoft.Xna.Framework.Color.Red.Hex3(),
                                                vItem.Infinity ? "+" : vItem.Amount.ToString(),
                                                Microsoft.Xna.Framework.Color.Magenta.Hex3(),
                                                _buyPrice.ToString(),
                                                (_sellPrice.Value < 0) ? "" : string.Format("[c/{0}:S:] {1}",
                                                                                       Microsoft.Xna.Framework.Color.Magenta.Hex3(),
                                                                                       _sellPrice.ToString()));
                return;
            }
            #endregion
            args.Player.SendErrorMessage("Error: Invalid command, please do {0}{1} to see the list of commands!", TShock.Config.Settings.CommandSpecifier, config.ShopCommand);
        }
       
        private bool TryToBuy(TSPlayer pPlayer, ItemInfo pItem, int pStack, int pNumber)
        {
            var Bank = SEconomyPlugin.Instance.GetBankAccount(pPlayer);
            if(Int32.TryParse(pItem.BuyPrice, out var x) && x < 0)
            {
                pPlayer.SendInfoMessage("[Shop] We don't sell {0}. We only buy!", pItem.Name);
                return false;
            }
            int _itemCost = Convert.ToInt32(pItem.BuyPrice) * pStack;
            if (Bank == null || Bank.IsAccountEnabled == false  || !Money.TryParse(Convert.ToString(_itemCost), out var itemCost))
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

            pPlayer.SendInfoMessage(config.Messages["OnBuy"],
                                    Color.Green.Hex3(), Color.Magenta.Hex3(), pStack, pItem.modifierId, pItem.Id, itemCost.ToString(), Bank.Balance.ToString());

            

            TShock.Log.Write(string.Format("{0} bought {1} for {2}", pPlayer.Name, pItem.Name, _itemCost.ToString()), System.Diagnostics.TraceLevel.Info);

            Item tItem = TShock.Utils.GetItemById(pItem.Id);

            double _slotsToFill = pStack / tItem.maxStack;
            int slotsToFill = (int)Math.Ceiling(_slotsToFill);
            if (pStack < tItem.maxStack)
                slotsToFill = 1;
            for(int i = 0; i < slotsToFill; i++)
            {
                int amountToFill = tItem.maxStack;
                if (i == slotsToFill - 1)
                    amountToFill = pStack - ((slotsToFill - 1) * tItem.maxStack);
                pPlayer.GiveItem(pItem.Id, amountToFill, pItem.modifierId);
            }

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
            if (Int32.TryParse(vItem.SelPrice, out var x) && x < 1)
            {
                args.Player.SendInfoMessage("[Shop] We don't buy {0}. We only sell!", vItem.Name);
                return false;
            }

            int amount = args.Player.SelectedItem.stack;

            int _totalCost = amount * Convert.ToInt32(vItem.SelPrice);

            if (Bank == null || Bank.IsAccountEnabled == false || !Money.TryParse(Convert.ToString(_totalCost), out var itemCost))
            {
                args.Player.SendErrorMessage(config.Messages["Error"]);
                return false;
            }

            TSPlayer player = new TSPlayer(args.Player.Index);

            bool isSSC = Main.ServerSideCharacter;

            if (!isSSC)
            {
                Main.ServerSideCharacter = true;
                NetMessage.SendData(7, player.Index, -1, null, 0, 0.0f, 0.0f, 0.0f, 0, 0, 0);
                player.IgnoreSSCPackets = true;
            }


            int InvItem = Array.IndexOf(player.TPlayer.inventory, player.SelectedItem);

            (player.TPlayer.inventory[InvItem]).netDefaults(0);

            NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, player.Index, InvItem, 0, 0, 0, 0, 0);

            ShopManager(amount, Number, false);

            SEconomyPlugin.Instance.WorldAccount.TransferTo(Bank, itemCost, Wolfje.Plugins.SEconomy.Journal.BankAccountTransferOptions.AnnounceToReceiver,
                                                            "Sell item", string.Format("[Shop] Sold {0} {1} for {2} money",
                                                                                        amount, Terraria.Lang.GetItemNameValue(vItem.Id), _totalCost));
            player.SendInfoMessage(config.Messages["OnSell"], Color.Green.Hex3(), Color.Magenta.Hex3(), amount, vItem.modifierId, vItem.Id, _totalCost, Bank.Balance.ToString());
            TShock.Log.Write(string.Format("{0} sold {1} for {2}", args.Player.Name, vItem.Name, ((Money)_totalCost).ToString()), System.Diagnostics.TraceLevel.Info);

            if (!isSSC)
            {
                Main.ServerSideCharacter = false;
                NetMessage.SendData(7, player.Index, -1, null, 0, 0.0f, 0.0f, 0.0f, 0, 0, 0);
                player.IgnoreSSCPackets = false;
            }

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


