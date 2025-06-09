using UnityEngine;
using System.Linq;
using UnityEngine.Playables;

public class KarakterYonetici : MonoBehaviour
{
    [Header("Tüm karakterlerin verileri")]
    public KarakterVerisi[] tumKarakterler;

    private KarakterVerisi aktifKarakter;

    public static KarakterYonetici instance;

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Seçilen karakterin giriş cutscene'ini başlatır.
    /// </summary>
    public void KarakterSeansiBaslat(string karakterAdi)
    {
        aktifKarakter = tumKarakterler.FirstOrDefault(k => k.karakterAdi == karakterAdi);

        if (aktifKarakter == null)
        {
            Debug.LogError("Karakter bulunamadı: " + karakterAdi);
            return;
        }

        aktifKarakter.karakterPrefab.SetActive(true);
        aktifKarakter.girisCutscene.Play();

        Debug.Log($"{karakterAdi} için giriş cutscene başlatıldı.");
    }

    /// <summary>
    /// Giriş cutscene bittiğinde çağrılır. Seansı başlatır.
    /// </summary>
    public void GirisCutsceneBitinceSeansiBaslat()
    {
        if (aktifKarakter == null)
        {
            Debug.LogError("Giriş cutscene sonrası: Aktif karakter yok!");
            return;
        }

        GameObject seansSistemi = aktifKarakter.karakterPrefab;
        seansSistemi.SetActive(true);

        SeansGecisYoneticisi gecisYoneticisi = seansSistemi.GetComponentInChildren<SeansGecisYoneticisi>();
        if (gecisYoneticisi != null)
        {
            gecisYoneticisi.JsonDosyalariniAyarla(aktifKarakter.seansJsonlar);
        }
        else
        {
            Debug.LogWarning("SeansGecisYoneticisi bulunamadı.");
        }

        DiyalogYoneticisi diyalogYoneticisi = seansSistemi.GetComponentInChildren<DiyalogYoneticisi>();
        if (diyalogYoneticisi != null)
        {
            diyalogYoneticisi.diyalogJson = aktifKarakter.seansJsonlar[0];
            diyalogYoneticisi.SeansiYenidenBaslat();
            Debug.Log("Diyalog başlatıldı.");
        }
        else
        {
            Debug.LogError("DiyalogYoneticisi bulunamadı.");
        }
    }

    /// <summary>
    /// Seans sona erdiğinde çıkış cutscene başlatılır.
    /// </summary>
    public void SeansSonuCutsceneBaslat()
    {
        if (aktifKarakter != null && aktifKarakter.cikisCutscene != null)
        {
            aktifKarakter.cikisCutscene.Play();
            Debug.Log("Çıkış cutscene başlatıldı.");
        }
        else
        {
            Debug.LogWarning("Çıkış cutscene başlatılamadı.");
        }
    }

    /// <summary>
    /// Çıkış cutscene sona erdiğinde çağrılır. Oyunu idle moda döndürür.
    /// </summary>
    public void IdleModaDon()
    {
        if (aktifKarakter != null && aktifKarakter.karakterPrefab != null)
        {
            aktifKarakter.karakterPrefab.SetActive(false);
        }

        CrosshairEtkilesim.instance.CrosshairVeKontrolGeriGetir();
        Debug.Log("Oyun idle moda döndü.");
    }

    /// <summary>
    /// Şu anda aktif olan karakteri döndürür.
    /// </summary>
    public KarakterVerisi GetAktifKarakter()
    {
        return aktifKarakter;
    }
}