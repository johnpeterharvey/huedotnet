﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace huedotnet
{
    class Program
    {
        private static String bridgeIP;
        private static String username;
        private static WebClient webClient;
        private static Dictionary<int, HueLamp> lamps;

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;
            
            bool loadConfigSuccess = loadConfig();
            if (!loadConfigSuccess)
            {
                Console.WriteLine("Failed to load config!");
                Console.ReadLine();
                return;
            }

            getWebClient();

            getLampList();


            foreach (int i in lamps.Keys)
            {
                HueLamp l;
                lamps.TryGetValue(i, out l);
                Console.WriteLine("Lamp [" + i + "] on [" + l.GetState() + "] name [" + l.GetName() + "] rgb [" + l.GetR() + ", " + l.GetG() + ", " + l.GetB() + "]");
            }

            Console.ReadLine();
            showMainMenu();
        }

        private static void getLampList()   
        {
            String jsonState = webClient.DownloadString(webClient.BaseAddress);

            JsonLampList lampList = JsonConvert.DeserializeObject<JsonLampList>(jsonState);
            lamps = lampList.ConvertToHueLamps();
        }

        private static bool loadConfig()
        {
            XDocument doc = XDocument.Load("Settings.xml");

            var data = from item in doc.Descendants("settings")
                       select new
                       {
                           bridge = item.Element("bridgeip").Value,
                           user = item.Element("username").Value
                       };

            foreach (var val in data)
            {
                bridgeIP = val.bridge;
                username = val.user;
                break;
            }

            Regex ipRegex = new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}");

            bool success = (bridgeIP != null && ipRegex.IsMatch(bridgeIP) && !String.IsNullOrWhiteSpace(username));

            Console.WriteLine("Load config returned bridge ip [" + bridgeIP + "] and username [" + username + "] and return code [" + success + "]");
            return success;
        }

        private static void getWebClient()
        {
            webClient = new WebClient();
            webClient.BaseAddress = "http://" + bridgeIP + "/api/" + username + "/";
            Console.WriteLine("Creating a web client with base address [" + webClient.BaseAddress + "]");
        }

        private static void showMainMenu()
        {
            drawMainMenu();

            while (true)
            {
                ConsoleKeyInfo enteredText = Console.ReadKey();
                while (!new String[] { "m", "p", "x", "a", "o" }.Contains(enteredText.KeyChar.ToString().ToLower()))
                {
                    enteredText = Console.ReadKey();
                }

                switch (enteredText.KeyChar.ToString().ToLower())
                {
                    case "x":
                        return;
                    case "a":
                        AllOn();
                        break;
                    case "o":
                        AllOff();
                        break;
                    case "m":
                        showManualMenu();
                        break;
                    case "p":
                        showPresetMenu();
                        break;
                }

                drawMainMenu();
            }
        }

        private static void showManualMenu()
        {
            drawManualMenu();

            while (true)
            {
                ConsoleKeyInfo enteredText = Console.ReadKey();
                while (!new String[] { "l", "b", "c", "x" }.Contains(enteredText.KeyChar.ToString().ToLower()))
                {
                    enteredText = Console.ReadKey();
                }

                switch (enteredText.KeyChar.ToString().ToLower())
                {
                    case "x":
                        return;
                    case "l":
                        showLampSelectionMenu();
                        break;
                    case "b":
                        showBrightnessMenu();
                        break;
                }
            }
        }

        private static void showLampSelectionMenu()
        {

        }

        private static void showBrightnessMenu()
        {

        }

        private static void showPresetMenu()
        {

        }

        private static void drawMainMenu()
        {
            Console.Clear();
            Console.WriteLine("\n\n");
            Console.WriteLine("\t[Main Menu]\n");
            Console.WriteLine("\tAll on");
            Console.WriteLine("\tall Off");
            Console.WriteLine("\tManual");
            Console.WriteLine("\tPresets");
            Console.WriteLine("\teXit");
        }

        private static void drawManualMenu()
        {
            Console.Clear();
            Console.WriteLine("\n\n");
            Console.WriteLine("\t[Manual Mode]\n");
            Console.WriteLine("\tLamp [0]");
            Console.WriteLine("\tBrightness [ ]");
            Console.WriteLine("\tColor [ ]");
            Console.WriteLine("\teXit");
        }

        private static void drawLampSelectMenu()
        {
            Console.Clear();
            Console.WriteLine("\n\n");
            Console.WriteLine("\t[Lamp Selection]\n");
            Console.WriteLine("\tLamp [0]");
            Console.WriteLine("\tBrightness [ ]");
            Console.WriteLine("\tColor [ ]");
            Console.WriteLine("\teXit");
        }


        private static void AllOn()
        {
            foreach (int i in lamps.Keys)
            {
                HueLamp l;
                lamps.TryGetValue(i, out l);
                l.SetState(true);
                Stream writeData = webClient.OpenWrite(webClient.BaseAddress + "lights/" + i + "/state", "PUT");
                writeData.Write(Encoding.ASCII.GetBytes(l.GetJson()), 0, l.GetJson().Length);
                writeData.Close();
            }
        }

        private static void AllOff()
        {
            foreach (int i in lamps.Keys)
            {
                HueLamp l;
                lamps.TryGetValue(i, out l);
                l.SetState(false);
                Stream writeData = webClient.OpenWrite(webClient.BaseAddress + "lights/" + i + "/state", "PUT");
                writeData.Write(Encoding.ASCII.GetBytes(l.GetJson()), 0, l.GetJson().Length);
                writeData.Close();
            }
        }
    }
}
