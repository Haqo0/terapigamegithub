using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AnalizGosterici : MonoBehaviour
{
    [Header("UI Paneli")]
    public GameObject analizPaneli;
    
    [Header("Metin Alanları")]
    public TMP_Text analizBasligi;
    public TMP_Text analizAciklamasi;
    public TMP_Text detayliAnaliz;
    public TMP_Text secimOzeti;
    public TMP_Text puanTablosu;
    
    [Header("Butonlar")]
    public Button yeniseansButonu;
    public Button kapatButonu;

    private void Start()
    {
        // Panel başlangıçta kapalı
        analizPaneli.SetActive(false);
        
        // Buton eventleri
        if (yeniseansButonu != null)
            yeniseansButonu.onClick.AddListener(YeniSeansBaslat);
            
        if (kapatButonu != null)
            kapatButonu.onClick.AddListener(PaneliKapat);
    }

    public void AnalizeGoster(AnalizSonucu analiz, Dictionary<string, int> secimPuanlari, List<string> yapilanSecimler)
    {
        // Panel aktif et
        analizPaneli.SetActive(true);
        
        // Analiz bilgilerini göster
        analizBasligi.text = analiz.baslik;
        analizAciklamasi.text = analiz.aciklama;
        detayliAnaliz.text = analiz.detayliAnaliz;
        
        // Yapılan seçimleri listele
        string secimMetni = "Yaptığınız Seçimler:\n";
        for (int i = 0; i < yapilanSecimler.Count; i++)
        {
            secimMetni += $"{i + 1}. {yapilanSecimler[i]}\n";
        }
        secimOzeti.text = secimMetni;
        
        // Puan tablosunu oluştur
        string puanMetni = "Psikolojik Profil Puanları:\n";
        foreach (var kvp in secimPuanlari)
        {
            string kategoriAdi = KategoriAdiniCevir(kvp.Key);
            puanMetni += $"• {kategoriAdi}: {kvp.Value} puan\n";
        }
        puanTablosu.text = puanMetni;
    }

    private string KategoriAdiniCevir(string etiket)
    {
        switch (etiket.ToLower())
        {
            case "empati":
                return "Empatik Yaklaşım";
            case "analitik":
                return "Analitik Düşünce";
            case "destekleyici":
                return "Destekleyici Tavır";
            case "sabırlı":
                return "Sabırlı Dinleme";
            case "merakli":
                return "Meraklı Sorgulama";
            case "pratik":
                return "Pratik Çözümler";
            default:
                return etiket;
        }
    }

    private void YeniSeansBaslat()
    {
        // Yeni seans başlatma mantığı
        PaneliKapat();
        
        // Diyalog yöneticisini yeniden başlat
        DiyalogYoneticisi diyalogYoneticisi = FindObjectOfType<DiyalogYoneticisi>();
        if (diyalogYoneticisi != null)
        {
            // Yeni seansa başla
            diyalogYoneticisi.SendMessage("Start");
        }
    }

    private void PaneliKapat()
    {
        analizPaneli.SetActive(false);
    }
}