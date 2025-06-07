using UnityEngine;
using System.IO;

public class CrosshairEtkilesim : MonoBehaviour
{
    public GameObject seansSistemi;
    public GameObject diyalogPaneli;
    public GameObject crosshairObjesi;

    [Header("Tüm Seans Objeleri (Mert, Ece vb.)")]
    public GameObject[] seansObjeleri;

    private MonoBehaviour kameraKontrolScripti;
    private Camera kamera;
    private bool seansBasladi = false;

    public ProfilGosterici profilGosterici;

    void Start()
    {
        kamera = Camera.main;
        seansSistemi.SetActive(false);
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
                    KarakterEtiketi etiket = hit.collider.GetComponent<KarakterEtiketi>();
                    if (etiket == null)
                    {
                        Debug.LogWarning("KarakterEtiketi scripti bulunamadı!");
                        return;
                    }

                    string karakterAdi = etiket.karakterAdi;
                    string dosyaYolu = Path.Combine(Application.streamingAssetsPath, karakterAdi + "_seans1.json");

                    Debug.Log("📁 Aranan dosya tam yolu: " + dosyaYolu);

                    if (!File.Exists(dosyaYolu))
                    {
                        Debug.LogWarning("JSON dosyası bulunamadı: " + dosyaYolu);
                        return;
                    }

                    string jsonIcerik = File.ReadAllText(dosyaYolu);
                    TextAsset jsonAsset = new TextAsset(jsonIcerik);

                    DiyalogYoneticisi.instance.SonrakiSeansiBaslat(jsonAsset);
                    SeansGecisYoneticisi.instance.SetSeansListesi(etiket.digerSeanslar);

                    seansSistemi.SetActive(true);
                    diyalogPaneli.SetActive(true);
                    hit.collider.gameObject.SetActive(false);

                    seansBasladi = true;

                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    crosshairObjesi.SetActive(false);

                    if (kameraKontrolScripti != null)
                        kameraKontrolScripti.enabled = false;
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

    public void SeansiBitir()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        crosshairObjesi.SetActive(true);
        seansBasladi = false;

        if (kameraKontrolScripti != null)
            kameraKontrolScripti.enabled = true;

        foreach (GameObject obj in seansObjeleri)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }
}