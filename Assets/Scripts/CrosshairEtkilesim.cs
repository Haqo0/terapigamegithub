using UnityEngine;

public class CrosshairEtkilesim : MonoBehaviour
{
    [Header("UI Elemanları")]
    public GameObject diyalogPaneli;
    public GameObject crosshairObjesi;

    private MonoBehaviour kameraKontrolScripti;
    private Camera kamera;
    private bool seansBasladi = false;

    public ProfilGosterici profilGosterici;

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
                    // Tıklanan objeden doğrudan seans verilerini al
                    SeansObjesi seansObjesi = hit.collider.GetComponent<SeansObjesi>();
                    
                    if (seansObjesi != null)
                    {
                        SeansVerileri seansVerileri = seansObjesi.GetSeansVerileri();
                        SeansBaslat(seansVerileri, hit.collider.gameObject);
                    }
                    else
                    {
                        Debug.LogError($"'{hit.collider.gameObject.name}' objesinde SeansObjesi component'i bulunamadı!");
                        Debug.LogError("SeansObjesi tag'ına sahip objelerde SeansObjesi script'i olmalı.");
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
        // Validasyon kontrolleri
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

        // Önce tüm seans sistemlerini bul ve kapat
        SeansObjesi[] tumSeansObjeleri = FindObjectsOfType<SeansObjesi>();
        foreach (SeansObjesi obj in tumSeansObjeleri)
        {
            if (obj.seansSistemiObjesi != null)
                obj.seansSistemiObjesi.SetActive(false);
        }

        // Seçilen seans sistemini aktif et
        Debug.Log($"Seans sistemi aktif ediliyor: {seansVerileri.seansSistemi.name}");
        seansVerileri.seansSistemi.SetActive(true);
        
        // Kısa bekleme sonrası DiyalogYoneticisi kontrolü
        StartCoroutine(DiyalogYoneticisiKontrolEt(seansVerileri));
        
        // JSON dosyalarını SeansGecisYoneticisi'ne ata - önce kendisinde, sonra alt objelerde ara
        SeansGecisYoneticisi gecisYoneticisi = seansVerileri.seansSistemi.GetComponent<SeansGecisYoneticisi>();
        
        // Eğer kendisinde yoksa alt objelerde ara
        if (gecisYoneticisi == null)
        {
            gecisYoneticisi = seansVerileri.seansSistemi.GetComponentInChildren<SeansGecisYoneticisi>();
        }
        
        if (gecisYoneticisi != null)
        {
            gecisYoneticisi.JsonDosyalariniAyarla(seansVerileri.jsonDosyalari);
            Debug.Log($"{seansVerileri.karakterAdi} için {seansVerileri.jsonDosyalari.Length} JSON dosyası yüklendi");
        }
        else
        {
            Debug.LogWarning($"SeansGecisYoneticisi bulunamadı: {seansVerileri.seansSistemi.name}");
            Debug.LogWarning("Bu karakter için çoklu seans geçişi çalışmayacak. Sadece ilk seans oynanabilir.");
            Debug.LogWarning("Çoklu seans için SeansGecisYoneticisi component'ini ekleyin.");
        }

        // UI ayarları
        if (diyalogPaneli != null)
            diyalogPaneli.SetActive(true);

        // Tıklanan objeyi kapatma - bu soruna neden oluyor
        // tiklananObj.SetActive(false);
        
        // Bunun yerine objeyi sadece tıklanamaz yap
        Collider objCollider = tiklananObj.GetComponent<Collider>();
        if (objCollider != null)
        {
            objCollider.enabled = false;
            Debug.Log($"{tiklananObj.name} objesi tıklanamaz yapıldı");
        }

        seansBasladi = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        crosshairObjesi.SetActive(false);

        if (kameraKontrolScripti != null)
            kameraKontrolScripti.enabled = false;

        Debug.Log($"Seans başlatıldı: {seansVerileri.karakterAdi} ({tiklananObj.name})");
    }

    private System.Collections.IEnumerator DiyalogYoneticisiKontrolEt(SeansVerileri seansVerileri)
    {
        yield return new WaitForSeconds(0.1f); // Kısa bekleme
        
        GameObject seansSistemi = seansVerileri.seansSistemi;
        
        // DiyalogYoneticisi'ni bul
        DiyalogYoneticisi diyalogYoneticisi = seansSistemi.GetComponent<DiyalogYoneticisi>();
        if (diyalogYoneticisi == null)
        {
            diyalogYoneticisi = seansSistemi.GetComponentInChildren<DiyalogYoneticisi>();
        }
        
        if (diyalogYoneticisi != null)
        {
            Debug.Log($"DiyalogYoneticisi bulundu: {diyalogYoneticisi.gameObject.name}");
            Debug.Log($"   - Enabled: {diyalogYoneticisi.enabled}");
            Debug.Log($"   - GameObject Active: {diyalogYoneticisi.gameObject.activeInHierarchy}");
            
            // İlk JSON dosyasını DiyalogYoneticisi'ne ata
            if (seansVerileri.jsonDosyalari != null && seansVerileri.jsonDosyalari.Length > 0)
            {
                diyalogYoneticisi.diyalogJson = seansVerileri.jsonDosyalari[0];
                Debug.Log($"İlk JSON DiyalogYoneticisi'ne atandı: {seansVerileri.jsonDosyalari[0].name}");
                
                // Manuel seans başlatma
                Debug.Log("Manuel seans başlatılıyor...");
                diyalogYoneticisi.SeansiYenidenBaslat();
            }
            else
            {
                Debug.LogError("JSON dosyaları boş! SeansObjesi'nde JSON'lar atanmamış.");
            }
        }
        else
        {
            Debug.LogError($"DiyalogYoneticisi bulunamadı: {seansSistemi.name}");
            Debug.LogError("Seans sistemi objesinde DiyalogYoneticisi component'i olmalı!");
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

        // Tüm seans sistemlerini kapat
        SeansObjesi[] tumSeansObjeleri = FindObjectsOfType<SeansObjesi>();
        foreach (SeansObjesi obj in tumSeansObjeleri)
        {
            if (obj.seansSistemiObjesi != null)
                obj.seansSistemiObjesi.SetActive(false);
                
            // Objeleri tekrar tıklanabilir yap
            Collider objCollider = obj.GetComponent<Collider>();
            if (objCollider != null)
            {
                objCollider.enabled = true;
            }
        }

        Debug.Log("Seans bitirildi - Ana menüye dönüldü");
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
            
            // SeansGecisYoneticisi kontrolü
            if (obj.seansSistemiObjesi != null)
            {
                SeansGecisYoneticisi gecisYoneticisi = obj.seansSistemiObjesi.GetComponent<SeansGecisYoneticisi>();
                if (gecisYoneticisi == null)
                {
                    gecisYoneticisi = obj.seansSistemiObjesi.GetComponentInChildren<SeansGecisYoneticisi>();
                }
                
                Debug.Log($"   SeansGecisYoneticisi: {(gecisYoneticisi != null ? "BULUNDU" : "BULUNAMADI")}");
            }
        }
    }
}