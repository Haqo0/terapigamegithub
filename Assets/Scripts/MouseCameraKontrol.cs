using UnityEngine;

public class MouseCameraKontrol : MonoBehaviour
{
    [Header("Mouse Ayarları")]
    public float mouseSensitivity = 100f;
    
    [Header("Dönme Sınırları")]
    public float minXRotation = -60f; 
    public float maxXRotation = 60f; 
    
    public float minYRotation = -90f; 
    public float maxYRotation = 90f;  
    
    [Header("Referanslar")]
    public Transform oyuncuGovdesi;

    private float xRotation = 0f;
    private float yRotation = 0f;
    private Vector3 baslangicRotasyonu;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        
        baslangicRotasyonu = transform.localEulerAngles;
        xRotation = baslangicRotasyonu.x;
        
        if (oyuncuGovdesi != null)
        {
            yRotation = oyuncuGovdesi.eulerAngles.y;
        }
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minXRotation, maxXRotation);
        yRotation += mouseX;
        yRotation = Mathf.Clamp(yRotation, minYRotation, maxYRotation);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        
        if (oyuncuGovdesi != null)
        {
            oyuncuGovdesi.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }
    }

    public void KamerayiSifirla()
    {
        xRotation = baslangicRotasyonu.x;
        yRotation = 0f;
        
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        if (oyuncuGovdesi != null)
        {
            oyuncuGovdesi.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }
    }
    public void RotasyonSinirlariniAyarla(float yeniMinX, float yeniMaxX, float yeniMinY, float yeniMaxY)
    {
        minXRotation = yeniMinX;
        maxXRotation = yeniMaxX;
        minYRotation = yeniMinY;
        maxYRotation = yeniMaxY;
        
        xRotation = Mathf.Clamp(xRotation, minXRotation, maxXRotation);
        yRotation = Mathf.Clamp(yRotation, minYRotation, maxYRotation);
    }
}