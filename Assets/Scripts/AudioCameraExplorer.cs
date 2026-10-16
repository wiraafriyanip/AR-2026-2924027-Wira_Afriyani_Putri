using UnityEngine;

// Pertemuan 3: gerakkan kamera saat Play untuk mendengar efek 3D dekat-jauh.
public class AudioCameraExplorer : MonoBehaviour
{
    public float movementSpeed = 5f;
    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float height = 0f;
        if (Input.GetKey(KeyCode.E)) height += 1f;
        if (Input.GetKey(KeyCode.Q)) height -= 1f;
        Vector3 movement = new Vector3(horizontal, height, vertical).normalized;
        transform.position += movement * movementSpeed * Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.R)) transform.position = initialPosition;
    }
}
