using UnityEngine;

public static class Shop {
    private static float Speed = 2f;
    private static float Acceleration = 4f;
    private static float JumpHeight = 12f;
    private static int JumpLength = 12;
    private static int MaxJumps = 1;
    private static float MassPerSize = 1f;
    private static float MassMultiplier = 1f;
    private static float BaseSize = 0.1f;
    private static int MaxHealth = 100;
    private static float MaxHealth_SizeMultiplier = 0.01f;

    private static bool Automatic = false;
    private static int ReloadTime = 150;
    private static int MaxAmmoCount = 3;
    private static int BurstSize = 1;
    private static int BurstTimeBetweenShots = 8;
    private static int TimeBetweenShots = 25;
    private static float InheritInertia = 0;
    private static float Recoil = 0;

    private static float Spread = 0;
    private static int AttackLifeTime = 200;
    private static int AttackCount = 1;
    private static float AttackVelocity = 10;
    private static float AttackAcceleration = 0;
    private static float AttackGravity = 0.5f;
    private static float Knockback = 1;
    private static int Damage = 30;

    public static ShopItem[] shopItems;

    private static void SetShopItems() {
        Shop.shopItems = GameObject.FindObjectsByType<ShopItem>(0);
    }

    public static void Deselect() {
        if (shopItems == null) {
            SetShopItems();
        }
        foreach (var shopItem in shopItems) {
            if (shopItem == null) {
                SetShopItems();
                Deselect();
                return;
            }
            shopItem?.Deselect();
        }
    }

    public static void ApplyChanges() {
        if (shopItems == null) SetShopItems();

        if (shopItems == null || shopItems.Length <= 0) return;
        float speed = Speed;
        float acceleration = Acceleration;
        float jumpHeight = JumpHeight;
        int jumpLength = JumpLength;
        int maxJumps = MaxJumps;
        float massPerSize = MassPerSize;
        float massMultiplier = MassMultiplier;
        float baseSize = BaseSize;
        int maxHealth = MaxHealth;
        float maxHealth_SizeMultiplier = MaxHealth_SizeMultiplier;

        bool automatic = Automatic;
        int reloadTime = ReloadTime;
        int maxAmmoCount = MaxAmmoCount;
        int burstSize = BurstSize;
        int burstTimeBetweenShots = BurstTimeBetweenShots;
        int timeBetweenShots = TimeBetweenShots;
        float inheritInertia = InheritInertia;
        float recoil = Recoil;

        float spread = Spread;
        int attackLifeTime = AttackLifeTime;
        int attackCount = AttackCount;
        float attackVelocity = AttackVelocity;
        float attackAcceleration = AttackAcceleration;
        float attackGravity = AttackGravity;
        float knockback = Knockback;
        int damage = Damage;

        foreach (var shopItem in shopItems) {
            if (shopItem == null) {
                SetShopItems();
                ApplyChanges();
                return;
            }

            speed += shopItem.GetSpeed() ?? 0f;
            acceleration += shopItem.GetAcceleration() ?? 0f;
            jumpHeight += shopItem.GetJumpHeight() ?? 0f;
            jumpLength += shopItem.GetJumpLength() ?? 0;
            maxJumps += shopItem.GetMaxJumps() ?? 0;
            massPerSize += shopItem.GetMassPerSize() ?? 0f;
            massMultiplier += shopItem.GetMassMultiplier() ?? 0f;
            baseSize += shopItem.GetBaseSize() ?? 0f;
            maxHealth += shopItem.GetMaxHealth() ?? 0;
            maxHealth_SizeMultiplier += shopItem.GetMaxHealth_SizeMultiplier() ?? 0f;

            automatic = shopItem.GetAutomatic() ?? automatic;
            reloadTime += shopItem.GetReloadTime() ?? 0;
            maxAmmoCount += shopItem.GetMaxAmmoCount() ?? 0;
            burstSize += shopItem.GetBurstSize() ?? 0;
            burstTimeBetweenShots += shopItem.GetBurstTimeBetweenShots() ?? 0;
            timeBetweenShots = Mathf.RoundToInt(timeBetweenShots / (((float)(shopItem.GetAttackSpeed() ?? 0) + 100) / 100f));//percentage for this since it worked weirdly
            inheritInertia += shopItem.GetInheritInertia() ?? 0f;
            recoil += shopItem.GetRecoil() ?? 0f;

            spread += shopItem.GetSpread() ?? 0f;
            attackLifeTime += shopItem.GetAttackLifeTime() ?? 0;
            attackCount += shopItem.GetAttackCount() ?? 0;
            attackVelocity += shopItem.GetAttackVelocity() ?? 0f;
            attackAcceleration += shopItem.GetAttackAcceleration() ?? 0f;
            attackGravity += shopItem.GetAttackGravity() ?? 0f;
            knockback += shopItem.GetKnockback() ?? 0f;
            damage = Mathf.Max(damage + Mathf.RoundToInt(Damage * ((float)(shopItem.GetDamage() ?? 0) / 100f)), 0);//also for damage working with percentage but slightly different
        }

        if (PlayerController.Instance != null) {
            PlayerController.Instance.Speed = speed;
            PlayerController.Instance.Acceleration = acceleration;
            PlayerController.Instance.JumpHeight = jumpHeight;
            PlayerController.Instance.JumpLength = jumpLength;
            PlayerController.Instance.MaxJumps = maxJumps;
            PlayerController.Instance.MassPerSize = massPerSize;
            PlayerController.Instance.MassMultiplier = massMultiplier;
            PlayerController.Instance.BaseSize = baseSize;
            PlayerController.Instance.MaxHealth = maxHealth;
            PlayerController.Instance.MaxHealth_SizeMultiplier = maxHealth_SizeMultiplier;
            if (PlayerController.Instance.Weapon != null) {
                PlayerController.Instance.Weapon.Automatic = automatic;
                PlayerController.Instance.Weapon.ReloadTime = reloadTime;
                PlayerController.Instance.Weapon.MaxAmmoCount = maxAmmoCount;
                PlayerController.Instance.Weapon.BurstSize = burstSize;
                PlayerController.Instance.Weapon.BurstTimeBetweenShots = burstTimeBetweenShots;
                PlayerController.Instance.Weapon.TimeBetweenShots = timeBetweenShots;
                PlayerController.Instance.Weapon.InheritInertia = inheritInertia;
                PlayerController.Instance.Weapon.Recoil = recoil;

                PlayerController.Instance.Weapon.Spread = spread;
                PlayerController.Instance.Weapon.AttackLifeTime = attackLifeTime;
                PlayerController.Instance.Weapon.AttackCount = attackCount;
                PlayerController.Instance.Weapon.AttackVelocity = attackVelocity;
                PlayerController.Instance.Weapon.AttackAcceleration = attackAcceleration;
                PlayerController.Instance.Weapon.AttackGravity = attackGravity;
                PlayerController.Instance.Weapon.Knockback = knockback;
                PlayerController.Instance.Weapon.Damage = damage;
            }
        }
    }
}
