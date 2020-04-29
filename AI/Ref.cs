using UnityEngine;
using System;

namespace AI {

    [System.Serializable]
    public class Ref<T> {
        public virtual T val {
            get { return _val; }
            set { _val = value; }
        }
        private T _val;
        public Ref(T t) { val = t; }
    }
    public class LambdaRef<T> : Ref<T> {
        override public T val {
            get {
                if (_val == null) {
                    _val = func();
                }
                return _val;
            }
            set {
                _val = value;
            }
        }
        private T _val;
        public Func<T> func;
        public LambdaRef(T t, Func<T> func) : base(t) {
            _val = func();
            this.func = func;
        }
    }
    // public class WorldRef<T> : Ref<T> where T : Component {
    //     new public T val {
    //         get {
    //             if (_val == null) { UpdateRef(); }
    //             return _val;
    //         }
    //         set { _val = value; }
    //     }
    //     private T _val;
    //     public Ref<GameObject> gameObject = new Ref<GameObject>(null);
    //     public WorldRef(T t) : base(t) {
    //         if (t == null)
    //             UpdateRef();
    //     }
    //     void UpdateRef() {
    //         _val = GameObject.FindObjectOfType<T>();
    //         if (_val != null)
    //             gameObject.val = _val.gameObject;
    //     }
    // }
    // public class LiquidRef : Ref<LiquidContainer> {
    //     new public LiquidContainer val {
    //         get {
    //             if (_val == null) { UpdateRef(); }
    //             return _val;
    //         }
    //         set { _val = value; }
    //     }
    //     public string liquidName;
    //     private LiquidContainer _val;
    //     // public Ref<GameObject> gameObject = new Ref<GameObject>(null);
    //     public Ref<GameObject> gameObject {
    //         get {
    //             Debug.Log("liquid gameobject ref get");
    //             if (_gameObject.val == null) { UpdateRef(); }
    //             return _gameObject;
    //         }
    //         set { _gameObject = value; }
    //     }
    //     private Ref<GameObject> _gameObject = new Ref<GameObject>(null);
    //     public LiquidRef(string liquidName) : base(null) {
    //         this.liquidName = liquidName;
    //         UpdateRef();
    //     }
    //     void UpdateRef() {
    //         foreach (LiquidContainer container in GameObject.FindObjectsOfType<LiquidContainer>()) {
    //             Debug.Log(container);
    //             Debug.Log(container.amount);
    //             Debug.Log(container.liquid.name);
    //             if (container.liquid != null && container.amount > 0 && container.liquid.name == liquidName) {
    //                 // container.liquid.ingredients.Contains(liquidName);
    //                 _val = container;
    //                 break;
    //             }
    //         }
    //         if (_val != null)
    //             gameObject.val = _val.gameObject;
    //     }
    // }
}