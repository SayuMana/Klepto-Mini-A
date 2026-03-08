using UnityEngine;

public class GameManager : MonoBehaviour
{
    public void PlayerCaught()
    {
        Debug.Log("GAME OVER — Player tertangkap!");
        // Tambahkan logika: tampil UI, reload scene, dll
        // Contoh: SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}