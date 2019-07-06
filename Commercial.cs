using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Text;

public enum CommercialComparison {
    equal, notequal, greater, less, greaterEqual, lessEqual
}
public class CommercialDescription {
    public List<EventData> allEvents;
    public List<EventData> maxDisturbing = new List<EventData>();
    public List<EventData> maxDisgusting = new List<EventData>();
    public List<EventData> maxChaos = new List<EventData>();
    public List<EventData> maxOffense = new List<EventData>();
    public List<EventData> maxPositive = new List<EventData>();
    public List<EventData> notableEvents = new List<EventData>();
    public List<EventData> outlierEvents = new List<EventData>();
    public List<string> FrequentNouns(List<EventData> events) {
        List<string> frequentNouns = new List<string>();
        var nouns = events.GroupBy(i => i.noun);
        Dictionary<string, float> nounCounts = new Dictionary<string, float>();
        foreach (var grp in nouns) {
            nounCounts[grp.Key] = grp.Count();
        }
        var sortedNounCounts = nounCounts.ToList();
        sortedNounCounts.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
        sortedNounCounts.Reverse();
        foreach (KeyValuePair<string, float> kvp in sortedNounCounts) {
            frequentNouns.Add(kvp.Key);
        }
        return frequentNouns;
    }
    public CommercialDescription(List<EventData> inputEvents) {
        allEvents = inputEvents;
        HashSet<EventData> events = new HashSet<EventData>(inputEvents);
        // initialize dataset
        maxDisturbing = events.OrderBy(o => o.ratings[Rating.disturbing]).ToList();
        maxDisgusting = events.OrderBy(o => o.ratings[Rating.disgusting]).ToList();
        maxChaos = events.OrderBy(o => o.ratings[Rating.chaos]).ToList();
        maxOffense = events.OrderBy(o => o.ratings[Rating.offensive]).ToList();
        maxPositive = events.OrderBy(o => o.ratings[Rating.positive]).ToList();

        Dictionary<EventData, int> occurrencesInTop3 = new Dictionary<EventData, int>();
        foreach (EventData e in events) {
            occurrencesInTop3[e] = 0;
        }

        Dictionary<Rating, List<EventData>> maxLists = new Dictionary<Rating, List<EventData>>{
            {Rating.disturbing, maxDisturbing},
            {Rating.disgusting, maxDisgusting},
            {Rating.chaos, maxChaos},
            {Rating.offensive, maxOffense},
            {Rating.positive, maxPositive}
        };

        // calculate frequency of top3s
        foreach (List<EventData> list in maxLists.Values) {
            list.Reverse();
            for (int i = 0; i < 3; i++) {
                occurrencesInTop3[list[i]] += 1;
            }
        }

        // calculate the events that most frequently show up in the top 3
        var sortedFrequency = occurrencesInTop3.ToList();
        sortedFrequency.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
        sortedFrequency.Reverse();
        foreach (KeyValuePair<EventData, int> kvp in sortedFrequency) {
            // Debug.Log(kvp.Key.whatHappened + " " + kvp.Value.ToString());
            notableEvents.Add(kvp.Key);
        }

        Dictionary<EventData, float> largestDeltas = new Dictionary<EventData, float>();
        // calculate the 3 events with the highest deltas
        foreach (Rating key in maxLists.Keys) {
            List<EventData> list = maxLists[key];
            Dictionary<EventData, float> deltas = new Dictionary<EventData, float>();
            // calc deltas
            for (int i = 0; i < list.Count - 1; i++) {
                deltas[list[i + 1]] = list[i].ratings[key] - list[i + 1].ratings[key];
            }
            // populate list of event, highest delta
            foreach (EventData eventData in deltas.Keys) {
                float delta = -1f;
                if (largestDeltas.TryGetValue(eventData, out delta)) {
                    if (delta < deltas[eventData]) {
                        largestDeltas[eventData] = deltas[eventData];
                    }
                } else {
                    largestDeltas[eventData] = deltas[eventData];
                }
            }
        }
        // reverse list, take highest n
        var sortedDeltas = largestDeltas.ToList();
        sortedDeltas.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
        sortedDeltas.Reverse();
        foreach (KeyValuePair<EventData, float> kvp in sortedDeltas) {
            outlierEvents.Add(kvp.Key);
        }
    }
    public void DebugLists() {
        // calculate the 3 most common type of event
        List<string> commonTypes = FrequentNouns(allEvents);
        Debug.Log("top 3 notable");
        Debug.Log(notableEvents[0].whatHappened);
        Debug.Log(notableEvents[1].whatHappened);
        Debug.Log(notableEvents[2].whatHappened);

        Debug.Log("top 3 outliers");
        Debug.Log(outlierEvents[0].whatHappened);
        Debug.Log(outlierEvents[1].whatHappened);
        Debug.Log(outlierEvents[2].whatHappened);

        Debug.Log("top 3 most common types");
        Debug.Log(commonTypes[0]);
        Debug.Log(commonTypes[1]);
        Debug.Log(commonTypes[2]);
    }
}

