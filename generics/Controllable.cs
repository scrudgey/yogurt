﻿using UnityEngine;
using System.Collections.Generic;
using System;

public class Controllable : MonoBehaviour {
    public enum HitState { none, stun, unconscious, dead };
    public enum ControlType { none, AI, player }
    public Interaction defaultInteraction;
    public static List<Type> AIComponents = new List<Type>(){
        typeof(DecisionMaker),
        typeof(PeterPicklebottom)
    };
    static public HitState AddHitState(HitState orig, HitState argument) {
        if (argument > orig) {
            return argument;
        }
        return orig;
    }
    static public HitState RemoveHitState(HitState orig, HitState argument) {
        if (argument >= orig) {
            return HitState.none;
        }
        return orig;
    }
    private bool _upFlag;
    private bool _downFlag;
    private bool _leftFlag;
    private bool _rightFlag;
    public bool upFlag {
        get { if (disabled) return false; else return _upFlag; }
        set { _upFlag = value; }
    }
    public bool downFlag {
        get { if (disabled) return false; else return _downFlag; }
        set { _downFlag = value; }
    }
    public bool leftFlag {
        get { if (disabled) return false; else return _leftFlag; }
        set { _leftFlag = value; }
    }
    public bool rightFlag {
        get { if (disabled) return false; else return _rightFlag; }
        set { _rightFlag = value; }
    }
    public string lastPressed = "right";
    private Vector2 _direction = Vector2.right;
    public Vector2 direction {
        get { return _direction; }
        set {
            _direction = value;
            directionAngle = Toolbox.Instance.ProperAngle(_direction.x, _direction.y);
            foreach (IDirectable directable in directables) {
                directable.DirectionChange(value);
            }
        }
    }
    public float directionAngle = 0;
    public List<IDirectable> directables = new List<IDirectable>();
    public bool fightMode;
    public bool disabled = false;
    public HitState hitState;
    public GameObject lastRightClicked;
    public Rigidbody2D rigidBody2D;
    private ControlType _control;
    public ControlType control {
        get {return _control;}
        set {
            SetControl(value);
            _control = value;
        }
    }
    private DecisionMaker decisionMaker;
    public void ResetInput() {
        upFlag = false;
        downFlag = false;
        leftFlag = false;
        rightFlag = false;
    }
    public virtual void Awake() {
        // TODO: more sophisticated AI detecting here: there will be a whole class
        // of components that can control controllables.
        // plus, the player will eventually have attached AI that will have to be overridden
        decisionMaker = GetComponent<DecisionMaker>();
        if (decisionMaker != null) {
            control = ControlType.AI;
        } else {
            control = ControlType.none;
        }
        Toolbox.RegisterMessageCallback<MessageHitstun>(this, HandleHitStun);
        Toolbox.RegisterMessageCallback<MessageDirectable>(this, HandleDirectable);
    }
    public virtual void Start() {
        foreach (Component component in gameObject.GetComponentsInChildren<Component>()) {
            if (component is IDirectable) {
                directables.Add((IDirectable)component);
            }
        }
        rigidBody2D = GetComponent<Rigidbody2D>();
        UpdateDefaultInteraction();
    }
    public HashSet<Interaction> UpdateDefaultInteraction(){
        defaultInteraction = null;
        Inventory inv = GetComponent<Inventory>();
        HashSet<Interaction> manualActions = new HashSet<Interaction>(Interactor.GetInteractions(gameObject, gameObject));
        // if (inv != null){
        //     if (inv.holding) {
        //         manualActions.UnionWith(Interactor.GetInteractions(inv.holding.gameObject, gameObject));
        //     }
        // }
        defaultInteraction = Interactor.GetDefaultAction(manualActions);
        return manualActions;
    }
    void HandleHitStun(MessageHitstun message){
        hitState = message.hitState;
    }
    void HandleDirectable(MessageDirectable message){
        if (message.addDirectable != null)
            directables.Add(message.addDirectable);
        if (message.removeDirectable != null)
            directables.Remove(message.removeDirectable);
    }
    private void SetControl(ControlType type) {
        switch (type) {
            case ControlType.AI:
                if (decisionMaker)
                    decisionMaker.enabled = true;
                break;
            case ControlType.player:
                if (decisionMaker)
                    decisionMaker.enabled = false;
                break;
            case ControlType.none:
            default:
                if (decisionMaker)
                    decisionMaker.enabled = false;
                break;
        }
    }
    void Update() {
        if (hitState > 0) {
            ResetInput();
        }
        if (hitState > Controllable.HitState.none) {
            ResetInput();
            return;
        }
        if (rightFlag || leftFlag)
            lastPressed = "right";
        if (downFlag)
            lastPressed = "down";
        if (upFlag)
            lastPressed = "up";
        // update direction vector if speed is above a certain value
        if (GetComponent<Rigidbody2D>().velocity.normalized.magnitude > 0.1) {// && (upFlag || downFlag || leftFlag || rightFlag) ){
            SetDirection(rigidBody2D.velocity.normalized);
            directionAngle = Toolbox.Instance.ProperAngle(direction.x, direction.y);
        }
    }
    public virtual void SetDirection(Vector2 d) {
        d = d.normalized;
        if (d == Vector2.zero)
            return;
        direction = d;
    }
    public void ShootPressed() {
        if (fightMode){
            Toolbox.Instance.SendMessage(gameObject, this, new MessagePunch());
        } else {
            if (defaultInteraction != null){
                defaultInteraction.DoAction();
            }
        }
    }
    public void ShootHeld() {
        if (defaultInteraction != null && defaultInteraction.continuous) {
            defaultInteraction.DoAction();
        }
    }
    public virtual void ToggleFightMode() {
        fightMode = !fightMode;
    }
    
}
