using UnityEngine;
using System.Collections.Generic;
using System;


public class TerminateOnStop : MonoBehaviour {
    public Rigidbody2D myRigidBody;
    List<Type> typesToRemove = new List<Type>() { typeof(Rigidbody2D), typeof(Collider2D), typeof(FlipSpriteRandom), typeof(OrderByY), typeof(FlipScintillator) };
    void Awake() {
        myRigidBody = GetComponent<Rigidbody2D>();
    }
    void Update() {
        if (myRigidBody.velocity.magnitude < 0.01) {
            DestroyComponents();
            PhysicalBootstrapper pb = GetComponent<PhysicalBootstrapper>();
            if (pb) {
                pb.DestroyPhysical();
                Destroy(pb);
            }
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            renderer.sortingLayerName = "background";
            renderer.sortingOrder = 50;
            Destroy(this);
        }
    }
    void DestroyComponents() {
        foreach (Type type in typesToRemove) {
            Component component = GetComponent(type);
            while (component != null) {
                DestroyImmediate(component);
                component = GetComponent(type);
            }
        }
    }
}

