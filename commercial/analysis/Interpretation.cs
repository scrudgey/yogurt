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
        public static string PrimaryNouns(Commercial commercial) {
            /**
            "the commercial is characterized by {adjective} {noun}"
                high intensity
                    negative rating
                        The commercial spits in the audience's face with its insistence on {adjective} {noun}
                        Flirting dangerously with insanity, the commercial features repeated imagery of {adjective} {noun}
                        A low point of the commercial features heinous yogurt vomiting.

                    positive rating
                        It's a crowd pleaser, with a focus on {adjective} {noun}
                low intensity
                    negative rating
                        Mildly unpleasant, featuring {adjective} {noun}
                    positive rating
                        Measured, thoughtful, mildly {adjective} {noun} 

            the commercial is primarily characterized by eating, vomiting yogurt, and yogurt eating.
            **/

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
            g.Load("interpretation_sentencereview");

            return g.Parse("{review}");
        }

        // what to do when there are no notable nouns?
        public static string NotableNouns(Commercial commercial) {
            Grammar g = new Grammar();
            CommercialAnalysis analysis = new CommercialAnalysis(commercial);

            // get notable nouns
            List<NounNotability> notables = analysis.NotableNouns();
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
            g.Load("interpretation_notablenoun");
            return g.Parse("{review}");
        }
        public static string FrequentQualities(Commercial commercial) {
            Grammar g = new Grammar();
            // the events of this commercial are frequently disgusting and chaotic
            CommercialAnalysis analysis = new CommercialAnalysis(commercial);
            List<Rating> frequents = analysis.FrequentQualities();
            return "";
        }
        public static string Climax(Commercial commercial) {
            Grammar g = new Grammar();
            // the climax of the commercial was when test vomited up vomited-up puddle of yogurt
            CommercialAnalysis analysis = new CommercialAnalysis(commercial);
            DescribableOccurrenceData occurrence = analysis.Climax(0);
            return "";
        }
        public static string Memorable(Commercial commercial) {
            Grammar g = new Grammar();
            // it was memorable when test said Yuck
            CommercialAnalysis analysis = new CommercialAnalysis(commercial);
            DescribableOccurrenceData occurrence = analysis.Memorable();
            return "";
        }

        public string ReactToEvent(Describable data, FocusGroupMenu.FocusGroupPersonality personality) {
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
            return "";
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