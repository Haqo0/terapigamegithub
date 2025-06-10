using UnityEngine;

public class CrosshairEtkilesim : MonoBehaviour
{
    [Header("Seans Sistemleri")]
    [Tooltip("Karakterlerin seans sistemlerini karakterAdi ile eşleştirin")]
    public SeansSistemiEslesmesi[] seansEslesmeler;

    [Header("UI Elemanları")]
    public GameObject diyalogPaneli;
    public GameObject crosshairObjesi;

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
        [Tooltip("Karakter adı (küçük harf): mert, ece")]
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

                    // KarakterYonetici'den karakteri başlat
                    if (karakterYonetici != null)
                    {
                        karakterYonetici.KarakterSeansiBaslat(karakterAdi);
                        seansBasladi = true;
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                        crosshairObjesi.SetActive(false);

                        if (kameraKontrolScripti != null)
                            kameraKontrolScripti.enabled = false;

                        Collider objCollider = tiklananObj.GetComponent<Collider>();
                        if (objCollider != null)
                        {
                            objCollider.enabled = false;
                            Debug.Log($"{tiklananObj.name} objesi tıklanamaz yapıldı");
                        }
                        tiklananObj = hit.collider.gameObject;
                        seansBasladi = true;
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

    // Geriye uyumluluk için eski metodlar
    public void CrosshairVeKontrolGeriGetir()
    {
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

        // Farklı isimlendirme formatlarını destekle
        if (karakterAdi.Contains("mert"))
            return "mert";
        else if (karakterAdi.Contains("ece"))
            return "ece";

        Debug.LogWarning($"Bilinmeyen karakter objesi: {objeName}");
        return null;
    }
}