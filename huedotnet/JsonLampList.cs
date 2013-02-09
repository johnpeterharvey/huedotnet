using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace huedotnet
{
    public class JsonLampList
    {
        private HSVRGB hsvrgb = new HSVRGB();

        public Dictionary<int, JsonLamp> lights { get; set; }

        public Dictionary<int, HueLamp> ConvertToHueLamps()
        {
            Dictionary<int, HueLamp> lampSet = new Dictionary<int, HueLamp>();
            foreach (int i in lights.Keys)
            {
                lampSet.Add(i, ConvertToHueLamp(i));
            }

            return lampSet;
        }

        public HueLamp ConvertToHueLamp(int lampNumber)
        {
            HueLamp lamp = new HueLamp(lampNumber);

            JsonLamp jsonLamp = null;
            lights.TryGetValue(lampNumber, out jsonLamp);

            if (jsonLamp != null)
            {
                lamp.SetName(jsonLamp.name);
                lamp.SetState(jsonLamp.state.on);

                int r, g, b;
                hsvrgb.ConvertToRGB(jsonLamp.state.GetHueAsDegree(), jsonLamp.state.GetSaturation(), jsonLamp.state.GetBrightness(), out r, out g, out b);

                lamp.SetRGB(r, g, b);
            }

            return lamp;
        }
    }
}
