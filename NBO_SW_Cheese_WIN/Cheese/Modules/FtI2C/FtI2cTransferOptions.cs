using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Universal_Toolkit.Types
{
    [Flags]
    public enum FtI2cTransferOptions : int
    {
        START_BIT = 0x00000001,
        STOP_BIT = 0x00000002,
        BREAK_ON_NACK = 0x00000004,
        NACK_LAST_BYTE = 0x00000008,
        FAST_TRANSFER_BYTES = 0x00000010,
        FAST_TRANSFER_BITS = 0x00000020,
        FAST_TRANSFER = 0x00000030,
        NO_ADDRESS = 0x00000040,
    }
    
}
