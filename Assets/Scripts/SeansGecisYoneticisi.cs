using System.Collections;
using UnityEngine;
using TMPro;

public class SeansGecisYoneticisi : MonoBehaviour
{
    [Header("UI Bileşenleri")]
    public GameObject gecisPaneli;
    public TMP_Text gecisMesaji;
    public GameObject devamButonu;

    [Header("Diyalog Yöneticisi")]
    public DiyalogYoneticisi diyalogYoneticisi;

    [Header("Yüklenecek Yeni Seans")]
    public TextAsset yeniSeansJson; // Yeni seans JSON dosyası (örneğin Mert_seans2.json)

    private void Start()
    {
        // Panel ve buton başlangıçta kapalı olmalı
        if (gecisPaneli != null) gecisPaneli.SetActive(false);
        if (devamButonu != null) devamButonu.SetActive(false);
    }

    public void SeansiHazirla(string mesaj = "Birazdan yeni seansa geçiyoruz...")
    {
        if (gecisPaneli != null)
        {
            gecisPaneli.SetActive(true);
        }

        if (gecisMesaji != null)
        {
            gecisMesaji.text = mesaj;
        }

        StartCoroutine(BekleVeDevamGelsin());
    }

    private IEnumerator BekleVeDevamGelsin()
    {
        yield return new WaitForSeconds(3f); // Geçiş mesajı 3 saniye kalır

        if (devamButonu != null)
        {
            devamButonu.SetActive(true); // Oyuncu manuel başlatabilir
        }
    }

    public void DevamEt()
    {
        if (diyalogYoneticisi != null && yeniSeansJson != null)
        {
            // Yeni seansı başlat
            diyalogYoneticisi.SonrakiSeansiBaslat(yeniSeansJson);
        }

        // UI öğelerini kapat
        if (gecisPaneli != null) gecisPaneli.SetActive(false);
        if (devamButonu != null) devamButonu.SetActive(false);
    }
}