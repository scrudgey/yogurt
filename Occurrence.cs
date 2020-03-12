using Nimrod;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
//TODO: not all of these things need static constructors?
[System.Serializable]
public enum Rating { disturbing, disgusting, chaos, offensive, positive };
public class Occurrence : MonoBehaviour {
    // we use a separate class here so that the data is serializable although the involvedparties is not.
    public OccurrenceData data;
    public HashSet<GameObject> involvedParties() {
        if (data == null) {
            Debug.Log("empty occurrence data");
            return null;
        }
        return data.involvedParties();
    }
    public Occurrence(OccurrenceData data) {
        this.data = data;
    }
}