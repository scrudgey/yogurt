using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Necronomicon : Interactive {
    public ParticleSystem rayFx;
    public ParticleSystem smokeFx;
    public void Awake() {
        Interaction read = new Interaction(this, "Read", "Read");
        read.otherOnSelfConsent = false;
        read.defaultPriority = 6;
        read.descString = "Read the necronomicon";
        interactions.Add(read);
    }
    public void Read() {
        CutsceneManager.Instance.InitializeCutscene<CutsceneNeconomicon>();
    }
    public void StartFx() {
        rayFx.Play();
        smokeFx.Play();
    }
    public void StopFx() {
        rayFx.Stop();
        smokeFx.Stop();
    }
    public void EldritchOccurrence() {
        OccurrenceNecronomicon data = new OccurrenceNecronomicon();
        Toolbox.Instance.OccurenceFlag(gameObject, data);
    }
}
