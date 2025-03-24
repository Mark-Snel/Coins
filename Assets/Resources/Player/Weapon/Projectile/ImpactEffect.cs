using UnityEngine;

public class ImpactEffect : MonoBehaviour {
    private float lifetime;
    private float timer;

    public void Activate(float duration) {
        lifetime = duration;
        timer = 0f;
        gameObject.SetActive(true);
    }

    private void Update() {
        timer += Time.deltaTime;
        if (timer >= lifetime) {
            ReturnToPool();
        }
    }

    private void ReturnToPool() {
        gameObject.SetActive(false);
        ObjectPool<ImpactEffect>.Return(this);
    }
}
