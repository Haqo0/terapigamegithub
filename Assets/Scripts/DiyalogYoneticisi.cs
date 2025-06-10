using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class DiyalogYoneticisi : MonoBehaviour
{
    [Header("Karakter Bilgisi")]
    [Tooltip("Bu diyalog yöneticisinin hangi karaktere ait olduğunu belirler")]
    public string karakterAdi = "mert"; // "mert" veya "ece"
    
    [Header("Analiz Kontrolü")]
    [Tooltip("Her seans için analiz gösterilip gösterilmeyeceğini belirler")]
    public bool[] seansAnalizGoster;

    [Header("Zaman Ayarları")]
    public float seceneklerGecikmesi = 2.0f;
    public float analizGecikmesi = 3.0f;

    [Header("UI Referansları")]
    public TMP_Text npcText;
    public SecimSistemi secimSistemi;
    public AnalizGosterici analizGosterici;
    public SecenekGecikmesi gecikmeGostergesi;

    [Header("İlk Seans Verisi")]
    public TextAsset ilkSeansJson;

    [Header("Paneller")]
    public GameObject diyalogPanel;

    private List<DiyalogAdimi> adimlar;
    private DiyalogAdimi mevcutAdim;
    private DiyalogData diyalogData;

    private Dictionary<string, int> secimPuanlari = new Dictionary<string, int>();
    private List<string> yapilanSecimler = new List<string>();
    private Coroutine secenekGostermeCoroutine;
    private Coroutine analizGostermeCoroutine;

    // Her karakter kendi seans sayacını tutsun
    private int mevcutSeansIndex = 0;

    // Static değil - her karakterin kendi instance'ı
    public static Dictionary<string, DiyalogYoneticisi> karakterInstances = new Dictionary<string, DiyalogYoneticisi>();

    private void Awake()
    {
        // Her karakterin kendi instance'ını kaydet
        if (!karakterInstances.ContainsKey(karakterAdi))
        {
            karakterInstances[karakterAdi] = this;
        }
        else
        {
            Debug.LogWarning($"Aynı karakter ({karakterAdi}) için birden fazla DiyalogYoneticisi bulundu!");
        }
    }

    void Start()
    {
        // SADECE VERİLERİ HAZIRLA, SEANSI BAŞLATMA!
        if (ilkSeansJson != null)
        {
            diyalogData = JsonUtility.FromJson<DiyalogData>(ilkSeansJson.text);
            adimlar = diyalogData.adimlar;
            
            Debug.Log($"{karakterAdi} - Veriler hazırlandı: {ilkSeansJson.name} (Henüz başlatılmadı)");
        }
        else
        {
            Debug.LogError($"{karakterAdi} için ilk seans JSON'u atanmamış!");
        }

        // Panel'i kapat - sadece seçildiğinde açılacak
        if (diyalogPanel != null)
            diyalogPanel.SetActive(false);
    }

    // Seansı manuel başlatma metodu
    public void SeansiBaslat()
    {
        if (diyalogData == null || adimlar == null)
        {
            Debug.LogError($"{karakterAdi} - Veri yüklenmemiş, seans başlatılamıyor!");
            return;
        }

        if (diyalogPanel != null)
            diyalogPanel.SetActive(true);

        Debug.Log($"{karakterAdi} - Seans başlatılıyor: Seans {mevcutSeansIndex + 1}");
        DebugSeansAnalizDurumu();
        AdimiYukle("1");
    }

    public void AdimiYukle(string id)
    {
        if (secenekGostermeCoroutine != null)
            StopCoroutine(secenekGostermeCoroutine);

        mevcutAdim = adimlar.Find(adim => adim.id == id);

        if (mevcutAdim == null)
        {
            Debug.LogWarning($"{karakterAdi} - ID bulunamadı: {id}");
            return;
        }

        secimSistemi.SecenekleriTemizle();
        npcText.text = mevcutAdim.anlatim;

        if (mevcutAdim.seansSonu)
        {
            StartCoroutine(SeansiSonlandirGecikmeli());
            return;
        }

        if (mevcutAdim.secenekler != null && mevcutAdim.secenekler.Count > 0)
        {
            secenekGostermeCoroutine = StartCoroutine(SeçenekleriGecikmeligöster());
        }
    }

    private IEnumerator SeçenekleriGecikmeligöster()
    {
        if (gecikmeGostergesi != null)
            gecikmeGostergesi.GecikmeGosteriminiBaslat(seceneklerGecikmesi);

        yield return new WaitForSeconds(seceneklerGecikmesi);

        if (gecikmeGostergesi != null)
            gecikmeGostergesi.GecikmeGosteriminiDurdur();

        npcText.text = "";
        secimSistemi.SecenekleriGoster(mevcutAdim.secenekler, SecimYapildi);
    }

    private void SecimYapildi(Secenek secilenSecenek)
    {
        yapilanSecimler.Add(secilenSecenek.metin);

        if (!string.IsNullOrEmpty(secilenSecenek.etiket))
        {
            if (secimPuanlari.ContainsKey(secilenSecenek.etiket))
                secimPuanlari[secilenSecenek.etiket] += secilenSecenek.puan;
            else
                secimPuanlari[secilenSecenek.etiket] = secilenSecenek.puan;
        }

        secimSistemi.SecenekleriTemizle();

        Debug.Log($"{karakterAdi} - Seçim yapıldı: {secilenSecenek.metin}");

        AdimiYukle(secilenSecenek.sonrakiID);
    }

    private IEnumerator SeansiSonlandirGecikmeli()
    {
        yield return new WaitForSeconds(2.5f);
        SeansiSonlandir();
    }

    private void SeansiSonlandir()
    {
        Debug.Log($"=== {karakterAdi.ToUpper()} SEANS SONLANIYOR ===");
        Debug.Log($"Mevcut Seans Index: {mevcutSeansIndex}");

        bool analizGosterilecekMi = ShouldShowAnalysis();
        Debug.Log($"Analiz gösterilecek mi: {analizGosterilecekMi}");

        if (analizGosterilecekMi)
        {
            analizGostermeCoroutine = StartCoroutine(AnalizeGecikmeliGoster());
        }
        else
        {
            Debug.Log($"{karakterAdi} Seans {mevcutSeansIndex + 1} için analiz atlandı");

            if (diyalogPanel != null)
                diyalogPanel.SetActive(false);

            SeansiGecir();
        }
    }

    private IEnumerator AnalizeGecikmeliGoster()
    {
        Debug.Log($"{karakterAdi} - Analiz paneli {analizGecikmesi} saniye sonra gösterilecek...");
        yield return new WaitForSeconds(analizGecikmesi);

        AnalizSonucu uygunAnaliz = AnaliziBul();
        if (uygunAnaliz == null)
        {
            Debug.LogWarning($"{karakterAdi} - Uygun analiz bulunamadı");
            uygunAnaliz = VarsayilanAnalizOlustur();
        }

        if (diyalogPanel != null)
            diyalogPanel.SetActive(false);

        analizGosterici.AnalizeGoster(uygunAnaliz, secimPuanlari, yapilanSecimler);
        Debug.Log($"{karakterAdi} - Analiz gösteriliyor - Seans {mevcutSeansIndex + 1}");
    }

    void SeansiGecir()
    {
        // Karakter-spesifik geçiş sistemi kullan
        SeansGecisYoneticisi gecisYoneticisi = GetComponent<SeansGecisYoneticisi>();
        if (gecisYoneticisi != null)
        {
            gecisYoneticisi.SeansiHazirlaKarakterSpesifik(karakterAdi);
        }
        else
        {
            Debug.LogError($"{karakterAdi} için SeansGecisYoneticisi bulunamadı!");
        }
    }

    private bool ShouldShowAnalysis()
    {
        if (seansAnalizGoster == null || seansAnalizGoster.Length == 0)
        {
            Debug.LogWarning($"{karakterAdi} - seansAnalizGoster dizisi boş!");
            return false;
        }

        if (mevcutSeansIndex >= 0 && mevcutSeansIndex < seansAnalizGoster.Length)
        {
            bool sonuc = seansAnalizGoster[mevcutSeansIndex];
            Debug.Log($"{karakterAdi} Seans {mevcutSeansIndex + 1} - Analiz: {(sonuc ? "GÖSTER" : "ATLA")}");
            return sonuc;
        }

        Debug.LogWarning($"{karakterAdi} - Seans index ({mevcutSeansIndex}) dizi boyutunun ({seansAnalizGoster.Length}) dışında!");
        return false;
    }

    private AnalizSonucu AnaliziBul()
    {
        if (diyalogData?.analizSonuclari == null) return null;

        foreach (AnalizSonucu analiz in diyalogData.analizSonuclari)
        {
            bool uygun = true;
            int toplamPuan = 0;

            foreach (string etiket in analiz.gerekenEtiketler)
            {
                if (secimPuanlari.ContainsKey(etiket))
                    toplamPuan += secimPuanlari[etiket];
                else
                {
                    uygun = false;
                    break;
                }
            }

            if (uygun && toplamPuan >= analiz.minPuan)
                return analiz;
        }

        return null;
    }

    public void SonrakiSeansiBaslat(TextAsset yeniJson)
    {
        if (yeniJson == null)
        {
            Debug.LogWarning($"{karakterAdi} - Yeni JSON dosyası atanmadı.");
            return;
        }

        if (secenekGostermeCoroutine != null)
            StopCoroutine(secenekGostermeCoroutine);

        if (analizGostermeCoroutine != null)
            StopCoroutine(analizGostermeCoroutine);

        mevcutSeansIndex++;

        diyalogData = JsonUtility.FromJson<DiyalogData>(yeniJson.text);
        adimlar = diyalogData.adimlar;
        yapilanSecimler.Clear();
        secimPuanlari.Clear();

        if (diyalogPanel != null)
            diyalogPanel.SetActive(true);

        Debug.Log($"{karakterAdi} - Yeni seans başlatıldı: {yeniJson.name} - Seans {mevcutSeansIndex + 1}");
        DebugSeansAnalizDurumu();
        AdimiYukle("1");
    }

    // GameObject aktifleştirildiğinde çağrılacak
    private void OnEnable()
    {
        Debug.Log($"{karakterAdi} objesi aktifleştirildi");
        
        // Eğer veriler hazırsa ve henüz seans başlamamışsa başlat
        if (diyalogData != null && adimlar != null)
        {
            SeansiBaslat();
        }
    }

    // GameObject deaktifleştirildiğinde çağrılacak
    private void OnDisable()
    {
        Debug.Log($"{karakterAdi} objesi deaktifleştirildi");
        
        // Çalışan coroutine'leri durdur
        if (secenekGostermeCoroutine != null)
            StopCoroutine(secenekGostermeCoroutine);

        if (analizGostermeCoroutine != null)
            StopCoroutine(analizGostermeCoroutine);
    }

    private AnalizSonucu VarsayilanAnalizOlustur()
    {
        return new AnalizSonucu
        {
            baslik = "Genel Değerlendirme",
            aciklama = "Bu seans için özel bir analiz bulunamadı.",
            detay = "Seçimleriniz kaydedildi ve genel değerlendirme yapıldı.",
            ozet = $"{karakterAdi} seansınız tamamlandı.",
            gerekenEtiketler = new List<string>(),
            minPuan = 0
        };
    }

    private void DebugSeansAnalizDurumu()
    {
        Debug.Log($"=== {karakterAdi.ToUpper()} SEANS ANALİZ DURUMU ===");
        Debug.Log($"Mevcut Seans: {mevcutSeansIndex + 1} (Index: {mevcutSeansIndex})");

        if (seansAnalizGoster != null && seansAnalizGoster.Length > 0)
        {
            for (int i = 0; i < seansAnalizGoster.Length; i++)
            {
                string durum = seansAnalizGoster[i] ? "ANALİZ GÖSTER" : "ANALİZ ATLA";
                string aktif = (i == mevcutSeansIndex) ? " ← MEVCUT" : "";
                Debug.Log($"{karakterAdi} Seans {i + 1}: {durum}{aktif}");
            }
        }
        else
        {
            Debug.LogWarning($"{karakterAdi} - seansAnalizGoster dizisi ayarlanmamış!");
        }
    }

    // Karakter-spesifik seans sıfırlama
    [ContextMenu("Seans Index'i Sıfırla")]
    public void SeansIndexSifirla()
    {
        mevcutSeansIndex = 0;
        Debug.Log($"{karakterAdi} - Seans index sıfırlandı");
        DebugSeansAnalizDurumu();
    }

    // Diğer metodlar aynı kalabilir...
    public void PanelleriKapat()
    {
        Debug.Log($"{karakterAdi} - Paneller kapatılıyor...");

        if (diyalogPanel != null)
            diyalogPanel.SetActive(false);

        if (analizGosterici != null)
            analizGosterici.PaneliKapat();
    }

    public void PanelleriKapatVeDevamEt()
    {
        Debug.Log($"{karakterAdi} - Paneller kapatılıyor ve seans geçişi başlatılıyor...");

        if (diyalogPanel != null)
            diyalogPanel.SetActive(false);

        if (analizGosterici != null)
            analizGosterici.PaneliKapat();

        SeansiGecir();
    }

    // Geriye uyumluluk için eski özellikler
    public TextAsset diyalogJson
    {
        get { return ilkSeansJson; }
        set { ilkSeansJson = value; }
    }

    // Hızlandırma metodları
    [ContextMenu("Analizi Hemen Göster")]
    public void AnalizeHemenGöster()
    {
        if (analizGostermeCoroutine != null)
        {
            StopCoroutine(analizGostermeCoroutine);

            AnalizSonucu uygunAnaliz = AnaliziBul();
            if (uygunAnaliz == null)
                uygunAnaliz = VarsayilanAnalizOlustur();

            if (diyalogPanel != null)
                diyalogPanel.SetActive(false);

            analizGosterici.AnalizeGoster(uygunAnaliz, secimPuanlari, yapilanSecimler);
            Debug.Log($"{karakterAdi} - Analiz hızlandırılarak gösterildi");
        }
    }

    [ContextMenu("Seçenekleri Hemen Göster")]
    public void SeçenekleriHemenGöster()
    {
        if (secenekGostermeCoroutine != null)
            StopCoroutine(secenekGostermeCoroutine);

        if (gecikmeGostergesi != null)
            gecikmeGostergesi.GecikmeGosteriminiDurdur();

        npcText.text = "";

        if (mevcutAdim?.secenekler != null && mevcutAdim.secenekler.Count > 0)
        {
            secimSistemi.SecenekleriGoster(mevcutAdim.secenekler, SecimYapildi);
        }
    }

    // Geriye uyumluluk için eski metodlar
    public void SeansiYenidenBaslat()
    {
        Debug.Log($"=== {karakterAdi} - Seans sıfırdan başlatılıyor ===");

        if (secenekGostermeCoroutine != null)
            StopCoroutine(secenekGostermeCoroutine);

        if (analizGostermeCoroutine != null)
            StopCoroutine(analizGostermeCoroutine);

        yapilanSecimler.Clear();
        secimPuanlari.Clear();

        mevcutSeansIndex = 0;
        
        if (ilkSeansJson != null)
        {
            diyalogData = JsonUtility.FromJson<DiyalogData>(ilkSeansJson.text);
            adimlar = diyalogData.adimlar;
        }

        if (diyalogPanel != null)
            diyalogPanel.SetActive(true);

        DebugSeansAnalizDurumu();
        AdimiYukle("1");
    }
}