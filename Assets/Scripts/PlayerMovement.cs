using UnityEngine;

// Pertemuan 5: menggunakan input Horizontal/Vertical dan Rigidbody.MovePosition.
// Ditambah tombol layar sentuh agar build Pertemuan 7 juga dapat dimainkan.
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 3f;

    private Rigidbody rb;
    private float horizontal;
    private float vertical;
    private Vector2 mobileDirection = Vector2.zero;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetMobileDirection(Vector2 direction)
    {
        mobileDirection = Vector2.ClampMagnitude(direction, 1f);
    }

    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
    }

    void FixedUpdate()
    {
        Vector2 desktop = new Vector2(horizontal, vertical);
        Vector2 combined = Vector2.ClampMagnitude(desktop + mobileDirection, 1f);
        Vector3 direction = new Vector3(combined.x, 0f, combined.y);
        Vector3 nextPosition = rb.position + direction * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(nextPosition);
    }

    void OnDisable()
    {
        mobileDirection = Vector2.zero;
    }
}
