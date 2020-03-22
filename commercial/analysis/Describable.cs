using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Nimrod;

namespace analysis {
    [System.Serializable]
    public class Describable {
        public System.Guid id = System.Guid.NewGuid();
        public string whatHappened;
        public SerializableDictionary<Rating, float> quality = new SerializableDictionary<Rating, float>(){
            {Rating.disgusting, 0f},
            {Rating.disturbing, 0f},
            {Rating.chaos, 0f},
            {Rating.offensive, 0f},
            {Rating.positive, 0f}
        };
        public Describable() {
            quality = new SerializableDictionary<Rating, float>(){
                {Rating.disgusting, 0f},
                {Rating.disturbing, 0f},
                {Rating.chaos, 0f},
                {Rating.offensive, 0f},
                {Rating.positive, 0f}
            };
        }
        public Tuple<Rating, float> Quality() {
            Dictionary<Rating, float> absRates = new Dictionary<Rating, float>();
            foreach (Rating key in quality.Keys) {
                absRates[key] = Mathf.Abs(quality[key]);
            }
            Rating rating = absRates.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
            float amount = quality[rating];
            return new Tuple<Rating, float>(rating, amount);
        }

        public float Norm() {
            float norm = 0;
            foreach (Rating key in quality.Keys) {
                norm += Mathf.Abs(quality[key]);
            }
            return norm;
        }
    }
}