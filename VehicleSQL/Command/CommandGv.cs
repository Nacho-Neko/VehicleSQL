using PlayerLibrary;
using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using VehicleSQL.Model;
using Logger = Rocket.Core.Logging.Logger;

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


            if (command.Length != 1) { Rocket.Unturned.Chat.UnturnedChat.Say(caller, "请使用格式:/gv ID"); return; };

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

            CSteamID group = CSteamID.Nil;
            if (playerInfo.group == null)
            {
                group = CSteamID.Nil;
            }
            else
            {
                ulong.TryParse(playerInfo.group.groupID, out ulong csteamid);
                group = (CSteamID)csteamid;
            }


            bool sirens = false, blimp = false, headlights = false, taillights = false, isExploded = false;
            ItemJar[] array2 = null;
            byte[][] turrets = null;

            if (vehicle.state != null)
            {
                MyBlock block = new MyBlock(vehicle.state);

                sirens = block.readBoolean();
                blimp = block.readBoolean();
                headlights = block.readBoolean();
                taillights = block.readBoolean();


                turrets = new byte[block.readByte()][];
                byte b2 = 0;

#if DEBUG
                Logger.Log(turrets.Length.ToString());
#endif

                while (b2 < turrets.Length)
                {
                    turrets[b2] = block.readByteArray();
#if DEBUG
                    Logger.Log("turrets[b2] : " + turrets[b2].Length);
#endif
                    b2++;
                }

                bool flag = block.readBoolean();

                if (flag)
                {
                    array2 = new ItemJar[(int)block.readByte()];
                    byte b3 = 0;
                    while ((int)b3 < array2.Length)
                    {
                        byte new_x = block.readByte();
                        byte new_y = block.readByte();
                        byte newRot = block.readByte();
                        ushort num5 = block.readUInt16();
                        byte newAmount = block.readByte();
                        byte newQuality = block.readByte();
                        byte[] newState = block.readByteArray();
                        if ((ItemAsset)Assets.find(EAssetType.ITEM, num5) != null)
                        {
                            Item newItem = new Item(num5, newAmount, newQuality, newState);
                            array2[(int)b3] = new ItemJar(new_x, new_y, newRot, newItem);
                        }
                        b3 += 1;
                    }
                }
            }
            uint InstanceID = VehicleSQL.Instance.allocateInstanceID();
            VehicleSQL.Instance.AddVehicle(vehicle.vehicle, 0, 0, 0f, vector, Player.Player.transform.rotation, sirens, blimp, headlights, taillights, vehicle.fuel, false, vehicle.health, vehicle.batteryCharge, Player.CSteamID, Player.SteamGroupID, true, null, turrets, InstanceID, vehicle.tireAliveMask);

            /*
            VehicleSQL.vehicleManager.channel.openWrite();
            //sendVehicle(VehicleManager.vehicles[VehicleManager.vehicles.Count - 1]);
            VehicleSQL.vehicleManager.channel.closeWrite("tellVehicle", ESteamCall.OTHERS, ESteamPacket.UPDATE_RELIABLE_CHUNK_BUFFER);
            Transform transform = VehicleManager.vehicles[VehicleManager.vehicles.Count - 1].transform;
            BarricadeManager.askPlants(transform);
            */
            VehicleSQL.vehicleManager.channel.openWrite();
            VehicleSQL.vehicleManager.sendVehicle(VehicleManager.vehicles[VehicleManager.vehicles.Count - 1]);
            Transform transform = VehicleManager.vehicles[VehicleManager.vehicles.Count - 1].transform;
            VehicleSQL.vehicleManager.channel.closeWrite("tellVehicle", ESteamCall.OTHERS, ESteamPacket.UPDATE_RELIABLE_CHUNK_BUFFER);
            BarricadeManager.askPlants(transform);
            VehicleSQL.Db.Deleteable(vehicle).ExecuteCommand();
            if (VehicleSQL.pairs.ContainsKey(Player.CSteamID.m_SteamID))
            {
                VehicleSQL.pairs[Player.CSteamID.m_SteamID] = InstanceID;
            }
            else
            {
                VehicleSQL.pairs.Add(Player.CSteamID.m_SteamID, InstanceID);
            }
           
        }

        public void sendVehicle(InteractableVehicle vehicle)
        {
            Vector3 position;
            if (vehicle.asset.engine == EEngine.TRAIN)
            {
                position = new Vector3(vehicle.roadPosition, 0f, 0f);
            }
            else
            {
                position = vehicle.transform.position;
            }
            VehicleSQL.vehicleManager.channel.write(new object[]
            {
                vehicle.id,
                vehicle.skinID,
                vehicle.mythicID,
                position,
                MeasurementTool.angleToByte2(vehicle.transform.rotation.eulerAngles.x),
                MeasurementTool.angleToByte2(vehicle.transform.rotation.eulerAngles.y),
                MeasurementTool.angleToByte2(vehicle.transform.rotation.eulerAngles.z),
                vehicle.sirensOn,
                vehicle.isBlimpFloating,
                vehicle.headlightsOn,
                vehicle.taillightsOn,
                vehicle.fuel,
                vehicle.isExploded,
                vehicle.health,
                vehicle.batteryCharge,
                vehicle.lockedOwner,
                vehicle.lockedGroup,
                vehicle.isLocked,
                vehicle.instanceID,
                vehicle.tireAliveMask
            });
            VehicleSQL.vehicleManager.channel.write((byte)vehicle.passengers.Length);
            byte b = 0;
            while (b < vehicle.passengers.Length)
            {
                Passenger passenger = vehicle.passengers[b];
                if (passenger.player != null)
                {
                    VehicleSQL.vehicleManager.channel.write(passenger.player.playerID.steamID);
                }
                else
                {
                    VehicleSQL.vehicleManager.channel.write(CSteamID.Nil);
                }
                b++;
            }
        }
    }
}
