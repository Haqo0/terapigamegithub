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

    private void Start()
    {
        // Otomatik başlatmayı engellemek için güvenlik
        if (diyalogJson == null)
        {
            Debug.LogWarning($"{gameObject.name}: JSON atanmadığı için Start() iptal edildi.");
            return;
        }

        if (diyalogPanel == null || !diyalogPanel.activeInHierarchy)
        {
            Debug.LogWarning($"{gameObject.name}: Panel aktif değil → Start() iptal.");
            return;
        }

        Debug.Log($"{gameObject.name}: Start() → JSON verisi yükleniyor.");
        diyalogData = JsonUtility.FromJson<DiyalogData>(diyalogJson.text);
        adimlar = diyalogData.adimlar;

        AdimiYukle("1");
    }

    public void AdimiYukle(string id)
    {
        if (secenekGostermeCoroutine != null)
            StopCoroutine(secenekGostermeCoroutine);

        mevcutAdim = adimlar.Find(adim => adim.id == id);

        if (mevcutAdim == null)
        {
            Debug.LogWarning($"ID bulunamadı: {id}");
            return;
        }

        secimSistemi.SecenekleriTemizle();

        if (npcText != null)
            npcText.text = mevcutAdim.anlatim;

        if (mevcutAdim.seansSonu)
        {
            StartCoroutine(SeansiSonlandirGecikmeli());
            return;
        }

        if (mevcutAdim.secenekler != null && mevcutAdim.secenekler.Count > 0)
            secenekGostermeCoroutine = StartCoroutine(SeçenekleriGecikmeligöster());
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
        AdimiYukle(secilenSecenek.sonrakiID);
    }

    private IEnumerator SeansiSonlandirGecikmeli()
    {
        yield return new WaitForSeconds(2.5f);
        SeansiSonlandir();
    }

    private void SeansiSonlandir()
    {
        bool analizGosterilecekMi = ShouldShowAnalysis();

        if (analizGosterilecekMi)
            analizGostermeCoroutine = StartCoroutine(AnalizeGecikmeliGoster());
        else
        {
            if (diyalogPanel != null)
                diyalogPanel.SetActive(false);

            SeansiGecir();
        }
    }

    private IEnumerator AnalizeGecikmeliGoster()
    {
        yield return new WaitForSeconds(analizGecikmesi);

        AnalizSonucu uygunAnaliz = AnaliziBul();
        if (uygunAnaliz == null)
            uygunAnaliz = VarsayilanAnalizOlustur();

        if (diyalogPanel != null)
            diyalogPanel.SetActive(false);

        analizGosterici.AnalizeGoster(uygunAnaliz, secimPuanlari, yapilanSecimler);
    }

    void SeansiGecir()
    {
        SeansGecisYoneticisi.SeansiHazirla();
    }

    private bool ShouldShowAnalysis()
    {
        if (seansAnalizGoster == null || seansAnalizGoster.Length == 0)
            return false;

        if (mevcutSeansIndex >= 0 && mevcutSeansIndex < seansAnalizGoster.Length)
            return seansAnalizGoster[mevcutSeansIndex];

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
            ozet = "Seansınız tamamlandı.",
            gerekenEtiketler = new List<string>(),
            minPuan = 0
        };
    }

    public void PanelleriKapatVeDevamEt()
    {
        if (diyalogPanel != null)
            diyalogPanel.SetActive(false);

        if (analizGosterici != null)
            analizGosterici.PaneliKapat();

        SeansGecisYoneticisi.SeansiHazirla();
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

        AdimiYukle("1");
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

    [ContextMenu("Seansı Yeniden Başlat")]
    public void SeansiYenidenBaslat()
    {
        if (secenekGostermeCoroutine != null)
            StopCoroutine(secenekGostermeCoroutine);

        if (analizGostermeCoroutine != null)
            StopCoroutine(analizGostermeCoroutine);

        yapilanSecimler.Clear();
        secimPuanlari.Clear();

        if (diyalogJson == null)
        {
            Debug.LogWarning("Yeniden başlatılacak JSON atanmadı.");
            return;
        }

        diyalogData = JsonUtility.FromJson<DiyalogData>(diyalogJson.text);
        adimlar = diyalogData.adimlar;

        if (diyalogPanel != null)
            diyalogPanel.SetActive(true);

        AdimiYukle("1");
    }

    public void DiyalogBaslat(TextAsset yeniJson)
{
    if (yeniJson == null)
    {
        Debug.LogWarning($"{gameObject.name}: Başlatılacak JSON atanmadı.");
        return;
    }

    Debug.Log($"{gameObject.name}: DiyalogBaşlat çağrıldı → {yeniJson.name}");

    if (secenekGostermeCoroutine != null)
        StopCoroutine(secenekGostermeCoroutine);

    if (analizGostermeCoroutine != null)
        StopCoroutine(analizGostermeCoroutine);

    mevcutSeansIndex = 0;
    yapilanSecimler.Clear();
    secimPuanlari.Clear();

    diyalogJson = yeniJson;
    diyalogData = JsonUtility.FromJson<DiyalogData>(diyalogJson.text);
    adimlar = diyalogData.adimlar;

    if (diyalogPanel != null)
        diyalogPanel.SetActive(true);

    AdimiYukle("1");
}
}