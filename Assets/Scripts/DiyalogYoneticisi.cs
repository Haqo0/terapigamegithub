using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class DiyalogYoneticisi : MonoBehaviour
{
    [Header("Analiz Kontrolü")]
    [Tooltip("Her seans için analiz gösterilip gösterilmeyeceğini belirler. Index 0 = Seans 1")]
    public bool[] seansAnalizGoster;

    [Header("Zaman Ayarları")]
    public float seceneklerGecikmesi = 2.0f; // Saniye cinsinden gecikme

    [Header("UI Referansları")]
    public TMP_Text npcText;
    public SecimSistemi secimSistemi;
    public AnalizGosterici analizGosterici;
    public SecenekGecikmesi gecikmeGostergesi;

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
    private Coroutine secenekGostermeCoroutine;

    // Seans takibi - SIFIRDAN BAŞLIYOR (Seans 1 = index 0)
    private int mevcutSeansIndex = 0;

    public static DiyalogYoneticisi instance;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (diyalogPanel != null)
            diyalogPanel.SetActive(true);

        // JSON'dan verileri al
        diyalogData = JsonUtility.FromJson<DiyalogData>(diyalogJson.text);
        adimlar = diyalogData.adimlar;

        // İlk adımı yükle
        AdimiYukle("1");

        // Debug için seans bilgilerini göster
        DebugSeansAnalizDurumu();
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
        Debug.Log($"=== SEANS SONLANIYOR ===");
        Debug.Log($"Mevcut Seans Index: {mevcutSeansIndex}");

        // Bu seansta analiz gösterilecek mi kontrol et
        bool analizGosterilecekMi = ShouldShowAnalysis();
        Debug.Log($"Analiz gösterilecek mi: {analizGosterilecekMi}");

        if (analizGosterilecekMi)
        {
            AnalizSonucu uygunAnaliz = AnaliziBul();

            // Uygun analiz bulunamasa bile varsayılan analizi göster
            if (uygunAnaliz == null)
            {
                Debug.LogWarning("Uygun analiz bulunamadı - Varsayılan analiz gösteriliyor");
                uygunAnaliz = VarsayilanAnalizOlustur();
            }

            // Diyalog panelini önce kapat
            if (diyalogPanel != null)
                diyalogPanel.SetActive(false);

            // Analiz göster
            analizGosterici.AnalizeGoster(uygunAnaliz, secimPuanlari, yapilanSecimler);
            Debug.Log($"Analiz gösteriliyor - Seans {mevcutSeansIndex + 1}");
        }
        else
        {
            Debug.Log($"Seans {mevcutSeansIndex + 1} için analiz atlandı - Paneller kapatılıyor");
            PanelleriKapat();
        }
    }

    // Analiz gösterilip gösterilmeyeceğini kontrol eder
    private bool ShouldShowAnalysis()
    {
        // Array'in varlığını kontrol et
        if (seansAnalizGoster == null || seansAnalizGoster.Length == 0)
        {
            Debug.LogWarning("seansAnalizGoster dizisi boş! Inspector'da ayarlayın.");
            return false; // Varsayılan olarak analiz gösterme
        }

        // Index sınırları içinde mi kontrol et
        if (mevcutSeansIndex >= 0 && mevcutSeansIndex < seansAnalizGoster.Length)
        {
            bool sonuc = seansAnalizGoster[mevcutSeansIndex];
            Debug.Log($"Seans {mevcutSeansIndex + 1} (Index: {mevcutSeansIndex}) - Analiz: {(sonuc ? "GÖSTER" : "ATLA")}");
            return sonuc;
        }

        // Index sınırların dışındaysa
        Debug.LogWarning($"Seans index ({mevcutSeansIndex}) dizi boyutunun ({seansAnalizGoster.Length}) dışında!");
        return false;
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

        // Uygun analiz bulunamadı
        return null;
    }
    public void PanelleriKapat()
    {
        Debug.Log("Paneller kapatılıyor...");

        // Tüm panelleri kapat
        if (diyalogPanel != null)
            diyalogPanel.SetActive(false);

        if (analizGosterici != null)
            analizGosterici.PaneliKapat();

    }
    // Varsayılan analiz oluştur
    private AnalizSonucu VarsayilanAnalizOlustur()
    {
        AnalizSonucu varsayilanAnaliz = new AnalizSonucu
        {
            baslik = "Genel Değerlendirme",
            aciklama = "Bu seans için özel bir analiz bulunamadı.",
            detay = "Seçimleriniz kaydedildi ve genel değerlendirme yapıldı.",
            ozet = "Seansınız tamamlandı. Yaptığınız seçimler genel değerlendirme kapsamında analiz edilmiştir.",
            gerekenEtiketler = new List<string>(),
            minPuan = 0
        };

        return varsayilanAnaliz;
    }

    //Panelleri kapat ve seans geçişini başlat
    public void PanelleriKapatVeDevamEt()
    {
        Debug.Log("Paneller kapatılıyor ve seans geçişi başlatılıyor...");

        // Tüm panelleri kapat
        if (diyalogPanel != null)
            diyalogPanel.SetActive(false);

        // Analiz panelini de kapat (eğer açıksa)
        if (analizGosterici != null)
            analizGosterici.PaneliKapat();

        // Seans geçişini başlat
        SeansGecisYoneticisi.SeansiHazirla();
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
        Debug.Log($" Yeni seans başlatıldı - Seans {mevcutSeansIndex + 1} (Index: {mevcutSeansIndex})");
        DebugSeansAnalizDurumu();
        AdimiYukle("1");
    }

    // Debug için seans analiz durumunu göster
    private void DebugSeansAnalizDurumu()
    {
        Debug.Log($"=== SEANS ANALİZ DURUMU ===");
        Debug.Log($"Mevcut Seans: {mevcutSeansIndex + 1} (Index: {mevcutSeansIndex})");

        if (seansAnalizGoster != null && seansAnalizGoster.Length > 0)
        {
            for (int i = 0; i < seansAnalizGoster.Length; i++)
            {
                string durum = seansAnalizGoster[i] ? "ANALİZ GÖSTER" : " ANALİZ ATLA";
                string aktif = (i == mevcutSeansIndex) ? " ← MEVCUT" : "";
                Debug.Log($"Seans {i + 1}: {durum}{aktif}");
            }
        }
        else
        {
            Debug.LogWarning("seansAnalizGoster dizisi ayarlanmamış!");
        }
    }

    // Manuel kontroller (test için)
    [ContextMenu("Seans Index'i Sıfırla")]
    public void SeansIndexSifirla()
    {
        mevcutSeansIndex = 0;
        Debug.Log("Seans index sıfırlandı");
        DebugSeansAnalizDurumu();
    }

    [ContextMenu("Mevcut Seans Bilgisini Göster")]
    public void MevcutSeansBilgisiGoster()
    {
        DebugSeansAnalizDurumu();
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