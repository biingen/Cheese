using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Universal_Toolkit.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Ft_I2C_ChannelConfig
    {
        public Ft_I2C_ClockRate ClockRate;
        public uint LatencyTimer;
        public UInt32 Options;
    }
}
