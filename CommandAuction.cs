﻿using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using fr34kyn01535.Uconomy;
using Steamworks;

namespace LIGHT
{
    public class CommandAuction : IRocketCommand
    {       
        public string Name
        {
            get
            {
                return "auction";
            }
        } 
        public AllowedCaller AllowedCaller
        {
            get
            {
                return AllowedCaller.Player;
            }
        }
        public string Help
        {
            get
            {
                return "Allows you to auction your items from your inventory.";
            }
        }
        public string Syntax
        {
            get
            {
                return "<name or id>";
            }
        }
        public List<string> Aliases
        {
            get { return new List<string>(); }
        }
        public List<string> Permissions
        {
            get 
            {
                return new List<string>() {"auction"};
            }
        }
        public void Execute(IRocketPlayer caller, params string[] command)
        {
            if(!LIGHT.Instance.Configuration.Instance.AllowAuction)
            {
                UnturnedChat.Say(caller, LIGHT.Instance.Translate("auction_disabled"));
                return;
            }
            UnturnedPlayer player = (UnturnedPlayer)caller;
            if (command.Length == 0)
            {
                UnturnedChat.Say(player, LIGHT.Instance.Translate("auction_command_usage"));
                return;
            }
            if(command.Length == 1)
            {
                switch (command[0])
                {
                    case ("add"):
                        UnturnedChat.Say(player, LIGHT.Instance.Translate("auction_addcommand_usage"));
                        return;
                    case ("list"):
                        string Message = "";
                        string[] ItemNameAndQuality = LIGHT.Instance.DatabaseAuction.GetAllItemNameWithQuality();
                        string[] AuctionID = LIGHT.Instance.DatabaseAuction.GetAllAuctionID();
                        string[] ItemPrice = LIGHT.Instance.DatabaseAuction.GetAllItemPrice();
                        int count = 0;
                        for (int x = 0; x < ItemNameAndQuality.Length; x++)
                        {
                            if(x < ItemNameAndQuality.Length-1)
                                Message += "[" + AuctionID[x] + "]: " + ItemNameAndQuality[x] + " for "+ ItemPrice[x] + Uconomy.Instance.Configuration.Instance.MoneyName +", ";
                            else
                                Message += "[" + AuctionID[x] + "]: " + ItemNameAndQuality[x] + " for " + ItemPrice[x] + Uconomy.Instance.Configuration.Instance.MoneyName;
                            count ++;
                            if(count == 2)
                            {
                                UnturnedChat.Say(player, Message);
                                Message = "";
                                count = 0;
                            }
                        }
                        if(Message != "")
                            UnturnedChat.Say(player, Message);
                        break;
                    case ("buy"):
                        UnturnedChat.Say(player, LIGHT.Instance.Translate("auction_buycommand_usage"));
                        return;
                }
            }
            if (command.Length == 2)
            {
                switch (command[0])
                {
                    case ("add"):
                        UnturnedChat.Say(player, LIGHT.Instance.Translate("auction_addcommand_usage2"));
                        return;
                    case ("buy"):
                        int auctionid = 0;
                        if (int.TryParse(command[1], out auctionid))
                        {
                            try
                            {
                                string[] itemInfo = LIGHT.Instance.DatabaseAuction.AuctionBuy(auctionid);
                                decimal balance = Uconomy.Instance.Database.GetBalance(player.Id);
                                decimal cost = 1.00m;
                                decimal.TryParse(itemInfo[2], out cost);                               
                                if (balance < cost)
                                {
                                    UnturnedChat.Say(player, LIGHT.Instance.DefaultTranslations.Translate("not_enough_currency_msg", Uconomy.Instance.Configuration.Instance.MoneyName, itemInfo[1]));
                                    return;
                                }                                                               
                                player.GiveItem(ushort.Parse(itemInfo[0]), 1);
                                InventorySearch inventory = player.Inventory.has(ushort.Parse(itemInfo[0]));
                                byte index = player.Inventory.getIndex(inventory.page, inventory.jar.PositionX, inventory.jar.PositionY);
                                player.Inventory.updateQuality(inventory.page, index, byte.Parse(itemInfo[3]));
                                LIGHT.Instance.DatabaseAuction.DeleteAuction(command[1]);
                                decimal newbal = Uconomy.Instance.Database.IncreaseBalance(player.CSteamID.ToString(), (cost * -1));
                                UnturnedChat.Say(player, LIGHT.Instance.Translate("auction_buy_msg", itemInfo[1], cost, Uconomy.Instance.Configuration.Instance.MoneyName, newbal, Uconomy.Instance.Configuration.Instance.MoneyName));
                                decimal sellernewbalance = Uconomy.Instance.Database.IncreaseBalance(itemInfo[4], (cost * 1));
                            }
                            catch
                            {
                                UnturnedChat.Say(player, LIGHT.Instance.Translate("auction_addcommand_idnotexist"));
                                return;
                            }
                            
                        }
                        else
                        {
                            UnturnedChat.Say(player, LIGHT.Instance.Translate("auction_addcommand_usage2"));
                            return;
                        }
                        break;
                }
            }
            if (command.Length > 2)
            {
                switch (command[0])
                {
                    case ("add"):
                        byte amt = 1;
                        ushort id;
                        string name = null;
                        ItemAsset vAsset = null;
                        string itemname = "";
                        for (int x = 1; x < command.Length - 1; x++)
                        {
                            itemname += command[x] + " ";
                        }
                        itemname = itemname.Trim();
                        if (!ushort.TryParse(itemname, out id))
                        {
                            Asset[] array = Assets.find(EAssetType.ITEM);
                            Asset[] array2 = array;
                            for (int i = 0; i < array2.Length; i++)
                            {
                                vAsset = (ItemAsset)array2[i];
                                if (vAsset != null && vAsset.Name != null && vAsset.Name.ToLower().Contains(itemname.ToLower()))
                                {
                                    id = vAsset.Id;
                                    name = vAsset.Name;
                                    break;
                                }
                            }
                        }
                        if (name == null && id == 0)
                        {
                            UnturnedChat.Say(player, LIGHT.Instance.Translate("could_not_find", itemname));
                            return;
                        }
                        else if (name == null && id != 0)
                        {
                            try
                            {
                                vAsset = (ItemAsset)Assets.find(EAssetType.ITEM, id);
                                name = vAsset.Name;
                            }
                            catch
                            {
                                UnturnedChat.Say(player, LIGHT.Instance.Translate("item_invalid"));
                                return;
                            }
                        }
                        if (player.Inventory.has(id) == null)
                        {
                            UnturnedChat.Say(player, LIGHT.Instance.Translate("not_have_item_auction", name));
                            return;
                        }
                        List<InventorySearch> list = player.Inventory.search(id, true, true);                        
                        if (vAsset.Amount > 1)
                        {
                            UnturnedChat.Say(player, LIGHT.Instance.Translate("auction_item_mag_ammo", name));
                            return;
                        }
                        decimal price = 0.00m;
                        if (LIGHT.Instance.Configuration.Instance.EnableShop)
                        {
                            price = LIGHT.Instance.ShopDB.GetItemCost(id);
                            if (price <= 0.00m)
                            {
                                UnturnedChat.Say(player, LIGHT.Instance.Translate("auction_item_notinshop", name));
                                price = 0.00m;
                            }
                        }
                        byte quality = 100;
                        switch (vAsset.Amount)
                        {
                            case 1:
                                // These are single items, not ammo or magazines
                                while (amt > 0)
                                {
                                    try
                                    {
                                        if (player.Player.Equipment.checkSelection(list[0].InventoryGroup, list[0].ItemJar.PositionX, list[0].ItemJar.PositionY))
                                        {
                                            player.Player.Equipment.dequip();
                                        }
                                    }
                                    catch
                                    {
                                        UnturnedChat.Say(player, LIGHT.Instance.Translate("auction_unequip_item", name));
                                        return;
                                    }
                                    quality = list[0].ItemJar.Item.Durability;
                                    player.Inventory.removeItem(list[0].InventoryGroup, player.Inventory.getIndex(list[0].InventoryGroup, list[0].ItemJar.PositionX, list[0].ItemJar.PositionY));
                                    list.RemoveAt(0);
                                    amt--;
                                }
                                break;
                            default:
                                UnturnedChat.Say(player, LIGHT.Instance.Translate("auction_item_mag_ammo", name));
                                return;
                        }
                        decimal SetPrice;
                        if(!decimal.TryParse(command[command.Length - 1], out SetPrice))
                            SetPrice = price;
                        if (LIGHT.Instance.DatabaseAuction.AddAuctionItem(LIGHT.Instance.DatabaseAuction.GetLastAuctionNo(), id.ToString(), name, SetPrice, price, (int)quality, player.Id))
                            UnturnedChat.Say(player, LIGHT.Instance.Translate("auction_item_succes", name, SetPrice, Uconomy.Instance.Configuration.Instance.MoneyName));
                        else
                            UnturnedChat.Say(player, LIGHT.Instance.Translate("auction_item_failed"));
                        break;
                }
                
            }
        }

    }

}