using UnityEngine;

public class CrosshairEtkilesim : MonoBehaviour
{
    public GameObject seansSistemi;
    public GameObject diyalogPaneli;
    private Camera kamera;

    void Start()
    {
        kamera = Camera.main;
        seansSistemi.SetActive(false);
        diyalogPaneli.SetActive(false);
    }

    void Update()
    {
        // Ekranın ortasından bir ray gönder
        Vector3 ekranOrtasi = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Ray ray = kamera.ScreenPointToRay(ekranOrtasi);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            if (hit.collider.CompareTag("SeansObjesi"))
            {
                if (Input.GetMouseButtonDown(0)) // Sol tık
                {
                    seansSistemi.SetActive(true);
                    diyalogPaneli.SetActive(true);
                    hit.collider.gameObject.SetActive(false);
                }
            }
        }
    }
}