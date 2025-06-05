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

        if (kapatButonu != null)
        {
            kapatButonu.onClick.AddListener(() => PaneliKapat());
        }
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

        Debug.Log("Analiz gösteriliyor...");
    }
}