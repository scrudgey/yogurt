using UnityEngine;
// using System.Collections;
using UnityEngine.UI;

public class HueShiftText : MonoBehaviour {

    private Text text;
	private HSBColor color;
	// Use this for initialization
	void Start () {
	   text = GetComponent<Text>();
	   color = HSBColor.FromColor(text.color);
	}
    void Update () {
		color.h += Time.deltaTime / 10f;
		if (color.h > 1)
			color.h -= 1f;
		text.color = color.ToColor();
	}
}
