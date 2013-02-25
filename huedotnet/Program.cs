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
            double brightness = 254;
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
                        if (selectedLamp.Equals("A"))
                        {
                            brightness = 254;
                        }
                        else
                        {
                            brightness = GetCurrentLampBrightness(Convert.ToInt16(selectedLamp));
                        }
                        break;
                    case "b":
                        brightness = showBrightnessMenu(brightness);
                        break;
                    case "r":
                        if (selectedLamp.Equals("A"))
                        {
                            ChangeAllLampState(new LampStateChange((HueLamp l) => l.brightness = brightness / 255.0));
                        }
                        else
                        {
                            ChangeLampState(Convert.ToInt16(selectedLamp), new LampStateChange((HueLamp l) => l.brightness = brightness / 255.0));
                        }
                        break;
                }

                drawManualMenu(selectedLamp, brightness);
            }
        }

        public static double GetCurrentLampBrightness(int lampNumber)
        {
            HueLamp lamp;
            lamps.TryGetValue(lampNumber, out lamp);

            if (lamp == null)
            {
                return 254;
            }
            else
            {
                return Math.Round(lamp.brightness * 255);
            }
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

        private static double showBrightnessMenu(double brightness)
        {
            drawBrightnessSelectMenu(brightness);

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
                        return brightness;
                    default:
                        if (String.IsNullOrEmpty(enteredText))
                        {
                            return brightness;
                        }
                        else
                        {
                            try {
                                return Convert.ToInt16(enteredText);
                            } catch {
                                return brightness;
                            }
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

        private static void drawManualMenu(String lampNumber, double brightness)
        {
            Console.Clear();
            Console.WriteLine("\n\n");
            Console.WriteLine("\t[Manual Mode]\n");
            Console.WriteLine("\tLamp [" + lampNumber + "]");
            Console.WriteLine("\tBrightness [" + (lampNumber.Equals("A") ? "---" : brightness.ToString()) + "]");
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
                Console.WriteLine("\t" + lampPair.Key + " - " + lampPair.Value.name);
            }
            Console.WriteLine();
            Console.WriteLine("\teXit");
        }

        private static void drawBrightnessSelectMenu(double currentBrightness)
        {
            Console.Clear();
            Console.WriteLine("\n\n");
            Console.WriteLine("\t[Brightness]\n");
            Console.WriteLine("\t0 - 254\n");
            Console.WriteLine("\tCurrent value: " + currentBrightness.ToString() + "\n");

            int current = (int) Math.Floor((currentBrightness * (Console.WindowWidth - 20)) / 254);
            int left = Console.WindowWidth - 20 - current;

            Console.WriteLine("\t[" + new String('-', current) + new String(' ', left) + "]\n");

            Console.WriteLine();
            Console.WriteLine("\teXit");
        }

        private static void AllOn()
        {
            ChangeAllLampState(new LampStateChange((HueLamp l) => l.state = true));
        }

        private static void AllOff()
        {
            ChangeAllLampState(new LampStateChange((HueLamp l) => l.state = false));
        }

        private delegate void LampStateChange(HueLamp lamp);

        private static void ChangeAllLampState(Delegate stateChange)
        {
            foreach (HueLamp lamp in lamps.Values)
            {
                stateChange.DynamicInvoke(lamp);
            }
            messaging.SendMessage(lamps.Values.ToList<HueLamp>());
        }

        private static void ChangeLampState(int lampNumber, Delegate stateChange)
        {
            HueLamp lamp;
            lamps.TryGetValue(lampNumber, out lamp);

            if (lamp == null)
            {
                Console.WriteLine("Didn't find lamp for number " + lampNumber);
                return;
            }

            stateChange.DynamicInvoke(lamp);

            messaging.SendMessage(lamp);
        }
    }
}
