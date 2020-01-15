using System;
using SDG.Unturned;
using SqlSugar;

namespace VehicleSQL.Model
{
    public class Vehicles
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public uint id { get; set; }
        public uint player { get; set; }
        public ushort vehicle { get; set; }
        public ushort health { get; set; }
        public ushort fuel { get; set; }
        public ushort batteryCharge { get; set; }
        public byte tireAliveMask { get; set; }
        public byte[] state { get; set; }
        public Vehicles() { }

        public Vehicles(uint player, ushort vehicle, ushort health, ushort fuel, ushort batteryCharge, byte tireAliveMask)
        {
            this.player = player;
            this.vehicle = vehicle;
            this.health = health;
            this.fuel = fuel;
            this.batteryCharge = batteryCharge;
            this.tireAliveMask = tireAliveMask;
        }


    }
}
