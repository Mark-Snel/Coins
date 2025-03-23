using UnityEngine;
using System.Collections.Generic;

public static class ProjectilePool {
    private static int poolSize = 300;
    private static Queue<Projectile> pool = new Queue<Projectile>();
    private static GameObject projectileContainer;

    public static void Reset() {
        pool = new Queue<Projectile>();
    }

    public static Projectile GetProjectile() {
        if (pool.Count > 0) {
            Projectile projectile = pool.Dequeue();
            projectile.gameObject.SetActive(true);
            return projectile;
        } else {
            ExpandPool();
            return GetProjectile();
        }
    }

    public static void ReturnProjectile(Projectile projectile) {
        projectile.gameObject.SetActive(false);
        pool.Enqueue(projectile);
    }

    private static void ExpandPool() {
        if (projectileContainer == null) {
            projectileContainer = GameObject.Find("ProjectileContainer");
            if (projectileContainer == null) {
                projectileContainer = new GameObject("ProjectileContainer");
                GameObject.DontDestroyOnLoad(projectileContainer);
            }
        }
        for (int i = 0; i < poolSize; i++) {
            GameObject projectilePrefab = Resources.Load<GameObject>("Player/Weapon/Projectile/Projectile");
            GameObject projectile = GameObject.Instantiate(projectilePrefab, projectileContainer.transform);
            projectile.SetActive(false);
            pool.Enqueue(projectile.GetComponent<Projectile>());
        }
    }
}
