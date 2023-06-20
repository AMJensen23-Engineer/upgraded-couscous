using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPS9266xEvaluationModule
{
    public class SPIcommadReturnData
    {
        public string info { get; set; }
        public string returnData { get; set; }
        public UInt16 assembledReturn { get; set; }
        public string statusMessage { get; set; }
    }

    class Globals
    {
        // constants
        public enum Cmds : byte
        {
            GET_FW_VER = 0x00,
            ENABLE_DEVICE = 0x01,
            //GET_EN = 0x02,
            PWM1_F_ADJ = 0x03,
            PWM1_DC_ADJ = 0x04,
            //GET_PWM1 = 0x05,
            PWM2_F_ADJ = 0x06,
            PWM2_DC_ADJ = 0x07,
            //GET_PWM2 = 0x08,
            VREG_ADJ = 0x09,
            //GET_VREG = 0x0A,
            SYNC_PWMS = 0x0B,
            //GET_SYNC_STATE = 0x0C,
            SET_SPI_F = 0x0D,
            GET_SPI_F = 0x0E,
            SPI_CMD_518 = 0x20,
            SPI_CMD_682 = 0x21,
            SYNC_CLOCK_EN_682 = 0x22,
            SYNC_CLOCK_ADJ_682 = 0x23,
            EN_520_NO_WATCHDOG = 0x29,
            EN_520_W_WATCHDOG = 0x30,
            SPI_CLEAR_READ_BUFFER = 0x31,
            VREG_EN = 0x32,
            SET_WATCHDOG_TIME = 0x33,
            SET_DEVICE_520_W_WATCHDOG = 0x34,
            START_WATCHDOG_TIMER = 0x35,
            STOP_WATCHDOG_TIMER = 0x36
        }

        public static readonly string[] EVMS = {
            "TPS92518 - PWR878",
            "TPS92520 - EVM133 / PSIL-133",
            "TPS92520, TPS92682 - LPP074-E1",
            "TPS92520, TPS92682, TPS9266X - LPP074-A",
            "TPS92682 CV - LPP111/PSIL069",
            "TPS92682 CC - LPP112/PSIL070",
            "TPS92682 CV 4 Phase BOOST - LPP124",
            "Custom"
        };

        public const String swVersion = "2.32";
        public const String enablePinOld = "PG0";
        public const String enablePinNew = "PL4";
        public const double MIN_FW_VERSION = 3.1;
        public const double MIN_DLL_VERSION = 2.1;
        public const int RX_TIMEOUT = 500;
        public const byte PASS = 0x00;
        public const byte FETCH_FW_ERROR = 0x01;
        public const byte UPGRADE_AVAILABLE = 0x02;
        public const byte FETCH_PRO_ERROR = 0x03;

        private const byte LMMBUFDEPTH = 40;

        // variables
        public static String enablePin;
        public static byte userNumDevices;
        public static byte userSelectedEVM;
        public static string fwVersion = null;
        public static string dllVersion = null;
        public static byte[] uartBuf = new byte[LMMBUFDEPTH];
        public static bool updateMode = false;
        public static bool bothGenDevicesPresent = false;
        public static UInt32 baud_rate = 1000000;
        public static byte processor = 0;
        public static bool watchdogActive = false;

        // objects
        public static MCUcommands mcuCommand;
    }
}
