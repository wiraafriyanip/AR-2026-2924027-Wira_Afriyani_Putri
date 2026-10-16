using UnityEngine;

// Pertemuan 5: membedakan Trigger (Target) dan Collision (Obstacle).
public class PlayerInteraction : MonoBehaviour
{
    public GameHUD hud;
    public AudioClip targetSound;
    public AudioClip obstacleSound;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        HandleContact(other.gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        HandleContact(collision.gameObject);
    }

    private void HandleContact(GameObject touchedObject)
    {
        if (touchedObject.CompareTag("Target"))
        {
            Debug.Log("Target tersentuh. Praktikum berhasil.");
            touchedObject.SetActive(false);
            if (hud != null) hud.OnTargetReached();
            if (audioSource != null && targetSound != null)
                audioSource.PlayOneShot(targetSound);
        }
        else if (touchedObject.CompareTag("Obstacle"))
        {
            Debug.Log("Obstacle tersentuh: " + touchedObject.name);
            if (hud != null) hud.OnObstacleHit(touchedObject.name);
            if (audioSource != null && obstacleSound != null)
                audioSource.PlayOneShot(obstacleSound);
        }
    }
}
