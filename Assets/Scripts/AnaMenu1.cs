using UnityEngine;
using UnityEngine.SceneManagement;

public class AnaMenu : MonoBehaviour
{
    // "Oyuna Başla" butonuna atanacak
    public void OyunaBasla()
    {
        SceneManager.LoadScene("SampleScene");  // Oyun sahnenin adı neyse o
    }

    // "Oyundan Çık" butonuna atanacak
    public void OyundanCik()
    {
        Application.Quit();
        Debug.Log("Oyun kapatılıyor... (Bu sadece build'de çalışır)");
    }
}