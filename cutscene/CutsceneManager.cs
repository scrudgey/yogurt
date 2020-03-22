using UnityEngine;
using UnityEngine.UI;
using Easings;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;

public abstract class Cutscene {
    public virtual void Update() { }
    public virtual void Configure() { }
    public virtual void EscapePressed() { }
    public virtual void CleanUp() { }
    public bool complete;
    public bool configured;
}

public class CutsceneManager : Singleton<CutsceneManager> {
    public List<Type> lateConfigure = new List<Type>(){
        typeof(CutsceneNewDay),
        typeof(CutsceneBoardroom),
        typeof(CutsceneFall),
        typeof(CutsceneAntiMayor)
        };
    public Cutscene cutscene;
    void Start() {
        SceneManager.sceneLoaded += LevelWasLoaded;
    }
    public void InitializeCutscene<T>() where T : Cutscene, new() {
        Controller.Instance.state = Controller.ControlState.cutscene;
        cutscene = new T();
        if (!lateConfigure.Contains(typeof(T)))
            cutscene.Configure();
    }
    public void InitializeCutscene(Cutscene cut) {
        cutscene = cut;
    }
    public void LevelWasLoaded(Scene scene, LoadSceneMode mode) {
        if (cutscene == null)
            return;
        if (cutscene.complete) {
            cutscene.CleanUp();
            cutscene = null;
            return;
        }
        if (cutscene.configured == false) {
            if (lateConfigure.Contains(cutscene.GetType())) {
                cutscene.Configure();
            }
        }
    }
    void Update() {
        if (cutscene == null) {
            return;
        }
        if (cutscene.complete) {
            cutscene.CleanUp();
            cutscene = null;
            Controller.Instance.state = Controller.ControlState.normal;
        } else {
            if (cutscene.configured) {
                cutscene.Update();
            } else {
                cutscene.Configure();
            }
        }
    }
    public void EscapePressed() {
        if (cutscene != null)
            cutscene.EscapePressed();
    }
}
