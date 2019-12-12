using PlayerLibrary;
using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using VehicleSQL.Model;

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
                    PlayerInfo  playerInfo = PlayerLibrary.PlayerLibrary.GetPlayerByCSteam(Player.CSteamID.m_SteamID);

                    Vehicles vehicles = new Vehicles((uint)playerInfo.player.Id, vehicle.id, vehicle.health, vehicle.fuel, vehicle.batteryCharge);

                    if (BarricadeManager.tryGetPlant(vehicle.transform, out byte x, out byte y, out ushort plant, out BarricadeRegion barricadeRegion))
                    {
                        VehicleSQL.pairs.Remove(Player.CSteamID.m_SteamID);

                        Rocket.Unturned.Chat.UnturnedChat.Say(caller, "保存车辆成功!");


                        vehicles.SetState(vehicle);

                        VehicleSQL.Db.Insertable(vehicles).ExecuteCommand();

                        VehicleManager.askVehicleDestroy(vehicle);
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
    }
}
