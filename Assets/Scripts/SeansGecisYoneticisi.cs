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
        if (gecisPaneli != null) gecisPaneli.SetActive(false);
        if (devamButonu != null) devamButonu.gameObject.SetActive(false);
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
        if (gecisMesaji != null) gecisMesaji.text = "";
        yield return new WaitForSeconds(2f);

        if (gecisMesaji != null) gecisMesaji.text = mesaj;
        yield return new WaitForSeconds(3f);

        if (devamButonu != null)
        {
            devamButonu.gameObject.SetActive(true);
            devamButonu.onClick.RemoveAllListeners();
            devamButonu.onClick.AddListener(DevamEtInstance); // ✔ doğru yöntem
        }
    }

    // ❗️ Bu static DEĞİL – doğru şekilde çağrılıyor
    private void DevamEtInstance()
    {
        if (guncelSeansIndex >= sonrakiSeanslar.Length)
        {
            Debug.Log("Tüm seanslar tamamlandı!");
            return;
        }

        TextAsset sonrakiJson = sonrakiSeanslar[guncelSeansIndex];
        guncelSeansIndex++; // ✔ ARTIK ARTACAK

        diyalogYoneticisi.SonrakiSeansiBaslat(sonrakiJson);

        if (gecisPaneli != null) gecisPaneli.SetActive(false);
        if (devamButonu != null) devamButonu.gameObject.SetActive(false);
    }
}