using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SeansGecisYoneticisi : MonoBehaviour
{
    [Header("Karakter Bilgisi")]
    [Tooltip("Bu geçiş yöneticisinin hangi karaktere ait olduğunu belirler")]
    public string karakterAdi = "mert"; // "mert" veya "ece"

    [Header("UI Bileşenleri")]
    public GameObject gecisPaneli;
    public TMP_Text gecisMesaji;
    public Button devamButonu;

    [Header("Diyalog Yöneticisi")]
    public DiyalogYoneticisi diyalogYoneticisi;

    [Header("Bu Karakterin Seansları (Seans 2-3-4-5)")]
    [Tooltip("Bu karaktere özel seansları sırayla ekleyin")]
    public TextAsset[] karakterSeansları;

    // Her karakter kendi seans sayacını tutacak
    private int guncelSeansIndex = 0;

    private void Start()
    {
        if (gecisPaneli != null)
            gecisPaneli.SetActive(false);

        if (devamButonu != null)
            devamButonu.gameObject.SetActive(false);

        // Karakter adını kontrol et
        if (string.IsNullOrEmpty(karakterAdi))
        {
            Debug.LogError("Karakter adı atanmamış!");
        }

        Debug.Log($"{karakterAdi} SeansGecisYoneticisi başlatıldı");
    }

    // Karakter-spesifik seans hazırlama
    public void SeansiHazirlaKarakterSpesifik(string karakter, string mesaj = "Bir sonraki seansa geçiliyor...")
    {
        if (karakter != karakterAdi)
        {
            Debug.Log($"Bu geçiş yöneticisi {karakterAdi} için, gelen istek {karakter} için - İşlem yapılmıyor");
            return;
        }

        if (gecisPaneli != null)
        {
            gecisPaneli.SetActive(true);
            StartCoroutine(GecikmeliMesajVeDevam(mesaj));
        }

        if (guncelSeansIndex >= 4)
        {
            gecisPaneli.SetActive(false);
            CrosshairEtkilesim.instance.CrosshairVeKontrolGeriGetir();
        }
    
    }


    // Genel static metod - geriye uyumluluk için
    public static void SeansiHazirla(string mesaj = "Bir sonraki seansa geçiliyor...")
    {
        Debug.Log("Static SeansiHazirla() kullanıldı - Karakter-spesifik metodları kullanın");
    }

    private IEnumerator GecikmeliMesajVeDevam(string mesaj)
    {
        // Önlem: buton sıfırlansın
        if (devamButonu != null)
        {
            devamButonu.onClick.RemoveAllListeners();
            devamButonu.gameObject.SetActive(false);
        }

        // İlk boş bekleme
        if (gecisMesaji != null)
            gecisMesaji.text = "";

        yield return new WaitForSeconds(2f);

        // Mesaj göster
        if (gecisMesaji != null)
            gecisMesaji.text = mesaj;

        // Mesaj görünsün → sonra buton gelsin
        yield return new WaitForSeconds(2.5f);

        if (devamButonu != null)
        {
            devamButonu.gameObject.SetActive(true);
            devamButonu.onClick.AddListener(DevamEt);
        }
    }

    public void DevamEt()
    {
        Debug.Log($"{karakterAdi} - DevamEt() çağrıldı. Güncel seans index: {guncelSeansIndex}");

        if (guncelSeansIndex >= karakterSeansları.Length)
        {
            Debug.Log($"{karakterAdi} - Tüm seanslar tamamlandı!");
            if (gecisMesaji != null)
                gecisMesaji.text = $"{karakterAdi} karakterinin tüm seansları tamamlandı.";

            if (devamButonu != null)
                devamButonu.gameObject.SetActive(false);

            // Çıkış cutscene'ini başlat
            if (KarakterYonetici.instance != null)
            {
                KarakterYonetici.instance.SeansSonuCutsceneBaslat();
            }

            return;
        }

        TextAsset sonrakiJson = karakterSeansları[guncelSeansIndex];

        if (sonrakiJson == null)
        {
            Debug.LogError($"{karakterAdi} - Seans {guncelSeansIndex + 2} JSON'u atanmamış!");
            return;
        }

        Debug.Log($"{karakterAdi} - Sonraki seans yükleniyor: {sonrakiJson.name}");

        guncelSeansIndex++;

        if (diyalogYoneticisi != null)
        {
            diyalogYoneticisi.SonrakiSeansiBaslat(sonrakiJson);
        }
        else
        {
            Debug.LogError($"{karakterAdi} - DiyalogYoneticisi referansı bulunamadı!");
        }

        if (gecisPaneli != null)
            gecisPaneli.SetActive(false);

        if (devamButonu != null)
            devamButonu.gameObject.SetActive(false);
    }

    [ContextMenu("Seans Index'i Sıfırla")]
    public void SeansIndexSifirla()
    {
        guncelSeansIndex = 0;
        Debug.Log($"{karakterAdi} - Seans geçiş index'i sıfırlandı");
    }

    // Geriye uyumluluk için eski metodlar
    public void JsonDosyalariniAyarla(TextAsset[] jsonDosyalari)
    {
        if (jsonDosyalari != null && jsonDosyalari.Length > 0)
        {
            karakterSeansları = jsonDosyalari;
            Debug.Log($"{karakterAdi} - {jsonDosyalari.Length} JSON dosyası ayarlandı");
        }
        else
        {
            Debug.LogWarning($"{karakterAdi} - Boş JSON dizisi gönderildi");
        }
    }

    [ContextMenu("Mevcut Durumu Göster")]
    public void MevcutDurumuGoster()
    {
        Debug.Log($"=== {karakterAdi.ToUpper()} SEANS GEÇİŞ DURUMU ===");
        Debug.Log($"Güncel Seans Index: {guncelSeansIndex}");
        Debug.Log($"Toplam Seans Sayısı: {karakterSeansları.Length}");

        for (int i = 0; i < karakterSeansları.Length; i++)
        {
            string durum = (i == guncelSeansIndex) ? " ← SONRAKİ" : "";
            string jsonAdi = karakterSeansları[i] != null ? karakterSeansları[i].name : "NULL";
            Debug.Log($"Seans {i + 2}: {jsonAdi}{durum}");
        }
    }
}