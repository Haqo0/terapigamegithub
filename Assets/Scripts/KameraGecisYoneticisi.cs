using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class KameraGecisYoneticisi : MonoBehaviour
{
    [Header("Kamera Referansları")]
    public Camera kamera;
    public Transform oyuncuGovdesi;
    public MouseCameraKontrol mousKontrol;

    [Header("Hedef Pozisyon")]
    public float hedefXRotation = 3.7f;
    public float hedefYRotation = 0f; // İsteğe bağlı Y rotasyonu da ayarlayabilirsiniz

    [Header("Geçiş Paneli")]
    public GameObject gecisPaneli;
    public Image fadeImage; // Panel içinde siyah bir Image olmalı
    public float fadeHizi = 2f;

    [Header("Geçiş Ayarları")]
    public float gecisHizi = 2f;

    private Vector3 oncekiKameraRotation;
    private Vector3 oncekiOyuncuRotation;
    private bool gecisYapiliyor = false;

    public static KameraGecisYoneticisi instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (gecisPaneli != null)
            gecisPaneli.SetActive(false);

        // Fade image'ı başta transparan yap
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;
        }
    }

    // CrosshairEtkilesim'den çağrılacak
    public void SeansaGirisGecisi()
    {
        if (gecisYapiliyor) return;

        StartCoroutine(SeansGecisAnimasyonu());
    }

    // Seans bitiminde çağrılacak
    public void SeansCikisGecisi()
    {
        if (gecisYapiliyor) return;

        StartCoroutine(SeansCikisAnimasyonu());
    }

    private IEnumerator SeansGecisAnimasyonu()
    {
        gecisYapiliyor = true;

        // Önceki rotasyonları kaydet
        oncekiKameraRotation = kamera.transform.localEulerAngles;
        oncekiOyuncuRotation = oyuncuGovdesi.eulerAngles;

        // Mouse kontrolünü devre dışı bırak
        if (mousKontrol != null)
            mousKontrol.enabled = false;

        // Panel aktif et
        if (gecisPaneli != null)
            gecisPaneli.SetActive(true);

        // Fade In (Karartma)
        yield return StartCoroutine(FadeIn());

        // Kamera pozisyonunu ayarla (karanlıkta)
        kamera.transform.localRotation = Quaternion.Euler(hedefXRotation, 0f, 0f);
        oyuncuGovdesi.rotation = Quaternion.Euler(0f, hedefYRotation, 0f);

        // Kısa bekleme
        yield return new WaitForSeconds(0.5f);

        // Fade Out (Aydınlatma)
        yield return StartCoroutine(FadeOut());

        // Panel kapat
        if (gecisPaneli != null)
            gecisPaneli.SetActive(false);

        gecisYapiliyor = false;
    }

    private IEnumerator SeansCikisAnimasyonu()
    {
        gecisYapiliyor = true;

        // Panel aktif et
        if (gecisPaneli != null)
            gecisPaneli.SetActive(true);

        // Fade In (Karartma)
        yield return StartCoroutine(FadeIn());

        // Önceki rotasyonlara geri dön (karanlıkta)
        kamera.transform.localRotation = Quaternion.Euler(oncekiKameraRotation);
        oyuncuGovdesi.rotation = Quaternion.Euler(oncekiOyuncuRotation);

        // Mouse kontrolünü tekrar aktif et
        if (mousKontrol != null)
            mousKontrol.enabled = true;

        // Kısa bekleme
        yield return new WaitForSeconds(0.5f);

        // Fade Out (Aydınlatma)
        yield return StartCoroutine(FadeOut());

        // Panel kapat
        if (gecisPaneli != null)
            gecisPaneli.SetActive(false);

        gecisYapiliyor = false;
    }

    private IEnumerator FadeIn()
    {
        if (fadeImage == null) yield break;

        float timer = 0f;
        Color color = fadeImage.color;

        while (timer < 1f)
        {
            timer += Time.deltaTime * fadeHizi;
            color.a = Mathf.Lerp(0f, 1f, timer);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;
    }

    private IEnumerator FadeOut()
    {
        if (fadeImage == null) yield break;

        float timer = 0f;
        Color color = fadeImage.color;

        while (timer < 1f)
        {
            timer += Time.deltaTime * fadeHizi;
            color.a = Mathf.Lerp(1f, 0f, timer);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;
    }

    // Alternatif: Yumuşak geçiş istiyorsanız (animasyonlu)
    public void YumusakSeansGecisi()
    {
        if (gecisYapiliyor) return;

        StartCoroutine(YumusakGecisAnimasyonu());
    }

    private IEnumerator YumusakGecisAnimasyonu()
    {
        gecisYapiliyor = true;

        // Önceki rotasyonları kaydet
        Vector3 baslangicKamera = kamera.transform.localEulerAngles;
        Vector3 baslangicOyuncu = oyuncuGovdesi.eulerAngles;

        Vector3 hedefKamera = new Vector3(hedefXRotation, 0f, 0f);
        Vector3 hedefOyuncu = new Vector3(0f, hedefYRotation, 0f);

        // Mouse kontrolünü devre dışı bırak
        if (mousKontrol != null)
            mousKontrol.enabled = false;

        // Panel aktif et
        if (gecisPaneli != null)
            gecisPaneli.SetActive(true);

        // Fade In
        yield return StartCoroutine(FadeIn());

        // Yumuşak rotasyon geçişi (karanlıkta)
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime * gecisHizi;

            Vector3 yeniKameraRot = Vector3.Lerp(baslangicKamera, hedefKamera, timer);
            Vector3 yeniOyuncuRot = Vector3.Lerp(baslangicOyuncu, hedefOyuncu, timer);

            kamera.transform.localRotation = Quaternion.Euler(yeniKameraRot);
            oyuncuGovdesi.rotation = Quaternion.Euler(yeniOyuncuRot);

            yield return null;
        }

        // Hedef pozisyona kesin olarak ayarla
        kamera.transform.localRotation = Quaternion.Euler(hedefKamera);
        oyuncuGovdesi.rotation = Quaternion.Euler(hedefOyuncu);

        // Fade Out
        yield return StartCoroutine(FadeOut());

        // Panel kapat
        if (gecisPaneli != null)
            gecisPaneli.SetActive(false);

        gecisYapiliyor = false;
    }

    // MouseCameraKontrol'den erişim için
    public bool GecisYapiliyorMu()
    {
        return gecisYapiliyor;
    }
}