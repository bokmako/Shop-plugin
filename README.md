# Shop-plugin
Shop plugin to sell/buy items from a global shop with SEconomy.

- Originally made by [bokmako](https://github.com/bokmako).
- Updated to **TShock** `5.2` for `1.4.4.9` and translated to English by Maxthegreat99.

## How to Install
1. Put the .dll into the `\ServerPlugins\` folder.
2. Restart the server.
3. Give your desired group the the permissions defined in the configs folder.

## How to Use
#### NOTE: you can modify most commands / permissions / messages in the configs.
### User instructions
- define a region with the same name as the `ShopRegionName` field defined in the configs, `Shop` being the default assigned value. You are needed to be within the shop region to use most commands.
- give your desired groups the different permissions depending on the commands you want the group to be able to use, `shop.shop` being the default permission to use the base command.
- add items to the shop using the configs and do `/Shop list` within the shop to see if they were added properly.

### Commands and Usage
- `shop` - base commands, gives you a list a commands if in the shop region.
- `shop list` - list items presently in the shop in an index, this command can be used anywhere in the world.
- `shop buy (Item index/name) [Quantity]` - (Item index/name) [Quantity] - buy an item depending on its index/name.
- `shop sell` - sell whats in your selected slot in the hotbar(If you give a group this permission, it is recommended that the world has SSC on).
- `shop search Optional:(Item ID/name)`  - searches if an item from either its id/name or from what you have selected in your hotbar is in the shop and gives you its prices/index.

### Config Options
#### Configs not stated are configs to change commands / permissions or messages.
- `ShopRegionName` - Name of the Shop region where the commands will be available for players to use.
- `Items` - List of items available in the global shop
  - `Id` - ID of an item in global shop.
  - `Name` - the name of an item in the global shop.
  - `Infinity` - wheather or not this item should be in infinite supply.
  - `Amount` - the amount of that item that is presently in the shop, may be changed no matter what.
  - `modifierId` - the modifier of the item in the shop, of course wont work with some items.
  - `SelPrice` - the price which the shop buys the item, players cant sell if the selling price is below 1.
  - `BuyPrice` - the price which the shop sells the item, players cant buy buy items with a price below 0.

### Important
- you of course need SEconomy to use this plugin, if you dont have it you can get it [here](https://github.com/Maxthegreat99/SEconomy).
    
## How to Build
1. Download the source code.
2. Open the `.sln` file.
3. Check to make sure the references are all correct and up to date.
4. Build.

## Notes
- As of right now the plugin is still in pre-release so all the features are not added yet and there might be some bugs or unusual things, if you do find them you can report them to this [discord](https://discord.gg/e465y7Xeba) where I(Maxthegreat) will be notified and try to fix them.
- I did not keep the russian version of the plugin and i am sorry for that, i shall add an option to use the plugin in russian in the configs in the release as well as make a russian readme.
- Note that [bokmako](https://github.com/bokmako) did an amazing work with this plugin and that it would have been hard for me to make what he did from scratch, most of the credits goes to them.

## Original repository
https://github.com/bokmako/Shop-plugin

  
