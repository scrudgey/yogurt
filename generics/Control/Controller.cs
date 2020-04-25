using UnityEngine;
using System.Collections.Generic;
using System;
public enum DirectionEnum { left, right, up, down, none }

public class Controller : IDisposable {
    public void Dispose() {
        Deregister();
        // GC.SuppressFinalize(this);
    }
    public Controller() { }
    public Controller(GameObject g) {
        if (g != null)
            Register(g.GetComponent<Controllable>());
    }
    public Controller(Controllable controllable) {
        if (controllable != null)
            Register(controllable);
    }
    // class to use to interface with ControllableProxy
    // must use this instead of using Controllable directly.
    // ensures that we interfce with the controllable proxy

    // anything that Controls needs to subclass Controller,
    // or instantiate a Controller.
    // then call controller.register(Controllable) instead of GetComponent<Controllable>()

    // leftflag, rightflag, etc.
    // register(IControllable) <- use this instead of GetComponent<Controllable>()
    // calls setDirection() on Controllable

    // Controllable
    // setDirection(direction, IController)
    //      checks vs. controller stack to see if we accept the input
    //      sets the flags of controllable, which are private.
    // stack of controllers
    //

    public Controllable controllable;
    public void Register(Controllable c) {
        // deregsiter existing controllable
        // add to control stack
        // set _controllable
        Deregister();
        c.Register(this);
        this.controllable = c;
    }
    public void Deregister() {
        if (controllable != null) {
            controllable.Deregister(this);
        }
        controllable = null;
    }

    private bool _upFlag;
    private bool _downFlag;
    private bool _leftFlag;
    private bool _rightFlag;
    public bool upFlag {
        get { return _upFlag; }
        set {
            SetDirection(DirectionEnum.up, value);
            _upFlag = value;
        }
    }
    public bool downFlag {
        get { return _downFlag; }
        set {
            SetDirection(DirectionEnum.down, value);
            _downFlag = value;
        }
    }
    public bool leftFlag {
        get { return _leftFlag; }
        set {
            SetDirection(DirectionEnum.left, value);
            _leftFlag = value;
        }
    }
    public bool rightFlag {
        get { return _rightFlag; }
        set {
            SetDirection(DirectionEnum.right, value);
            _rightFlag = value;
        }
    }

    private void SetDirection(DirectionEnum d, bool value) {
        if (controllable != null) {
            controllable.SetDirectionFlag(d, value, this);
        } else {
            Debug.LogError("SetDirection called on null controllable. Did you Register first?");
        }
    }
    public void ResetInput() {
        SetDirection(DirectionEnum.none, false);
    }

    public virtual void GainedControl(Controllable controllable) {

    }
    public virtual void LostControl(Controllable controllable) {

    }
    public void ShootPressed() {
        if (controllable != null) {
            controllable.ShootPressed(this);
        }
    }
    public void ShootHeld() {
        if (controllable != null) {
            controllable.ShootHeld(this);
        }
    }

    public void LookAtPoint(Vector3 target) {
        if (controllable != null) {
            controllable.LookAtPoint(target, this);
        }
    }
    public void SetDirection(Vector2 d) {
        if (controllable != null) {
            controllable.SetDirection(d, this);
        }
    }
    public void ToggleFightMode() {
        if (controllable != null) {
            controllable.ToggleFightMode(this);
        }
    }
}