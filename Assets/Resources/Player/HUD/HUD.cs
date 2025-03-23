using System.Collections.Generic;
using UnityEngine;

public class HUD : MonoBehaviour {
    public GameObject bulletPrefab;
    private Vector2 startPosition = new Vector2(-0.675f, 0);
    private float spacing = 0.3375f;
    private int maxSquaresPerRow = 5;

    public Transform ammoContainer;
    private int currentAmmo = 0;
    public Transform reloadBar;
    public Transform reloadProgressBar;

    public Transform healthBar;

    private List<GameObject> ammoSquares = new List<GameObject>();

    void Start() {
        ammoContainer = transform.Find("Ammo");
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
        healthBar.localScale = new Vector3((float)health/(float)maxHealth, healthBar.localScale.y, healthBar.localScale.z);
    }
}
