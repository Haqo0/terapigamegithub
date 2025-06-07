using UnityEngine;

public class CrosshairEtkilesim : MonoBehaviour
{
    public GameObject seansSistemi;
    public GameObject diyalogPaneli;
    public GameObject crosshairObjesi;
    public GameObject seansObjesi;
    private MonoBehaviour kameraKontrolScripti;
    private Camera kamera;
    private bool seansBasladi = false;

    public ProfilGosterici profilGosterici; // 👈 karakter profili

    void Start()
    {
        kamera = Camera.main;
        seansSistemi.SetActive(false);
        diyalogPaneli.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        crosshairObjesi.SetActive(true);

        kameraKontrolScripti = Camera.main.GetComponent<MouseCameraKontrol>();
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
            else if (hit.collider.CompareTag("ProfilObjesi")) // 👈 karakter profiline tıklama
            {
                if (Input.GetMouseButtonDown(0))
                {
                    profilGosterici.ProfilPanelAc(); // ✅ doğru metot adı
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

        if (seansObjesi != null)
            seansObjesi.SetActive(true);
    }
}