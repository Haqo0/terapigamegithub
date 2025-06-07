using UnityEngine;

public class AnalizEtkilesim : MonoBehaviour
{
    public GameObject analizPaneli;
    public GameObject crosshair;

    private Camera kamera;
    private MonoBehaviour kameraKontrolScripti;

    void Start()
    {
        kamera = Camera.main;

        if (kamera != null)
            kameraKontrolScripti = kamera.GetComponent<MouseCameraKontrol>();

        if (analizPaneli != null)
            analizPaneli.SetActive(false);
    }

    void Update()
    {
        Ray ray = kamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            if (hit.collider.gameObject == gameObject)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (analizPaneli != null)
                        analizPaneli.SetActive(true);

                    if (crosshair != null)
                        crosshair.SetActive(false);

                    if (kameraKontrolScripti != null)
                        kameraKontrolScripti.enabled = false;

                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;

                    if (AnalizGosterici.instance != null)
                        AnalizGosterici.instance.GecmisAnalizleriGoster();
                }
            }
        }
    }

    // UI butonuna atamak için paneli kapatma fonksiyonu
    public void PaneliKapat()
    {
        if (analizPaneli != null)
            analizPaneli.SetActive(false);

        if (crosshair != null)
            crosshair.SetActive(true);

        if (kameraKontrolScripti != null)
            kameraKontrolScripti.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}