using PlayerLibrary;
using Rocket.API;
using Rocket.Unturned.Player;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using VehicleSQL.Model;

namespace VehicleSQL.Command
{
    class CommandGv : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "gv";

        public string Help => "取出云车库中的车!";

        public string Syntax => "ID";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "gv" };

        public void Execute(IRocketPlayer caller, string[] command)
        {


            if (command.Length != 1) { Rocket.Unturned.Chat.UnturnedChat.Say(caller, "请使用格式:/gv ID");return; };

            UnturnedPlayer Player = (UnturnedPlayer)caller;
            PlayerInfo playerInfo = PlayerLibrary.PlayerLibrary.GetPlayerByCSteam(Player.CSteamID.m_SteamID);

            Vector3 vector = Player.Position;
            vector.y += 10f;
            

            ushort.TryParse(command[0], out ushort id);
            Vehicles vehicle = VehicleSQL.Db.Queryable<Vehicles>().Where(it => it.player == playerInfo.player.Id && it.vehicle == id).First();
            if (vehicle == null)
            {
                Rocket.Unturned.Chat.UnturnedChat.Say(caller, "没有保存ID为: " + id.ToString() + " 的载具!");
                return;
            }
            VehicleSQL.Db.Deleteable(vehicle).ExecuteCommand();

            CSteamID group = CSteamID.Nil;

            if (playerInfo.group == null)
            {
                group = CSteamID.Nil;
            }

            uint InstanceID = VehicleSQL.method.spawnVehicleInternal(vehicle, vector, Player.Player.transform.rotation, Player.CSteamID, group);

            VehicleSQL.pairs.Add(Player.CSteamID.m_SteamID, InstanceID);
        }
    }
}
