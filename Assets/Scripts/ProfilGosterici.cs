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

    [Header("JSON Dosya Adı")]
    public string karakterDosyaAdi = "mert"; // örnek: "mert", "seda"

    void Start()
    {
        // Kamera kontrol scriptini al
        kameraKontrolScripti = Camera.main.GetComponent<MouseCameraKontrol>();

        // Başlangıçta panel kapalı
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
        string dosyaYolu = Path.Combine(Application.streamingAssetsPath, karakterDosyaAdi + "_profil.json");

        if (File.Exists(dosyaYolu))
        {
            string json = File.ReadAllText(dosyaYolu);
            KarakterProfil veri = JsonUtility.FromJson<KarakterProfil>(json);

            isimText.text = veri.isim;
            yasText.text = "Yaş: " + veri.yas.ToString();
            meslekText.text = "Meslek: " + veri.meslek;
            ozetText.text = veri.ozet;
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