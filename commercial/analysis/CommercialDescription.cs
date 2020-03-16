// using System.Collections.Generic;
// using UnityEngine;
// using System.Linq;

// namespace analysis {

//     public class CommercialDescription {
//         public List<EventData> allEvents;
//         public List<EventData> maxDisturbing = new List<EventData>();
//         public List<EventData> maxDisgusting = new List<EventData>();
//         public List<EventData> maxChaos = new List<EventData>();
//         public List<EventData> maxOffense = new List<EventData>();
//         public List<EventData> maxPositive = new List<EventData>();
//         public List<EventData> notableEvents = new List<EventData>();
//         public List<EventData> outlierEvents = new List<EventData>();
//         public List<string> FrequentNouns(List<EventData> events) {
//             List<string> frequentNouns = new List<string>();
//             var nouns = events.GroupBy(i => i.noun);
//             Dictionary<string, float> nounCounts = new Dictionary<string, float>();
//             foreach (var grp in nouns) {
//                 nounCounts[grp.Key] = grp.Count();
//             }
//             var sortedNounCounts = nounCounts.ToList();
//             sortedNounCounts.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
//             sortedNounCounts.Reverse();
//             foreach (KeyValuePair<string, float> kvp in sortedNounCounts) {
//                 frequentNouns.Add(kvp.Key);
//             }
//             return frequentNouns;
//         }
//         public CommercialDescription(List<EventData> inputEvents) {
//             allEvents = inputEvents;
//             HashSet<EventData> events = new HashSet<EventData>(inputEvents);
//             // initialize dataset
//             maxDisturbing = events.OrderBy(o => o.quality[Rating.disturbing]).ToList();
//             maxDisgusting = events.OrderBy(o => o.quality[Rating.disgusting]).ToList();
//             maxChaos = events.OrderBy(o => o.quality[Rating.chaos]).ToList();
//             maxOffense = events.OrderBy(o => o.quality[Rating.offensive]).ToList();
//             maxPositive = events.OrderBy(o => o.quality[Rating.positive]).ToList();

//             Dictionary<EventData, int> occurrencesInTop3 = new Dictionary<EventData, int>();
//             foreach (EventData e in events) {
//                 occurrencesInTop3[e] = 0;
//             }

//             Dictionary<Rating, List<EventData>> maxLists = new Dictionary<Rating, List<EventData>>{
//             {Rating.disturbing, maxDisturbing},
//             {Rating.disgusting, maxDisgusting},
//             {Rating.chaos, maxChaos},
//             {Rating.offensive, maxOffense},
//             {Rating.positive, maxPositive}
//         };

//             // calculate frequency of top3s
//             foreach (List<EventData> list in maxLists.Values) {
//                 list.Reverse();
//                 for (int i = 0; i < 3; i++) {
//                     occurrencesInTop3[list[i]] += 1;
//                 }
//             }

//             // calculate the events that most frequently show up in the top 3
//             var sortedFrequency = occurrencesInTop3.ToList();
//             sortedFrequency.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
//             sortedFrequency.Reverse();
//             foreach (KeyValuePair<EventData, int> kvp in sortedFrequency) {
//                 // Debug.Log(kvp.Key.whatHappened + " " + kvp.Value.ToString());
//                 notableEvents.Add(kvp.Key);
//             }

//             Dictionary<EventData, float> largestDeltas = new Dictionary<EventData, float>();
//             // calculate the 3 events with the highest deltas
//             foreach (Rating key in maxLists.Keys) {
//                 List<EventData> list = maxLists[key];
//                 Dictionary<EventData, float> deltas = new Dictionary<EventData, float>();
//                 // calc deltas
//                 for (int i = 0; i < list.Count - 1; i++) {
//                     deltas[list[i + 1]] = list[i].quality[key] - list[i + 1].quality[key];
//                 }
//                 // populate list of event, highest delta
//                 foreach (EventData eventData in deltas.Keys) {
//                     float delta = -1f;
//                     if (largestDeltas.TryGetValue(eventData, out delta)) {
//                         if (delta < deltas[eventData]) {
//                             largestDeltas[eventData] = deltas[eventData];
//                         }
//                     } else {
//                         largestDeltas[eventData] = deltas[eventData];
//                     }
//                 }
//             }
//             // reverse list, take highest n
//             var sortedDeltas = largestDeltas.ToList();
//             sortedDeltas.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
//             sortedDeltas.Reverse();
//             foreach (KeyValuePair<EventData, float> kvp in sortedDeltas) {
//                 outlierEvents.Add(kvp.Key);
//             }
//         }
//         public void DebugLists() {
//             // calculate the 3 most common type of event
//             List<string> commonTypes = FrequentNouns(allEvents);
//             Debug.Log("top 3 notable");
//             Debug.Log(notableEvents[0].whatHappened);
//             Debug.Log(notableEvents[1].whatHappened);
//             Debug.Log(notableEvents[2].whatHappened);

//             Debug.Log("top 3 outliers");
//             Debug.Log(outlierEvents[0].whatHappened);
//             Debug.Log(outlierEvents[1].whatHappened);
//             Debug.Log(outlierEvents[2].whatHappened);

//             Debug.Log("top 3 most common types");
//             Debug.Log(commonTypes[0]);
//             Debug.Log(commonTypes[1]);
//             Debug.Log(commonTypes[2]);
//         }
//     }
// }