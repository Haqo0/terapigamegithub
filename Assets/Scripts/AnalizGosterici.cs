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
    public GameObject devamEtButonu;

    [Header("Gecikme")]
    public float analizGecikmesi = 2.0f;

    [Header("Bağlantılar")]
    public CrosshairEtkilesim crosshairRef;

    // Geçmiş analizleri saklamak için liste
    private List<string> analizKayitlari = new List<string>();

    private void Awake()
    {
        instance = this;
    }

    // Seans sonunda çağrılır
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
        if (devamEtButonu != null) devamEtButonu.SetActive(true);

        if (paneller.Count > 0)
        {
            paneller[0].SetActive(true); // örneğin AnalizPaneli
        }

        analizKayitlari.Add(analiz.ozet);
    }

    // Not defterine tıklanınca çağrılır
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
            foreach (string kayit in analizKayitlari)
            {
                analizMetni.text += "• " + kayit + "\n\n";
            }
        }

        if (kapatButonu != null) kapatButonu.SetActive(true);
        if (yeniseansButonu != null) yeniseansButonu.SetActive(false);
        if (devamEtButonu != null) devamEtButonu.SetActive(false);
    }

    // Sadece analiz panelini kapatır ve crosshair'i geri getirir
    public void SeansaDevamEt()
    {
        PaneliKapat();

        if (crosshairRef != null)
        {
            crosshairRef.CrosshairVeKontrolGeriGetir();
        }
        else
        {
            Debug.LogWarning("CrosshairEtkilesim referansı atanmadı!");
        }

        Debug.Log("Seansa devam edildi, analiz paneli kapatıldı.");
    }

    public void PaneliKapat()
    {
        foreach (GameObject panel in paneller)
        {
            panel.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (devamEtButonu != null) devamEtButonu.SetActive(false);
        if (kapatButonu != null) kapatButonu.SetActive(false);
        if (yeniseansButonu != null) yeniseansButonu.SetActive(false);
    }
}