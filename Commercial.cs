using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Text;

public enum CommercialComparison {
    equal, notequal, greater, less, greaterEqual, lessEqual
}

[System.Serializable]
[XmlRoot("Commercial")]
public class Commercial {
    public string name = "default";
    public string description = "default";
    public string cutscene = "default";
    public SerializableDictionary<string, CommercialProperty> properties = new SerializableDictionary<string, CommercialProperty>();
    public List<string> unlockUponCompletion;
    public List<string> requiredLocations = new List<string>();
    public HashSet<string> visitedLocations = new HashSet<string>();
    public string unlockItem = "";
    public string email = "";
    public List<EventData> eventData = new List<EventData>();
    [XmlIgnore]
    public CommercialDescription analysis;
    [XmlIgnore]
    public List<Objective> objectives = new List<Objective>();
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
            string key = bits[0];
            if (key == "unlock") {
                c.unlockUponCompletion.Add(bits[1]);
            } else if (key == "item") {
                c.unlockItem = bits[1];
            } else if (key == "email") {
                c.email = bits[1];
            } else if (key == "location") {
                c.objectives.Add(new ObjectiveLocation(bits));
            } else c.objectives.Add(new ObjectiveProperty(bits));
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
            properties[data.key].desc = data.popupDesc;
        }
        float initvalue = properties[data.key].val;
        float finalvalue = initvalue + data.val;
        if (data.key == "table_fire" & initvalue > 0) {
            return;
        }
        UINew.Instance.PopupCounter(data.popupDesc, initvalue, finalvalue, this);
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
    public bool Evaluate(Commercial required) {
        bool requirementsMet = true;
        foreach (Objective objective in required.objectives) {
            requirementsMet &= objective.RequirementsMet(this);
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
}