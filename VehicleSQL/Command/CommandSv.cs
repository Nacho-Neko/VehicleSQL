using PlayerLibrary;
using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using VehicleSQL.Model;
using Logger = Rocket.Core.Logging.Logger;

namespace VehicleSQL.Command
{
    class CommandSv : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "sv";

        public string Help => "车辆保存至云车库";

        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "sv" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer Player = (UnturnedPlayer)caller;
            if (VehicleSQL.pairs.ContainsKey(Player.CSteamID.m_SteamID))
            {
                uint instanceID = VehicleSQL.pairs[Player.CSteamID.m_SteamID];
                InteractableVehicle vehicle = VehicleManager.getVehicle(instanceID);
                if (vehicle.lockedOwner.m_SteamID == Player.CSteamID.m_SteamID)
                {
                    PlayerInfo playerInfo = PlayerLibrary.PlayerLibrary.GetPlayerByCSteam(Player.CSteamID.m_SteamID);

                    Vehicles vehicles = new Vehicles((uint)playerInfo.player.Id, vehicle.id, vehicle.health, vehicle.fuel, vehicle.batteryCharge, vehicle.tireAliveMask);

                    if (BarricadeManager.tryGetPlant(vehicle.transform, out byte x, out byte y, out ushort plant, out BarricadeRegion barricadeRegion))
                    {


                        if (!vehicle.isDead)
                        {
                            vehicles.state = GetState(vehicle);

                            VehicleSQL.pairs.Remove(Player.CSteamID.m_SteamID);

                            Rocket.Unturned.Chat.UnturnedChat.Say(caller, "保存车辆成功!");


                            
                            VehicleSQL.Db.Insertable(vehicles).ExecuteCommand();
                            VehicleManager.askVehicleDestroy(vehicle);
                        }
                        else
                        {
                            Rocket.Unturned.Chat.UnturnedChat.Say(caller, "车辆已经被摧毁了无法继续给你保存!");
                        }
                    }
                    else
                    {
                        Rocket.Unturned.Chat.UnturnedChat.Say(caller, "保存车辆失败!");
                    }
                }
                else
                {
                    Rocket.Unturned.Chat.UnturnedChat.Say(caller, "你当前没有上锁的车辆!");
                }
            }
            else
            {
                Rocket.Unturned.Chat.UnturnedChat.Say(caller, "你当前没有上锁的车辆!");
            }
        }


        public static byte[] GetState(InteractableVehicle interactableVehicle)
        {

            MyBlock block = new MyBlock();

            block.writeBoolean(interactableVehicle.sirensOn);
            block.writeBoolean(interactableVehicle.isBlimpFloating);
            block.writeBoolean(interactableVehicle.headlightsOn);
            block.writeBoolean(interactableVehicle.taillightsOn);

            if (interactableVehicle.turrets != null)
            {
                byte b = (byte)interactableVehicle.turrets.Length;
                block.writeByte(b);


#if DEBUG
                Logger.Log(b.ToString());
 #endif

                for (byte b2 = 0; b2 < b; b2++)
                {
                    Passenger passenger = interactableVehicle.turrets[b2];
                    if (passenger != null && passenger.state != null)
                    {
                        block.writeBytes(passenger.state);

#if DEBUG
                        Logger.Log(" passenger.state " + passenger.state.Length.ToString());
 #endif
                    }
                    else
                    {
                        block.writeBytes(new byte[0]);
                    }
                }
            }
            else
            {
                block.writeByte(0);
            }

            if (interactableVehicle.trunkItems != null && interactableVehicle.trunkItems.height > 0)
            {
                block.writeBoolean(true);

                byte itemCount = interactableVehicle.trunkItems.getItemCount();
                block.writeByte(itemCount);
                for (byte b3 = 0; b3 < itemCount; b3 += 1)
                {
                    ItemJar item = interactableVehicle.trunkItems.getItem(b3);
                    block.writeByte((item != null) ? item.x : (byte)0);
                    block.writeByte((item != null) ? item.y : (byte)0);
                    block.writeByte((item != null) ? item.rot : (byte)0);
                    block.writeUInt16((item != null) ? item.item.id : (byte)0);
                    block.writeByte((item != null) ? item.item.amount : (byte)0);
                    block.writeByte((item != null) ? item.item.quality : (byte)0);
                    block.writeBytes((item != null) ? item.item.state : new byte[0]);
                }
            }
            else
            {
                block.writeBoolean(false);
            }
            return block.getBuffer();
        }
    }
}
