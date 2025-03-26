using System.Collections.Generic;

public static class ProjectileRegistry {
    public static Dictionary<(byte fromPlayerId, int projectileId), Projectile> RegisteredProjectiles = new();

    // Register a projectile.
    public static void RegisterProjectile(Projectile projectile, byte fromPlayerId, int projectileId) {
        var key = (fromPlayerId, projectileId);
        RegisteredProjectiles[key] = projectile;
    }

    // Unregister a projectile.
    public static void UnregisterProjectile(byte fromPlayerId, int projectileId) {
        var key = (fromPlayerId, projectileId);
        RegisteredProjectiles.Remove(key);
    }
}
