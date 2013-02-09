using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Drawing;

namespace huedotnet
{
    class HueLamp
    {
        private int lampNumber;
        private int? brightness = null;
        private bool? on = null;
        private int? r = null;
        private int? g = null;
        private int? b = null;
        private int? transitionTime = null;

        public HueLamp(int lampNumber)
        {
            this.lampNumber = lampNumber;
        }

        public void SetBrightness(int brightness)
        {
            this.brightness = brightness;
        }

        public void SetState(bool onoff)
        {
            this.on = onoff;
        }

        public void SetRGB(int r, int g, int b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public void SetTransitionTime(int timeMilli)
        {
            this.transitionTime = (int) Math.Round(timeMilli / 100d);
        }

        public int GetLampNumber()
        {
            return this.lampNumber;
        }

        public String GetJson()
        {
            ArrayList commands = new ArrayList();

            if (on != null) commands.Add("\"on\": " + on);
            if (brightness != null) commands.Add("\"bri\": " + brightness);
            if (transitionTime != null) commands.Add("\"transitiontime\": " + transitionTime);

            if (r != null && g != null && b != null) {
                Color newColor = Color.FromArgb((int) r, (int) g, (int) b);
                commands.Add("\"hue\": " + (int) newColor.GetHue());
                commands.Add("\"sat\": " + (int) newColor.GetSaturation());
            }

            return "{" + String.Join(", ", commands) + "}";
        }

    }
}
