using System.Collections.Generic;
using System.Linq;
using System;
using Nimrod;
using UnityEngine;

namespace analysis {
    public enum PreferenceType { hates, likes };
    [System.Serializable]
    public struct Preference {
        public Rating type;
        public PreferenceType pref;
        public Preference(Rating type, PreferenceType pref) {
            this.type = type;
            this.pref = pref;
        }
    }

    public class Interpretation {
        public static List<Rating> BAD_RATINGS = new List<Rating>{
            Rating.chaos, Rating.disgusting, Rating.disturbing, Rating.offensive
        };
        public static List<Rating> GOOD_RATINGS = new List<Rating>{
            Rating.positive
        };

        public static List<string> Review(Commercial commercial, FocusGroupPersonality personality = null) {
            HashSet<string> sentences = new HashSet<string>();

            foreach (Func<Commercial, FocusGroupPersonality, string> func in new List<Func<Commercial, FocusGroupPersonality, string>>(){
                PrimaryNouns,
                NotableNouns,
                FrequentQualities,
                Climax,
                Memorable}) {
                string review = func(commercial, personality);
                if (review != "") {
                    sentences.Add(review);
                }
            }
            return Toolbox.Shuffle(sentences.ToList()).Take(UnityEngine.Random.Range(1, 3)).ToList();
        }

        public static string PrimaryNouns(Commercial commercial, FocusGroupPersonality personality = null) {
            Grammar g = new Grammar();
            CommercialAnalysis analysis = new CommercialAnalysis(commercial);

            // get nouns
            List<string> primaries = analysis.PrimaryNouns();
            string noun1 = primaries[0];
            string noun2 = primaries[1];

            // get adjectives
            Tuple<Rating, float> quality1 = analysis.labelDescriptionDictionary[noun1].QualityMax();
            Tuple<Rating, float> quality2 = analysis.labelDescriptionDictionary[noun2].QualityMax();
            string adjective1 = AdjectiveKey(quality1.Item1, quality1.Item2);
            string adjective2 = AdjectiveKey(quality2.Item1, quality2.Item2);

            string introKey = "";
            if (BAD_RATINGS.Contains(quality1.Item1)) {
                if (quality1.Item2 < 2) {
                    introKey = "{bad_low}";
                } else {
                    introKey = "{bad_high}";
                }
            } else if (GOOD_RATINGS.Contains(quality1.Item1)) {
                if (quality1.Item2 < 2) {
                    introKey = "{good_low}";
                } else {
                    introKey = "{good_high}";
                }
            }
            // Debug.Log(introKey);
            // Debug.Log(noun1);
            // Debug.Log(noun2);
            // Debug.Log(adjective1);
            // Debug.Log(adjective2);

            // set up dictionary
            g.SetSymbol("intro", introKey);
            g.SetSymbol("primary1", noun1);
            g.SetSymbol("primary2", noun2);
            g.SetSymbol("adjective1", adjective1);
            g.SetSymbol("adjective2", adjective2);

            g.Load("ratings");
            if (personality == null) {
                g.Load("interpretation_sentencereview");
            } else {
                g.Load("interpretation_sentencereview_personal");
            }

            return g.Parse("{review}");
        }

