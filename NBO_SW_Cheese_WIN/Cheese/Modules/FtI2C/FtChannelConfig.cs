using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
namespace Universal_Toolkit.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FtChannelConfig
    {
        public int ClockRate;
        public byte LatencyTimer;
        public FtConfigOptions configOptions;
        public int Pin;
        public short reserved;
    }
}
