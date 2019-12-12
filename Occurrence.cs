using Nimrod;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
//TODO: not all of these things need static constructors?
[System.Serializable]
public enum Rating { disturbing, disgusting, chaos, offensive, positive };
public class Occurrence : MonoBehaviour {
    // An occurrence is a little bit of code that lives on a temporarily persistent flag in the world
    // that knows how to describe an event in terms of EventData. 
    // occurrences can also be noticed by perceptive components which use the flag properties to compose 
    // a stimulus.
    public OccurrenceData data;
    public HashSet<GameObject> involvedParties() {
        if (data == null) {
            Debug.Log("empty occurrence data");
            return null;
        }
        return data.involvedParties();
    }
    public void CalculateDescriptions() {
        data.CalculateDescriptions();
    }
    public Occurrence(OccurrenceData data) {
        this.data = data;
    }
}