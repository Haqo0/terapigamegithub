using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class DiyalogYoneticisi : MonoBehaviour
{
    [Header("Zaman Ayarları")]
    public float seceneklerGecikmesi = 2.0f;

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

    public static DiyalogYoneticisi instance;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (diyalogPanel != null)
            diyalogPanel.SetActive(true); // Panel oyun başında açık olmalı

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
            Debug.LogWarning("ID bulunamadı: " + id);
            return;
        }

        secimSistemi.SecenekleriTemizle();
        npcText.text = mevcutAdim.anlatim;

        if (mevcutAdim.seansSonu)
        {
            SeansiSonlandir();
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

    private void SeansiSonlandir()
    {
        Debug.Log("Seans sonlandırılıyor...");
        AnalizSonucu uygunAnaliz = AnaliziBul();

        if (uygunAnaliz != null)
        {
            analizGosterici.AnalizeGoster(uygunAnaliz, secimPuanlari, yapilanSecimler);
        }
        else
        {
            Debug.LogWarning("Uygun analiz bulunamadı.");
        }

        if (diyalogPanel != null)
            diyalogPanel.SetActive(false);

        SeansGecisYoneticisi.SeansiHazirla();
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

        return diyalogData.analizSonuclari.FirstOrDefault();
    }

    // 🔄 Seçenekleri hemen gösteren metod
    public void SeçenekleriHemenGöster()
    {
        if (secenekGostermeCoroutine != null)
            StopCoroutine(secenekGostermeCoroutine);

        if (gecikmeGostergesi != null)
            gecikmeGostergesi.GecikmeGosteriminiDurdur();

        npcText.text = "";

        if (mevcutAdim?.secenekler != null && mevcutAdim.secenekler.Count > 0)
            secimSistemi.SecenekleriGoster(mevcutAdim.secenekler, SecimYapildi);
    }

    // 🔄 Yeni JSON yüklendiğinde temiz başlat
    public void SonrakiSeansiBaslat(TextAsset yeniJson)
    {
        if (yeniJson == null)
        {
            Debug.LogWarning("Yeni JSON dosyası atanmadı.");
            return;
        }

        diyalogJson = yeniJson;
        diyalogData = JsonUtility.FromJson<DiyalogData>(diyalogJson.text);
        adimlar = diyalogData.adimlar;
        yapilanSecimler.Clear();
        secimPuanlari.Clear();

        if (diyalogPanel != null)
            diyalogPanel.SetActive(true);

        AdimiYukle("1");
    }
}