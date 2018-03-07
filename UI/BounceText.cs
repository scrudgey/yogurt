using UnityEngine;
using UnityEngine.UI;
using Easings;

public class BounceText : MonoBehaviour {
    public GameObject target;
    private Text textObject;
    public string text = "Success!";
    private FollowGameObjectInCamera followScript;
    public float lifeTime = 2f;
    public float timer = 0f;
    private RectTransform trans;
    private Vector3 tempScale;
    void Start() {
        followScript = GetComponentInChildren<FollowGameObjectInCamera>();
        if (target)
            followScript.target = target;
        trans = transform.Find("Text").GetComponent<RectTransform>();
        textObject = transform.Find("Text").GetComponent<Text>();
        textObject.text = text;
        tempScale = Vector3.one;
    }
    void Update() {
        timer += Time.deltaTime;
        if (timer > lifeTime) {
            Destroy(gameObject);
        }
        tempScale.x = (float)PennerDoubleAnimation.ElasticEaseOut(timer, 0.001, 1, 0.8);
        tempScale.y = (float)PennerDoubleAnimation.ElasticEaseOut(timer, 0.001, 1, 1.0);
        trans.localScale = tempScale;
    }

}
