using Harmony;
using SDG.Unturned;
using Steamworks;
using System;
using UnityEngine;
using VehicleSQL.Model;
using Logger = Rocket.Core.Logging.Logger;

namespace VehicleSQL
{
    public class Method
    {

        private static VehicleManager vehicleManager;
        public Method()
        {
            vehicleManager = VehicleManager.instance;
            CheckSchema();
        }

        private void CheckSchema()
        {
            if (PlayerLibrary.PlayerLibrary.CheckTable("Vehicles"))
            {
                PlayerLibrary.PlayerLibrary.CreateTables("CREATE TABLE `Vehicles` ( `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, `player` int(10) UNSIGNED NOT NULL, `vehicle` smallint(5) UNSIGNED NOT NULL, `health` smallint(5) UNSIGNED DEFAULT NULL, `fuel` smallint(5) UNSIGNED DEFAULT NULL, `batteryCharge` smallint(5) UNSIGNED DEFAULT NULL, `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP, `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP, PRIMARY KEY (`id`), KEY `vehicles_player` USING HASH (`player`), CONSTRAINT `vehicles_player` FOREIGN KEY (`player`) REFERENCES `Players` (`id`) ON DELETE CASCADE ON UPDATE CASCADE ) ENGINE = InnoDB AUTO_INCREMENT = 2 CHARSET = utf8;");
            }
        }

