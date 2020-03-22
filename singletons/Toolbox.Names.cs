using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.Audio;
public partial class Toolbox : Singleton<Toolbox> {
    static private List<string> weirdNames = new List<string>(){
        "Shemp",
        "Frog",
        "Crogus",
        "Smitty",
        "Scrummy Bingus",
        "Fang",
        "Ziplock McBaggins",
        "Questor",
        "Bigfoot",
        "Quangle Cringleberry",
        "Wicks Cherrycoke",
        "Sauncho Smilax",
        "Moe",
        "Tyrone Slothrop",
        "Voorwerp",
        "Scrauncho",
        "Bengis",
        "Pingy",
        "Scrints",
        "Chungus",
        "Beppo",
        "Bippy",
        "Smits",
        "Oprah Noodlemantra",
        "Hash Burnslide",
        "Bandit Slumbody",
        "Regina Phalange",
        "Horngus",
        "Scrungle",
        "Nutte"
    };
    static private List<string> normalMaleNames = new List<string>(){
        "Steve",
        "Joe",
        "Jeff",
        "Bob",
        "Mike",
        "John",
        "Josh",
        "Joe",
        "Dave",
        "Adam"
    };
    static private List<string> normalFemaleNames = new List<string>(){
        "Jennifer",
        "Linda",
        "Elizabeth",
        "Barbara",
        "Susan",
        "Jessica",
        "Nancy",
        "Patty",
        "Carol"
    };
    public static string SuggestWeirdName() {
        return weirdNames[UnityEngine.Random.Range(0, weirdNames.Count)];
    }
    public static string SuggestNormalName(Gender gender) {
        if (gender == Gender.male) {
            return normalMaleNames[UnityEngine.Random.Range(0, normalMaleNames.Count)];
        } else {
            return normalFemaleNames[UnityEngine.Random.Range(0, normalFemaleNames.Count)];
        }
    }
}
