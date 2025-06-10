using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AnalizGosterici : MonoBehaviour
{
    public static AnalizGosterici instance;

    [Header("UI Panelleri")]
    public List<GameObject> paneller;

    [Header("Metin Alanları")]
    public TMP_Text analizMetni;

    [Header("Butonlar")]
    public GameObject yeniseansButonu;
    public GameObject kapatButonu;

    [Header("Gecikme")]
    public float analizGecikmesi = 2.0f;

    // ✅ Yeni: geçmiş analizleri saklamak için liste
    private List<string> analizKayitlari = new List<string>();

    // ✅ Yeni: cutscene'in sadece bir kere oynatılmasını sağlamak için flag
    private bool cutsceneZatenOynatildi = false;

    private void Awake()
    {
        instance = this;
    }

    // 🎯 Seans sonunda çağrılır
    public void AnalizeGoster(AnalizSonucu analiz, Dictionary<string, int> puanlar, List<string> secimler)
    {
        foreach (GameObject panel in paneller)
        {
            panel.SetActive(false);
        }

        if (analizMetni != null)
        {
            analizMetni.text = $"<b>{analiz.baslik}</b>\n\n{analiz.ozet}\n\n{analiz.detay}";
        }

        if (kapatButonu != null) kapatButonu.SetActive(true);
        if (yeniseansButonu != null) yeniseansButonu.SetActive(true);

        if (paneller.Count > 0)
        {
            paneller[0].SetActive(true); // örneğin AnalizPaneli
        }

        // ✅ Yeni: analiz ozetini geçmişe ekle
        analizKayitlari.Add(analiz.ozet);

        // Cursor ayarları
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (DiyalogYoneticisi.instance != null)
        {
            if (DiyalogYoneticisi.instance.mevcutSeansIndex >= 4)
            {
                yeniseansButonu.SetActive(false);
            }
        }
    }

    // DevamEt butonu - Normal analiz akışını sürdürür
    public void DevamEt()
    {
        foreach (GameObject panel in paneller)
        {
            panel.SetActive(false);
        }

        // Normal akışı sürdür - SeansiGecir metodunu çağır
        if (KarakterYonetici.instance != null)
        {
            KarakterVerisi aktifKarakter = KarakterYonetici.instance.GetAktifKarakter();
            if (aktifKarakter != null)
            {
                DiyalogYoneticisi diyalogYoneticisi = aktifKarakter.karakterPrefab.GetComponentInChildren<DiyalogYoneticisi>();
                if (diyalogYoneticisi != null)
                {
                    Debug.Log("Normal akış sürdürülüyor - Seans geçiş paneli gösterilecek");
                    diyalogYoneticisi.SeansiGecir(); // Normal geçiş (seans geçiş paneli ile)
                }
                else
                {
                    Debug.LogError("DiyalogYoneticisi bulunamadı!");
                }
            }
        }

        // Cursor ayarlarını normale döndür
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // 🎯 Not defterine tıklanınca çağrılır
    public void GecmisAnalizleriGoster()
    {
        foreach (GameObject panel in paneller)
        {
            panel.SetActive(false);
        }

        if (paneller.Count > 0)
        {
            paneller[0].SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (analizMetni != null)
        {
            analizMetni.text = "<b>Geçmiş Seans Analizleri</b>\n\n";

            if (analizKayitlari.Count == 0)
            {
                analizMetni.text += "Henüz analiz kaydı bulunmuyor.";
            }
            else
            {
                foreach (string kayit in analizKayitlari)
                {
                    analizMetni.text += "• " + kayit + "\n\n";
                }
            }
        }

        if (kapatButonu != null) kapatButonu.SetActive(true);
        if (yeniseansButonu != null) yeniseansButonu.SetActive(false); // yeni seans butonu gerekmez
    }

    public void PaneliKapat()
    {
        foreach (GameObject panel in paneller)
        {
            panel.SetActive(false);
        }

        // CrosshairEtkilesim varsa cursor ayarlarını ona bırak
        if (CrosshairEtkilesim.instance != null)
        {
            CrosshairEtkilesim.instance.CrosshairVeKontrolGeriGetir();
        }
        else
        {
            // Fallback cursor ayarları
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (DiyalogYoneticisi.instance != null)
        {
            if (DiyalogYoneticisi.instance.mevcutSeansIndex >= 4)
            {
                if (!cutsceneZatenOynatildi)
                {
                    KarakterYonetici.instance.SeansSonuCutsceneBaslat();
                    cutsceneZatenOynatildi = true;
                    Debug.Log("Cutscene bir kez oynatıldı.");
                }
                else
                {
                    Debug.Log("Cutscene zaten oynatıldı, tekrar tetiklenmedi.");
                }
            }
        }
    }

    public void YeniSeansaBasla()
    {
        foreach (GameObject panel in paneller)
        {
            panel.SetActive(false);
        }

        // Karakteri sıfırla ve idle moda dön
        if (KarakterYonetici.instance != null)
        {
            KarakterYonetici.instance.IdleModaDon();
        }

        Debug.Log("Yeni seans için idle moda döndü");
    }

    // Analiz kayıtlarını temizleme (debug için)
    [ContextMenu("Analiz Kayıtlarını Temizle")]
    public void AnalizKayitlariniTemizle()
    {
        analizKayitlari.Clear();
        Debug.Log("Analiz kayıtları temizlendi");
    }

    [ContextMenu("Kayıtlı Analiz Sayısını Göster")]
    public void KayitliAnalizSayisiniGoster()
    {
        Debug.Log($"Toplam kayıtlı analiz sayısı: {analizKayitlari.Count}");
    }
}