        private static uint allocateInstanceID()
        {
            var highestInstanceID = Traverse.Create<VehicleManager>().Field("highestInstanceID");
            uint num = highestInstanceID.GetValue<uint>();
            //highestInstanceID.SetValue();
            highestInstanceID.SetValue(++num);
            //Logger.LogError(num.ToString());
            return num;
        }
        private InteractableVehicle addVehicle(ushort id, ushort skinID, ushort mythicID, float roadPosition, Vector3 point, Quaternion angle, bool sirens, bool blimp, bool headlights, bool taillights, ushort fuel, bool isExploded, ushort health, ushort batteryCharge, CSteamID owner, CSteamID group, bool locked, CSteamID[] passengers, byte[][] turrets, uint instanceID, byte tireAliveMask)
        {

            VehicleAsset vehicleAsset = (VehicleAsset)Assets.find(EAssetType.VEHICLE, id);

            IDeferredAsset<GameObject> expr_60 = vehicleAsset.model;
            GameObject gameObject = (expr_60 != null) ? expr_60.getOrLoad() : null;
            if (gameObject == null)
            {
                Debug.LogWarningFormat("Unable to spawn any gameobject for vehicle {0}", new object[]
                {
            id
                });
                return null;
            }
            InteractableVehicle interactableVehicle = null;
            try
            {
                Transform transform = VehicleManager.Instantiate(gameObject).transform;
                transform.name = id.ToString();
                transform.parent = LevelVehicles.models;
                transform.position = point;
                transform.rotation = angle;
                Rigidbody orAddComponent = transform.GetOrAddComponent<Rigidbody>();
                orAddComponent.useGravity = true;
                orAddComponent.isKinematic = false;
                interactableVehicle = transform.gameObject.AddComponent<InteractableVehicle>();
                interactableVehicle.roadPosition = roadPosition;
                interactableVehicle.instanceID = instanceID;
                interactableVehicle.id = id;
                interactableVehicle.skinID = skinID;
                interactableVehicle.mythicID = mythicID;
                interactableVehicle.fuel = fuel;
                interactableVehicle.isExploded = isExploded;
                interactableVehicle.health = health;
                interactableVehicle.batteryCharge = batteryCharge;
                interactableVehicle.init();
                interactableVehicle.tellSirens(sirens);
                interactableVehicle.tellBlimp(blimp);
                interactableVehicle.tellHeadlights(headlights);
                interactableVehicle.tellTaillights(taillights);
                interactableVehicle.tellLocked(owner, group, locked);
                interactableVehicle.tireAliveMask = tireAliveMask;
                if (Provider.isServer)
                {
                    if (turrets != null && turrets.Length == interactableVehicle.turrets.Length)
                    {
                        byte b = 0;
                        while ((int)b < interactableVehicle.turrets.Length)
                        {
                            interactableVehicle.turrets[(int)b].state = turrets[(int)b];
                            b += 1;
                        }
                    }
                    else
                    {
                        byte b2 = 0;
                        while ((int)b2 < interactableVehicle.turrets.Length)
                        {
                            ItemAsset itemAsset = (ItemAsset)Assets.find(EAssetType.ITEM, vehicleAsset.turrets[(int)b2].itemID);
                            if (itemAsset != null)
                            {
                                interactableVehicle.turrets[(int)b2].state = itemAsset.getState();
                            }
                            else
                            {
                                interactableVehicle.turrets[(int)b2].state = null;
                            }
                            b2 += 1;
                        }
                    }
                }
                if (passengers != null)
                {
                    byte b3 = 0;
                    while ((int)b3 < passengers.Length)
                    {
                        if (passengers[(int)b3] != CSteamID.Nil)
                        {
                            interactableVehicle.addPlayer(b3, passengers[(int)b3]);
                        }
                        b3 += 1;
                    }
                }
                if (vehicleAsset.trunkStorage_Y > 0)
                {
                    interactableVehicle.trunkItems = new Items(PlayerInventory.STORAGE);
                    interactableVehicle.trunkItems.resize(vehicleAsset.trunkStorage_X, vehicleAsset.trunkStorage_Y);
                }
                VehicleManager.vehicles.Add(interactableVehicle);
                BarricadeManager.waterPlant(transform);
                if (interactableVehicle.trainCars != null)
                {
                    for (int i = 1; i < interactableVehicle.trainCars.Length; i++)
                    {
                        BarricadeManager.waterPlant(interactableVehicle.trainCars[i].root);
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarningFormat("Exception while spawning vehicle: {0}", new object[]
                {
            id
                });
                Debug.LogException(exception);
            }
            return interactableVehicle;
        }

        public uint spawnVehicleInternal(Vehicles vehicles, Vector3 point, Quaternion angle, CSteamID owner, CSteamID group)
        {
            uint InstanceID = 0;
            VehicleAsset vehicleAsset = (VehicleAsset)Assets.find(EAssetType.VEHICLE, vehicles.vehicle);
            if (vehicleAsset != null)
            {
                InstanceID = allocateInstanceID();
                bool locked = owner != CSteamID.Nil;

                bool sirens =false, blimp = false, headlights = false, taillights = false, isExploded = false;
                ItemJar[] array2 = null;
                if (vehicles.state != null)
                {
                    MyBlock block = new MyBlock(vehicles.state);

                    sirens = block.readBoolean();
                    blimp = block.readBoolean();
                    headlights = block.readBoolean();
                    taillights = block.readBoolean();
                    isExploded = block.readBoolean();

                    byte[][] turrets = new byte[(int)block.readByte()][];
                    byte b2 = 0;
                    while ((int)b2 < turrets.Length)
                    {
                        turrets[(int)b2] = block.readByteArray();
                        b2 += 1;
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


                InteractableVehicle interactableVehicle = addVehicle(vehicles.vehicle, 0, 0, 0f, point, angle, sirens, blimp, headlights, taillights, vehicleAsset.fuel, isExploded, vehicleAsset.health, vehicles.batteryCharge, owner, group, locked, null, null, InstanceID, 255);
                if (interactableVehicle != null && array2 != null && array2.Length != 0 && interactableVehicle.trunkItems != null && interactableVehicle.trunkItems.height > 0)
                {
                    byte b4 = 0;
                    while ((int)b4 < array2.Length)
                    {
                        ItemJar itemJar = array2[(int)b4];
                        if (itemJar != null)
                        {
                            interactableVehicle.trunkItems.loadItem(itemJar.x, itemJar.y, itemJar.rot, itemJar.item);
                        }
                        b4 += 1;
                    }
                }
                
                vehicleManager.channel.openWrite();
                sendVehicle(VehicleManager.vehicles[VehicleManager.vehicles.Count - 1]);
                vehicleManager.channel.closeWrite("tellVehicle", ESteamCall.OTHERS, ESteamPacket.UPDATE_RELIABLE_CHUNK_BUFFER);
                Transform transform = VehicleManager.vehicles[VehicleManager.vehicles.Count - 1].transform;
                BarricadeManager.askPlants(transform);
            }
            return InstanceID;

        }
        // SDG.Unturned.VehicleManager
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
            vehicleManager.channel.write(new object[]
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
            vehicleManager.channel.write((byte)vehicle.passengers.Length);
            byte b = 0;
            while ((int)b < vehicle.passengers.Length)
            {
                Passenger passenger = vehicle.passengers[(int)b];
                if (passenger.player != null)
                {
                    vehicleManager.channel.write(passenger.player.playerID.steamID);
                }
                else
                {
                    vehicleManager.channel.write(CSteamID.Nil);
                }
                b += 1;
            }
        }


    }
}
