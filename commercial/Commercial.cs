using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.IO;
using analysis;

[System.Serializable]
public enum CommercialComparison {
    equal, notequal, greater, less, greaterEqual, lessEqual
}

[System.Serializable]
[XmlRoot("Commercial")]
public class Commercial : MetaDescribable<DescribableOccurrenceData> {
    public string name = "default";
    public string description = "default";
    public string cutscene = "default";
    public SerializableDictionary<string, CommercialProperty> properties = new SerializableDictionary<string, CommercialProperty>();
    public List<string> unlockUponCompletion;
    public string unlockItem = "";
    public string email = "";
    public string hallucination = "";
    public HashSet<string> yogurtEaterOutfits = new HashSet<string>();
    public HashSet<string> yogurtEaterNames = new HashSet<string>();
    public HashSet<string> visitedLocations = new HashSet<string>();
    public HashSet<string> outfits = new HashSet<string>();
    public bool gravy;
    public bool sabotage;
    public bool gremlin;

    public List<Objective> objectives = new List<Objective>();
    [XmlIgnore]
    private Dictionary<string, int> uniqueEvents = new Dictionary<string, int>();
    public Commercial(Commercial other) {
        this.name = other.name;
        this.description = other.description;
        this.cutscene = other.cutscene;
        this.properties = new SerializableDictionary<string, CommercialProperty>();
        this.unlockUponCompletion = new List<string>(other.unlockUponCompletion);
        this.objectives = other.objectives;
        // this.visitedLocations = new HashSet<string>(other.visitedLocations);
        this.unlockItem = other.unlockItem;
        this.email = other.email;
        this.gravy = other.gravy;
        this.sabotage = other.sabotage;
        this.hallucination = other.hallucination;
        this.gremlin = other.gremlin;
    }
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
            if (line == "")
                continue;
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
            } else if (key == "outfit") {
                c.objectives.Add(new ObjectiveEat(bits));
            } else if (key == "eatName") {
                c.objectives.Add(new ObjectiveEatName(bits));
            } else if (key == "killScorpion") {
                c.objectives.Add(new ObjectiveScorpion(bits));
            } else if (key == "mayorhead") {
                c.objectives.Add(new ObjectiveMayorHead());
            } else if (key == "gravyCommercial") {
                c.gravy = true;
            } else if (key == "sabotage") {
                c.sabotage = true;
            } else if (key == "hallucination") {
                c.hallucination = bits[1];
            } else if (key == "gremlins") {
                c.gremlin = true;
            } else {
                c.objectives.Add(new ObjectiveProperty(bits));
            }
        }
        c.quality = new SerializableDictionary<Rating, float>(){
            {Rating.disgusting, 0f},
            {Rating.disturbing, 0f},
            {Rating.chaos, 0f},
            {Rating.offensive, 0f},
            {Rating.positive, 0f}
        };
        return c;
    }
    public void ProcessOccurrence(Occurrence oc) {
        // add scenes
        visitedLocations.Add(SceneManager.GetActiveScene().name);
        OccurrenceData occurrence = oc.data;
        if (occurrence == null)
            return;
        // add outfits?
        foreach (GameObject obj in oc.involvedParties()) {
            if (obj == null)
                continue;
            Outfit outfit = obj.GetComponent<Outfit>();
            if (outfit != null) {
                outfits.Add(outfit.wornUniformName);
            }
        }

        OccurrenceEat eatOccurrence = occurrence as OccurrenceEat;
        if (eatOccurrence != null) {
            if (eatOccurrence.yogurt) {
                yogurtEaterOutfits.Add(eatOccurrence.eaterOutfitName);
                yogurtEaterNames.Add(eatOccurrence.eaterName);
            }
        }
        foreach (EventData data in occurrence.NetEvents()) {
            IncrementValue(data);
            if (data.transcriptLine != null) {
                transcript.Add(data.transcriptLine);
            }
        }

        if (uniqueEvents.ContainsKey(occurrence.describable.Qualstring())) {
            // accept the event with a probability inversely proportional to the number we have seen
            if (Random.Range(0, uniqueEvents[oc.data.describable.Qualstring()]) < 1) {
                uniqueEvents[oc.data.describable.Qualstring()] += 2;
                RecordOccurrence(oc);
            }
        } else {
            uniqueEvents[oc.data.describable.Qualstring()] = 1;
            RecordOccurrence(oc);
        }

        UINew.Instance.UpdateObjectives();
    }

    public void RecordOccurrence(Occurrence oc) {
        AddChild(oc.data.NetDescribable());
        UINew.Instance.UpdateMetrics(o: oc);
    }
    public Commercial() {
        unlockUponCompletion = new List<string>();
    }
    public List<string> transcript = new List<string>();
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
        Poptext.PopupCounter(data.popupDesc, initvalue, finalvalue, this);
        properties[data.key].val = finalvalue;
    }
    public void WriteReport() {
        System.Guid guid = System.Guid.NewGuid();
        // string outName = "commercial_history_" + guid.ToString() + ".xml";
        string filename = Path.Combine(Application.persistentDataPath, "commercial_history_" + guid.ToString() + ".txt");
        StreamWriter writer = new StreamWriter(filename, false);
        foreach (DescribableOccurrenceData data in GetChildren()) {
            writer.WriteLine(data.whatHappened);
        }
        writer.Close();
        filename = Path.Combine(Application.persistentDataPath, "commercial_events_" + guid.ToString() + ".txt");
        writer = new StreamWriter(filename, false);
        foreach (DescribableOccurrenceData data in GetChildren()) {
            foreach (EventData child in data.GetChildren()) {
                string line = data.id + ";" + child.noun + ";" + child.quality[Rating.disturbing].ToString() + ";" + child.quality[Rating.disgusting].ToString() + ";" + child.quality[Rating.chaos].ToString() + ";" + child.quality[Rating.offensive].ToString() + ";" + child.quality[Rating.positive].ToString() + ";" + child.whatHappened;
                writer.WriteLine(line);
            }
        }
        writer.Close();
    }
    public bool Evaluate() {
        bool requirementsMet = true;
        foreach (Objective objective in objectives) {
            requirementsMet &= objective.RequirementsMet(this);
        }
        return requirementsMet;
    }
}
[System.Serializable]
public class CommercialProperty {
    public string key;
    public float val;
    public string desc;
}