using UnityEngine;

public class CrosshairEtkilesim : MonoBehaviour
{
    [Header("Seans Sistemleri")]
    [Tooltip("Karakterlerin seans sistemlerini karakterAdi ile eşleştirin")]
    public SeansSistemiEslesmesi[] seansEslesmeler;

    [Header("UI Elemanları")]
    public GameObject diyalogPaneli;
    public GameObject crosshairObjesi;

    // 👈 SADECE BU SATIR EKLENDİ
    [Header("Kamera Geçişi")]
    public KameraGecisYoneticisi kameraGecisYoneticisi;

    private MonoBehaviour kameraKontrolScripti;
    private Camera kamera;
    private bool seansBasladi = false;
    private GameObject tiklananObj;

    public ProfilGosterici profilGosterici;
    public KarakterYonetici karakterYonetici;

    // Singleton pattern için static instance
    public static CrosshairEtkilesim instance;

    [System.Serializable]
    public class SeansSistemiEslesmesi
    {
        [Tooltip("Karakter adı (küçük harf): mert, ece, alev")]
        public string karakterAdi;

        [Tooltip("Bu karakterin seans sistemi objesi")]
        public GameObject seansObjesi;
    }

    void Start()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("Birden fazla CrosshairEtkilesim instance'ı var!");
        }

        kamera = Camera.main;

        if (diyalogPaneli != null)
            diyalogPaneli.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (crosshairObjesi != null)
            crosshairObjesi.SetActive(true);

        kameraKontrolScripti = kamera.GetComponent<MouseCameraKontrol>();
        if (kameraKontrolScripti != null)
            kameraKontrolScripti.enabled = true;
        else
            Debug.LogWarning("Kamera kontrol scripti bulunamadı!");

        Debug.Log("CrosshairEtkilesim başlatıldı");
    }

    void Update()
    {
        if (seansBasladi) return;

        Ray ray = kamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            if (hit.collider.CompareTag("SeansObjesi"))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    string karakterAdi = KarakterAdiniCikart(hit.collider.gameObject.name);

                    if (string.IsNullOrEmpty(karakterAdi))
                    {
                        Debug.LogWarning($"Karakter adı çıkarılamadı: {hit.collider.gameObject.name}");
                        return;
                    }

                    // 👈 SADECE BU BLOK EKLENDİ (kamera geçişi)
                    if (kameraGecisYoneticisi != null)
                    {
                        kameraGecisYoneticisi.SeansaGirisGecisi();
                        StartCoroutine(SeansBaslatGecikmeli(karakterAdi, hit.collider.gameObject));
                        return;
                    }

                    // ESKİ KOD AYNEN KALIDI
                    if (karakterYonetici != null)
                    {
                        // Önce objeyi ata
                        tiklananObj = hit.collider.gameObject;

                        // Collider'ı devre dışı bırak
                        BoxCollider objCollider = tiklananObj.GetComponent<BoxCollider>();
                        if (objCollider != null)
                        {
                            objCollider.enabled = false;
                            Debug.Log($"{tiklananObj.name} objesi tıklanamaz yapıldı");
                        }

                        // Seansı başlat
                        karakterYonetici.KarakterSeansiBaslat(karakterAdi);
                        seansBasladi = true;

                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;

                        if (crosshairObjesi != null)
                            crosshairObjesi.SetActive(false);

                        if (kameraKontrolScripti != null)
                            kameraKontrolScripti.enabled = false;
                    }
                    else
                    {
                        Debug.LogError("KarakterYonetici referansı bulunamadı!");
                    }
                }
            }
            else if (hit.collider.CompareTag("ProfilObjesi"))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    profilGosterici.ProfilPanelAc();
                }
            }
        }
    }

    // 👈 SADECE BU METOD EKLENDİ
    private System.Collections.IEnumerator SeansBaslatGecikmeli(string karakterAdi, GameObject tiklananObje)
    {
        // Kamera geçişi bitene kadar bekle
        while (kameraGecisYoneticisi.GecisYapiliyorMu())
        {
            yield return null;
        }

        // ESKİ KODUN AYNISI ÇALIŞSIN
        if (karakterYonetici != null)
        {
            // Önce objeyi ata
            tiklananObj = tiklananObje;

            // Collider'ı devre dışı bırak
            BoxCollider objCollider = tiklananObj.GetComponent<BoxCollider>();
            if (objCollider != null)
            {
                objCollider.enabled = false;
                Debug.Log($"{tiklananObj.name} objesi tıklanamaz yapıldı");
            }

            // Seansı başlat
            karakterYonetici.KarakterSeansiBaslat(karakterAdi);
            seansBasladi = true;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (crosshairObjesi != null)
                crosshairObjesi.SetActive(false);

            if (kameraKontrolScripti != null)
                kameraKontrolScripti.enabled = false;
        }
        else
        {
            Debug.LogError("KarakterYonetici referansı bulunamadı!");
        }
    }

    public void CrosshairVeKontrolGeriGetir()
    {
        // 👈 SADECE BU BLOK EKLENDİ (çıkış geçişi)
        if (kameraGecisYoneticisi != null)
        {
            kameraGecisYoneticisi.SeansCikisGecisi();
            StartCoroutine(CrosshairGeriGetirGecikmeli());
            return;
        }

        // ESKİ KOD AYNEN KALIDI
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (crosshairObjesi != null)
            crosshairObjesi.SetActive(true);

        seansBasladi = false;

        if (kameraKontrolScripti != null)
            kameraKontrolScripti.enabled = true;

        if (diyalogPaneli != null)
            diyalogPaneli.SetActive(false);

        Debug.Log("Crosshair ve kontrol geri getirildi");
    }

    // 👈 SADECE BU METOD EKLENDİ
    private System.Collections.IEnumerator CrosshairGeriGetirGecikmeli()
    {
        // Kamera geçişi bitene kadar bekle
        while (kameraGecisYoneticisi.GecisYapiliyorMu())
        {
            yield return null;
        }

        // ESKİ KODUN AYNISI ÇALIŞSIN
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (crosshairObjesi != null)
            crosshairObjesi.SetActive(true);

        seansBasladi = false;

        if (kameraKontrolScripti != null)
            kameraKontrolScripti.enabled = true;

        if (diyalogPaneli != null)
            diyalogPaneli.SetActive(false);

        Debug.Log("Crosshair ve kontrol geri getirildi");
    }

    private string KarakterAdiniCikart(string objeName)
    {
        string karakterAdi = objeName.ToLower();

        // Seans eşleşmelerini kontrol et
        foreach (var eslesme in seansEslesmeler)
        {
            if (karakterAdi.Contains(eslesme.karakterAdi.ToLower()))
                return eslesme.karakterAdi.ToLower();
        }

        Debug.LogWarning($"Bilinmeyen karakter objesi: {objeName}");
        return null;
    }
}