using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SeansGecisYoneticisi : MonoBehaviour
{
    [Header("UI Bileşenleri")]
    public GameObject gecisPaneli;
    public TMP_Text gecisMesaji;
    public Button devamButonu;

    [Header("Diyalog Yöneticisi")]
    public DiyalogYoneticisi diyalogYoneticisi;

    [Header("Yüklenecek Yeni Seanslar")]
    public TextAsset[] sonrakiSeanslar;
    private int guncelSeansIndex = 0;

    public static SeansGecisYoneticisi instance;

    private bool seansYuklendi = false; // ✅ tekrar yüklemeyi engeller

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (gecisPaneli != null)
            gecisPaneli.SetActive(false);

        if (devamButonu != null)
            devamButonu.gameObject.SetActive(false);
    }

    public void JsonDosyalariniAyarla(TextAsset[] yeniJsonlar)
    {
        sonrakiSeanslar = yeniJsonlar;
        guncelSeansIndex = 0;
        seansYuklendi = false;
        Debug.Log($"SeansGecisYoneticisi: {yeniJsonlar.Length} JSON dosyası ayarlandı");
    }

    public static void SeansiHazirla(string mesaj = "Bir sonraki seansa geçiliyor...")
    {
        if (instance != null)
        {
            instance.seansYuklendi = false; // ✅ yeni geçişte sıfırla
            instance.gecisPaneli.SetActive(true);
            instance.StartCoroutine(instance.GecikmeliMesajVeDevam(mesaj));
        }
    }

    private IEnumerator GecikmeliMesajVeDevam(string mesaj)
    {
        if (devamButonu != null)
        {
            devamButonu.onClick.RemoveAllListeners();
            devamButonu.gameObject.SetActive(false);
        }

        if (gecisMesaji != null)
            gecisMesaji.text = "";

        yield return new WaitForSeconds(2f);

        if (gecisMesaji != null)
            gecisMesaji.text = mesaj;

        yield return new WaitForSeconds(2.5f);

        if (devamButonu != null)
        {
            devamButonu.gameObject.SetActive(true);
            devamButonu.onClick.AddListener(DevamEtInstance);
        }
    }

    private void DevamEtInstance()
    {
        if (seansYuklendi) return; // ✅ önceden yüklendiyse tekrar yükleme

        seansYuklendi = true; // ✅ bir kez yüklendiğini işaretle

        if (sonrakiSeanslar == null || sonrakiSeanslar.Length == 0)
        {
            Debug.LogWarning("Sonraki seanslar dizisi boş! CrosshairEtkilesim'den JSON'lar atanmamış olabilir.");
            return;
        }

        if (guncelSeansIndex >= sonrakiSeanslar.Length)
        {
            Debug.Log("Tüm seanslar tamamlandı!");
            if (gecisMesaji != null)
                gecisMesaji.text = "Terapinin tüm seansları tamamlandı. Teşekkür ederiz.";

            if (devamButonu != null)
                devamButonu.gameObject.SetActive(false);

            return;
        }

        TextAsset sonrakiJson = sonrakiSeanslar[guncelSeansIndex];

        Debug.Log($"Yükleniyor: {sonrakiJson.name} (Seans {guncelSeansIndex + 1}/{sonrakiSeanslar.Length})");

        diyalogYoneticisi.SonrakiSeansiBaslat(sonrakiJson);

        guncelSeansIndex++;

        if (gecisPaneli != null)
            gecisPaneli.SetActive(false);

        if (devamButonu != null)
            devamButonu.gameObject.SetActive(false);
    }

    [ContextMenu("Debug - Mevcut JSON'ları Listele")]
    private void DebugMevcutJsonlar()
    {
        Debug.Log("=== MEVCUT JSON DOSYALARI ===");
        if (sonrakiSeanslar != null && sonrakiSeanslar.Length > 0)
        {
            for (int i = 0; i < sonrakiSeanslar.Length; i++)
            {
                string durum = (i < guncelSeansIndex) ? "TAMAMLANDI" :
                              (i == guncelSeansIndex) ? "SONRAKİ" : "BEKLİYOR";
                Debug.Log($"Seans {i + 1}: {sonrakiSeanslar[i].name} - {durum}");
            }
        }
        else
        {
            Debug.Log("Hiç JSON dosyası atanmamış!");
        }
    }
}