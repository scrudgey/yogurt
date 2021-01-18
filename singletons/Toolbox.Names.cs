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
        "Beppo",
        "Bippy",
        "Smits",
        "Oprah Noodlemantra",
        "Hash Burnslide",
        "Bandit Slumbody",
        "Regina Phalange",
        "Horngus",
        "Scrungle",
        "Nutte",
        "Crispin",
        "Jean-Pierre",
        "Pokey",
        "Gong Farmer",
        "Warbler",
        "Ol' Drinky",
        "Pointyknife, The Manstabber",
        "The Traveler",
        "Scrumpi",
        "Poggle",
        "Hoggle",
        "Chewy",
        "Scuzzbucket",
        "Frobenius",
        "Crad",
        "Vern",
        "The Pope"
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

    public static Stack<string> weirdNameStack = new Stack<string>();
    public static Stack<string> normalMaleNameStack = new Stack<string>();
    public static Stack<string> normalFemaleNameStack = new Stack<string>();
    public static string SuggestWeirdName() {
        if (weirdNameStack.Count == 0) {
            weirdNameStack = new Stack<string>(Toolbox.Shuffle(weirdNames));
        }
        return weirdNameStack.Pop();
    }
    public static string SuggestNormalName(Gender gender) {
        if (gender == Gender.male) {
            if (normalMaleNameStack.Count == 0) {
                normalMaleNameStack = new Stack<string>(Toolbox.Shuffle(normalMaleNames));
            }
            return normalMaleNameStack.Pop();
            // return normalMaleNames[UnityEngine.Random.Range(0, normalMaleNames.Count)];
        } else {
            if (normalFemaleNameStack.Count == 0) {
                normalFemaleNameStack = new Stack<string>(Toolbox.Shuffle(normalFemaleNames));
            }
            return normalFemaleNameStack.Pop();
            // return normalFemaleNames[UnityEngine.Random.Range(0, normalFemaleNames.Count)];
        }
    }
}
