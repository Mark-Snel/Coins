using UnityEngine;
using System;
using System.Collections.Generic;

public static class ObjectPool<T> where T : Component {
    private static Dictionary<Type, Queue<T>> pools = new Dictionary<Type, Queue<T>>();
    private static Dictionary<Type, GameObject> poolContainers = new Dictionary<Type, GameObject>();
    private static int defaultPoolSize = 300;

    public static void Reset() {
        pools.Clear();
    }

    public static T Get(GameObject prefab = null) {
        Type type = typeof(T);

        if (prefab == null) {
            prefab = LoadPrefabBasedOnType(type);
        }

        if (!pools.ContainsKey(type)) {
            pools[type] = new Queue<T>();
            ExpandPool(prefab, defaultPoolSize);
        }

        if (pools[type].Count > 0) {
            T obj = pools[type].Dequeue();
            obj.gameObject.SetActive(true);
            return obj;
        } else {
            ExpandPool(prefab, defaultPoolSize / 2);
            return Get(prefab);
        }
    }

    public static void Return(T obj) {
        Type type = typeof(T);

        if (!pools.ContainsKey(type)) {
            pools[type] = new Queue<T>();
        }

        obj.gameObject.SetActive(false);
        pools[type].Enqueue(obj);
    }

    private static void ExpandPool(GameObject prefab, int amount) {
        if (prefab == null) {
            Debug.LogError("Prefab is null, cannot expand pool.");
            return;
        }

        Type type = prefab.GetComponent<T>().GetType();

        if (!poolContainers.ContainsKey(type)) {
            GameObject container = new GameObject(type.Name + "_Pool");
            GameObject.DontDestroyOnLoad(container);
            poolContainers[type] = container;
        }

        for (int i = 0; i < amount; i++) {
            GameObject instance = GameObject.Instantiate(prefab, poolContainers[type].transform);
            instance.SetActive(false);
            pools[type].Enqueue(instance.GetComponent<T>());
        }
    }

    private static GameObject LoadPrefabBasedOnType(Type type) {
        if (type == typeof(Projectile)) {
            return Resources.Load<GameObject>("Player/Weapon/Projectile/Projectile");
        } else if (type == typeof(ImpactEffect)) {
            return Resources.Load<GameObject>("Player/Weapon/Projectile/ImpactEffect");
        }

        Debug.LogError("Prefab for type not found: " + type.Name);
        return null;
    }
}
