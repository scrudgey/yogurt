using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using Easings;
public class VideoCamera : Interactive {
    public GameObject doneBubble;
    private GameObject regionIndicator;
    private HashSet<GameObject> seenFlags = new HashSet<GameObject>();
    void Awake() {
        doneBubble = transform.Find("doneBubble").gameObject;
        doneBubble.SetActive(false);
        regionIndicator = transform.Find("CameraRegion").gameObject;
        regionIndicator.SetActive(false);

        Interaction finish = new Interaction(this, "Finish", "FinishButtonClick");
        finish.validationFunction = true;
        finish.unlimitedRange = true;
        finish.holdingOnOtherConsent = false;
        interactions.Add(finish);

        Interaction enableAct = new Interaction(this, "Start New", "Enable");
        enableAct.descString = "Start new commercial";
        enableAct.validationFunction = true;
        enableAct.holdingOnOtherConsent = false;
        interactions.Add(enableAct);

        Interaction cancelAct = new Interaction(this, "Stop", "Cancel");
        cancelAct.descString = "Abort commercial";
        cancelAct.validationFunction = true;
        cancelAct.holdingOnOtherConsent = false;
        interactions.Add(cancelAct);

        // add a "restart / start new" interaction
    }
    void Start() {
        UpdateStatus();
    }
    public void EnableBubble() {
        doneBubble.SetActive(true);
        Transform bubbleImage = doneBubble.transform.Find("bubbleFrame1/Image");
        StartCoroutine(EaseIn(bubbleImage));
        if (!GameManager.Instance.data.finishedCommercial) {
            GameManager.Instance.data.finishedCommercial = true;
            StartCoroutine(ShowDiary());
        }
    }
    IEnumerator ShowDiary() {
        yield return new WaitForSeconds(1f);
        GameManager.Instance.ShowDiaryEntry("firstCommercial");
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
        // GameManager.Instance.data.recordingCommercial = false;
        GameManager.Instance.SetRecordingStatus(false);
        DisableBubble();
        GameManager.Instance.EvaluateCommercial();
    }
    public bool FinishButtonClick_Validation() {
        if (GameManager.Instance.data.activeCommercial == null)
            return false;
        return GameManager.Instance.data.activeCommercial.Evaluate();
    }
    public string FinishButtonClick_desc() {
        return "Finish commercial";
    }
    // TODO: there could be an issue here with the same occurrence triggering
    // multiple collisions. I will have to handle that eventually.
    void OnTriggerEnter2D(Collider2D col) {
        if (GameManager.Instance.data == null)
            return;
        if (!GameManager.Instance.data.recordingCommercial)
            return;
        if (seenFlags.Contains(col.transform.root.gameObject))
            return;
        if (col.tag == "occurrenceSound")
            return;
        seenFlags.Add(col.transform.root.gameObject);
        Qualities qualities = col.GetComponent<Qualities>();
        if (qualities != null) {
            // TODO: no messageoccurrence??
            EventData data = qualities.ToEvent();
            GameManager.Instance.data.activeCommercial.eventData.Add(data);
        }
        Occurrence occurrence = col.gameObject.GetComponent<Occurrence>();
        if (occurrence != null)
            GameManager.Instance.data.activeCommercial.ProcessOccurrence(occurrence);
    }

    public void UpdateStatus() {
        if (GameManager.Instance.data == null) {
            return;
        }
        if (GameManager.Instance.data.recordingCommercial) {
            regionIndicator.SetActive(true);
        } else {
            regionIndicator.SetActive(false);
        }

        if (GameManager.Instance.data.activeCommercial == null) {
            return;
        } else if (GameManager.Instance.data.activeCommercial.Evaluate()) {
            EnableBubble();
        } else {
            DisableBubble();
        }
    }
    public void Enable() {
        Debug.Log("start camera");
        UINew.Instance.ShowMenu(UINew.MenuType.scriptSelect);
    }
    public bool Enable_Validation() {
        CameraTutorialText ctt = GetComponent<CameraTutorialText>();
        if (ctt != null)
            ctt.Disable();
        return !GameManager.Instance.data.recordingCommercial;
    }
    public void Cancel() {
        GameManager.Instance.SetRecordingStatus(false);
        UINew.Instance.ClearObjectives();
        regionIndicator.SetActive(false);
    }
    public bool Cancel_Validation() {
        return GameManager.Instance.data.recordingCommercial;
    }

}
