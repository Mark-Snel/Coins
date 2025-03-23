using System.Collections.Generic;
using UnityEngine;

public class HUD : MonoBehaviour {
    private GameObject bulletPrefab;
    private Vector2 startPosition = new Vector2(-0.675f, 0);
    private float spacing = 0.3375f;
    private int maxSquaresPerRow = 5;

    private Transform ammoContainer;
    private int currentAmmo = 0;
    private Transform reloadBar;
    private Transform reloadProgressBar;

    private Transform HealthBar;

    private List<GameObject> ammoSquares = new List<GameObject>();

    void Start() {
        bulletPrefab = Resources.Load<GameObject>("Player/HUD/Bullet");
        ammoContainer = transform.Find("Ammo");
        reloadBar = transform.Find("ReloadBar");
        reloadProgressBar = reloadBar?.Find("Progress");
        HealthBar = transform.Find("HealthBar")?.Find("Health");
    }
    public void UpdateReload(int reloadTime, int reloadProgress) {
        if (currentAmmo <= 0) {
            reloadBar.gameObject.SetActive(true);
            reloadProgressBar.localScale = new Vector3(1 - reloadProgress/Mathf.Max(reloadTime, 0.1f), reloadProgressBar.localScale.y, reloadProgressBar.localScale.z);
        } else {
            reloadBar.gameObject.SetActive(false);
        }
    }

    public void UpdateMaxAmmo(int maxAmmo) {
        while(ammoSquares.Count < maxAmmo) {
            int index = ammoSquares.Count;
            int row = index / maxSquaresPerRow;
            int col = index % maxSquaresPerRow;
            Vector2 pos = startPosition + new Vector2(col * spacing, row * spacing);
            GameObject newBullet = Instantiate(bulletPrefab, ammoContainer);
            newBullet.transform.localPosition = pos;
            ammoSquares.Add(newBullet);
        }
    }
    public void UpdateAmmo(int currentAmmo) {
        if (currentAmmo > 0) {
            reloadBar.gameObject.SetActive(false);
        }
        this.currentAmmo = currentAmmo;
        for(int i = 0; i < ammoSquares.Count; i++) {
            ammoSquares[i].SetActive(i < currentAmmo);
        }
    }

    public void UpdateHealth(int health, int maxHealth) {
        HealthBar.localScale = new Vector3((float)health/(float)maxHealth, HealthBar.localScale.y, HealthBar.localScale.z);
    }
}
