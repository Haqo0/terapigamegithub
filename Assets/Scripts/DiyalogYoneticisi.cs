using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class DiyalogYoneticisi : MonoBehaviour
{
    [Header("Analiz Kontrolü")]
    public bool[] seansAnalizGoster; 
    
    [Header("Zaman Ayarları")]
    public float seceneklerGecikmesi = 2.0f; // Saniye cinsinden gecikme
    
    [Header("UI Referansları")]
    public TMP_Text npcText;
    public SecimSistemi secimSistemi;
    public AnalizGosterici analizGosterici;
    public SecenekGecikmesi gecikmeGostergesi; // İsim değişti
    
    [Header("Veri")]
    public TextAsset diyalogJson;

    [Header("Paneller")]
    public GameObject diyalogPanel;

    private List<DiyalogAdimi> adimlar;
    private DiyalogAdimi mevcutAdim;
    private DiyalogData diyalogData;
    
    // Seçim takip sistemi
    private Dictionary<string, int> secimPuanlari = new Dictionary<string, int>();
    private List<string> yapilanSecimler = new List<string>();
    private Coroutine secenekGostermeCoroutine; // Aktif coroutine referansı
    
    // Seans takibi
    private int mevcutSeansIndex = 0; // Kaçıncı seansta olduğumuzu takip eder

    public static DiyalogYoneticisi instance;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (diyalogPanel != null)
            diyalogPanel.SetActive(true); // Panel oyun başında açık olmalı

        // JSON'dan verileri al
        diyalogData = JsonUtility.FromJson<DiyalogData>(diyalogJson.text);
        adimlar = diyalogData.adimlar;

        // İlk adımı yükle
        AdimiYukle("1");
    }

    public void AdimiYukle(string id)
    {
        // Önceki gecikme coroutine'ini durdur
        if (secenekGostermeCoroutine != null)
        {
            StopCoroutine(secenekGostermeCoroutine);
        }

        mevcutAdim = adimlar.Find(adim => adim.id == id);

        if (mevcutAdim == null)
        {
            Debug.LogWarning("ID bulunamadı: " + id);
            return;
        }

        // Önce seçenekleri temizle
        secimSistemi.SecenekleriTemizle();

        // Diyalogu göster
        npcText.text = mevcutAdim.anlatim;

        // Seans sonu kontrolü
        if (mevcutAdim.seansSonu)
        {
            SeansiSonlandir();
            return;
        }

        // Seçenekler varsa gecikme ile göster
        if (mevcutAdim.secenekler != null && mevcutAdim.secenekler.Count > 0)
        {
            secenekGostermeCoroutine = StartCoroutine(SeçenekleriGecikmeligöster());
        }
    }

    private IEnumerator SeçenekleriGecikmeligöster()
    {
        // Gecikme göstergesini başlat
        if (gecikmeGostergesi != null)
        {
            gecikmeGostergesi.GecikmeGosteriminiBaslat(seceneklerGecikmesi);
        }

        // Belirlenen süre kadar bekle
        yield return new WaitForSeconds(seceneklerGecikmesi);
        
        // Gecikme göstergesini durdur
        if (gecikmeGostergesi != null)
        {
            gecikmeGostergesi.GecikmeGosteriminiDurdur();
        }
        
        // DİYALOGU GİZLE - Metni temizle
        npcText.text = "";
        
        // Seçenekleri göster (artık diyalog alanında)
        secimSistemi.SecenekleriGoster(mevcutAdim.secenekler, SecimYapildi);
    }

    private void SecimYapildi(Secenek secilenSecenek)
    {
        // Seçimi kaydet
        yapilanSecimler.Add(secilenSecenek.metin);
        
        // Etiket puanını güncelle
        if (!string.IsNullOrEmpty(secilenSecenek.etiket))
        {
            if (secimPuanlari.ContainsKey(secilenSecenek.etiket))
            {
                secimPuanlari[secilenSecenek.etiket] += secilenSecenek.puan;
            }
            else
            {
                secimPuanlari[secilenSecenek.etiket] = secilenSecenek.puan;
            }
        }

        // Seçenekleri temizle (seçim yapıldıktan sonra)
        secimSistemi.SecenekleriTemizle();

        // Debug için
        Debug.Log($"Seçim yapıldı: {secilenSecenek.metin} (Etiket: {secilenSecenek.etiket}, Puan: {secilenSecenek.puan})");
        
        // Sonraki adıma geç
        AdimiYukle(secilenSecenek.sonrakiID);
    }

    private void SeansiSonlandir()
    {
        Debug.Log($"Seans sonlandırılıyor... Mevcut seans: {mevcutSeansIndex}");
        
        // Bu seansta analiz gösterilecek mi kontrol et
        bool analizGosterilecekMi = ShouldShowAnalysis();
        
        if (analizGosterilecekMi)
        {
            AnalizSonucu uygunAnaliz = AnaliziBul();
            
            if (uygunAnaliz != null)
            {
                analizGosterici.AnalizeGoster(uygunAnaliz, secimPuanlari, yapilanSecimler);
                Debug.Log($"Analiz gösteriliyor - Seans {mevcutSeansIndex}");
            }
            else
            {
                Debug.LogWarning("Uygun analiz bulunamadı.");
            }
        }
        else
        {
            Debug.Log($"Seans {mevcutSeansIndex} için analiz atlandı");
        }

        if (diyalogPanel != null)
            diyalogPanel.SetActive(false);

        SeansGecisYoneticisi.SeansiHazirla();
    }
    
    // Analiz gösterilip gösterilmeyeceğini kontrol eder
    private bool ShouldShowAnalysis()
    {
        // Array'in boyutunu kontrol et
        if (seansAnalizGoster == null || seansAnalizGoster.Length == 0)
        {
            Debug.LogWarning("seansAnalizGoster dizisi boş! Varsayılan olarak analiz gösteriliyor.");
            return true; // Varsayılan davranış
        }
        
        // Index sınırları içinde mi?
        if (mevcutSeansIndex >= 0 && mevcutSeansIndex < seansAnalizGoster.Length)
        {
            return seansAnalizGoster[mevcutSeansIndex];
        }
        
        // Index sınırların dışındaysa varsayılan davranış
        Debug.LogWarning($"Seans index ({mevcutSeansIndex}) dizi boyutunun ({seansAnalizGoster.Length}) dışında! Varsayılan olarak analiz gösteriliyor.");
        return true;
    }

    private AnalizSonucu AnaliziBul()
    {
        foreach (AnalizSonucu analiz in diyalogData.analizSonuclari)
        {
            bool uygun = true;
            int toplamPuan = 0;

            // Gerekli etiketleri kontrol et
            foreach (string etiket in analiz.gerekenEtiketler)
            {
                if (secimPuanlari.ContainsKey(etiket))
                {
                    toplamPuan += secimPuanlari[etiket];
                }
                else
                {
                    uygun = false;
                    break;
                }
            }

            // Minimum puan kontrolü
            if (uygun && toplamPuan >= analiz.minPuan)
            {
                return analiz;
            }
        }

        // Varsayılan analiz döndür
        return diyalogData.analizSonuclari.FirstOrDefault();
    }

    // Seçenekleri anında göstermek için (acil durumlar için)
    [ContextMenu("Seçenekleri Hemen Göster")]
    public void SeçenekleriHemenGöster()
    {
        if (secenekGostermeCoroutine != null)
        {
            StopCoroutine(secenekGostermeCoroutine);
        }

        // Gecikme göstergesini durdur
        if (gecikmeGostergesi != null)
        {
            gecikmeGostergesi.GecikmeGosteriminiDurdur();
        }
        
        // Diyalogu gizle
        npcText.text = "";
        
        if (mevcutAdim?.secenekler != null && mevcutAdim.secenekler.Count > 0)
        {
            secimSistemi.SecenekleriGoster(mevcutAdim.secenekler, SecimYapildi);
        }
    }

    // 🔄 Yeni JSON yüklendiğinde temiz başlat
    public void SonrakiSeansiBaslat(TextAsset yeniJson)
    {
        if (yeniJson == null)
        {
            Debug.LogWarning("Yeni JSON dosyası atanmadı.");
            return;
        }

        // Seans indexini artır
        mevcutSeansIndex++;
        
        diyalogJson = yeniJson;
        diyalogData = JsonUtility.FromJson<DiyalogData>(diyalogJson.text);
        adimlar = diyalogData.adimlar;
        yapilanSecimler.Clear();
        secimPuanlari.Clear();

        if (diyalogPanel != null)
            diyalogPanel.SetActive(true);

        Debug.Log($"Yeni seans başlatıldı - Seans Index: {mevcutSeansIndex}");
        AdimiYukle("1");
    }
    
    // 🔧 Manuel seans index ayarlama (test için)
    [ContextMenu("Seans Index'i Sıfırla")]
    public void SeansIndexSifirla()
    {
        mevcutSeansIndex = 0;
        Debug.Log("Seans index sıfırlandı");
    }
    
    // 🔧 Mevcut seans bilgisini göster (debug için)
    [ContextMenu("Mevcut Seans Bilgisini Göster")]
    public void MevcutSeansBilgisiGoster()
    {
        Debug.Log($"=== SEANS BİLGİSİ ===");
        Debug.Log($"Mevcut Seans Index: {mevcutSeansIndex}");
        Debug.Log($"Analiz Array Boyutu: {(seansAnalizGoster?.Length ?? 0)}");
        
        if (seansAnalizGoster != null)
        {
            for (int i = 0; i < seansAnalizGoster.Length; i++)
            {
                Debug.Log($"Seans {i}: {(seansAnalizGoster[i] ? "Analiz Göster" : "Analiz Atla")}");
            }
        }
        
        Debug.Log($"Bu seansta analiz gösterilecek mi: {ShouldShowAnalysis()}");
    }

    [ContextMenu("Mevcut Puanları Göster")]
    private void MevcutPuanlariGoster()
    {
        Debug.Log("=== Mevcut Seçim Puanları ===");
        foreach (var kvp in secimPuanlari)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value}");
        }
    }
}