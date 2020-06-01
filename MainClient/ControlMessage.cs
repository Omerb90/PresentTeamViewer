using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace MainClient
{
    class ControlMessage
    {
        public const int LeftDown = 1;
        public const int LeftUp = 2;
        public const int LeftClick = 3;
        public int clickFlag = 0;
        public const int RightDown = 4;
        public const int RightUp = 5;
        public const int RightClick = 6;
        public double MouseX;
        public double MouseY;
        public byte KeyCode;
        public int KeyBoardFlag;
        public const int KeyUp = 1;
        public const int KeyDown = 2;
        //bool MouseClick;

        public ControlMessage()
        {

        }
        public ControlMessage(double MouseX, double MouseY)
        {
            this.MouseX = MouseX;
            this.MouseY = MouseY;
        }
        public byte[] ToBytes()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter bw = new BinaryWriter(ms);
                bw.Write(this.clickFlag);
                bw.Write(this.MouseX);
                bw.Write(this.MouseY);
                bw.Write(this.KeyCode);
                bw.Write(this.KeyBoardFlag);
                return ms.ToArray();
            }
        }

        public static ControlMessage FromBytes(byte[] buffer)
        {
            ControlMessage retVal = new ControlMessage();

            using (MemoryStream ms = new MemoryStream(buffer))
            {
                BinaryReader br = new BinaryReader(ms);
                retVal.clickFlag = br.ReadInt32();
                retVal.MouseX = br.ReadDouble();
                retVal.MouseY = br.ReadDouble();
                retVal.KeyCode = br.ReadByte();
                retVal.KeyBoardFlag = br.ReadInt32();
            }

            return retVal;
        }
    }
}