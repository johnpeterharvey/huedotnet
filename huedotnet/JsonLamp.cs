using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace huedotnet
{
    public class JsonLamp
    {
        public String name { get; set; }
        public CurrentState state { get; set; }
    }

    public class CurrentState
    {
        public int bri { get; set; }
        public int hue { get; set; }
        public bool on { get; set; }
        public bool reachable { get; set; }
        public int sat { get; set; }

        public double GetHueAsDegree()
        {
            return (hue * 360.0) / UInt16.MaxValue;
        }

        public double GetSaturation()
        {
            return sat / 256.0;
        }

        public double GetBrightness()
        {
            return bri / 256.0;
        }
    }
}
