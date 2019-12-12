using Harmony;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace VehicleSQL
{
    public class Patch
    {

        [HarmonyPrefix]
        public static void SpawnVehicle(VehicleManager __instance , ushort id, Vector3 point, Quaternion angle, CSteamID owner)
        {
            Rocket.Core.Logging.Logger.LogError("Vehicle id:" + id.ToString());
        }


        [HarmonyPrefix]
        public static void tellLockedPrefix(InteractableVehicle __instance, CSteamID owner, CSteamID group, bool locked)
        {

            if (VehicleSQL.pairs.ContainsKey(__instance.lockedOwner.m_SteamID))
            {
                if (locked)
                {
                    VehicleSQL.pairs[__instance.lockedOwner.m_SteamID] = __instance.instanceID;

                }
                else
                {
                    VehicleSQL.pairs.Remove(__instance.lockedOwner.m_SteamID);

                }
            }
            else
            {
                if (locked)
                    VehicleSQL.pairs.Add(__instance.lockedOwner.m_SteamID, __instance.instanceID);
            }

            //Rocket.Core.Logging.Logger.LogError("Vehicle Lock:" + __instance.id.ToString()  + " "+ locked.ToString());
        }



    }
}
