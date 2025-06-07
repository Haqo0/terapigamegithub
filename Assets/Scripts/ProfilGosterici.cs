using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ProfilGosterici : MonoBehaviour
{
    public GameObject profilPanel;
    public GameObject crosshairObjesi;
    private MonoBehaviour kameraKontrolScripti;

    [Header("UI Alanları")]
    public Text isimText;
    public Text yasText;
    public Text meslekText;
    public Text ozetText;

    [Header("Karakter Ayarı")]
    [Tooltip("Sadece karakterin adı: mert, ece, seda vs. JSON dosyasında '_profil.json' uzantısı otomatik eklenir.")]
    public string karakterDosyaAdi = "mert";  // Örn: "ece" -> ece_profil.json

    void Start()
    {
        kameraKontrolScripti = Camera.main.GetComponent<MouseCameraKontrol>();

        if (profilPanel != null)
            profilPanel.SetActive(false);
    }

    public void ProfilPanelAc()
    {
        if (profilPanel != null)
            profilPanel.SetActive(true);

        if (crosshairObjesi != null)
            crosshairObjesi.SetActive(false);

        if (kameraKontrolScripti != null)
            kameraKontrolScripti.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        KarakterBilgisiYukle();
    }

    public void ProfilPanelKapat()
    {
        if (profilPanel != null)
            profilPanel.SetActive(false);

        if (crosshairObjesi != null)
            crosshairObjesi.SetActive(true);

        if (kameraKontrolScripti != null)
            kameraKontrolScripti.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void KarakterBilgisiYukle()
    {
        string dosyaAdi = karakterDosyaAdi.ToLower() + "_profil.json";
        string dosyaYolu = Path.Combine(Application.streamingAssetsPath, dosyaAdi);

        if (File.Exists(dosyaYolu))
        {
            string json = File.ReadAllText(dosyaYolu);
            KarakterProfil veri = JsonUtility.FromJson<KarakterProfil>(json);

            if (isimText != null) isimText.text = veri.isim;
            if (yasText != null) yasText.text = "Yaş: " + veri.yas.ToString();
            if (meslekText != null) meslekText.text = "Meslek: " + veri.meslek;
            if (ozetText != null) ozetText.text = veri.ozet;
        }
        else
        {
            Debug.LogWarning("Profil dosyası bulunamadı: " + dosyaYolu);
        }
    }
}

[System.Serializable]
public class KarakterProfil
{
    public string isim;
    public int yas;
    public string meslek;
    public string ozet;
}