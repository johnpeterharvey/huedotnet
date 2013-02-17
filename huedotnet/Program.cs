using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using NDesk.Options;

namespace huedotnet
{
    class Program
    {
        private static String bridgeIP;
        private static String username;
        private static HueMessaging messaging;
        private static Dictionary<int, HueLamp> lamps;

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;
            
            bool loadConfigSuccess = LoadConfig();
            if (!loadConfigSuccess)
            {
                Console.WriteLine("Failed to load config!");
                Console.ReadLine();
                return;
            }

            getWebClient();

            getLampList();

            if (args.Length == 0)
            {
                showMainMenu();
            }
            else
            {
                ProcessArguments(args);
            }
        }

        private static void ProcessArguments(string[] arguments)
        {
            bool allOn = false;
            bool allOff = false;

            bool help = false;

            OptionSet options = new OptionSet() {
                {"a|all|on", "Turn all hue lamps on", v => allOn = true},
                {"o|off", "Turn all hue lamps off", v => allOff = true},
                {"h|help|?", "Help", v => help = true}
            };

            List<String> leftOver = options.Parse(arguments);
            if (leftOver.Count > 0)
            {
                help = true;
            }

            if (help)
            {
                Console.WriteLine("\nHue.net Parameters:\n\n");
                Console.WriteLine("No parameters starts app in interactive mode\n\n");
                options.WriteOptionDescriptions(Console.Out);
            }
            else if (allOn)
            {
                AllOn();
            }
            else if (allOff)
            {
                AllOff();
            }
        }

        private static void getLampList()   
        {
            JsonLampList lampList = JsonConvert.DeserializeObject<JsonLampList>(messaging.DownloadState());
            lamps = lampList.ConvertToHueLamps();
        }

        private static bool LoadConfig()
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

            //Console.WriteLine("Load config returned bridge ip [" + bridgeIP + "] and username [" + username + "] and return code [" + success + "]");
            return success;
        }

        private static void getWebClient()
        {
            messaging = new HueMessaging(bridgeIP, username);
        }

        private static void showMainMenu()
        {
            drawMainMenu();

            while (true)
            {
                ConsoleKeyInfo enteredText = Console.ReadKey();
                while (!new String[] { "m", "p", "x", "q", "a", "o" }.Contains(enteredText.KeyChar.ToString().ToLower()))
                {
                    enteredText = Console.ReadKey();
                }

                switch (enteredText.KeyChar.ToString().ToLower())
                {
                    case "x":
                    case "q":
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
            String selectedLamp = "A";
            int? brightness = null;
            drawManualMenu(selectedLamp, brightness);

            while (true)
            {
                ConsoleKeyInfo enteredText = Console.ReadKey();
                while (!new String[] { "l", "b", "c", "x", "q", "r" }.Contains(enteredText.KeyChar.ToString().ToLower()))
                {
                    enteredText = Console.ReadKey();
                }

                switch (enteredText.KeyChar.ToString().ToLower())
                {
                    case "x":
                    case "q":
                        return;
                    case "l":
                        selectedLamp = showLampSelectionMenu();
                        break;
                    case "b":
                        brightness = showBrightnessMenu();
                        break;
                    case "r":
                        if (selectedLamp.Equals("A"))
                        {

                        }
                        break;
                }

                drawManualMenu(selectedLamp, brightness);
            }
        }

        private void UpdateLampManual(int lampNumber, int? brightness)
        {
            HueLamp lamp = null;
            lamps.TryGetValue(lampNumber, out lamp);
            if (lamp == null)
                return;

            //if (brightness != null)
            //    lamp.
        }

        private static String showLampSelectionMenu()
        {
            drawLampSelectMenu();

            while (true)
            {
                Regex validLamps = new Regex("(a|(0-9)*|x|q)");
                ConsoleKeyInfo enteredText = Console.ReadKey();
                while (!validLamps.IsMatch(enteredText.KeyChar.ToString()))
                {
                    enteredText = Console.ReadKey();
                }

                switch (enteredText.KeyChar.ToString().ToLower())
                {
                    case "x":
                    case "q":
                    case "a":
                        return "A";
                    default:
                        return enteredText.KeyChar.ToString();
                }
            }
        }

        private static int? showBrightnessMenu()
        {
            drawBrightnessSelectMenu();

            while (true)
            {
                Regex validNumbers = new Regex("(0-9)*");
                String enteredText = Console.ReadLine();

                while (!new String[] { "q", "x" }.Contains(enteredText) && !validNumbers.IsMatch(enteredText))
                {
                    enteredText = Console.ReadLine();
                }
                
                switch (enteredText)
                {
                    case "x":
                    case "q":
                        return null;
                    default:
                        if (String.IsNullOrEmpty(enteredText))
                        {
                            return null;
                        }
                        else
                        {
                            return Convert.ToInt16(enteredText);
                        }
                }
            }
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

        private static void drawManualMenu(String lampNumber, int? brightness)
        {
            Console.Clear();
            Console.WriteLine("\n\n");
            Console.WriteLine("\t[Manual Mode]\n");
            Console.WriteLine("\tLamp [" + lampNumber + "]");
            Console.WriteLine("\tBrightness [" + (brightness == null ? " " : brightness.ToString()) + "]");
            Console.WriteLine("\tColor [ ]");
            Console.WriteLine("\tRun");
            Console.WriteLine("\teXit");
        }

        private static void drawLampSelectMenu()
        {
            Console.Clear();
            Console.WriteLine("\n\n");
            Console.WriteLine("\t[Lamp Selection]\n");
            Console.WriteLine("\tA - All");
            foreach (KeyValuePair<int, HueLamp> lampPair in lamps)
            {
                Console.WriteLine("\t" + lampPair.Key + " - " + lampPair.Value.GetName());
            }
            Console.WriteLine();
            Console.WriteLine("\teXit");
        }

        private static void drawBrightnessSelectMenu()
        {
            Console.Clear();
            Console.WriteLine("\n\n");
            Console.WriteLine("\t[Brightness]\n");
            Console.WriteLine("\t0 - 255");
            Console.WriteLine();
            Console.WriteLine("\teXit");
        }

        private static void AllOn()
        {
            AllLamps(new AllLampsStateChange((HueLamp l) => l.SetState(true)));
        }

        private static void AllOff()
        {
            AllLamps(new AllLampsStateChange((HueLamp l) => l.SetState(false)));
        }

        private delegate void AllLampsStateChange(HueLamp lamp);

        private static void AllLamps(Delegate stateChange)
        {
            foreach (HueLamp lamp in lamps.Values)
            {
                stateChange.DynamicInvoke(lamp);
            }
            messaging.SendMessage(lamps.Values.ToList<HueLamp>());
        }
    }
}
