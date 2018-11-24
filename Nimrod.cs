using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using UnityEngine;

namespace Nimrod {
    public class Grammar {
        private Regex symbol_hook = new Regex(@"(\{(.+?)\})", RegexOptions.Multiline);
        private Regex prob_hook = new Regex(@"(<([\.\d]+)\|(.+?)>)", RegexOptions.Multiline);
        private Regex def_hook = new Regex(@"^#(.+)");
        private Dictionary<string, List<string>> symbols = new Dictionary<string, List<string>>();
        public void Load(string filename) {
            TextAsset textData = Resources.Load("data/nimrod/" + filename) as TextAsset;
            string currentSymbol = "";
            foreach (string line in textData.text.Split('\n')) {
                if (line.Length == 0)
                    continue;
                if (line[0] == '%'){
                    continue;
                }
                Match match = def_hook.Match(line);
                if (match.Success) {
                    currentSymbol = match.Groups[1].Value;
                    if (!symbols.ContainsKey(currentSymbol))
                        symbols[currentSymbol] = new List<string>();
                } else {
                    if (line != "")
                        symbols[currentSymbol].Add(line);
                }
            }
        }
        public string Parse(string parseText) {
            string result = "";
            int iterations = 0;

            while (result != parseText && iterations < 10) {
                iterations += 1;
                result = parseText;
                // replace symbol instances
                MatchCollection matches = symbol_hook.Matches(parseText);
                if (matches.Count > 0) {
                    foreach (Match match in matches) {
                        StringBuilder builder = new StringBuilder(parseText);

                        int firstOccurrence = builder.ToString().IndexOf(match.Groups[1].Value);
                        builder.Replace(match.Groups[1].Value, Interpret(match.Groups[2].Value), firstOccurrence, match.Groups[1].Value.Length);
                        parseText = builder.ToString();
                    }
                }
                // replace with probability
                matches = prob_hook.Matches(parseText);
                if (matches.Count > 0) {
                    foreach (Match match in matches) {
                        if (Random.Range(0.0f, 1.0f) < float.Parse(match.Groups[2].Value)) {
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
        public string Interpret(string symbol) {
            if (symbols.ContainsKey(symbol)) {
                List<string> values = symbols[symbol];
                return Parse(values[Random.Range(0, values.Count)]);
            } else {
                return symbol;
            }
        }
        public void AddSymbol(string key, string val) {
            if (!symbols.ContainsKey(key)) {
                symbols[key] = new List<string>();
            }
            symbols[key].Add(val);
        }
        public void SetSymbol(string key, string val) {
            symbols[key] = new List<string>();
            symbols[key].Add(val);
        }

        public static Grammar ObjectToGrammar(GameObject target){
            Grammar g = new Grammar();

            g.AddSymbol("target-item", "none");
            g.AddSymbol("target-clothes", "none");
            g.AddSymbol("target-hat", "none");

            // insult possessions
            DecisionMaker targetDM = target.GetComponent<DecisionMaker>();
            if (targetDM != null) {
                if (targetDM.possession != null) {
                    string possessionName = targetDM.possession.name;
                    g.SetSymbol("target-item", possessionName);
                }
            }

            // insult outfit
            Outfit targetOutfit = target.GetComponent<Outfit>();
            if (targetOutfit != null) {
                string clothesName = targetOutfit.readableUniformName;
                g.SetSymbol("target-clothes", clothesName);
            }

            // insult hat
            Head targetHead = target.GetComponentInChildren<Head>();
            if (targetHead != null) {
                if (targetHead.hat != null) {
                    string hatName = targetHead.hat.name;
                    g.SetSymbol("target-hat", hatName);
                }
            }

            return g;
        }
    }
}