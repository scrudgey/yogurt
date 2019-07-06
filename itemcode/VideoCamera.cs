using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using Easings;
public class VideoCamera : Interactive, ISaveable {
    public Commercial commercial = new Commercial();
    public OccurrenceData watchForOccurrence = null;
    public bool live;
    public GameObject doneBubble;
    private GameObject regionIndicator;
    private HashSet<GameObject> seenFlags = new HashSet<GameObject>();
    void Awake() {
        doneBubble = transform.Find("doneBubble").gameObject;
        doneBubble.SetActive(false);
        regionIndicator = transform.Find("Graphic").gameObject;
        regionIndicator.SetActive(false);
        live = false;
        Interaction finish = new Interaction(this, "Finish", "FinishButtonClick");
        finish.validationFunction = true;
        finish.unlimitedRange = true;
        interactions.Add(finish);
        Interaction enableAct = new Interaction(this, "Start", "Enable");
        enableAct.descString = "Start commercial...";
        enableAct.validationFunction = true;
        interactions.Add(enableAct);

        Interaction cancelAct = new Interaction(this, "Cancel", "Cancel");
        cancelAct.descString = "Abort commercial...";
        cancelAct.validationFunction = true;
        interactions.Add(cancelAct);
    }
    public void EnableBubble() {
        doneBubble.SetActive(true);
        Transform bubbleImage = doneBubble.transform.Find("bubbleFrame1/Image");
        StartCoroutine(EaseIn(bubbleImage));
    }
    IEnumerator EaseIn(Transform target) {
        float t = 0;
        while (t < 1) {
            t += Time.deltaTime;
            Vector3 newScale = target.localScale;
            newScale.x = (float)PennerDoubleAnimation.ElasticEaseOut(t, 0.5, 0.5, 1);
            newScale.y = (float)PennerDoubleAnimation.ElasticEaseOut(t, 0.5, 0.5, 1);
            target.localScale = newScale;
            yield return new WaitForEndOfFrame();
        }
    }
    public void DisableBubble() {
        doneBubble.SetActive(false);
    }
    public void FinishButtonClick() {
        live = false;
        DisableBubble();
        GameManager.Instance.EvaluateCommercial(commercial);
    }
    public bool FinishButtonClick_Validation() {
        if (commercial == null)
            return false;
        if (GameManager.Instance.activeCommercial == null)
            return false;
        return commercial.Evaluate(GameManager.Instance.activeCommercial);
    }
    public string FinishButtonClick_desc() {
        return "Finish commercial";
    }
    void SaveCommercial() {
        var serializer = new XmlSerializer(typeof(Commercial));
        string path = Path.Combine(Application.persistentDataPath, GameManager.Instance.saveGameName);
        path = Path.Combine(path, "commercial.xml");
        using (FileStream sceneStream = File.Create(path)) {
            serializer.Serialize(sceneStream, commercial);
        }
        // sceneStream.Close();
    }
    // TODO: there could be an issue here with the same occurrence triggering
    // multiple collisions. I will have to handle that eventually.
    void OnTriggerEnter2D(Collider2D col) {
        if (!live)
            return;
        if (seenFlags.Contains(col.gameObject))
            return;
        seenFlags.Add(col.gameObject);
        Qualities qualities = col.GetComponent<Qualities>();
        if (qualities != null) {
            // TODO: no messageoccurrence??
            EventData data = qualities.ToEvent();
            commercial.eventData.Add(data);
        }
        Occurrence occurrence = col.gameObject.GetComponent<Occurrence>();
        if (occurrence != null)
            ProcessOccurrence(occurrence);
    }
    void ProcessOccurrence(Occurrence oc) {
        foreach (OccurrenceData occurrence in oc.data) {
            foreach (EventData data in occurrence.events) {
                commercial.IncrementValue(data);
                UINew.Instance.UpdateObjectives(commercial);
            }
            commercial.eventData.AddRange(occurrence.events);
            foreach (EventData data in occurrence.events) {
                if (data.transcriptLine != null) {
                    commercial.transcript.Add(data.transcriptLine);
                }
            }
        }
    }
    public void Enable() {
        if (GameManager.Instance.activeCommercial != null) {
            live = true;
            regionIndicator.SetActive(true);
            UINew.Instance.UpdateRecordButtons(commercial);
            foreach (KeyValuePair<string, CommercialProperty> kvp in GameManager.Instance.activeCommercial.properties) {
                UINew.Instance.AddObjective(kvp.Value);
                UINew.Instance.UpdateObjectives(commercial);
            }
            StartCoroutine(WaitAndStartScript(1f));
        } else {
            live = false;
            regionIndicator.SetActive(false);
            UINew.Instance.ShowMenu(UINew.MenuType.scriptSelect);
        }
    }
    public bool Enable_Validation() {
        return live == false;
    }
    public void Cancel() {
        live = false;
        GameManager.Instance.activeCommercial = null;
        commercial = new Commercial();
        UINew.Instance.ClearObjectives();
        regionIndicator.SetActive(false);
        UINew.Instance.UpdateRecordButtons(commercial);
        DisableBubble();
    }
    public bool Cancel_Validation() {
        return live;
    }
    public IEnumerator WaitAndStartScript(float waitTime) {
        yield return new WaitForSeconds(waitTime);
        // prompt the actor to say line
        MessageSpeech prompt = new MessageSpeech("Bob Yogurt is so good, we bet a passer-by will really like it!");
        foreach (DecisionMaker ai in GameManager.FindObjectsOfType<DecisionMaker>()) {
            if (ai.personality.actor == Personality.Actor.yes) {
                Toolbox.Instance.SendMessage(ai.gameObject, this, prompt);
            }
        }
    }
    public void SaveData(PersistentComponent data) {
        data.commercials = new List<Commercial>();
        data.commercials.Add(commercial);
        data.bools["live"] = live;
    }
    public void LoadData(PersistentComponent data) {
        commercial = data.commercials[0];
        live = data.bools["live"];
        if (data.bools["live"]) {
            Enable();
        }
    }
}
