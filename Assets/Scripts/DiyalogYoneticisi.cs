using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class DiyalogYoneticisi : MonoBehaviour
{
    [Header("Zaman Ayarları")]
    public float seceneklerGecikmesi = 2.0f; // Saniye cinsinden gecikme
    
    [Header("UI Referansları")]
    public TMP_Text npcText;
    public SecimSistemi secimSistemi;
    public AnalizGosterici analizGosterici;
    public SecenekGecikmesi gecikmeGostergesi; // Yeni eklenen
    
    [Header("Veri")]
    public TextAsset diyalogJson;

    private List<DiyalogAdimi> adimlar;
    private DiyalogAdimi mevcutAdim;
    private DiyalogData diyalogData;
    
    // Seçim takip sistemi
    private Dictionary<string, int> secimPuanlari = new Dictionary<string, int>();
    private List<string> yapilanSecimler = new List<string>();
    private Coroutine secenekGostermeCoroutine; // Aktif coroutine referansı

    void Start()
    {
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
        // En uygun analizi bul
        AnalizSonucu uygunAnaliz = AnaliziBul();
        
        if (uygunAnaliz != null)
        {
            analizGosterici.AnalizeGoster(uygunAnaliz, secimPuanlari, yapilanSecimler);
        }
        else
        {
            Debug.LogWarning("Uygun analiz bulunamadı!");
        }
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

    // Debug için
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