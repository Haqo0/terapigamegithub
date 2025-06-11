using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AnaMenuKontrol : MonoBehaviour
{
    [Header("UI Bileşenleri")]
    public GameObject menuPaneli;        // ESC ile açılıp kapanan menü
    public GameObject crosshairObjesi;   // Crosshair objesi (ESC ile görünürlüğü yönetilecek)

    private bool menuAcik = false;

    void Start()
    {
        menuPaneli.SetActive(false);
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (crosshairObjesi != null)
            crosshairObjesi.SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            menuAcik = !menuAcik;

            if (menuAcik)
                MenuyuAc();
            else
                StartCoroutine(MenuyuKapatGecikmeli());
        }
    }

    public void MenuyuAc()
    {
        menuPaneli.SetActive(true);
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (crosshairObjesi != null)
            crosshairObjesi.SetActive(false);
    }

    private IEnumerator MenuyuKapatGecikmeli()
    {
        menuPaneli.SetActive(false);
        Time.timeScale = 1f;

        yield return new WaitForEndOfFrame(); // ⏳ Frame sonunu bekle

        Cursor.visible = false;

        // 🔒 Cursor lock işlemini garantilemek için iki adımlı "reset"
        Cursor.lockState = CursorLockMode.None;
        yield return null;
        Cursor.lockState = CursorLockMode.Locked;

        if (crosshairObjesi != null)
            crosshairObjesi.SetActive(true);
    }

    public void AnaMenuyeDon()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("AnaMenu");
    }

    public void OyunuKapat()
    {
        Application.Quit();
        Debug.Log("Oyun kapatılıyor... (Bu sadece build alındığında çalışır)");
    }
}