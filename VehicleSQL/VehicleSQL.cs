using Harmony;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using SDG.Unturned;
using SqlSugar;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace VehicleSQL
{
    public class VehicleSQL : RocketPlugin<VehicleSQLConfiguration>
    {
        public static VehicleSQL Instance;
        public static VehicleManager vehicleManager;
        public static MethodInfo addVehicle;

        public static SqlSugarClient Db;
        public static Dictionary<ulong, uint> pairs = new Dictionary<ulong, uint>();

        protected override void Load()
        {
            Instance = this;

            Db = PlayerLibrary.DbMySQL.Db;

            Type type = typeof(VehicleManager);
            FieldInfo manager = type.GetField("manager", BindingFlags.Static | BindingFlags.NonPublic);
            vehicleManager = (VehicleManager)manager.GetValue(null);
            addVehicle = type.GetMethod("addVehicle", BindingFlags.Instance | BindingFlags.NonPublic);

            Db.MappingTables.Add("Vehicles", Configuration.Instance.TableName);

            if (PlayerLibrary.DbMySQL.CheckTable(Configuration.Instance.TableName))
            {
                PlayerLibrary.DbMySQL.CreateTables("CREATE TABLE `" + Configuration.Instance.TableName + "` ( `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT, `player` int(10) UNSIGNED NOT NULL, `vehicle` smallint(5) UNSIGNED NOT NULL, `health` smallint(5) UNSIGNED DEFAULT NULL, `fuel` smallint(5) UNSIGNED DEFAULT NULL, `batteryCharge` smallint(5) UNSIGNED DEFAULT NULL,`tireAliveMask` tinyint(3) UNSIGNED DEFAULT NULL,`state` blob DEFAULT NULL, `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP, `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP, PRIMARY KEY (`id`), KEY `vehicles_player` USING HASH (`player`), CONSTRAINT `vehicles_player` FOREIGN KEY (`player`) REFERENCES `Players` (`id`) ON DELETE CASCADE ON UPDATE CASCADE ) ENGINE = InnoDB AUTO_INCREMENT = 2 CHARSET = utf8;");
            }

            var harmony = HarmonyInstance.Create("com.hana.vehiclesql");
            var spawnVehicle = typeof(VehicleManager).GetMethod("spawnVehicleInternal", BindingFlags.NonPublic | BindingFlags.Static);

#if DEBUG
            Logger.LogError(spawnVehicle.Name);
#endif
            var spawnVehiclePrefix = typeof(Patch).GetMethod("SpawnVehicle");
            harmony.Patch(spawnVehicle, new HarmonyMethod(spawnVehiclePrefix));


            var spawnVehicleInternal = typeof(InteractableVehicle).GetMethod("tellLocked");
            var tellLockedPrefix = typeof(Patch).GetMethod("tellLockedPrefix");
            harmony.Patch(spawnVehicleInternal, new HarmonyMethod(tellLockedPrefix));
        }

        public InteractableVehicle AddVehicle(ushort id, ushort skinID, ushort mythicID, float roadPosition, Vector3 point, Quaternion angle, bool sirens, bool blimp, bool headlights, bool taillights, ushort fuel, bool isExploded, ushort health, ushort batteryCharge, CSteamID owner, CSteamID group, bool locked, CSteamID[] passengers, byte[][] turrets, uint instanceID, byte tireAliveMask)
        {
            InteractableVehicle interactableVehicle = (InteractableVehicle)addVehicle.Invoke(vehicleManager, new object[] { id, skinID, mythicID, roadPosition, point, angle, sirens, blimp, headlights, taillights, fuel, false, health, batteryCharge, owner, group, locked, passengers, turrets, instanceID, tireAliveMask });
            return interactableVehicle;
        }

        public uint allocateInstanceID()
        {
            var highestInstanceID = Traverse.Create<VehicleManager>().Field("highestInstanceID");
            uint num = highestInstanceID.GetValue<uint>();
            //highestInstanceID.SetValue();
            highestInstanceID.SetValue(++num);
            //Logger.LogError(num.ToString());
            return num;
        }
        protected override void Unload()
        {
            Instance = null;

        }
    }
}
