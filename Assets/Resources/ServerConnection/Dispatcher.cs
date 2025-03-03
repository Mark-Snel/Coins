using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Dispatcher : MonoBehaviour {
    private static SynchronizationContext unityContext;
    private static readonly Queue<Action> executionQueue = new Queue<Action>();
    private static Dispatcher instance;
    public static event Action OnFixedUpdate;
    public static event Action OnUpdate;

    void Awake() {
        unityContext = SynchronizationContext.Current;
    }

    public static void StartCoro(Func<IEnumerator> action) {
        Initialize();
        instance.StartCoroutine(action.Invoke());
    }

    public static void Initialize() {
        if (instance == null) {
            var obj = new GameObject("Dispatcher");
            instance = obj.AddComponent<Dispatcher>();
            DontDestroyOnLoad(obj);
        }
    }

    public static void MainNow(SendOrPostCallback callback, object state = null) {
        if (Thread.CurrentThread.ManagedThreadId == 1) {
            callback(state); // Already on main thread
        } else {
            unityContext.Send(callback, state);
        }
    }

    public static void Enqueue(Action action) {
        lock (executionQueue) {
            executionQueue.Enqueue(action);
        }
    }

    public static void ClearFixedUpdate() {
        OnFixedUpdate = null;
    }

    void Update() {
        OnUpdate?.Invoke();
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
