using UnityEngine;

public class CrosshairEtkilesim : MonoBehaviour
{
    public GameObject[] seansSistemleri;        // 🔄 Tüm karakterlerin SeansSistemi objeleri (Mert, Ece vb.)
    public GameObject diyalogPaneli;
    public GameObject crosshairObjesi;

    private MonoBehaviour kameraKontrolScripti;
    private Camera kamera;
    private bool seansBasladi = false;

    public ProfilGosterici profilGosterici;     // 👈 karakter profili

    void Start()
    {
        kamera = Camera.main;

        // Hepsini pasif başlat
        foreach (var sistem in seansSistemleri)
        {
            if (sistem != null)
                sistem.SetActive(false);
        }

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
                    string karakterAdi = hit.collider.gameObject.name.ToLower();

                    bool sistemBulundu = false;

                    foreach (var sistem in seansSistemleri)
                    {
                        if (sistem.name.ToLower().Contains(karakterAdi.Replace("dosyasi", "")))
                        {
                            sistem.SetActive(true);
                            sistemBulundu = true;
                        }
                        else
                        {
                            sistem.SetActive(false);
                        }
                    }

                    if (!sistemBulundu)
                    {
                        Debug.LogWarning($"Tıklanan karaktere uygun seans sistemi bulunamadı: {karakterAdi}");
                        return;
                    }

                    if (diyalogPaneli != null)
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

        // Seans objesini yeniden görünür yap
        foreach (var sistem in seansSistemleri)
        {
            if (sistem != null)
                sistem.SetActive(false);
        }
    }
}