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
        public byte[] state { get; set; }
        public Vehicles() { }

        public Vehicles(uint player, ushort vehicle, ushort health, ushort fuel, ushort batteryCharge)
        {
            this.player = player;
            this.vehicle = vehicle;
            this.health = health;
            this.fuel = fuel;
            this.batteryCharge = batteryCharge;
        }


        public void SetState(InteractableVehicle interactableVehicle)
        {

            MyBlock block = new MyBlock();

            block.writeBoolean(interactableVehicle.sirensOn);
            block.writeBoolean(interactableVehicle.isBlimpFloating);
            block.writeBoolean(interactableVehicle.headlightsOn);
            block.writeBoolean(interactableVehicle.taillightsOn);
            block.writeBoolean(interactableVehicle.isExitable);

            if (interactableVehicle.turrets != null)
            {
                byte b = (byte)interactableVehicle.turrets.Length;
                block.writeByte(b);
                for (byte b2 = 0; b2 < b; b2 += 1)
                {
                    Passenger passenger = interactableVehicle.turrets[(int)b2];
                    if (passenger != null && passenger.state != null)
                    {
                        block.writeBytes(passenger.state);
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
            state = block.getBuffer();


        }
    }
}
