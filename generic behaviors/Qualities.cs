using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Qualities : MonoBehaviour {
    public float disgusting;
    public float disturbing;
    public float chaos;
    public float offensive;
    public float positive;
    public EventData ToEvent() {
        EventData data = new EventData(
            disgusting:disgusting,
            disturbing:disturbing,
            chaos:chaos,
            offensive:offensive,
            positive:positive
        );
        data.whatHappened = "I saw "+Toolbox.Instance.GetName(gameObject);
        data.noun = "observation";
        return data;
    }
}
