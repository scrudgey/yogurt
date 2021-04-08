using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructionEvent : MonoBehaviour {
    public EventData data;
    bool destructed;
    public float disgusting;
    public float disturbing;
    public float offensive;
    public float chaos;
    public float positive;
    public void OnDestruct() {
        if (destructed)
            return;
        destructed = true;
        data.quality[Rating.chaos] = chaos;
        data.quality[Rating.disgusting] = disgusting;
        data.quality[Rating.disturbing] = disturbing;
        data.quality[Rating.offensive] = offensive;
        data.quality[Rating.positive] = positive;
        Toolbox.Instance.OccurenceFlag(gameObject, data);
    }
}
