using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TelevisionShow {
    public static Dictionary<string, Func<Music>> musicTracks = new Dictionary<string, Func<Music>>(){
        {"TVR2", () => new MusicTVR2()},
    };
    public static TelevisionShow LoadByFilename(string filename) {
        TelevisionShow show = new TelevisionShow();
        TextAsset dataFile = Resources.Load("data/tv/" + filename) as TextAsset;
        string[] lineArray = dataFile.text.Split('\n');
        System.Array.Reverse(lineArray);
        Stack<string> lines = new Stack<string>(lineArray);

        string graphicPath = lines.Pop();
        show.sprites = Resources.LoadAll<Sprite>("tvGraphic/" + graphicPath);
        show.elements = lines;
        return show;
    }


    public Sprite[] sprites;
    public Stack<string> elements = new Stack<string>();

    public bool HasNext() {
        return elements.Count > 0;
    }
    public string Next() {
        if (elements.Count == 0)
            return null;
        return elements.Pop().Trim();
    }
}