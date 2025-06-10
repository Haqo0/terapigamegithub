using UnityEngine;
using System.Linq;
using UnityEngine.Playables;

public class KarakterYonetici : MonoBehaviour
{
    [Header("Tüm karakterlerin verileri")]
    public KarakterVerisi[] tumKarakterler;
    
    private KarakterVerisi aktifKarakter;
    public static KarakterYonetici instance;
    
    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Seçilen karakterin giriş cutscene'ini başlatır.
    /// </summary>
    public void KarakterSeansiBaslat(string karakterAdi)
    {
        aktifKarakter = tumKarakterler.FirstOrDefault(k => k.karakterAdi == karakterAdi);
        if (aktifKarakter == null)
        {
            Debug.LogError("Karakter bulunamadı: " + karakterAdi);
            return;
        }

        // Karakter prefab'ını aktifleştir
        aktifKarakter.karakterPrefab.SetActive(true);
        
        // Giriş cutscene'ini başlat
        if (aktifKarakter.girisCutscene != null)
        {
            aktifKarakter.girisCutscene.Play();
            Debug.Log($"{karakterAdi} için giriş cutscene başlatıldı.");
        }
        else
        {
            Debug.LogWarning($"{karakterAdi} için giriş cutscene bulunamadı, direkt seans başlatılıyor");
            GirisCutsceneBitinceSeansiBaslat();
        }
    }

    /// <summary>
    /// Giriş cutscene bittiğinde çağrılır. Seansı başlatır.
    /// </summary>
    public void GirisCutsceneBitinceSeansiBaslat()
    {
        if (aktifKarakter == null)
        {
            Debug.LogError("Giriş cutscene sonrası: Aktif karakter yok!");
            return;
        }

        GameObject seansSistemi = aktifKarakter.karakterPrefab;
        seansSistemi.SetActive(true);

        // SeansGecisYoneticisi'ni ayarla
        SeansGecisYoneticisi gecisYoneticisi = seansSistemi.GetComponentInChildren<SeansGecisYoneticisi>();
        if (gecisYoneticisi != null)
        {
            // İlk seans hariç diğerlerini ayarla (seans 2-5)
            if (aktifKarakter.seansJsonlar.Length > 1)
            {
                TextAsset[] sonrakiSeanslar = new TextAsset[aktifKarakter.seansJsonlar.Length - 1];
                for (int i = 1; i < aktifKarakter.seansJsonlar.Length; i++)
                {
                    sonrakiSeanslar[i - 1] = aktifKarakter.seansJsonlar[i];
                }
                gecisYoneticisi.JsonDosyalariniAyarla(sonrakiSeanslar);
            }
        }
        else
        {
            Debug.LogWarning("SeansGecisYoneticisi bulunamadı.");
        }

        // DiyalogYoneticisi'ni başlat
        DiyalogYoneticisi diyalogYoneticisi = seansSistemi.GetComponentInChildren<DiyalogYoneticisi>();
        if (diyalogYoneticisi != null)
        {
            // İlk seans JSON'unu ayarla
            if (aktifKarakter.seansJsonlar.Length > 0)
            {
                diyalogYoneticisi.diyalogJson = aktifKarakter.seansJsonlar[0];
                diyalogYoneticisi.SeansiYenidenBaslat();
                Debug.Log($"{aktifKarakter.karakterAdi} seansı başlatıldı.");
            }
            else
            {
                Debug.LogError($"{aktifKarakter.karakterAdi} için seans JSON'u bulunamadı!");
            }
        }
        else
        {
            Debug.LogError("DiyalogYoneticisi bulunamadı.");
        }
    }

    /// <summary>
    /// Seans sona erdiğinde çıkış cutscene başlatılır.
    /// </summary>
    public void SeansSonuCutsceneBaslat()
    {
        if (aktifKarakter != null && aktifKarakter.cikisCutscene != null)
        {
            aktifKarakter.cikisCutscene.Play();
            Debug.Log($"{aktifKarakter.karakterAdi} için çıkış cutscene başlatıldı.");
        }
        else
        {
            Debug.LogWarning("Çıkış cutscene başlatılamadı - direkt idle moda dönülüyor");
            IdleModaDon();
        }
    }

    /// <summary>
    /// Çıkış cutscene sona erdiğinde çağrılır. Oyunu idle moda döndürür.
    /// </summary>
    public void IdleModaDon()
    {
        if (aktifKarakter != null && aktifKarakter.karakterPrefab != null)
        {
            aktifKarakter.karakterPrefab.SetActive(false);
            Debug.Log($"{aktifKarakter.karakterAdi} karakteri deaktifleştirildi.");
        }

        // CrosshairEtkilesim'i idle moda döndür
        if (CrosshairEtkilesim.instance != null)
        {
            CrosshairEtkilesim.instance.CrosshairVeKontrolGeriGetir();
        }

        aktifKarakter = null;
        Debug.Log("Oyun idle moda döndü.");
    }

    /// <summary>
    /// Şu anda aktif olan karakteri döndürür.
    /// </summary>
    public KarakterVerisi GetAktifKarakter()
    {
        return aktifKarakter;
    }

    // Geriye uyumluluk için eski metodlar
    public void GirisCutsceneBaslat(string karakterAdi)
    {
        KarakterSeansiBaslat(karakterAdi);
    }

    public void SeansiBaşlat(string karakterAdi)
    {
        KarakterSeansiBaslat(karakterAdi);
    }

    public void IdleModaDonus()
    {
        IdleModaDon();
    }

    // Debug metodları
    [ContextMenu("Aktif Karakter Durumunu Göster")]
    public void AktifKarakterDurumuGoster()
    {
        if (aktifKarakter != null)
        {
            Debug.Log($"=== AKTİF KARAKTER: {aktifKarakter.karakterAdi} ===");
            Debug.Log($"Prefab Aktif: {(aktifKarakter.karakterPrefab?.activeInHierarchy ?? false)}");
            Debug.Log($"Giriş Cutscene: {(aktifKarakter.girisCutscene != null ? "✓" : "✗")}");
            Debug.Log($"Çıkış Cutscene: {(aktifKarakter.cikisCutscene != null ? "✓" : "✗")}");
            Debug.Log($"Seans JSON Sayısı: {aktifKarakter.seansJsonlar?.Length ?? 0}");
        }
        else
        {
            Debug.Log("Aktif karakter yok - Idle modda");
        }
    }

    [ContextMenu("Tüm Karakterleri Listele")]
    public void TumKarakterleriListele()
    {
        Debug.Log("=== TÜM KARAKTERLER ===");
        for (int i = 0; i < tumKarakterler.Length; i++)
        {
            var karakter = tumKarakterler[i];
            Debug.Log($"{i + 1}. {karakter.karakterAdi}:");
            Debug.Log($"   Prefab: {(karakter.karakterPrefab != null ? "✓" : "✗")}");
            Debug.Log($"   Giriş Cutscene: {(karakter.girisCutscene != null ? "✓" : "✗")}");
            Debug.Log($"   Çıkış Cutscene: {(karakter.cikisCutscene != null ? "✓" : "✗")}");
            Debug.Log($"   JSON Sayısı: {karakter.seansJsonlar?.Length ?? 0}");
        }
    }
}