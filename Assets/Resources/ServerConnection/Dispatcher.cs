using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dispatcher : MonoBehaviour {
    private static readonly Queue<Action> executionQueue = new Queue<Action>();
    private static Dispatcher instance;
    public static event Action OnFixedUpdate;

    public static void StartCoro(Func<IEnumerator> action) {
        Create();
        instance.StartCoroutine(action.Invoke());
    }

    public static void Create() {
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

    public static void ClearFixedUpdate() {
        OnFixedUpdate = null;
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

    void FixedUpdate() {
        OnFixedUpdate?.Invoke();
    }
}