        public static string NotableNouns(Commercial commercial, FocusGroupPersonality personality = null) {
            Grammar g = new Grammar();
            CommercialAnalysis analysis = new CommercialAnalysis(commercial);

            // get notable nouns
            List<NounNotability> notables = analysis.NotableNouns();

            if (notables.Count == 0)
                return "";

            string noun1 = notables[0].labelDescription.label;

            // get noun qualities
            List<Rating> reasons1 = notables[0].reasons;

            g.SetSymbol("qualities11", QualityKey(reasons1[0]));
            string qualities1 = "{11-quality}";
            if (reasons1.Count == 2) {
                g.SetSymbol("qualities12", QualityKey(reasons1[1]));
                qualities1 = "{12-quality}";
            } else if (reasons1.Count > 2) {
                g.SetSymbol("qualities13", QualityKey(reasons1[2]));
                qualities1 = "{13-quality}";
            }

            g.SetSymbol("adjective1", AdjectiveKey(reasons1[0], UnityEngine.Random.Range(0, 3)));

            string main1 = "";
            Rating rating1 = reasons1[UnityEngine.Random.Range(0, reasons1.Count)];
            if (BAD_RATINGS.Contains(rating1)) {
                main1 = "{bad}";
            } else if (GOOD_RATINGS.Contains(rating1)) {
                main1 = "{good}";
            }

            g.SetSymbol("noun1", noun1);
            g.SetSymbol("main-1", main1);
            g.SetSymbol("quality1", qualities1);

            g.Load("ratings");
            if (personality == null) {
                g.Load("interpretation_notablenoun");
            } else {
                g.Load("interpretation_notablenoun_personal");
            }

            return g.Parse("{review}");
        }
        public static string FrequentQualities(Commercial commercial, FocusGroupPersonality personality = null) {
            Grammar g = new Grammar();
            CommercialAnalysis analysis = new CommercialAnalysis(commercial);
            List<Rating> frequents = analysis.FrequentQualities();

            Rating rating1 = frequents[0];
            Rating rating2 = frequents[1];

            float amount1 = commercial.Mode(rating1);
            float amount2 = commercial.Mode(rating2);

            string adjectiveKey1 = AdjectiveKey(rating1, Mathf.Min(amount1, 3));
            string adjectiveKey2 = AdjectiveKey(rating2, Mathf.Min(amount1, 3));

            g.SetSymbol("adjective1", adjectiveKey1);

            if (amount2 >= 1) {
                g.SetSymbol("adjective2", adjectiveKey2);
                g.SetSymbol("main", "{main-2}");
            } else {
                g.SetSymbol("main", "{main-1}");
            }

            g.Load("ratings");
            if (personality == null) {
                g.Load("interpretation_frequent");
            } else {
                g.Load("interpretation_frequent_personal");
            }
            return g.Parse("{review}");
        }
        public static string Climax(Commercial commercial, FocusGroupPersonality personality = null) {
            Grammar g = new Grammar();
            CommercialAnalysis analysis = new CommercialAnalysis(commercial);
            DescribableOccurrenceData occurrence = analysis.Climax(0);

            Tuple<Rating, float> quality1 = occurrence.QualityMax();
            string adjective1 = AdjectiveKey(quality1.Item1, quality1.Item2);

            string introKey = "";
            if (BAD_RATINGS.Contains(quality1.Item1)) {
                if (quality1.Item2 < 2) {
                    introKey = "{bad_low}";
                } else {
                    introKey = "{bad_high}";
                }
            } else if (GOOD_RATINGS.Contains(quality1.Item1)) {
                if (quality1.Item2 < 2) {
                    introKey = "{good_low}";
                } else {
                    introKey = "{good_high}";
                }
            }

            g.SetSymbol("main", introKey);
            g.SetSymbol("what-happened", occurrence.whatHappened.Trim());
            g.SetSymbol("adjective1", adjective1);

            g.Load("ratings");
            if (personality == null) {
                g.Load("interpretation_climax");
            } else {
                g.Load("interpretation_climax_personal");
            }
            return g.Parse("{review}");
        }
        public static string Memorable(Commercial commercial, FocusGroupPersonality personality = null) {
            Grammar g = new Grammar();
            CommercialAnalysis analysis = new CommercialAnalysis(commercial);
            DescribableOccurrenceData occurrence = analysis.Memorable();

            Tuple<Rating, float> quality1 = occurrence.QualityMax();
            string adjective1 = AdjectiveKey(quality1.Item1, quality1.Item2);

            string introKey = "";
            if (BAD_RATINGS.Contains(quality1.Item1)) {
                if (quality1.Item2 < 2) {
                    introKey = "{bad_low}";
                } else {
                    introKey = "{bad_high}";
                }
            } else if (GOOD_RATINGS.Contains(quality1.Item1)) {
                if (quality1.Item2 < 2) {
                    introKey = "{good_low}";
                } else {
                    introKey = "{good_high}";
                }
            }

            g.SetSymbol("main", introKey);
            g.SetSymbol("what-happened", occurrence.whatHappened.Trim());
            g.SetSymbol("adjective1", adjective1);

            g.Load("ratings");
            if (personality == null) {
                g.Load("interpretation_memorable");
            } else {
                g.Load("interpretation_memorable_personal");
            }
            return g.Parse("{review}");
        }

        public string ReactToEvent(Describable data, FocusGroupPersonality personality = null) {
            Tuple<Rating, float> quality = data.Quality();
            PreferenceType opinion = PreferenceType.likes;

            // we do it this way so it's serializable in the unity editor
            foreach (Preference pref in personality.preferences) {
                if (pref.type == quality.Item1)
                    opinion = pref.pref;
            }

            if (opinion == PreferenceType.hates) {
                return "\"I did not like when ";
            }
            return "\"I liked when ";
        }

        public static string Adjective(Describable describable, Rating rating) {
            Grammar grammar = new Grammar();
            grammar.Load("ratings");
            return Adjective(rating, describable.quality[rating]);
        }
        public static string Adjective(Rating rating, float amount) {
            Grammar grammar = new Grammar();
            grammar.Load("ratings");
            if (amount == 0)
                return "none";
            float severity = Mathf.Min(amount, 3);
            return grammar.Parse(AdjectiveKey(rating, severity));
        }
        public static string AdjectiveKey(Rating rating, float amount) {
            return "{" + rating.ToString() + "_" + amount.ToString() + "}";
        }
        public static string AdjectiveKey(Describable describable) {
            Tuple<Rating, float> quality = describable.Quality();
            float amount = describable.quality[quality.Item1];
            if (amount == 0)
                return "none";
            float severity = Mathf.Min(amount, 3);
            return AdjectiveKey(quality.Item1, severity);
        }
        public static string QualityKey(Rating rating) {
            switch (rating) {
                default:
                case Rating.chaos:
                    return "{quality-chaos}";
                case Rating.disgusting:
                    return "{quality-disgusting}";
                case Rating.disturbing:
                    return "{quality-disturbing}";
                case Rating.offensive:
                    return "{quality-offensive}";
                case Rating.positive:
                    return "{quality-positive}";
            }
        }
        // public HashSet<string> Adjectives(int number) {
        //     // warning: will have weird behavior if you try a big number
        //     // 100 retries
        //     int retries = 0;
        //     int maxRetries = 100 + number;
        //     HashSet<string> adjectives = new HashSet<string>();
        //     while (retries < 100 && adjectives.Count < number) {
        //         retries++;
        //         adjectives.Add(Adjective());
        //     }
        //     return adjectives;
        // }
    }
}