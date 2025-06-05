using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnalizGosterici : MonoBehaviour
{
    [Header("UI Paneli")]
    public GameObject[] paneller;

    [Header("Metin Alanları")]
    public TMP_Text analizMetni;

    [Header("Butonlar")]
    public Button yeniseansButonu;
    public Button kapatButonu;

    private void Start()
    {
        if (paneller != null)
        {
            foreach (var panel in paneller)
            {
                if (panel != null)
                    panel.SetActive(false);
            }
            Debug.Log("Analiz paneli başta gizli başlatıldı.");
        }

        // KAPAT BUTONU DÜZELTMESİ
        if (kapatButonu != null)
        {
            kapatButonu.onClick.RemoveAllListeners(); // Önceki listener'ları temizle
            kapatButonu.onClick.AddListener(() => {
                Debug.Log("Kapat butonu tıklandı - Paneller kapatılıyor ve seans geçişi başlatılıyor");
                KapatVeDevamEt();
            });
        }
    }

    // Hem analiz panelini kapat hem de seans geçişini başlat
    private void KapatVeDevamEt()
    {
        // Önce analiz panelini kapat
        PaneliKapat();
        
        // // Sonra DiyalogYoneticisi'ne panelleri kapatması ve devam etmesi için sinyal gönder
        // if (DiyalogYoneticisi.instance != null)
        // {
        //     DiyalogYoneticisi.instance.PanelleriKapatVeDevamEt();
        // }
        // else
        // {
        //     Debug.LogWarning("DiyalogYoneticisi instance bulunamadı!");
        // }
    }

    public void PaneliKapat()
    {
        if (paneller != null)
        {
            foreach (var panel in paneller)
            {
                if (panel != null)
                    panel.SetActive(false);
            }
        }
        Debug.Log("Analiz paneli kapatıldı.");
    }

    // 🔧 3 parametreli analiz gösterme fonksiyonu
    public void AnalizeGoster(AnalizSonucu analiz, Dictionary<string, int> puanlar, List<string> secimler)
    {
        if (paneller != null)
        {
            foreach (var panel in paneller)
            {
                if (panel != null)
                    panel.SetActive(true);
            }
        }

        if (analizMetni != null)
            analizMetni.text = analiz.ozet;

    }
}