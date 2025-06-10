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
        public string karakterAdi;                    // Küçük harf: "mert", "ece" vb.
        [Tooltip("Bu karakterin profil görselleri listesidir. Inspector'dan birden fazla ekleyebilirsiniz.")]
        public List<GameObject> profilGorselleri = new List<GameObject>(); // Birden fazla görsel
    }

    [Header("Karakter Profilleri")]
    public List<KarakterProfil> karakterProfilleri = new List<KarakterProfil>();

    [Header("Görsel Geçiş Butonları")]
    public Button oncekiButon;
    public Button sonrakiButon;

    private string aktifKarakterAdi = "";
    private KarakterProfil currentKarakterProfil = null;
    private int currentImageIndex = 0;

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

        // Buton dinleyicilerini ekle (opsiyonel, Inspector'dan da atanabilir)
        if (oncekiButon != null)
            oncekiButon.onClick.AddListener(PrevImage);
        if (sonrakiButon != null)
            sonrakiButon.onClick.AddListener(NextImage);
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
        currentKarakterProfil = null;
        currentImageIndex = 0;

        // Butonları başta pasif hale getir
        if (oncekiButon != null) oncekiButon.interactable = false;
        if (sonrakiButon != null) sonrakiButon.interactable = false;
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
        currentKarakterProfil = null;
        currentImageIndex = 0;
    }

    // Karakter butonlarına atanacak fonksiyon
    public void ButonaBasildi(string karakterAdi)
    {
        // Önce tüm görselleri kapat
        foreach (var k in karakterProfilleri)
            foreach (var gorsel in k.profilGorselleri)
                if (gorsel != null) gorsel.SetActive(false);

        // Yeni karakter profilini bul
        currentKarakterProfil = karakterProfilleri.Find(k => k.karakterAdi == karakterAdi);
        aktifKarakterAdi = karakterAdi;
        currentImageIndex = 0;

        if (currentKarakterProfil != null && currentKarakterProfil.profilGorselleri.Count > 0)
        {
            // İlk görseli göster
            if (currentKarakterProfil.profilGorselleri[currentImageIndex] != null)
                currentKarakterProfil.profilGorselleri[currentImageIndex].SetActive(true);

            // Eğer birden fazla görsel varsa butonları aktif et
            bool birdenFazla = currentKarakterProfil.profilGorselleri.Count > 1;
            if (oncekiButon != null) oncekiButon.interactable = birdenFazla;
            if (sonrakiButon != null) sonrakiButon.interactable = birdenFazla;
        }
    }

    // Sonraki görsel
    public void NextImage()
    {
        if (currentKarakterProfil == null) return;

        int count = currentKarakterProfil.profilGorselleri.Count;
        if (count <= 1) return;

        // Mevcut görseli gizle
        var gorsel = currentKarakterProfil.profilGorselleri[currentImageIndex];
        if (gorsel != null) gorsel.SetActive(false);

        // İndeksi artır
        currentImageIndex = (currentImageIndex + 1) % count;

        // Yeni görseli göster
        var yeniGorsel = currentKarakterProfil.profilGorselleri[currentImageIndex];
        if (yeniGorsel != null) yeniGorsel.SetActive(true);
    }

    // Önceki görsel
    public void PrevImage()
    {
        if (currentKarakterProfil == null) return;

        int count = currentKarakterProfil.profilGorselleri.Count;
        if (count <= 1) return;

        // Mevcut görseli gizle
        var gorsel = currentKarakterProfil.profilGorselleri[currentImageIndex];
        if (gorsel != null) gorsel.SetActive(false);

        // İndeksi azalt (döngüsel)
        currentImageIndex = (currentImageIndex - 1 + count) % count;

        // Yeni görseli göster
        var yeniGorsel = currentKarakterProfil.profilGorselleri[currentImageIndex];
        if (yeniGorsel != null) yeniGorsel.SetActive(true);
    }
}
