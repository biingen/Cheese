using System;
using System.Reflection;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using ModuleLayer;
using log4net;


namespace Cheese
{
    public class MonitorControl
    {
        public string Name { get; set; }
        public Capabilities Capabilities { get; set; }
        public SafePhysicalMonitorHandle Handle { get; set; }
        public static NativeMethods.MC_VCP_CODE_TYPE type;
        public static uint currentVal, maxVal;
        private static ILog log = GlobalData.Log;        //log4net

        public static List<MonitorControl> GetMonitors()
        {
            var result = new List<MonitorControl>();

            var windowHandler = NativeMethods.GetDesktopWindow();
            var monitorHandler = NativeMethods.MonitorFromWindow(windowHandler, NativeMethods.MONITOR_DEFAULT.MONITOR_DEFAULTTOPRIMARY);

            uint physicalMonitorCount;
            if (!NativeMethods.GetNumberOfPhysicalMonitorsFromHMONITOR(monitorHandler, out physicalMonitorCount))
                throw new InvalidOperationException($"{nameof(NativeMethods.GetNumberOfPhysicalMonitorsFromHMONITOR)} returned error 0x{Marshal.GetLastWin32Error():X8}");

            var physicalMonitors = new NativeMethods.PHYSICAL_MONITOR[physicalMonitorCount];
            if (!NativeMethods.GetPhysicalMonitorsFromHMONITOR(monitorHandler, physicalMonitorCount, physicalMonitors))
                throw new InvalidOperationException($"{nameof(NativeMethods.GetPhysicalMonitorsFromHMONITOR)} returned error 0x{Marshal.GetLastWin32Error():X8}");

            foreach (var physicalMonitor in physicalMonitors)
                result.Add(new MonitorControl
                {
                    Name = physicalMonitor.szPhysicalMonitorDescription,
                    Handle = new SafePhysicalMonitorHandle(physicalMonitor.hPhysicalMonitor)
                });

            return result;
        }


        public static List<MonitorControl> GetMonitorsAndFeatures()
        //public static Capabilities GetMonitorsAndFeatures(MonitorControl monCtrl)
        {
            //var result = new Capabilities();
            var result = new List<MonitorControl>();
            foreach (var monitor in GetMonitors())
            {
                uint length;

                if (!NativeMethods.GetCapabilitiesStringLength(monitor.Handle, out length))
                {
                    //throw new InvalidOperationException($"{nameof(NativeMethods.GetCapabilitiesStringLength)} returned error 0x{Marshal.GetLastWin32Error():X8}");
                    Console.WriteLine("[GetCapabilitiesStringLength] One of the monitors does not support DDC/CI!!!");
                }
                //string capString = "(prot(monitor)type(lcd)model(ACER XZ322QU S)cmds(01 02 03 07 0C 4E F3 E3)vcp(02 04 05 08 0B 0C 10 12 14(01 04 05 06 07 08 0A 0B) 16 18 1A 6C 6E 70 AC AE B6 C0 C6 C8 C9 CA CC(00 02 03 04 05 09 0A 0C 16 1E) D6(01 04) DC(00 01 02 03 04) DF 60(0F1112) 62 8D E0(00 01 02 03 04 05 06) E1(00 01 02 03 04 05 06) E2(COLOR_USER COLOR_STANDARD COLOR_ECO COLOR_GRAPHICS COLOR_GAME_ACTION_sRGB COLOR_GAME_RACING_READING COLOR_GAME_SPORTS_DARKROOM COLOR_HDR  VCP_E2h_Save_Game1_Action VCP_E2h_Save_Game2_Racing VCP_E2h_Save_Game3_Sports) E3 E4 E5 E7(00 01 02(00 01 02 03 04)) E8(00 01 02(00 01 02 03 04) 03 04))FF)mswhql(1)mccs_ver(2.0)asset_eep(32)mpu_ver(01))";
                //string capString = "(prot(monitor)type(lcd)model(ACER XZ322QU S)cmds(01 02 03 07 0C 4E F3 E3)vcp(02 04 05 08 0B 0C 10 12 14(01 04 05 06 07 08 0A 0B) 16 18 1A 6C 6E 70 AC AE B6 C0 C6 C8 C9 CACC(0002030405090A0C161E) D6(01 04) DC(00 01 02 03 04) DF 60(0F1112) 62 8D E0(00 01 02 03 04 05 06) E1(00 01 02 03 04 05 06) E2(COLOR_USER COLOR_STANDARD COLOR_ECO COLOR_GRAPHICS COLOR_GAME_ACTION_sRGB COLOR_GAME_RACING_READING COLOR_GAME_SPORTS_DARKROOM COLOR_HDR  VCP_E2h_Save_Game1_Action VCP_E2h_Save_Game2_Racing VCP_E2h_Save_Game3_Sports) E3 E4 E5 E7(00 01 02(00 01 02 03 04)) E8(00 01 02(00 01 02 03 04) 03 04))FF)mswhql(1)mccs_ver(2.0)asset_eep(32)mpu_ver(01))";
                //length = (uint)capString.Count<char>() + 1;
                var capabilitiesString = new StringBuilder((int)length);
                
                if (!NativeMethods.CapabilitiesRequestAndCapabilitiesReply(monitor.Handle, capabilitiesString, length))
                {
                    //throw new InvalidOperationException($"{nameof(NativeMethods.CapabilitiesRequestAndCapabilitiesReply)} returned error 0x{Marshal.GetLastWin32Error():X8}");
                    Console.WriteLine("[GetCapabilitiesStringLength] One of the monitors does not support DDC/CI!!!");
                }
                monitor.Capabilities = Capabilities.Parse(capabilitiesString.ToString());
                GetVCPFeatures(monitor.Handle, monitor.Capabilities);
                //AddHiddenFeatures(monitor);

                result.Add(monitor);
            }

            return result;
        }


