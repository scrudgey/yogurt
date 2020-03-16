using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;
using analysis;

public class FocusGroupMenu : MonoBehaviour {

    [System.Serializable]
    public class FocusGroupPersonality {
        // public enum ReviewType { none, outlier, notable, topDisturbing, topDisgusting, topChaos, topOffensive, topPositive };
        // public List<ReviewType> reviewTypes;
        public Sprite head_norm;
        public Sprite head_talking;
        public Sprite body;
        // we do it this way so it's serializable in the unity editor
        public List<Preference> preferences = new List<Preference>(){
            new Preference(Rating.chaos, PreferenceType.hates),
            new Preference(Rating.disturbing, PreferenceType.hates),
            new Preference(Rating.disgusting, PreferenceType.hates),
            new Preference(Rating.offensive, PreferenceType.hates),
            new Preference(Rating.positive, PreferenceType.likes)
        };
        public Describable SelectEvent(Commercial commercial) {
            CommercialAnalysis analysis = new CommercialAnalysis(commercial);
            // if (reviewTypes.Count == 0)
            return analysis.Climax(0);
            // return commercial.analysis.outlierEvents[n];
            // switch (reviewTypes[Random.Range(0, reviewTypes.Count)]) {
            //     case ReviewType.outlier:
            //         return commercial.analysis.outlierEvents[n];
            //     case ReviewType.notable:
            //         return commercial.analysis.notableEvents[n];
            //     case ReviewType.topDisturbing:
            //         return commercial.analysis.maxDisturbing[n];
            //     case ReviewType.topDisgusting:
            //         return commercial.analysis.maxDisgusting[n];
            //     case ReviewType.topChaos:
            //         return commercial.analysis.maxChaos[n];
            //     case ReviewType.topOffensive:
            //         return commercial.analysis.maxOffense[n];
            //     case ReviewType.topPositive:
            //         return commercial.analysis.maxPositive[n];
            //     default:
            //         return commercial.analysis.outlierEvents[n];
            // }
        }

        public string ReviewCommercial(Commercial commercial) {

            // react to the commercial's primary traits
            // the events are frequently chaotic: do i like or dislike?

            // react to the commercial's notable events: 
            // the vomit is notable for its offensiveness: do i like or dislike?

            StringBuilder builder = new StringBuilder();
            Describable eventd = SelectEvent(commercial);

            // TODO: FIX!
            // builder.Append(ReactToEvent(eventd));
            builder.Append(Interpretation.NotableNouns(commercial));
            // builder.Append(eventd.whatHappened.Trim());
            builder.Append(".\"");
            return Toolbox.RemoveExtraPunctuation(builder.ToString());
        }
    }
    public Commercial commercial;
    public Text reviewText;
    private int _index;
    public int index {
        get { return _index; }
        set {
            _index = value;
            if (_index > 2)
                _index = 0;
            IndexUpdate();
        }
    }
    public Dictionary<FocusGroupPersonality, GameObject> bubbles = new Dictionary<FocusGroupPersonality, GameObject>();
    public Dictionary<FocusGroupPersonality, Image> heads = new Dictionary<FocusGroupPersonality, Image>();
    public Dictionary<FocusGroupPersonality, Image> bodies = new Dictionary<FocusGroupPersonality, Image>();
    public Dictionary<FocusGroupPersonality, bool> mouths = new Dictionary<FocusGroupPersonality, bool>();
    public List<FocusGroupPersonality> personalities = new List<FocusGroupPersonality>();
    public float timer;
    public void Start() {
        Canvas canvas = GetComponent<Canvas>();
        canvas.worldCamera = GameManager.Instance.cam;
        reviewText = transform.Find("panel/textbox/Text").GetComponent<Text>();
        for (int i = 0; i < 3; i++) {
            int ii = i + 1;
            FocusGroupPersonality p = personalities[i];
            mouths[p] = false;
            bubbles[p] = transform.Find("panel/graphic/person" + ii.ToString() + "/bubble").gameObject;
            heads[p] = transform.Find("panel/graphic/person" + ii.ToString() + "/head").GetComponent<Image>();
            bodies[p] = transform.Find("panel/graphic/person" + ii.ToString()).GetComponent<Image>();
            bodies[p].sprite = p.body;
        }
        index = 0;
    }
    public void Update() {
        timer += Time.unscaledDeltaTime;
        if (timer > 0.1f) {
            FocusGroupPersonality p = personalities[index];
            mouths[p] = !mouths[p];
            if (mouths[p]) {
                heads[p].sprite = p.head_talking;
            } else {
                heads[p].sprite = p.head_norm;
            }
            timer = 0f;
        }
    }
    public void IndexUpdate() {
        FocusGroupPersonality p = personalities[index];

        foreach (FocusGroupPersonality person in personalities) {
            bubbles[person].SetActive(false);
            heads[person].sprite = person.head_norm;
        }
        bubbles[p].SetActive(true);
        mouths[p] = true;
        timer = 1f;
        reviewText.text = p.ReviewCommercial(commercial);
    }
    public void DoneButtonCallback() {
        Destroy(gameObject);
    }
    public void NextButtonCallback() {
        index += 1;
    }
}
