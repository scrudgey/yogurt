using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace analysis {
    [System.Serializable]
    public class Describable {
        public System.Guid id = System.Guid.NewGuid();
        // a really inefficient data structure
        public SerializableDictionary<Rating, float> quality = new SerializableDictionary<Rating, float>(){
            {Rating.disgusting, 0f},
            {Rating.disturbing, 0f},
            {Rating.chaos, 0f},
            {Rating.offensive, 0f},
            {Rating.positive, 0f}
        };
        private List<Describable> children = new List<Describable>();
        public SerializableDictionary<Rating, List<float>> qualities = new SerializableDictionary<Rating, List<float>>();

        public SerializableDictionary<Rating, float> rank = new SerializableDictionary<Rating, float>();
        public SerializableDictionary<Rating, bool> notable = new SerializableDictionary<Rating, bool>(); //top
        public int notables; // tops
        public int totalRank; // total_rank
        public string whatHappened;
        public Describable() { }
        public Describable(List<Describable> children) {
            foreach (Describable child in children) {
                AddChild(child);
            }
        }
        public void ResetChildren() {
            children = new List<Describable>();
        }
        public void AddChild(Describable child) {
            children.Add(child);
            UpdateChildren();
        }
        public List<Describable> GetChildren() {
            return children;
        }
        public void UpdateChildren() {
            qualities = new SerializableDictionary<Rating, List<float>>();
            foreach (Rating rating in Enum.GetValues(typeof(Rating))) {
                List<float> values = new List<float>();
                foreach (Describable child in children) {
                    values.Add(child.quality[rating]);
                }
                qualities[rating] = values;
                quality[rating] = Sum(rating);

                List<Describable> sortedChildren = children.OrderByDescending(c => c.quality[rating]).ToList();
                for (int i = 0; i < sortedChildren.Count; i++) {
                    sortedChildren[i].rank[rating] = i;
                    if (i <= 4) {
                        sortedChildren[i].notable[rating] = true;
                        sortedChildren[i].notables += 1;
                    }
                }
            }
            foreach (Describable child in children) {
                child.totalRank = 0;
                foreach (int val in child.rank.Values) {
                    child.totalRank += val;
                }
            }

        }

        public List<float> Qualities(Rating rating) {
            List<float> values = new List<float>();
            foreach (Describable child in children) {
                values.Add(child.quality[rating]);
            }
            return values;
        }
        public float Mean(Rating rating) {
            return Sum(rating) / Qualities(rating).Count;
        }
        public float Sum(Rating rating) {
            float total = 0;
            foreach (float q in Qualities(rating))
                total += q;
            return total;
        }
        public float Max(Rating rating) {
            return Mathf.Max(Qualities(rating).ToArray());
        }

        // constructors
        // public Describable(float chaos, float disgusting, float disturbing, float offensive, float positive) {
        //     this.quality[Rating.chaos] = chaos;
        //     this.quality[Rating.disturbing] = disturbing;
        //     this.quality[Rating.disgusting] = disgusting;
        //     this.quality[Rating.offensive] = offensive;
        //     this.quality[Rating.positive] = positive;
        // }
    }
}