using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MixerTestsaBot {
    class ChatBot {
        private Dictionary<string, List<string>> Brain = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> Quotes = new Dictionary<string, List<string>>();
        private string Uri;
        private string BrainFile = "brain.txt";
        private string QuoteFile = "quote.txt";
        private Random Rng = new Random();
        private const int MAXGEN = 450;

        public ChatBot() {
            Uri = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Brain = Load(BrainFile);
            Quotes = Load(QuoteFile);
        }

        public string? parseInput(string message, string user) {
            string? output = null;
            if (message.StartsWith("!")) {
                if (message.ToLower().StartsWith("!quote")) {
                    output = ParseQuoteCommand(message);
                }
            }
            else {
                UpdateBrain(Regex.Replace(message, Settings.Default.BotName, "{user}", RegexOptions.IgnoreCase));
                if (message.ToLower().Contains(Settings.Default.BotName)) {
                    output = OutputBrain().Replace("{user}", user);
                }
            }
            return output;
        }

        #region quotes

        private string? ParseQuoteCommand(string message) {
            string? output = null;
            var info = message.Split(' ');
            if (info.Count() == 1) {
                output = GetQuote();
            }
            else if (info.Count() == 2) {
                output = GetQuote(info[1]);
            }
            else {
                SaveQuote(message);
            }
            return output;
        }

        private string? GetQuote(string? user = null) {
            var users = Quotes.Select(x => x.Key).ToList();
            if (user == null) {
                user = users[Rng.Next(users.Count())];
            }
            user = user.ToLower();
            if (Quotes.Count(x => x.Key == user) == 0) {
                return "No quotes for that user";
            }
            var quotes = Quotes.First(x => x.Key == user).Value;
            return user + ": " + quotes[Rng.Next(quotes.Count())];
        }

        private void SaveQuote(string message) {
            var info = message.Split(' ');
            var user = info[1].ToLower();
            info[0] = "";
            info[1] = "";
            var final = string.Join("", info).TrimStart(' ').TrimEnd(' ');
            Quotes = addToInputs(user, final, Quotes);
            Save(QuoteFile, Quotes);
        }

        #endregion

        #region brain
        private void UpdateBrain(string message) {

            var inputSplit = message.Split(' ');
            for (int i = 0; i < (inputSplit.Count() - 2); i++) {
                var key = inputSplit[i] + ' ' + inputSplit[i + 1];
                var value = inputSplit[i + 2];
                Brain = addToInputs(key, value, Brain);
            }
            Save(BrainFile, Brain);
        }

        private string OutputBrain() {
            var key = Rng.Next(Brain.Count);
            string returnString = Brain.ElementAt(key).Key;
            string firstKey = returnString.Split(' ')[0];
            string secondKey = returnString.Split(' ')[1];
            while (true) {
                var temp = secondKey;
                secondKey = getRandomValue(firstKey + " " + secondKey);
                if (secondKey.Count() == 0) {
                    break;
                }
                if (returnString.Count() + secondKey.Count() > MAXGEN) {
                    break;
                }
                returnString += " " + secondKey;
                firstKey = temp;
            }

            return returnString;
        }

        private string getRandomValue(string key) {
            if (Brain.ContainsKey(key)) {
                var list = Brain[key];
                var listKey = Rng.Next(list.Count());
                return list.ElementAt(listKey);
            }
            return "";
        }

        #endregion


        private Dictionary<string, List<string>> addToInputs(string key, string value, Dictionary<string, List<string>> item) {
            if (item.Keys.Contains(key)) {
                item[key].Add(value);
            }
            else {
                item.Add(key, new List<string>() { value });
            }
            return item;
        }

        #region save/load files

        private Dictionary<string, List<string>> Load(string path) {
            var item = new Dictionary<string, List<string>>();
            if (File.Exists(Path.Combine(Uri, path))) {

                using (StreamReader inputFile = new StreamReader(Path.Combine(Uri, path))) {
                    var text = inputFile.ReadToEnd();
                    item = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(text);
                }
                return item;
            }
            else {
                return item;
            }

        }

        private void Save(string path, Dictionary<string, List<string>> item) {
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(Uri, path))) {
                var text = JsonConvert.SerializeObject(item, Formatting.None);
                outputFile.Write(text);
            }
        }
        #endregion
    }
}
