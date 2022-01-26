using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Universal_Toolkit.Types
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct FtDeviceInfo
    {
        public int Flags;
        public int Type;
        public int ID;
        public int LocId;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string SerialNumber;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]

        public string Description;
        public IntPtr ftHandle;
    }
}
