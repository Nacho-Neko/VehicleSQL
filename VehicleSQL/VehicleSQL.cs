using Harmony;
using Rocket.Core.Plugins;
using SDG.Unturned;
using SqlSugar;
using System.Collections.Generic;
using System.Reflection;

namespace VehicleSQL
{
    public class VehicleSQL : RocketPlugin
    {
        public static VehicleSQL Instance;
        public static SqlSugarClient Db;
        public static Method method;
        public static Dictionary<ulong, uint> pairs = new Dictionary<ulong, uint>();

        protected override void Load()
        {
            Instance = this;

            Db = PlayerLibrary.DbMySQL.Db;

            method = new Method();

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



        protected override void Unload()
        {
            Instance = null;
            method = null;

        }
    }
}
