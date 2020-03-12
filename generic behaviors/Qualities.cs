using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using analysis;

public class Qualities : MonoBehaviour {
    public float disgusting;
    public float disturbing;
    public float chaos;
    public float offensive;
    public float positive;
    public DescribableOccurrenceData ToDescribable() {
        EventData data = new EventData(
            disgusting: disgusting,
            disturbing: disturbing,
            chaos: chaos,
            offensive: offensive,
            positive: positive
        );
        data.whatHappened = "I saw " + Toolbox.Instance.GetName(gameObject);
        data.noun = "observation";
        return new DescribableOccurrenceData(new List<Describable> { data });
    }
}
