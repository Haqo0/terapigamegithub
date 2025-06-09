using UnityEngine;

public class CrosshairEtkilesim : MonoBehaviour
{
    public static CrosshairEtkilesim instance;

    [Header("UI Elemanları")]
    public GameObject diyalogPaneli;
    public GameObject crosshairObjesi;

    private MonoBehaviour kameraKontrolScripti;
    private Camera kamera;
    private bool seansBasladi = false;

    public ProfilGosterici profilGosterici;

    private GameObject sonTiklananObj;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        kamera = Camera.main;

        if (diyalogPaneli != null)
            diyalogPaneli.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        crosshairObjesi.SetActive(true);

        kameraKontrolScripti = kamera.GetComponent<MouseCameraKontrol>();
        if (kameraKontrolScripti != null)
            kameraKontrolScripti.enabled = true;
        else
            Debug.LogWarning("Kamera kontrol scripti bulunamadı!");
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
                    SeansObjesi seansObjesi = hit.collider.GetComponent<SeansObjesi>();

                    if (seansObjesi != null)
                    {
                        SeansVerileri seansVerileri = seansObjesi.GetSeansVerileri();
                        SeansBaslat(seansVerileri, hit.collider.gameObject);
                    }
                    else
                    {
                        Debug.LogError($"'{hit.collider.gameObject.name}' objesinde SeansObjesi component'i bulunamadı!");
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

    private void SeansBaslat(SeansVerileri seansVerileri, GameObject tiklananObj)
    {
        if (seansVerileri.seansSistemi == null)
        {
            Debug.LogError($"'{tiklananObj.name}' objesi için seans sistemi atanmamış!");
            return;
        }

        if (seansVerileri.jsonDosyalari == null || seansVerileri.jsonDosyalari.Length == 0)
        {
            Debug.LogError($"'{tiklananObj.name}' objesi için JSON dosyaları atanmamış!");
            return;
        }

        // ❌ Direkt olarak diyalog başlatmıyoruz
        // ✅ Cutscene üzerinden başlatılacak
        KarakterYonetici.instance.KarakterSeansiBaslat(seansVerileri.karakterAdi);

        // UI ve kontrol kısıtlamaları
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

        sonTiklananObj = tiklananObj;
        seansBasladi = true;

        Debug.Log($"Seans başlatıldı (cutscene ile): {seansVerileri.karakterAdi} ({tiklananObj.name})");
    }

    public void SeansiBitir()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        crosshairObjesi.SetActive(true);
        seansBasladi = false;

        if (kameraKontrolScripti != null)
            kameraKontrolScripti.enabled = true;

        if (sonTiklananObj != null)
        {
            Collider objCollider = sonTiklananObj.GetComponent<Collider>();
            if (objCollider != null)
                objCollider.enabled = true;

            Debug.Log($"{sonTiklananObj.name} objesi yeniden tıklanabilir hale getirildi.");
            sonTiklananObj = null;
        }

        Debug.Log("Seans bitirildi - hiçbir karakter objesi kapatılmadı.");
    }

    public void CrosshairVeKontrolGeriGetir()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (crosshairObjesi != null)
            crosshairObjesi.SetActive(true);

        if (kameraKontrolScripti != null)
            kameraKontrolScripti.enabled = true;

        seansBasladi = false;

        Debug.Log("Crosshair ve kamera kontrolü geri getirildi.");
    }

    [ContextMenu("Debug - Tüm Seans Objelerini Listele")]
    private void DebugTumSeansObjeleri()
    {
        Debug.Log("=== TÜM SEANS OBJELERİ ===");
        SeansObjesi[] seansObjeleri = FindObjectsOfType<SeansObjesi>();

        if (seansObjeleri.Length == 0)
        {
            Debug.LogWarning("Hiç SeansObjesi component'i bulunamadı!");
            return;
        }

        for (int i = 0; i < seansObjeleri.Length; i++)
        {
            SeansObjesi obj = seansObjeleri[i];
            Debug.Log($"{i + 1}. {obj.gameObject.name}");
            Debug.Log($"   Karakter: {obj.karakterAdi}");
            Debug.Log($"   Seans Sistemi: {(obj.seansSistemiObjesi != null ? obj.seansSistemiObjesi.name : "NULL")}");
            Debug.Log($"   JSON Sayısı: {(obj.seansJsonDosyalari != null ? obj.seansJsonDosyalari.Length : 0)}");

            if (obj.seansSistemiObjesi != null)
            {
                SeansGecisYoneticisi gecisYoneticisi = obj.seansSistemiObjesi.GetComponent<SeansGecisYoneticisi>();
                if (gecisYoneticisi == null)
                    gecisYoneticisi = obj.seansSistemiObjesi.GetComponentInChildren<SeansGecisYoneticisi>();

                Debug.Log($"   SeansGecisYoneticisi: {(gecisYoneticisi != null ? "BULUNDU" : "BULUNAMADI")}");
            }
        }
    }
}