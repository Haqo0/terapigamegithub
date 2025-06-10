using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ProfilGosterici : MonoBehaviour
{
    [Header("Panel ve Crosshair")]
    public GameObject profilPanel;
    public GameObject crosshairObjesi;

    private MonoBehaviour kameraKontrolScripti;

    [System.Serializable]
    public class KarakterProfil
    {
        public string karakterAdi;          // Küçük harf: "mert", "ece" vb.
        [Tooltip("Bu karakterin profil görselleri listesidir. Inspector'dan birden fazla ekleyebilirsiniz.")]
        public List<GameObject> profilGorselleri = new List<GameObject>(); // Birden fazla görsel
    }

    [Header("Karakter Profilleri")]
    public List<KarakterProfil> karakterProfilleri = new List<KarakterProfil>();

    private string aktifKarakterAdi = "";

    void Start()
    {
        // Kamera kontrol scriptini al
        kameraKontrolScripti = Camera.main.GetComponent<MouseCameraKontrol>();

        // Başlangıçta panel ve crosshair ayarları
        if (profilPanel != null) profilPanel.SetActive(false);
        if (crosshairObjesi != null) crosshairObjesi.SetActive(true);

        // Tüm profil görsellerini gizle
        foreach (var karakter in karakterProfilleri)
            foreach (var gorsel in karakter.profilGorselleri)
                if (gorsel != null) gorsel.SetActive(false);
    }

    // Paneli aç ve crosshair/kamera kontrolünü kapat
    public void ProfilPanelAc()
    {
        if (profilPanel != null) profilPanel.SetActive(true);
        if (kameraKontrolScripti != null) kameraKontrolScripti.enabled = false;
        if (crosshairObjesi != null) crosshairObjesi.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Tüm profil görsellerini gizle
        foreach (var karakter in karakterProfilleri)
            foreach (var gorsel in karakter.profilGorselleri)
                if (gorsel != null) gorsel.SetActive(false);

        aktifKarakterAdi = "";
    }

    // Paneli kapat ve crosshair/kamera kontrolünü geri getir
    public void ProfilPanelKapat()
    {
        if (profilPanel != null) profilPanel.SetActive(false);
        if (kameraKontrolScripti != null) kameraKontrolScripti.enabled = true;
        if (crosshairObjesi != null) crosshairObjesi.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Tüm profil görsellerini gizle
        foreach (var karakter in karakterProfilleri)
            foreach (var gorsel in karakter.profilGorselleri)
                if (gorsel != null) gorsel.SetActive(false);

        aktifKarakterAdi = "";
    }

    // Karakter butonlarına atanacak fonksiyon
    public void ButonaBasildi(string karakterAdi)
    {
        foreach (var karakter in karakterProfilleri)
        {
            if (karakter.karakterAdi == karakterAdi)
            {
                bool zatenAcik = false;
                // Seçili karakterin görsellerinden en az biri açıksa zatenAcik true olur
                foreach (var gorsel in karakter.profilGorselleri)
                {
                    if (gorsel != null && gorsel.activeSelf)
                    {
                        zatenAcik = true;
                        break;
                    }
                }

                // Tüm karakterlerin tüm görsellerini kapat
                foreach (var k in karakterProfilleri)
                    foreach (var gorsel in k.profilGorselleri)
                        if (gorsel != null) gorsel.SetActive(false);

                // Eğer zaten açıksa kapat, değilse tüm görselleri aç
                if (!zatenAcik)
                {
                    foreach (var gorsel in karakter.profilGorselleri)
                        if (gorsel != null) gorsel.SetActive(true);
                    aktifKarakterAdi = karakterAdi;
                }
                else
                {
                    aktifKarakterAdi = "";
                }

                break;
            }
        }
    }
}
