using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace MainClient
{
    class KeyBoardControl
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        // Declare some keyboard keys as constants with its respective code
        // See Virtual Code Keys: https://msdn.microsoft.com/en-us/library/dd375731(v=vs.85).aspx
        public const int KEYEVENTF_EXTENDEDKEY = 0x0001; //Key down flag
        public const int KEYEVENTF_KEYUP = 0x0002; //Key up flag
        public const byte VK_RCONTROL = (byte)Keys.RControlKey; //Right Control key code

        // Simulate a key press event

        public static void KeyDown(byte KeyCode)
        {
            keybd_event(KeyCode, 0, KEYEVENTF_EXTENDEDKEY, 0);
        }
        public static void KeyUp(byte KeyCode)
        {
            keybd_event (KeyCode, 0, KEYEVENTF_KEYUP, 0);
        }

    }
}
