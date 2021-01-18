using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Presocializer : MonoBehaviour {
    public List<GameObject> people;
    void Start() {
        foreach (GameObject person in people) {
            if (person == null)
                continue;
            Awareness awareness = person.GetComponent<Awareness>();
            if (awareness == null)
                continue;
            foreach (GameObject other in people) {
                if (other == person)
                    continue;
                PersonalAssessment pa = awareness.FormPersonalAssessment(other);
            }
            awareness.newPeopleList = new List<AI.Ref<GameObject>>();
        }
    }
}
