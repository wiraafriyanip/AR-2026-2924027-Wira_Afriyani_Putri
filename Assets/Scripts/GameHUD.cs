using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameHUD : MonoBehaviour
{
    public Text statusText;
    public Button restartButton;

    void Awake()
    {
        if (restartButton != null) restartButton.onClick.AddListener(RestartScene);
    }

    public void OnObstacleHit(string obstacleName)
    {
        if (statusText != null) statusText.text = "Penghalang: " + obstacleName + "  |  Cari jalan lain!";
    }

    public void OnTargetReached()
    {
        if (statusText != null) statusText.text = "BERHASIL! Target telah disentuh. Tekan ULANG untuk mencoba lagi.";
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
