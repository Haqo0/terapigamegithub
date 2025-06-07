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

    [HideInInspector]
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
        if (guncelSeansIndex >= sonrakiSeanslar.Length)
        {
            Debug.Log("✅ Tüm seanslar tamamlandı!");

            if (gecisMesaji != null)
                gecisMesaji.text = "Terapinin tüm seansları tamamlandı. Teşekkür ederiz.";

            if (devamButonu != null)
                devamButonu.gameObject.SetActive(false);

            return;
        }

        TextAsset sonrakiJson = sonrakiSeanslar[guncelSeansIndex];

        if (sonrakiJson == null)
        {
            Debug.LogWarning($"❌ Seans {guncelSeansIndex + 2} JSON dosyası atanmadı!");
            return;
        }

        guncelSeansIndex++;

        if (diyalogYoneticisi != null)
            diyalogYoneticisi.SonrakiSeansiBaslat(sonrakiJson);
        else
            Debug.LogWarning("❌ DiyalogYoneticisi referansı atanmadı!");

        if (gecisPaneli != null)
            gecisPaneli.SetActive(false);

        if (devamButonu != null)
            devamButonu.gameObject.SetActive(false);
    }

    /// <summary>
    /// 🔄 Dışarıdan karaktere özel seans dizisi atanır.
    /// </summary>
    public void SetSeansListesi(TextAsset[] yeniSeanslar)
    {
        if (yeniSeanslar == null || yeniSeanslar.Length == 0)
        {
            Debug.LogWarning("❌ SetSeansListesi: yeniSeanslar boş!");
            return;
        }

        sonrakiSeanslar = yeniSeanslar;
        guncelSeansIndex = 0;

        Debug.Log($"📌 Yeni seans listesi ayarlandı. Toplam: {sonrakiSeanslar.Length}");
    }
}