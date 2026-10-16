using UnityEngine;
using UnityEngine.EventSystems;

// UI pointer events: kontrol WASD versi touch untuk smartphone Android.
public class MobileDirectionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public PlayerMovement player;
    public Vector2 direction = Vector2.up;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (player != null) player.SetMobileDirection(direction);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (player != null) player.SetMobileDirection(Vector2.zero);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (player != null) player.SetMobileDirection(Vector2.zero);
    }

    void OnDisable()
    {
        if (player != null) player.SetMobileDirection(Vector2.zero);
    }
}
