using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using UnityEngine;

namespace Nimrod{
    public class Grammar{
        private Regex symbol_hook = new Regex(@"(\{(.+?)\})", RegexOptions.Multiline);
        private Regex prob_hook = new Regex(@"(<([\.\d]+)\|(.+?)>)", RegexOptions.Multiline);
        private Regex def_hook = new Regex(@"^#(.+)");
        private Dictionary<string, List<string>> symbols = new Dictionary<string, List<string>>();
        public void Load(string filename){
            TextAsset textData = Resources.Load("data/nimrod/"+filename) as TextAsset;
            string currentSymbol = "";
            foreach (string line in textData.text.Split('\n')){
                Match match = def_hook.Match(line);
                if (match.Success){
                    currentSymbol = match.Groups[1].Value;
                    symbols[currentSymbol] = new List<string>();
                } else {
                    if (line != "")
                        symbols[currentSymbol].Add(line);
                }
            }
        }
        public string Parse(string parseText){
            string result = "";
            int iterations = 0;
            
            while(result != parseText && iterations < 10){
                // Debug.Log(parseText);
                iterations += 1;
                result = parseText;

                // replace symbol instances
                MatchCollection matches = symbol_hook.Matches(parseText);
                if (matches.Count > 0){
                    foreach (Match match in matches){
                        StringBuilder builder = new StringBuilder(parseText);
                        builder.Replace(match.Groups[1].Value, Interpret(match.Groups[2].Value));
                        parseText = builder.ToString();
                    }
		        }

                // replace with probability
                matches = prob_hook.Matches(parseText);
                if (matches.Count > 0){
                    foreach (Match match in matches){
                        if (Random.Range(0, 1) < float.Parse(match.Groups[2].Value)){
                            StringBuilder builder = new StringBuilder(parseText);
                            builder.Replace(match.Groups[1].Value, Interpret(match.Groups[3].Value));
                            parseText = builder.ToString();
                        } else {
                            StringBuilder builder = new StringBuilder(parseText);
                            builder.Replace(match.Groups[1].Value, "");
                            parseText = builder.ToString();
                        }
                    }
		        }
            }
            return result;
        }
        public string Interpret(string symbol){
            if (symbols.ContainsKey(symbol)){
                List<string> values = symbols[symbol];
                return Parse(values[Random.Range(0, values.Count)]);
            } else {
                return symbol;
            }
        }
    }
}