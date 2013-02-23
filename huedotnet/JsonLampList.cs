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
            JsonLamp jsonLamp = null;
            lights.TryGetValue(lampNumber, out jsonLamp);

            if (jsonLamp != null)
            {
                HueLamp lamp = new HueLamp(lampNumber, jsonLamp.name, jsonLamp.state.on, jsonLamp.state.GetHueAsDegree(), jsonLamp.state.GetSaturation(), jsonLamp.state.GetBrightness());
                return lamp;
            }

            return null;
        }
    }
}
