using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace huedotnet
{
    class HueMessaging
    {
        private static WebClient webClient;
        
        public HueMessaging(string bridgeIP, string username)
        {
            webClient = new WebClient();
            webClient.BaseAddress = "http://" + bridgeIP + "/api/" + username + "/";
            Console.WriteLine("Creating a web client with base address [" + webClient.BaseAddress + "]");
        }

        public void SendMessage(HueLamp lampState)
        {
            Stream writeData = webClient.OpenWrite(webClient.BaseAddress + "lights/" + lampState.GetLampNumber() + "/state", "PUT");
            writeData.Write(Encoding.ASCII.GetBytes(lampState.GetJson()), 0, lampState.GetJson().Length);
            writeData.Close();
        }

        public void SendMessage(List<HueLamp> lampStates)
        {

        }

        public String DownloadState()
        {
            return webClient.DownloadString(webClient.BaseAddress);
        }

    }
}
