using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnalizGosterici : MonoBehaviour
{
    [Header("UI Paneli")]
    public GameObject analizPanel;

    [Header("Metin Alanları")]
    public TMP_Text analizMetni;

    [Header("Butonlar")]
    public Button yeniseansButonu;
    public Button kapatButonu;

    private void Start()
    {
        if (analizPanel != null)
        {
            analizPanel.SetActive(false); // Başlangıçta gizli
            Debug.Log("Analiz paneli başta gizli başlatıldı.");
        }

        if (kapatButonu != null)
        {
            kapatButonu.onClick.AddListener(() => PaneliKapat());
        }
    }

    public void PaneliKapat()
    {
        if (analizPanel != null)
            analizPanel.SetActive(false);
    }

    // 🔧 3 parametreli analiz gösterme fonksiyonu
    public void AnalizeGoster(AnalizSonucu analiz, Dictionary<string, int> puanlar, List<string> secimler)
    {
        if (analizPanel != null)
            analizPanel.SetActive(true);

        if (analizMetni != null)
            analizMetni.text = analiz.ozet; // veya analiz.detay, analiz.baslik gibi değiştirebilirsin

        Debug.Log("Analiz gösteriliyor...");
    }
}