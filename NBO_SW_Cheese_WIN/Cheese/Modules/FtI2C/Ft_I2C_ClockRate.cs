using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;
namespace Universal_Toolkit.Types
{

    [Flags]
    public enum Ft_I2C_ClockRate: int
    {

        I2C_CLOCK_STANDARD_MODE    = 100000,						/* 100kb/sec */
        I2C_CLOCK_FAST_MODE        = 400000,					    /* 400kb/sec */
        I2C_CLOCK_FAST_MODE_PLUS   = 1000000, 					/* 1000kb/sec */
        I2C_CLOCK_HIGH_SPEED_MODE  = 3400000, 					/* 3.4Mb/sec */
        YP  = 650000 					/* 3.4Mb/sec */
    }


}
