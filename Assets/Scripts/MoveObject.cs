using UnityEngine;

public class MoveObject : MonoBehaviour
{
    public float speed = 2f;
    public float direction = 1f;

    void Update()
    {
        transform.Translate(0f, 0f, direction * speed * Time.deltaTime);
    }
}
