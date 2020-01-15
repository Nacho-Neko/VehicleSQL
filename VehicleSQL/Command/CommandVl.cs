using PlayerLibrary;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using VehicleSQL.Model;

namespace VehicleSQL.Command
{
    class CommandVl : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "vl";

        public string Help => "查询云车库当前列表";

        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "vl" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer Player = (UnturnedPlayer)caller;

            PlayerInfo playerInfo = PlayerLibrary.PlayerLibrary.GetPlayerByCSteam(Player.CSteamID.m_SteamID);


            List<Vehicles> vehicles = VehicleSQL.Db.Queryable<Vehicles>().Where(it => it.player == playerInfo.player.Id).ToList();

            if (vehicles.Count > 0)
            {
                string str = "";
                foreach (var vehicle in vehicles)
                {
                    str += "ID:" + vehicle.vehicle;
                }
                UnturnedChat.Say(Player,"你当前保存的载具有:" + str);
            }
            else {
                UnturnedChat.Say(Player,"你一个载具也没有保存!");
            }

            

          


        }
    }
}
