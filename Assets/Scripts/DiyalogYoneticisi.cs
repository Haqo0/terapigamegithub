using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro için gerekli
using System.IO;

public class DiyalogYoneticisi : MonoBehaviour
{
    public TMP_Text npcText;                     // UI'daki metin (TextMeshPro)
    public SecimSistemi secimSistemi;            // Seçim sistemi referansı
    public TextAsset diyalogJson;                // JSON dosyası (TextAsset olarak import edilir)

    private List<DiyalogAdimi> adimlar;          // Tüm adımlar listesi
    private DiyalogAdimi mevcutAdim;             // Şu anki adım

    void Start()
    {
        // JSON'dan verileri al
        DiyalogData data = JsonUtility.FromJson<DiyalogData>(diyalogJson.text);
        adimlar = data.adimlar;

        // İlk adımı yükle
        AdimiYukle("1");
    }

    public void AdimiYukle(string id)
    {
        mevcutAdim = adimlar.Find(adim => adim.id == id);

        if (mevcutAdim == null)
        {
            Debug.LogWarning("ID bulunamadı: " + id);
            return;
        }

        npcText.text = mevcutAdim.anlatim;

        if (mevcutAdim.secenekler != null && mevcutAdim.secenekler.Count > 0)
        {
            secimSistemi.SecenekleriGoster(mevcutAdim.secenekler, AdimiYukle);
        }
        else
        {
            secimSistemi.SecenekleriTemizle();
        }
    }
}