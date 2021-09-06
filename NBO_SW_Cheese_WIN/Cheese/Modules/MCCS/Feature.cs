using System;
using System.Collections.Generic;
//using YamlDotNet.RepresentationModel;


namespace ModuleLayer
{
    public abstract class Feature
    {
        public string Name { get; protected set; }
        public byte Code { get; protected set; }
        public TimeSpan Delay { get; protected set; }
        public string Description { get; protected set; }


        protected Feature(string name, byte code, float delaySeconds = 0.5f)
        {
            Name = name;
            Code = code;
            Delay = TimeSpan.FromSeconds(delaySeconds);
        }

        //public abstract bool TryParseValue(Dictionary<> key, out uint value);
        public abstract bool TryParseValue(string str, out uint value);

        public abstract string ValueName(uint value);

        //public abstract string YamlConfigTemplate();
    }

    public class FeatureRange : Feature
    {
        public uint From { get; private set; }
        public uint To { get; private set; }

        public FeatureRange(string name, byte code, uint from, uint to, float delaySeconds = 0, string description = null) : base(name, code, delaySeconds)
        {
            From = from;
            To = to;
            Description = description ?? $"{Name}: value beetween {From} and {To}";
        }

        public override bool TryParseValue(string str, out uint value)
        {
            bool res = false;
            if (uint.TryParse(str, out value))
            {
                if (value >= From || value <= To)
                    res = true;
            }

            return res;
        }

        public override string ValueName(uint value)
        {
            return value.ToString();
        }
    }

    public class Feature<T> : Feature
    {
        public Feature(string name, byte code, float delaySeconds = 0) : base(name, code, delaySeconds)
        {
            Description = $"{Name}: {string.Join(", ", Enum.GetNames(typeof(T)))}";
        }

        public override bool TryParseValue(string str, out uint value)
        {
            bool res = false;
            try
            {
                value = (uint)Enum.Parse(typeof(T), str, true);
            }
            catch
            {
                value = 999;
                res = false;
                return res;
            }

            if (Enum.IsDefined(typeof(T), value))
                res = true;

            return res;
        }

        public override string ValueName(uint value)
        {
            if (Enum.IsDefined(typeof(Enum), value))
                return (Enum.ToObject(typeof(Enum), value)).ToString();
            else
                return value.ToString();
        }
    }


    public enum StandardColor : uint
    {
        NotAvailable = 0,
        sRGB = 1,
        Adobe = 14,
        EBU = 15,
        SMPTEC = 16,
        REC709 = 17,
        DICOMSIM = 18,
        DCIP3 = 19,
        CAL1 = 21,
        CAL2 = 22,
        CAL3 = 23,
        iPhone = 24,
        Custom = 255,
    }


    public enum AudioInput : uint
    {
        NotAvailable = 0,
        DisplayPort = 15,
        HDMI1 = 17,
        HDMI2 = 18,
        MiniDisplayPort = 21,
        TypeC = 23,
        Auto = 241,
    }


    public enum LowInputLag : uint
    {
        NotAvailable = 0,
        Off = 1,
        Advanced = 2,
        UltraFast = 3,
    }


    public enum ResponceTime : uint
    {
        NotAvailable = 0,
        Standard = 1,
        Advanced = 2,
        UltraFast = 3,
    }


    public enum VideoInputAutodetect : uint
    {
        NotAvailable = 0,
        Off = 1,
        On = 2,
    }

    public enum VideoInput : uint   //@MCCS_2.2a Tbale8-13
    {
        NotAvailable = 0,
        SUB1 = 1,
        SUB2 = 2,
        DVI1 = 3,
        DVI2 = 4,
        DisplayPort1 = 15,
        DisplayPort2 = 16,
        HDMI1 = 17,
        HDMI2 = 18,
        //MiniDisplayPort = 21,
        //TypeC = 23,
    }


    public enum AmbientLightSensor : uint
    {
        NotAvailable = 0,
        On = 1,
        Off = 2,
    }


    public enum PresenceSensor : uint
    {
        Off = 0,
        Level1 = 1,
        Level2 = 2,
        Level3 = 3,
    }


    public enum AudioMute : uint
    {
        NotAvailable = 0,
        Mute = 1,
        Unmute = 2,
    }


    public enum DisplayApplication : uint
    {
        Off = 0,
        Movie = 3,
        FPS1 = 8,
        FPS2 = 49,
        RTS = 50,
        MODA = 51,
        Web = 52,
        Text = 53,
        MAC = 54,
        CADCAM = 55,
        Animation = 56,
        VideoEdit = 57,
        Retro = 58,
        Photo = 59,
        Landscape = 60,
        Portrait = 61,
        Monochrome = 62,
    }


    public enum MultiPicture : uint
    {
        NotAvailable = 0,
        Off = 1,
        PIP = 2,
        PBPLeftRight = 3,
        PBPTopBottom = 4,
        QuadWindows = 5,
    }


    public enum Uniformity : uint
    {
        NotAvailable = 0,
        Off = 1,
        On = 2,
    }
}
