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
    public float seceneklerGecikmesi = 2.0f;
    public float analizGecikmesi = 3.0f;

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

    private Dictionary<string, int> secimPuanlari = new Dictionary<string, int>();
    private List<string> yapilanSecimler = new List<string>();
    private Coroutine secenekGostermeCoroutine;
    private Coroutine analizGostermeCoroutine;

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

        diyalogData = JsonUtility.FromJson<DiyalogData>(diyalogJson.text);
        adimlar = diyalogData.adimlar;

        AdimiYukle("1");
        DebugSeansAnalizDurumu();
    }

    public void AdimiYukle(string id)
    {
        if (secenekGostermeCoroutine != null)
            StopCoroutine(secenekGostermeCoroutine);

        mevcutAdim = adimlar.Find(adim => adim.id == id);

        if (mevcutAdim == null)
        {
            Debug.LogWarning("ID bulunamadı: " + id);
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

        Debug.Log($"Seçim yapıldı: {secilenSecenek.metin} (Etiket: {secilenSecenek.etiket}, Puan: {secilenSecenek.puan})");

        AdimiYukle(secilenSecenek.sonrakiID);
    }

    private IEnumerator SeansiSonlandirGecikmeli()
    {
        yield return new WaitForSeconds(2.5f);
        SeansiSonlandir();
    }

    private void SeansiSonlandir()
    {
        Debug.Log($"=== SEANS SONLANIYOR ===");
        Debug.Log($"Mevcut Seans Index: {mevcutSeansIndex}");

        bool analizGosterilecekMi = ShouldShowAnalysis();
        Debug.Log($"Analiz gösterilecek mi: {analizGosterilecekMi}");

        if (analizGosterilecekMi)
        {
            analizGostermeCoroutine = StartCoroutine(AnalizeGecikmeliGoster());
        }
        else
        {
            Debug.Log($"Seans {mevcutSeansIndex + 1} için analiz atlandı - Direkt sonraki seansa geçiliyor");

            if (diyalogPanel != null)
                diyalogPanel.SetActive(false);

            SeansiGecir();
        }
    }

    private IEnumerator AnalizeGecikmeliGoster()
    {
        Debug.Log($"Analiz paneli {analizGecikmesi} saniye sonra gösterilecek...");
        yield return new WaitForSeconds(analizGecikmesi);

        AnalizSonucu uygunAnaliz = AnaliziBul();
        if (uygunAnaliz == null)
        {
            Debug.LogWarning("Uygun analiz bulunamadı - Varsayılan analiz gösteriliyor");
            uygunAnaliz = VarsayilanAnalizOlustur();
        }

        if (diyalogPanel != null)
            diyalogPanel.SetActive(false);

        analizGosterici.AnalizeGoster(uygunAnaliz, secimPuanlari, yapilanSecimler);
        Debug.Log($"Analiz gösteriliyor - Seans {mevcutSeansIndex + 1}");
    }

    void SeansiGecir()
    {
        SeansGecisYoneticisi.SeansiHazirla();
    }

    private bool ShouldShowAnalysis()
    {
        if (seansAnalizGoster == null || seansAnalizGoster.Length == 0)
        {
            Debug.LogWarning("seansAnalizGoster dizisi boş! Inspector'da ayarlayın.");
            return false;
        }

        if (mevcutSeansIndex >= 0 && mevcutSeansIndex < seansAnalizGoster.Length)
        {
            bool sonuc = seansAnalizGoster[mevcutSeansIndex];
            Debug.Log($"Seans {mevcutSeansIndex + 1} (Index: {mevcutSeansIndex}) - Analiz: {(sonuc ? "GÖSTER" : "ATLA")}");
            return sonuc;
        }

        Debug.LogWarning($"Seans index ({mevcutSeansIndex}) dizi boyutunun ({seansAnalizGoster.Length}) dışında!");
        return false;
    }

    private AnalizSonucu AnaliziBul()
    {
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

    public void PanelleriKapat()
    {
        Debug.Log("Paneller kapatılıyor...");

        if (diyalogPanel != null)
            diyalogPanel.SetActive(false);

        if (analizGosterici != null)
            analizGosterici.PaneliKapat();
    }

    private AnalizSonucu VarsayilanAnalizOlustur()
    {
        return new AnalizSonucu
        {
            baslik = "Genel Değerlendirme",
            aciklama = "Bu seans için özel bir analiz bulunamadı.",
            detay = "Seçimleriniz kaydedildi ve genel değerlendirme yapıldı.",
            ozet = "Seansınız tamamlandı. Yaptığınız seçimler genel değerlendirme kapsamında analiz edilmiştir.",
            gerekenEtiketler = new List<string>(),
            minPuan = 0
        };
    }

    public void PanelleriKapatVeDevamEt()
    {
        Debug.Log("Paneller kapatılıyor ve seans geçişi başlatılıyor...");

        if (diyalogPanel != null)
            diyalogPanel.SetActive(false);

        if (analizGosterici != null)
            analizGosterici.PaneliKapat();

        SeansGecisYoneticisi.SeansiHazirla();
    }

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
            Debug.Log("Analiz hızlandırılarak gösterildi");
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

    public void SonrakiSeansiBaslat(TextAsset yeniJson)
    {
        if (yeniJson == null)
        {
            Debug.LogWarning("Yeni JSON dosyası atanmadı.");
            return;
        }

        if (secenekGostermeCoroutine != null)
            StopCoroutine(secenekGostermeCoroutine);

        if (analizGostermeCoroutine != null)
            StopCoroutine(analizGostermeCoroutine);

        mevcutSeansIndex++;

        diyalogJson = yeniJson;
        diyalogData = JsonUtility.FromJson<DiyalogData>(diyalogJson.text);
        adimlar = diyalogData.adimlar;
        yapilanSecimler.Clear();
        secimPuanlari.Clear();

        if (diyalogPanel != null)
            diyalogPanel.SetActive(true);

        Debug.Log($"Yeni seans başlatıldı - Seans {mevcutSeansIndex + 1} (Index: {mevcutSeansIndex})");
        DebugSeansAnalizDurumu();
        AdimiYukle("1");
    }

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

    public void SeansiYenidenBaslat()
    {
        Debug.Log("=== Seans sıfırdan başlatılıyor ===");

        if (secenekGostermeCoroutine != null)
            StopCoroutine(secenekGostermeCoroutine);

        if (analizGostermeCoroutine != null)
            StopCoroutine(analizGostermeCoroutine);

        yapilanSecimler.Clear();
        secimPuanlari.Clear();

        mevcutSeansIndex = 0;
        diyalogData = JsonUtility.FromJson<DiyalogData>(diyalogJson.text);
        adimlar = diyalogData.adimlar;

        if (diyalogPanel != null)
            diyalogPanel.SetActive(true);

        DebugSeansAnalizDurumu();
        AdimiYukle("1");
    }
}