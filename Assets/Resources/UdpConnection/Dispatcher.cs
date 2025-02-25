using System;
using System.Collections.Generic;
using UnityEngine;

public class Dispatcher : MonoBehaviour {
    private static readonly Queue<Action> executionQueue = new Queue<Action>();
    private static Dispatcher instance;

    public static void Instantiate() {
        if (instance == null) {
            var obj = new GameObject("Dispatcher");
            instance = obj.AddComponent<Dispatcher>();
            DontDestroyOnLoad(obj);
        }
    }

    public static void Enqueue(Action action) {
        if (instance == null) {
            var obj = new GameObject("Dispatcher");
            instance = obj.AddComponent<Dispatcher>();
            DontDestroyOnLoad(obj);
        }
        lock (executionQueue) {
            executionQueue.Enqueue(action);
        }
    }

    void Update() {
        while (executionQueue.Count > 0) {
            Action action;
            lock (executionQueue) {
                action = executionQueue.Dequeue();
            }
            action?.Invoke();
        }
    }
}
