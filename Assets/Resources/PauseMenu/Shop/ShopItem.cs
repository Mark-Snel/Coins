using System;
using UnityEngine;
using TMPro;

public class ShopItem : MonoBehaviour {
    [SerializeField] private NullableValue<bool> automatic;
    [SerializeField] private NullableValue<int> reloadTime;
    [SerializeField] private NullableValue<int> maxAmmoCount;
    [SerializeField] private NullableValue<int> burstSize;
    [SerializeField] private NullableValue<int> burstTimeBetweenShots;
    [SerializeField] private NullableValue<int> attackSpeed;
    [SerializeField] private NullableValue<float> inheritInertia;
    [SerializeField] private NullableValue<float> recoil;

    [SerializeField] private NullableValue<float> spread;
    [SerializeField] private NullableValue<int> attackLifeTime;
    [SerializeField] private NullableValue<int> attackCount;
    [SerializeField] private NullableValue<float> attackVelocity;
    [SerializeField] private NullableValue<float> attackAcceleration;
    [SerializeField] private NullableValue<float> attackGravity;
    [SerializeField] private NullableValue<float> knockback;
    [SerializeField] private NullableValue<int> damage;

    [SerializeField] private NullableValue<float> speed;
    [SerializeField] private NullableValue<float> acceleration;
    [SerializeField] private NullableValue<float> jumpHeight;
    [SerializeField] private NullableValue<int> jumpLength;
    [SerializeField] private NullableValue<int> maxJumps;
    [SerializeField] private NullableValue<float> massPerSize;
    [SerializeField] private NullableValue<float> massMultiplier;
    [SerializeField] private NullableValue<float> baseSize;
    [SerializeField] private NullableValue<int> maxHealth;
    [SerializeField] private NullableValue<float> maxHealth_SizeMultiplier;

    public int level = 0;
    public int maxUnlocked = 0;
    public int minUnlocked = 0;
    public int max = int.MaxValue;
    public int min = int.MinValue;
    public int pricePerLevel = 1;

    private float? ApplyLevel(NullableValue<float> value) => value.HasValue ? value * level : null;
    private int? ApplyLevel(NullableValue<int> value) => value.HasValue ? value * level : null;
    private bool? ApplyLevel(NullableValue<bool> value) => value.HasValue ? (level > 0 ? value.Value : !value.Value) : null;

    public bool? GetAutomatic() => ApplyLevel(automatic);
    public int? GetReloadTime() => ApplyLevel(reloadTime);
    public int? GetMaxAmmoCount() => ApplyLevel(maxAmmoCount);
    public int? GetBurstSize() => ApplyLevel(burstSize);
    public int? GetBurstTimeBetweenShots() => ApplyLevel(burstTimeBetweenShots);
    public int? GetAttackSpeed() => ApplyLevel(attackSpeed);
    public float? GetInheritInertia() => ApplyLevel(inheritInertia);
    public float? GetRecoil() => ApplyLevel(recoil);

    public float? GetSpread() => ApplyLevel(spread);
    public int? GetAttackLifeTime() => ApplyLevel(attackLifeTime);
    public int? GetAttackCount() => ApplyLevel(attackCount);
    public float? GetAttackVelocity() => ApplyLevel(attackVelocity);
    public float? GetAttackAcceleration() => ApplyLevel(attackAcceleration);
    public float? GetAttackGravity() => ApplyLevel(attackGravity);
    public float? GetKnockback() => ApplyLevel(knockback);
    public int? GetDamage() => ApplyLevel(damage);

    public float? GetSpeed() => ApplyLevel(speed);
    public float? GetAcceleration() => ApplyLevel(acceleration);
    public float? GetJumpHeight() => ApplyLevel(jumpHeight);
    public int? GetJumpLength() => ApplyLevel(jumpLength);
    public int? GetMaxJumps() => ApplyLevel(maxJumps);
    public float? GetMassPerSize() => ApplyLevel(massPerSize);
    public float? GetMassMultiplier() => ApplyLevel(massMultiplier);
    public float? GetBaseSize() => ApplyLevel(baseSize);
    public int? GetMaxHealth() => ApplyLevel(maxHealth);
    public float? GetMaxHealth_SizeMultiplier() => ApplyLevel(maxHealth_SizeMultiplier);

    public ShopItemButton upgradeButton;
    public TMP_Text upgradePriceTag;
    public ShopItemButton downgradeButton;
    public TMP_Text downgradePriceTag;
    public TMP_Text title;
    public string titleText;

    public void Deselect() {
        upgradeButton.selected = false;
        downgradeButton.selected = false;
    }

    void Start() {
        titleText = title.text;
        title.text = titleText + ": " + level;
        SetPriceTags();
        if (level <= min) {
            downgradeButton.selected = false;
            downgradeButton.gameObject.SetActive(false);
        }
        if (level >= max) {
            upgradeButton.selected = false;
            upgradeButton.gameObject.SetActive(false);
        }
    }

    void SetPriceTags() {
        if (maxUnlocked > level) {
            upgradePriceTag.text = "€ 0";
        } else {
            upgradePriceTag.text = "€ " + pricePerLevel;
        }
        if (minUnlocked < level) {
            downgradePriceTag.text = "€ 0";
        } else {
            downgradePriceTag.text = "€ " + pricePerLevel;
        }
    }

    public void Upgrade(bool positive) {
        if (positive) {
            if (level < max) {
                if (maxUnlocked < level + 1) {
                    if (GameController.GetTotalCoins() >= pricePerLevel) {
                        level++;
                        title.text = titleText + ": " + level;
                        maxUnlocked = level;
                        GameController.LoseCoins(pricePerLevel);
                    }
                } else {
                    level++;
                    title.text = titleText + ": " + level;
                }
                if (level >= max) {
                    upgradeButton.selected = false;
                    upgradeButton.gameObject.SetActive(false);
                }
                if (level > min) {
                    downgradeButton.selected = false;
                    downgradeButton.gameObject.SetActive(true);
                }
                SetPriceTags();
                Shop.ApplyChanges();
            }
        } else {
            if (level > min) {
                if (minUnlocked > level - 1) {
                    if (GameController.GetTotalCoins() >= pricePerLevel) {
                        level--;
                        title.text = titleText + ": " + level;
                        minUnlocked = level;
                        GameController.LoseCoins(pricePerLevel);
                    }
                } else {
                    level--;
                    title.text = titleText + ": " + level;
                }
                if (level <= min) {
                    downgradeButton .selected = false;
                    downgradeButton.gameObject.SetActive(false);
                }
                if (level < max) {
                    upgradeButton .selected = false;
                    upgradeButton.gameObject.SetActive(true);
                }
                SetPriceTags();
                Shop.ApplyChanges();
            }
        }
    }
}

[Serializable]
public struct NullableValue<T> where T : struct {
    [SerializeField] private bool hasValue;
    [SerializeField] private T value;

    public NullableValue(T? initialValue) {
        hasValue = initialValue.HasValue;
        value = initialValue.GetValueOrDefault();
    }

    public T? Value {
        get => hasValue ? value : (T?)null;
        set {
            hasValue = value.HasValue;
            this.value = value.GetValueOrDefault();
        }
    }

    public bool HasValue => hasValue;

    public static implicit operator T?(NullableValue<T> nullableValue) => nullableValue.Value;
    public static implicit operator NullableValue<T>(T? value) => new NullableValue<T>(value);

    public override string ToString() => hasValue ? value.ToString() : "null";
}

