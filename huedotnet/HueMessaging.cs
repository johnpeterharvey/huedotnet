using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading;

namespace huedotnet
{
    class HueMessaging
    {
        private String bridgeIP;
        private String username;
        
        public HueMessaging(string bridgeIP, string username)
        {
            this.bridgeIP = bridgeIP;
            this.username = username;
        }

        public void SendMessage(HueLamp lampState)
        {
            WebClient webClient = new WebClient();
            webClient.BaseAddress = "http://" + bridgeIP + "/api/" + username + "/lights/" + lampState.GetLampNumber() + "/state";
            String json = lampState.GetJson();

            Stream writeData = webClient.OpenWrite(webClient.BaseAddress, "PUT");
            writeData.Write(Encoding.ASCII.GetBytes(lampState.GetJson()), 0, lampState.GetJson().Length);
            writeData.Close();
        }

        private void SendMessage(object lampState)
        {
            HueLamp lamp = (HueLamp)lampState;
            SendMessage(lamp);
        }

        public void SendMessage(List<HueLamp> lampStates)
        {
            foreach (HueLamp lamp in lampStates)
            {
                new Thread(new ParameterizedThreadStart(SendMessage)).Start(lamp);
            }
        }

        public String DownloadState()
        {
            WebClient webClient = new WebClient();
            webClient.BaseAddress = "http://" + bridgeIP + "/api/" + username + "/";
            return webClient.DownloadString(webClient.BaseAddress);
        }

    }
}
