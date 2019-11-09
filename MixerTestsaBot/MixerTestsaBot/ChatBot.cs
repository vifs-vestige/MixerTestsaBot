using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MixerTestsaBot {
    class ChatBot {
        private Dictionary<string, List<string>> Brain = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> Quotes = new Dictionary<string, List<string>>();
        private string Uri;
        private string BrainFile = "brain.txt";
        private string QuoteFile = "quote.txt";

        public ChatBot() {
            Uri = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Brain = Load(BrainFile);
            Quotes = Load(QuoteFile);
        }

        public string? parseInput(string message) {
            string? output = null; 
            if(message.StartsWith("!")) {
                if(message.ToLower().StartsWith("!quote")) {
                    output = ParseQuoteCommand(message);
                }
            } else {
                UpdateBrain(message);
                if (message.ToLower().Contains(Settings.Default.BotName)) {
                    output = OutputBrain();
                }
            }
            return output;
        }

        #region quotes

        private string? ParseQuoteCommand(string message) {
            string? output = null;
            var info = message.Split(' ');
            if(info.Count() == 1) {
                output = GetQuote();
            } else if (info.Count() == 2) {
                output = GetQuote(info[1]);
            } else {
                SaveQuote(message);
            }
            return output;
        }

        private string GetQuote(string? User = null) {
            return "quotes" + User;
        }

        private void SaveQuote(string message) {

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
            return "brain";
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
            } else {
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
