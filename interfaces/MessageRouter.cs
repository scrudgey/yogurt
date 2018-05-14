using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MessageRouter : MonoBehaviour{
    public SerializableDictionary<Type, Action<Message>> messageHandlers = new SerializableDictionary<Type, Action<Message>>();
    // public Action<Message> postMessageAction;
    public void Subscribe<T>(Action<T> handler) where T: Message {
        Type type = typeof(T);
        // wrapper takes a generic message and casts it to specific subclass before invoking
        Action<Message> wrapper = message => handler((T)message);
        if (messageHandlers.ContainsKey(type)){
            messageHandlers[type] += wrapper;
        } else {
            messageHandlers.Add(type, wrapper);
        }
    }
    public void ReceiveMessage(Message message){
        Type type = message.GetType();
        // Debug.Log("received message of type "+type.ToString());
        // Debug.Log(messageHandlers.ContainsKey(type));
        if (messageHandlers.ContainsKey(type) && messageHandlers[type] != null){
            messageHandlers[type](message);
        }
        if (messageHandlers.ContainsKey(typeof(Message)) && messageHandlers[typeof(Message)] != null){
             messageHandlers[typeof(Message)](message);
        }
    }
}