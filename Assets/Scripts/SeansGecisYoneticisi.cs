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

    [Header("Yüklenecek Yeni Seanslar (Seans 2-3-4-5)")]
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

        diyalogYoneticisi.SonrakiSeansiBaslat(sonrakiJson);

        if (gecisPaneli != null)
            gecisPaneli.SetActive(false);

        if (devamButonu != null)
            devamButonu.gameObject.SetActive(false);
    }
}