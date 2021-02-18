using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace analysis {
    public struct NounNotability {
        public NounNotability(LabelDescription labelDescription, List<Rating> reasons) {
            this.labelDescription = labelDescription;
            this.reasons = reasons;
        }
        public LabelDescription labelDescription;
        public List<Rating> reasons;
    }
    public class LabelDescription : MetaDescribable<DescribableOccurrenceData> {
        public string label;
        public List<string> descriptors;
        public LabelDescription() { }
        public LabelDescription(string label, List<DescribableOccurrenceData> occurrenceDatas) {
            this.label = label;
            this.whatHappened = label;
            foreach (DescribableOccurrenceData data in occurrenceDatas) {
                if (data.nouns.Contains(label))
                    AddChild(data);
            }
        }
    }
    public class CommercialAnalysis {
        public Commercial commercial;
        HashSet<string> labels;
        public MetaDescribable<LabelDescription> labelCollection;
        public Dictionary<string, LabelDescription> labelDescriptionDictionary;
        public CommercialAnalysis(Commercial commercial) {
            this.commercial = commercial;
            List<DescribableOccurrenceData> occurrences = commercial.GetChildren();

            // labels
            labels = new HashSet<string>();
            foreach (DescribableOccurrenceData oc in occurrences) {
                labels.UnionWith(oc.nouns);
            }
            labelCollection = new MetaDescribable<LabelDescription>();
            labelDescriptionDictionary = new Dictionary<string, LabelDescription>();
            foreach (string label in labels) {
                if (label == null) {
                    continue;
                }
                LabelDescription labelDescription = new LabelDescription(label, occurrences);
                labelCollection.AddChild(labelDescription);
                labelDescriptionDictionary[label] = labelDescription;
            }
            // now the fractions are all properly tallied.
        }
        public List<string> PrimaryNouns() {
            IEnumerable<string> x = from label in labelCollection.GetChildren().OrderBy(l => labelCollection.totalRank[l.id]) select label.label;
            return x.Take(3).ToList();
        }
        public List<NounNotability> NotableNouns() {
            List<NounNotability> nouns = new List<NounNotability>();
            foreach (LabelDescription ld in labelCollection.GetChildren()) {
                // var dict = labelCollection.GetFraction(ld);
                // foreach (Rating r in Enum.GetValues(typeof(Rating))) {
                //     Debug.Log(ld.whatHappened + " " + r.ToString() + " " + dict[r].ToString());
                // }
                IEnumerable<Rating> reasons = from item in labelCollection.GetFraction(ld) where item.Value >= 0.5 select item.Key;
                if (reasons.Count() > 0) {
                    nouns.Add(new NounNotability(ld, reasons.ToList()));
                }
            }
            return nouns;
        }
        public List<Rating> FrequentQualities() {
            return new List<Rating>(){
                Rating.chaos,
                Rating.disgusting,
                Rating.disturbing,
                Rating.offensive,
                Rating.positive
            }
            .OrderByDescending(r => Mathf.Abs(commercial.Mean(r)))
            .Take(2)
            .ToList();
        }

        public DescribableOccurrenceData Climax(int i) {
            Rating topRating = FrequentQualities()[i];
            return commercial.GetChildren().OrderBy(o => commercial.GetRank(o)[topRating]).First();
        }

        public DescribableOccurrenceData Memorable() {
            return commercial.GetChildren().OrderByDescending(o => commercial.GetNotability(o)).First();
        }

        public List<DescribableOccurrenceData> TopEvents() {
            return commercial.GetChildren().OrderByDescending(o => o.quality.Values.Sum()).ToList();
        }
    }
}