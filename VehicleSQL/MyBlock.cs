using Steamworks;
using System;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace VehicleSQL
{
    public class MyBlock
    {
        private int step;
        private byte[] block;

        private byte[] buffer = new byte[65535];

        private bool longBinaryData = false;
        public MyBlock(byte[] contents)
        {
            reset(contents);
        }

        public MyBlock()
        {
            reset();
        }
        public byte[] getBuffer()
        {
            if (step == 0)
            {
                return new byte[0];
            }

            byte[] temp = new byte[step];
            Array.Copy(buffer, temp, step);
            return temp;
        }

        public byte readByte()
        {
            if (block != null && step <= block.Length - 1)
            {
                byte result = block[step];
                step++;
                return result;
            }
            return 0;
        }
        public ushort readUInt16()
        {
            if (block != null && step <= block.Length - 2)
            {
                ushort result = BitConverter.ToUInt16(block, step);
                step += 2;
                return result;
            }
            return 0;
        }


        public void writeSingle(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, buffer, step, bytes.Length);
            this.step += 4;
        }

        public void writeSingleVector3(Vector3 value)
        {
            writeSingle(value.x);
            writeSingle(value.y);
            writeSingle(value.z);
        }

        public void writeBytes(byte[] values)
        {
            if (buffer != null)
            {
                writeByteArray(values);
                return;
            }
        }


        public void writeSteamID(CSteamID steamID)
        {
            writeUInt64(steamID.m_SteamID);
        }

        public void writeUInt64(ulong value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            this.writeBitConverterBytes(bytes);
            this.step += 8;
        }


        public int readInt32()
        {
            if (block != null && step <= block.Length - 4)
            {
                int result = BitConverter.ToInt32(block, step);
                step += 4;
                return result;
            }
            return 0;
        }

        public Vector3 readSingleVector3()
        {

            return new Vector3(readSingle(), readSingle(), readSingle());

        }

        public float readSingle()
        {
            if (block != null && step <= block.Length - 4)
            {
                float result = BitConverter.ToSingle(block, step);
                step += 4;
                return result;
            }
            return 0f;
        }


        public uint readUInt32()
        {
            if (this.block != null && step <= block.Length - 4)
            {
                uint result = BitConverter.ToUInt32(block, step);
                this.step += 4;
                return result;
            }
            return 0u;
        }
        public CSteamID readSteamID()
        {
            return new CSteamID(readUInt64());
        }

        public ulong readUInt64()
        {
            if (block != null && step <= block.Length - 8)
            {
                ulong result = BitConverter.ToUInt64(block, step);
                step += 8;
                return result;
            }
            return 0uL;
        }

        public void writeInt16(short value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            this.writeBitConverterBytes(bytes);
            this.step += 2;
        }

        protected virtual void writeBitConverterBytes(byte[] bytes)
        {
            int dstOffset = this.step;
            int count = bytes.Length;
            Buffer.BlockCopy(bytes, 0, buffer, dstOffset, count);
        }





        public byte[] readByteArray()
        {
#if DEBUG
            Logger.LogError("block.Length " + block.Length.ToString());
#endif

            if (block != null && step < block.Length)
            {
                byte[] array;
                if (longBinaryData)
                {
                    int num = readInt32();
                    if (num >= 30000)
                    {
                        return new byte[0];
                    }
                    array = new byte[num];
                }
                else
                {
                    array = new byte[(block[step])];
                    step++;

#if DEBUG
                    Logger.LogError("array.Length " + array.Length.ToString());
#endif
                }
                if (step + array.Length <= block.Length)
                {
                    try
                    {
                        Buffer.BlockCopy(block, step, array, 0, array.Length);
                    }
                    catch
                    {

                    }
                }
                step += array.Length;
                return array;
            }
            return new byte[0];
        }


        public short readInt16()
        {
            if (block != null && step <= block.Length - 2)
            {
                short result = BitConverter.ToInt16(block, step);
                step += 2;
                return result;
            }
            return 0;
        }

        public void writeByte(byte value)
        {
            buffer[step] = value;
            step++;
        }

        public void writeInt32(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, buffer, step, bytes.Length);
            step += 4;
        }


        public void writeUInt16(ushort value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, buffer, step, bytes.Length);
            step += 2;
        }

        public void writeUInt32(uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, buffer, step, bytes.Length);
            this.step += 4;
        }


        public void writeByteArray(byte[] values)
        {
            if (values.Length >= 30000)
            {
                return;
            }
            if (longBinaryData)
            {
                this.writeInt32(values.Length);
                Buffer.BlockCopy(values, 0, buffer, this.step, values.Length);
                this.step += values.Length;
                return;
            }
            int b = values.Length;
#if DEBUG
            Logger.LogError("values.Length" + b.ToString());
#endif
            buffer[step] = (byte)b;
            step++;
            Buffer.BlockCopy(values, 0, buffer, step, b);
            step += b;
        }



        public bool readBoolean()
        {
            if (block != null && step <= block.Length - 1)
            {
                bool result = BitConverter.ToBoolean(block, step);
                step++;
                return result;
            }
            return false;
        }

        public void writeBoolean(bool value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            buffer[step] = bytes[0];
            step++;
        }


        public void reset()
        {
            step = 0;
            block = null;
        }

        private void reset(byte[] contents)
        {
            step = 0;
            block = contents;
        }

    }
}