        private static void GetVCPFeatures(SafePhysicalMonitorHandle handle, Capabilities capabilities)
        {
            foreach (var vcpCode in capabilities.VCPCodes)
            {
                if (NativeMethods.GetVCPFeatureAndVCPFeatureReply(handle, vcpCode.Key, out type, out currentVal, out maxVal))
                {
                    vcpCode.Value.Type = type;
                    vcpCode.Value.MaximumValue = maxVal;
                    vcpCode.Value.CurrentValue = currentVal;
                }
                else
                {
                    vcpCode.Value.Error = true;
                }
            }
        }


        private static void AddHiddenFeatures(MonitorControl monitor)
        {
            var сapabilities = monitor.Capabilities;

            var candidates = Enumerable
                .Range(1, 255)
                .Select(x => (byte)x)
                .Where(x => !сapabilities.Commands.Contains(x) && !сapabilities.VCPCodes.Keys.Contains(x));

            foreach (var candidate in candidates)
                if (NativeMethods.GetVCPFeatureAndVCPFeatureReply(monitor.Handle, candidate, out type, out currentVal, out maxVal))
                    сapabilities.VCPCodes.Add(candidate, new Capabilities.VCPCode
                    {
                        Type = type,
                        MaximumValue = maxVal,
                        CurrentValue = currentVal,
                    });
        }
    }


    public class MonitorFeatures
    {
        public List<Feature> features = new List<Feature>
        {
            new FeatureRange("Brightness", 0x10, 0, 100, description: "Luminance: value beetween 0 and 100 where 0 is the minimal luminance, 100 is the maximal luminance"),
            new FeatureRange("Contrast", 0x12, 0, 100, description: "Contrast: value beetween 0 and 100 where 0 is the minimal contrast, 100 is the maximal contrast"),
            new FeatureRange("BacklightControl", 0x13, 0, 100, description: "Backlight Control, R/W, C"),
            new Feature<StandardColor>("StandardColor", 0x14),  //"Select Color Preset, R/W, NC"//
            new FeatureRange("VideoGain_Red", 0x16, 0, 255, description: "Video Gain (Drive): Red, R/W, C"),
            new FeatureRange("VideoGain_Green", 0x18, 0, 255, description: "Video Gain (Drive): Green, R/W, C"),
            new FeatureRange("VideoGain_Blue", 0x1A, 0, 255, description: "Video Gain (Drive): Blue, R/W, C"),
            new Feature<AudioInput>("AudioInput", 0x1D, delaySeconds: 4),
            new Feature<LowInputLag>("LowInputLag", 0x23),
            new Feature<ResponceTime>("ResponceTime", 0x25),
            new Feature<VideoInputAutodetect>("VideoInputAutodetect", 0x33, delaySeconds: 4),
            new Feature<VideoInput>("VideoInput", 0x60, delaySeconds: 4),
            new FeatureRange("Volume", 0x62, 0, 100, description: "Volume: value beetween 0 and 100 where 0 is the minimal volume, 100 is the maximal volume"),
            new Feature<AmbientLightSensor>("AmbientLightSensor", 0x66),
            new Feature<PresenceSensor>("PresenceSensor", 0x67),
            new Feature<AudioMute>("AudioMute", 0x8D),
            //new FeaturePIPPosition("PIPPosition", 0x96),
            new FeatureRange("PIPSize", 0x97, 0, 10, description: "PIPSize: value beetween 0 and 10 where 0 is the minimal size, 10 is the maximal size"),
            new Feature<DisplayApplication>("DisplayApplication", 0xDC),
            new Feature<MultiPicture>("MultiPicture", 0xE8, delaySeconds: 4),
            new Feature<Uniformity>("Uniformity", 0xE9),
        };


        //public static bool TryParse(YamlNode keyNode, YamlNode valueNode, out Feature feature, out uint value)
        
        public bool TryParse(string featureName, string valueToBeSet, out Feature feature, out uint value)
        {
            value = 0;
            uint tempValue = uint.Parse(valueToBeSet);
            bool res = false;
            if ((feature = features.Find(_feature => _feature.Name == featureName)) != null)
            {
                if (feature.TryParseValue(featureName, out tempValue))
                    res = true;
            }
            else
            {
                featureName = feature.ValueName(feature.Code);
                value = tempValue;
                res = false;
            }
            
            return res;
        }

        /*
        public static string YamlConfigTemplate()
        {
            return string.Join("\r\n\r\n", features.Select(feature => feature.YamlConfigTemplate()));
        }
        */
    }
}
