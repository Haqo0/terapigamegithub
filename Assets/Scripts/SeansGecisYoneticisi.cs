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
        Debug.Log($"SeansGecisYoneticisi: {yeniJsonlar.Length} JSON dosyası ayarlandı");
    }

    public static void SeansiHazirla(string mesaj = "Bir sonraki seansa geçiliyor...")
    {
        if (instance != null)
        {
            instance.gecisPaneli.SetActive(true);
            instance.StartCoroutine(instance.GecikmeliMesajVeDevam(mesaj));
        }
    }

    private IEnumerator GecikmeliMesajVeDevam(string mesaj)
    {
        // Önlem: buton sıfırlansın
        if (devamButonu != null)
        {
            devamButonu.onClick.RemoveAllListeners();
            devamButonu.gameObject.SetActive(false);
        }

        // İlk boş bekleme
        if (gecisMesaji != null)
            gecisMesaji.text = "";

        yield return new WaitForSeconds(2f);

        // Mesaj göster
        if (gecisMesaji != null)
            gecisMesaji.text = mesaj;

        // Mesaj görünsün → sonra buton gelsin
        yield return new WaitForSeconds(2.5f);

        if (devamButonu != null)
        {
            devamButonu.gameObject.SetActive(true);
            devamButonu.onClick.AddListener(DevamEtInstance);
        }
    }

    private void DevamEtInstance()
    {
        // JSON dosyaları var mı?
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
        guncelSeansIndex++;

        Debug.Log($"Yükleniyor: {sonrakiJson.name} (Seans {guncelSeansIndex}/{sonrakiSeanslar.Length})");

        diyalogYoneticisi.SonrakiSeansiBaslat(sonrakiJson);

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