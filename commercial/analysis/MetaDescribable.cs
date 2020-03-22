using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Nimrod;

namespace analysis {
    [System.Serializable]
    public class MetaDescribable<T> : Describable where T : Describable, new() {
        private List<T> children = new List<T>();
        public bool debug;
        public SerializableDictionary<Rating, List<float>> qualities = new SerializableDictionary<Rating, List<float>>();

        public SerializableDictionary<System.Guid, SerializableDictionary<Rating, float>> rank = new SerializableDictionary<System.Guid, SerializableDictionary<Rating, float>>();
        public SerializableDictionary<System.Guid, SerializableDictionary<Rating, bool>> notable = new SerializableDictionary<System.Guid, SerializableDictionary<Rating, bool>>(); //top
        public SerializableDictionary<System.Guid, int> notables = new SerializableDictionary<Guid, int>(); // tops
        public SerializableDictionary<System.Guid, int> totalRank = new SerializableDictionary<System.Guid, int>(); // total_rank
        public SerializableDictionary<System.Guid, SerializableDictionary<Rating, float>> fraction = new SerializableDictionary<System.Guid, SerializableDictionary<Rating, float>>();

        public SerializableDictionary<Rating, float> GetRank(T child) {
            if (rank.ContainsKey(child.id)) {
                return rank[child.id];
            } else {
                SerializableDictionary<Rating, float> ranking = new SerializableDictionary<Rating, float> {
                    {Rating.disgusting, int.MaxValue},
                    {Rating.disturbing, int.MaxValue},
                    {Rating.chaos, int.MaxValue},
                    {Rating.offensive, int.MaxValue},
                    {Rating.positive, int.MaxValue}
                };
                rank[child.id] = ranking;
                return ranking;
            }
        }
        public SerializableDictionary<Rating, bool> GetNotable(T child) {
            if (notable.ContainsKey(child.id)) {
                return notable[child.id];
            } else {
                SerializableDictionary<Rating, bool> ranking = new SerializableDictionary<Rating, bool> {
                    {Rating.disgusting, false},
                    {Rating.disturbing, false},
                    {Rating.chaos, false},
                    {Rating.offensive, false},
                    {Rating.positive, false}
                };
                notable[child.id] = ranking;
                return ranking;
            }
        }
        public SerializableDictionary<Rating, float> GetFraction(T child) {
            if (fraction.ContainsKey(child.id)) {
                return fraction[child.id];
            } else {
                SerializableDictionary<Rating, float> ranking = new SerializableDictionary<Rating, float> {
                    {Rating.disgusting, 0f},
                    {Rating.disturbing, 0f},
                    {Rating.chaos, 0f},
                    {Rating.offensive, 0f},
                    {Rating.positive, 0f}
                };
                fraction[child.id] = ranking;
                return ranking;
            }
        }
        public int GetNotability(T child) {
            if (notables.ContainsKey(child.id)) {
                return notables[child.id];
            } else {
                return -1;
            }
        }

        public MetaDescribable() {
            children = new List<T>();
            quality = new SerializableDictionary<Rating, float>(){
            {Rating.disgusting, 0f},
            {Rating.disturbing, 0f},
            {Rating.chaos, 0f},
            {Rating.offensive, 0f},
            {Rating.positive, 0f}
        };
        }
        public MetaDescribable(bool debug = false) {
            this.debug = debug;
            children = new List<T>();
            quality = new SerializableDictionary<Rating, float>(){
            {Rating.disgusting, 0f},
            {Rating.disturbing, 0f},
            {Rating.chaos, 0f},
            {Rating.offensive, 0f},
            {Rating.positive, 0f}
        };
        }

        public void ResetChildren() {
            children = new List<T>();
        }
        virtual public void AddChild(T child) {
            children.Add(child);
            UpdateChildren();
        }
        public List<T> GetChildren() {
            return children;
        }
        public virtual void UpdateChildren() {
            qualities = new SerializableDictionary<Rating, List<float>>();
            foreach (Rating rating in Enum.GetValues(typeof(Rating))) {
                List<float> values = new List<float>();
                foreach (T child in children) {
                    values.Add(child.quality[rating]);
                }
                qualities[rating] = values;
                quality[rating] = Sum(rating);

                List<T> sortedChildren = children.OrderByDescending(c => Mathf.Abs(c.quality[rating])).ToList();
                for (int i = 0; i < sortedChildren.Count; i++) {
                    T child = sortedChildren[i];
                    GetRank(child)[rating] = i;
                    if (debug && rating == Rating.chaos) {
                        var x = sortedChildren[i];
                        Debug.Log(x.whatHappened + " " + x.quality[rating].ToString() + " " + rank[x.id][rating].ToString());
                    }
                    if (i <= 4) {
                        GetNotable(child)[rating] = true;
                        if (notables.ContainsKey(child.id)) {
                            notables[child.id] += 1;
                        } else {
                            notables[child.id] = 1;
                        }
                    }
                }
            }
            foreach (T child in children) {
                totalRank[child.id] = int.MaxValue;
                foreach (int val in GetRank(child).Values) {
                    totalRank[child.id] += val;
                }
                foreach (Rating rating in Enum.GetValues(typeof(Rating))) {
                    if (quality[rating] != 0) {
                        GetFraction(child)[rating] = child.quality[rating] / quality[rating];
                    } else {
                        GetFraction(child)[rating] = -1;
                    }
                }
            }

        }

        public List<float> Qualities(Rating rating) {
            List<float> values = new List<float>();
            foreach (T child in children) {
                values.Add(child.quality[rating]);
            }
            return values;
        }
        public float Mean(Rating rating) {
            return Sum(rating) / (float)Qualities(rating).Count;
        }
        public float Mode(Rating rating) {
            var x = from q in Qualities(rating) where q > 0 select q;
            if (x.Count() == 0) {
                return 0;
            }
            return x
            .GroupBy(v => v)
            .OrderByDescending(g => g.Count())
            .First()
            .Key;
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

        public Tuple<Rating, float> QualityMax() {
            Dictionary<Rating, float> absRates = new Dictionary<Rating, float>();

            foreach (Rating key in quality.Keys) {
                absRates[key] = Mathf.Abs(quality[key]);
            }
            Rating rating = absRates.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
            float amount = Mathf.Min(3, Max(rating));

            return new Tuple<Rating, float>(rating, amount);
        }

        public T NotableChild() {
            T notable = null;
            float maxVal = float.MinValue;
            foreach (T child in children) {
                float norm = child.Norm();
                if (norm > maxVal) {
                    notable = child;
                    maxVal = norm;
                }
            }
            return notable;
        }
    }
}