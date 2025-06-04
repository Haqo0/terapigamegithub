using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class SeansGecisYoneticisi : MonoBehaviour
{
    [Header("UI Bileşenleri")]
    public GameObject gecisPaneli;
    public TMP_Text gecisMesaji;
    public Button devamButonu;

    [Header("Diyalog Yöneticisi")]
    public DiyalogYoneticisi diyalogYoneticisi;

    [Header("Yüklenecek Yeni Seans")]
    public TextAsset[] yeniSeansJson; // Yeni seans JSON dosyası (örneğin Mert_seans2.json)
    //singelton pattern
    public static SeansGecisYoneticisi instance;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        // Panel ve buton başlangıçta kapalı olmalı
        if (gecisPaneli != null) gecisPaneli.SetActive(false);
        if (devamButonu != null) devamButonu.gameObject.SetActive(false);
    }

    public static void SeansiHazirla(string mesaj = "Bir sonraki seansa geçiliyor...")
    {
        Debug.Log("Seans geçişi hazırlanıyor: " + mesaj);
        if (instance != null && instance.gecisPaneli != null)
        {
            instance.gecisPaneli.SetActive(true);
        }

        if (instance != null && instance.gecisMesaji != null)
        {
            instance.StartCoroutine(instance.GecikmeliMesajVeDevam(mesaj));
        }

        if (instance != null)
        {
            instance.StartCoroutine(instance.BekleVeDevamGelsin());
        }
    }

    private IEnumerator GecikmeliMesajVeDevam(string mesaj)
    {
        if (gecisMesaji != null)
            gecisMesaji.text = "";

        yield return new WaitForSeconds(2f);

        if (gecisMesaji != null)
            gecisMesaji.text = mesaj;

        yield return BekleVeDevamGelsin();
    }

    private IEnumerator BekleVeDevamGelsin()
    {
        Debug.Log("Seans geçişi bekleniyor...");
        yield return new WaitForSeconds(3f); // Geçiş mesajı 3 saniye kalır

        if (devamButonu != null)
        {
            devamButonu.gameObject.SetActive(true);
            devamButonu.onClick.RemoveAllListeners(); // Önceki dinleyicileri temizle
            devamButonu.onClick.AddListener(DevamEt); // Devam et butonuna tıklandığında DevamEt metodunu çağır
        }
    }
    public static void DevamEt()
    {
        if (instance != null && instance.diyalogYoneticisi != null && instance.yeniSeansJson != null)
        {
            if (instance.yeniSeansJson.Length > 0)
            {
                instance.diyalogYoneticisi.SonrakiSeansiBaslat(instance.yeniSeansJson[0]);
            }
        }

        if (instance != null)
        {
            if (instance.gecisPaneli != null) instance.gecisPaneli.SetActive(false);
            if (instance.devamButonu != null) instance.devamButonu.gameObject.SetActive(false);
        }
    }

}