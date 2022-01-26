using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Universal_Toolkit.Types
{

    public struct PortInfo
    {
        public FtResult ftStatus;
        public IntPtr ftHandle;
        public FtChannelConfig SPI_Channel_Conf;
        public Ft_I2C_ChannelConfig I2C_Channel_Conf;
        public uint PortNum;
    }

    public enum FtResult
    {
        Ok = 0,
        InvalidHandle,
        DeviceNotFound,
        DeviceNotOpened,
        IoError,
        InsufficientResources,
        InvalidParameter,
        InvalidBaudRate,
    }
}