[System.Serializable]
[XmlRoot("Commercial")]
public class Commercial {
    public string name = "default";
    public string description = "default";
    public string cutscene = "default";
    public SerializableDictionary<string, CommercialProperty> properties = new SerializableDictionary<string, CommercialProperty>();
    public List<string> unlockUponCompletion;
    public string unlockItem = "";
    public string email = "";
    public List<EventData> eventData = new List<EventData>();
    [XmlIgnore]
    public CommercialDescription analysis;
    public static Commercial LoadCommercialByFilename(string filename) {
        Commercial c = new Commercial();
        TextAsset dataFile = Resources.Load("data/commercials/" + filename) as TextAsset;
        string[] lineArray = dataFile.text.Split('\n');
        System.Array.Reverse(lineArray);
        Stack<string> lines = new Stack<string>(lineArray);
        c.name = lines.Pop();
        c.description = lines.Pop();
        c.cutscene = lines.Pop();
        while (lines.Count > 0) {
            CommercialProperty prop = new CommercialProperty();
            string line = lines.Pop();
            string[] bits = line.Split(',');
            if (bits[0] == "unlock") {
                c.unlockUponCompletion.Add(bits[1]);
            } else if (bits[0] == "item") {
                c.unlockItem = bits[1];
            } else if (bits[0] == "email") {
                c.email = bits[1];
            } else {
                // yogurt,≥,1,eat yogurt on camera
                prop.val = float.Parse(bits[2]);
                switch (bits[1]) {
                    case "=":
                        prop.comp = CommercialComparison.equal;
                        break;
                    case ">":
                        prop.comp = CommercialComparison.greater;
                        break;
                    case "<":
                        prop.comp = CommercialComparison.less;
                        break;
                    case "≥":
                        prop.comp = CommercialComparison.greaterEqual;
                        break;
                    case "≤":
                        prop.comp = CommercialComparison.lessEqual;
                        break;
                    default:
                        break;
                }
                // add description
                prop.objective = bits[3];
                prop.key = bits[0];
                c.properties[bits[0]] = prop;
            }
        }
        return c;
    }
    public Commercial() {
        unlockUponCompletion = new List<string>();
    }
    public List<string> transcript = new List<string>();
    public EventData Total() {
        EventData total = new EventData();
        foreach (EventData data in eventData) {
            foreach (Rating key in data.ratings.Keys) {
                total.ratings[key] += data.ratings[key];
            }
        }
        return total;
    }
    public void IncrementValue(EventData data) {
        if (data.val == 0)
            return;
        CommercialProperty property = null;
        properties.TryGetValue(data.key, out property);
        if (property == null) {
            properties[data.key] = new CommercialProperty();
            properties[data.key].desc = data.desc;
        }
        float initvalue = properties[data.key].val;
        float finalvalue = initvalue + data.val;
        if (data.key == "table_fire" & initvalue > 0) {
            return;
        }
        UINew.Instance.PopupCounter(data.desc, initvalue, finalvalue, this);
        properties[data.key].val = finalvalue;
    }
    public void WriteReport() {
        string filename = Path.Combine(Application.persistentDataPath, "commercial_history.txt");
        StreamWriter writer = new StreamWriter(filename, false);
        foreach (EventData data in eventData) {
            writer.WriteLine(data.whatHappened);
        }
        writer.Close();
        filename = Path.Combine(Application.persistentDataPath, "commercial_events.txt");
        writer = new StreamWriter(filename, false);
        foreach (EventData data in eventData) {
            string line = data.noun + " " + data.ratings[Rating.disturbing].ToString() + " " + data.ratings[Rating.disgusting].ToString() + " " + data.ratings[Rating.chaos].ToString() + " " + data.ratings[Rating.offensive].ToString() + " " + data.ratings[Rating.positive].ToString();
            writer.WriteLine(line);
        }
        writer.Close();
        analysis = new CommercialDescription(eventData);
    }
    public bool Evaluate(Commercial other) {
        bool requirementsMet = true;
        foreach (KeyValuePair<string, CommercialProperty> kvp in other.properties) {
            CommercialProperty myProperty = null;
            CommercialProperty otherProperty = kvp.Value;
            properties.TryGetValue(kvp.Key, out myProperty);
            if (myProperty == null) {
                requirementsMet = false;
                break;
            }
            requirementsMet = myProperty.RequirementMet(otherProperty);
            if (!requirementsMet)
                break;
        }
        return requirementsMet;
    }
    public string SentenceReview() {
        // TODO: adjectives to describe the commercial based on key properties of prominent events
        List<string> adjectives = new List<string>();
        foreach (EventData eventd in analysis.outlierEvents) {
            string adj = eventd.Adjective();
            if (adj != "none")
                adjectives.Add(adj);
        }
        StringBuilder builder = new StringBuilder();
        builder.Append("A");
        switch (Mathf.Min(adjectives.Count, 3)) {
            case 3:
            case 2:
                builder.Append(" ");
                builder.Append(adjectives[0] + " and " + adjectives[1]);
                break;
            case 1:
                builder.Append(" ");
                builder.Append(adjectives[0]);
                break;
            case 0:
            default:
                break;
        }
        builder.Append(" commercial that prominently features ");
        List<string> nouns = new List<string>();
        foreach (EventData eventd in analysis.outlierEvents) {
            if (!nouns.Contains(eventd.noun))
                nouns.Add(eventd.noun);
        }
        builder.Append(nouns[0]);
        switch (nouns.Count) {
            case 3:
                builder.Append(", ");
                builder.Append(nouns[1] + ", and " + nouns[2]);
                break;
            case 2:
                builder.Append(" and ");
                builder.Append(nouns[1]);
                break;
            case 1:
            default:
                break;
        }
        builder.Append(".");
        return builder.ToString();
    }
}
[System.Serializable]
public class CommercialProperty {
    public string key;
    public float val;
    public string desc;
    public string objective;
    public CommercialComparison comp;
    public CommercialProperty() {
        val = 0;
        comp = CommercialComparison.equal;
    }
    public CommercialProperty(float val, bool truth, CommercialComparison comp) {
        this.val = val;
        this.comp = comp;
    }
    public bool RequirementMet(CommercialProperty otherProperty) {
        switch (otherProperty.comp) {
            case CommercialComparison.equal:
                return this.val == otherProperty.val;
            case CommercialComparison.notequal:
                return this.val != otherProperty.val;
            case CommercialComparison.greater:
                return this.val > otherProperty.val;
            case CommercialComparison.less:
                return this.val < otherProperty.val;
            case CommercialComparison.greaterEqual:
                return this.val >= otherProperty.val;
            case CommercialComparison.lessEqual:
                return this.val <= otherProperty.val;
            default:
                return true;
        }
    }
}