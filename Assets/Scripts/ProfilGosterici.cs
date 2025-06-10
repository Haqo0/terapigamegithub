using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
    [Tooltip("Index 0: Ece, Index 1: Mert")]
    public List<KarakterProfil> karakterProfilleri = new List<KarakterProfil>();

    [Header("Görsel Geçiş Butonları")]
    public Button oncekiButon;
    public Button sonrakiButon;

    private int aktifKarakterIndex = -1;
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

        // Buton referanslarını kontrol et ve dinleyicileri ekle
        SetupButtons();
        
        Debug.Log("ProfilGosterici başlatıldı");
    }

    private void SetupButtons()
    {
        Debug.Log("=== SetupButtons çağrıldı ===");
        
        // EventTrigger sistemi ile butonları ayarla
        SetupEventTrigger(oncekiButon, "Önceki", () => PrevImage());
        SetupEventTrigger(sonrakiButon, "Sonraki", () => NextImage());
    }

    private void SetupEventTrigger(Button button, string buttonName, System.Action action)
    {
        if (button == null)
        {
            Debug.LogError($"{buttonName} buton referansı null!");
            return;
        }

        // Önce mevcut listener'ları temizle
        button.onClick.RemoveAllListeners();

        // EventTrigger ekle
        EventTrigger trigger = button.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }
        
        // Mevcut eventleri temizle
        trigger.triggers.Clear();

        // Click eventi ekle
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => {
            Debug.Log($"{buttonName.ToUpper()} BUTONA BASILDI (EventTrigger)!");
            action.Invoke();
        });
        trigger.triggers.Add(entry);

        Debug.Log($"{buttonName} buton EventTrigger ile ayarlandı: {button.name}");
    }

    // Paneli aç ve crosshair/kamera kontrolünü kapat
    public void ProfilPanelAc()
    {
        Debug.Log("ProfilPanelAc() çağrıldı");
        
        if (profilPanel != null) profilPanel.SetActive(true);
        if (kameraKontrolScripti != null) kameraKontrolScripti.enabled = false;
        if (crosshairObjesi != null) crosshairObjesi.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Tüm profil görsellerini gizle
        foreach (var karakter in karakterProfilleri)
            foreach (var gorsel in karakter.profilGorselleri)
                if (gorsel != null) gorsel.SetActive(false);

        // SADECE İLK AÇILIŞTA sıfırla - eğer panel zaten açıksa sıfırlama!
        if (aktifKarakterIndex == -1)
        {
            Debug.Log("İlk açılış - Karakter seçimi sıfırlandı");
            currentKarakterProfil = null;
            currentImageIndex = 0;
            UpdateButtonStates();
        }
        else
        {
            Debug.Log($"Panel zaten açık, mevcut karakter korunuyor: Index {aktifKarakterIndex}");
        }
        
        Debug.Log("Profil paneli açıldı");
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

        // Karakter seçimini sıfırla
        aktifKarakterIndex = -1;
        currentKarakterProfil = null;
        currentImageIndex = 0;
        
        Debug.Log("Profil paneli kapatıldı");
    }

    // Karakter butonlarına atanacak fonksiyon
    public void ButonaBasildi(string karakterAdi)
    {
        Debug.Log($"ButonaBasildi() çağrıldı: {karakterAdi}");

        // Önce tüm görselleri kapat
        foreach (var k in karakterProfilleri)
            foreach (var gorsel in k.profilGorselleri)
                if (gorsel != null) gorsel.SetActive(false);

        // Index ile direkt karakter seç
        if (karakterAdi == "ece")
        {
            aktifKarakterIndex = 0; // Ece karakteri için index 0
        }
        else if (karakterAdi == "mert")
        {
            aktifKarakterIndex = 1; // Mert karakteri için index 1
        }
        else
        {
            Debug.LogError($"Bilinmeyen karakter adı: {karakterAdi}");
            return;
        }

        Debug.Log($"aktifKarakterIndex ayarlandı: {aktifKarakterIndex}");

        // Index kontrolü
        if (aktifKarakterIndex >= 0 && aktifKarakterIndex < karakterProfilleri.Count)
        {
            currentKarakterProfil = karakterProfilleri[aktifKarakterIndex];
            currentImageIndex = 0;

            Debug.Log($"{karakterAdi} profili bulundu (Index: {aktifKarakterIndex}), {currentKarakterProfil.profilGorselleri.Count} görsel var");
            Debug.Log($"currentKarakterProfil ayarlandı: {(currentKarakterProfil != null ? "Başarılı" : "Başarısız")}");
            
            // İlk görseli göster
            if (currentKarakterProfil.profilGorselleri.Count > 0 && currentKarakterProfil.profilGorselleri[currentImageIndex] != null)
            {
                currentKarakterProfil.profilGorselleri[currentImageIndex].SetActive(true);
                Debug.Log($"İlk görsel gösterildi: {currentKarakterProfil.profilGorselleri[currentImageIndex].name}");
            }

            // Buton durumlarını güncelle
            UpdateButtonStates();
            
            Debug.Log($"ButonaBasildi() tamamlandı - aktifKarakterIndex: {aktifKarakterIndex}");
        }
        else
        {
            Debug.LogError($"Geçersiz karakter index: {aktifKarakterIndex} (Toplam karakter: {karakterProfilleri.Count})");
        }
    }

    private void UpdateButtonStates()
    {
        bool hasMultipleImages = currentKarakterProfil != null && currentKarakterProfil.profilGorselleri.Count > 1;
        
        if (oncekiButon != null)
        {
            oncekiButon.interactable = hasMultipleImages;
            Debug.Log($"Önceki buton durumu: {(hasMultipleImages ? "Aktif" : "Pasif")}");
        }
        
        if (sonrakiButon != null)
        {
            sonrakiButon.interactable = hasMultipleImages;
            Debug.Log($"Sonraki buton durumu: {(hasMultipleImages ? "Aktif" : "Pasif")}");
        }
    }

    // Sonraki görsel
    public void NextImage()
    {   
        Debug.Log("=== NextImage çağrıldı ===");
        Debug.Log($"aktifKarakterIndex: {aktifKarakterIndex}");
        Debug.Log($"currentKarakterProfil null mu: {currentKarakterProfil == null}");
        
        // Eğer currentKarakterProfil null ise, index'ten tekrar al
        if (currentKarakterProfil == null && aktifKarakterIndex >= 0 && aktifKarakterIndex < karakterProfilleri.Count)
        {
            Debug.Log("currentKarakterProfil null - Index'ten yeniden alınıyor...");
            currentKarakterProfil = karakterProfilleri[aktifKarakterIndex];
            Debug.Log($"Index {aktifKarakterIndex}'ten karakter alındı: {(currentKarakterProfil != null ? "Başarılı" : "Başarısız")}");
        }
        
        if (currentKarakterProfil == null)
        {
            Debug.LogError("currentKarakterProfil hala null! NextImage iptal ediliyor.");
            Debug.LogError($"aktifKarakterIndex: {aktifKarakterIndex}, karakterProfilleri.Count: {karakterProfilleri.Count}");
            return;
        }

        int count = currentKarakterProfil.profilGorselleri.Count;
        Debug.Log($"Toplam görsel sayısı: {count}, Mevcut index: {currentImageIndex}");
        
        if (count <= 1)
        {
            Debug.LogWarning("Tek görsel var, değişim yapılmıyor");
            return;
        }

        // Mevcut görseli gizle
        var mevcutGorsel = currentKarakterProfil.profilGorselleri[currentImageIndex];
        if (mevcutGorsel != null)
        {
            mevcutGorsel.SetActive(false);
            Debug.Log($"Gizlenen görsel: {mevcutGorsel.name}");
        }

        // İndeksi artır
        int eskiIndex = currentImageIndex;
        currentImageIndex = (currentImageIndex + 1) % count;
        Debug.Log($"Index değişti: {eskiIndex} → {currentImageIndex}");

        // Yeni görseli göster
        var yeniGorsel = currentKarakterProfil.profilGorselleri[currentImageIndex];
        if (yeniGorsel != null)
        {
            yeniGorsel.SetActive(true);
            Debug.Log($"Gösterilen yeni görsel: {yeniGorsel.name}");
        }
        else
        {
            Debug.LogError($"Index {currentImageIndex}'deki görsel null!");
        }
    }

    // Önceki görsel
    public void PrevImage()
    {
        Debug.Log("=== PrevImage çağrıldı ===");
        
        if (currentKarakterProfil == null)
        {
            Debug.LogError("currentKarakterProfil null!");
            return;
        }

        int count = currentKarakterProfil.profilGorselleri.Count;
        Debug.Log($"Toplam görsel sayısı: {count}, Mevcut index: {currentImageIndex}");
        
        if (count <= 1)
        {
            Debug.LogWarning("Tek görsel var, değişim yapılmıyor");
            return;
        }

        // Mevcut görseli gizle
        var mevcutGorsel = currentKarakterProfil.profilGorselleri[currentImageIndex];
        if (mevcutGorsel != null)
        {
            mevcutGorsel.SetActive(false);
            Debug.Log($"Gizlenen görsel: {mevcutGorsel.name}");
        }

        // İndeksi azalt (döngüsel)
        int eskiIndex = currentImageIndex;
        currentImageIndex = (currentImageIndex - 1 + count) % count;
        Debug.Log($"Index değişti: {eskiIndex} → {currentImageIndex}");

        // Yeni görseli göster
        var yeniGorsel = currentKarakterProfil.profilGorselleri[currentImageIndex];
        if (yeniGorsel != null)
        {
            yeniGorsel.SetActive(true);
            Debug.Log($"Gösterilen yeni görsel: {yeniGorsel.name}");
        }
        else
        {
            Debug.LogError($"Index {currentImageIndex}'deki görsel null!");
        }
    }

    // Debug metodları
    [ContextMenu("Test: Ece Karakterini Seç")]
    public void TestEceSec()
    {
        ButonaBasildi("ece");
    }

    [ContextMenu("Test: Mert Karakterini Seç")]
    public void TestMertSec()
    {
        ButonaBasildi("mert");
    }

    [ContextMenu("Test: Sonraki Görsel")]
    public void TestSonrakiGorsel()
    {
        NextImage();
    }

    [ContextMenu("Test: Önceki Görsel")]
    public void TestOncekiGorsel()
    {
        PrevImage();
    }

    [ContextMenu("Debug: Sistem Durumu")]
    public void DebugSistemDurumu()
    {
        Debug.Log("=== SİSTEM DURUMU ===");
        Debug.Log($"Toplam karakter sayısı: {karakterProfilleri.Count}");
        Debug.Log($"Aktif karakter index: {aktifKarakterIndex}");
        Debug.Log($"currentKarakterProfil null mu: {currentKarakterProfil == null}");
        Debug.Log($"Mevcut görsel index: {currentImageIndex}");
        
        if (currentKarakterProfil != null)
        {
            Debug.Log($"Aktif karakterin görsel sayısı: {currentKarakterProfil.profilGorselleri.Count}");
        }
    }
}