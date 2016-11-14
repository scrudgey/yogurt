﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FadeInText : MonoBehaviour {

    private Text text;
	private HSBColor color;
	private HSBColor outlineColor;
	private List<Outline> outlines;
    
    public float speedConst = 10f;
	// Use this for initialization
	void Start () {
	   text = GetComponent<Text>();
	   outlines = new List<Outline>(GetComponents<Outline>());
	   color = HSBColor.FromColor(text.color);
	   if (outlines.Count > 0){
		   outlineColor = HSBColor.FromColor(outlines[0].effectColor); 
	   }
	   color.a = 0f;
	   outlineColor.a = 0f;
	}
    
    void OnEnable(){
       if (!text){
    	   text = GetComponent<Text>();
       }
	   color = HSBColor.FromColor(text.color);
	   color.a = 0f;
    }
    void Update () {
		color.a += Time.deltaTime / speedConst;
		outlineColor.a += Time.deltaTime / speedConst;
		// if (color.a > 1)
		// 	color.a = 1f;
		text.color = color.ToColor();
		foreach(Outline outline in outlines){
			outline.effectColor = outlineColor.ToColor();
		}
	}
}
