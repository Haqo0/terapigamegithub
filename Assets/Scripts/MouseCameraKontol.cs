using UnityEngine;

public class MouseKameraKontrol : MonoBehaviour
{
    public float mouseHassasiyet = 100f;
    public Transform oyuncuGovdesi;

    float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // İmleci gizle ve ortala
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseHassasiyet * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseHassasiyet * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Aşağı-yukarı sınırlama

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        oyuncuGovdesi.Rotate(Vector3.up * mouseX); // Sağa-sola dönüş
    }
}