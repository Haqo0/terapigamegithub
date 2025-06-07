using UnityEngine;

public class SeansObjesi : MonoBehaviour
{
    [Header("Bu Objenin Seans Verileri")]
    [Tooltip("Bu objeye tıklandığında aktif olacak seans sistemi")]
    public GameObject seansSistemiObjesi;
    
    [Tooltip("Bu objenin seans JSON dosyaları")]
    public TextAsset[] seansJsonDosyalari;
    
    [Header("Opsiyonel - Karakter Bilgileri")]
    [Tooltip("Debug için karakter adı (opsiyonel)")]
    public string karakterAdi = "";

    public SeansVerileri GetSeansVerileri()
    {
        return new SeansVerileri
        {
            seansSistemi = seansSistemiObjesi,
            jsonDosyalari = seansJsonDosyalari,
            karakterAdi = karakterAdi
        };
    }

    [ContextMenu("Debug - Seans Verilerini Göster")]
    private void DebugSeansVerileri()
    {
        Debug.Log($"=== SEANS OBJESİ: {gameObject.name} ===");
        Debug.Log($"Karakter: {(string.IsNullOrEmpty(karakterAdi) ? "Belirtilmemiş" : karakterAdi)}");
        Debug.Log($"Seans Sistemi: {(seansSistemiObjesi != null ? seansSistemiObjesi.name : "NULL")}");
        Debug.Log($"JSON Sayısı: {(seansJsonDosyalari != null ? seansJsonDosyalari.Length : 0)}");
        
        if (seansJsonDosyalari != null && seansJsonDosyalari.Length > 0)
        {
            Debug.Log("JSON Dosyaları:");
            for (int i = 0; i < seansJsonDosyalari.Length; i++)
            {
                Debug.Log($"  {i + 1}. {(seansJsonDosyalari[i] != null ? seansJsonDosyalari[i].name : "NULL")}");
            }
        }
    }
}

[System.Serializable]
public class SeansVerileri
{
    public GameObject seansSistemi;
    public TextAsset[] jsonDosyalari;
    public string karakterAdi;
}