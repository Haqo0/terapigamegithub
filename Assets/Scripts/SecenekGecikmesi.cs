using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SecenekGecikmesi : MonoBehaviour
{
    [Header("UI Elemanları")]
    public GameObject gecikmePanel;          // Gecikme sırasında gösterilecek panel
    public Slider zamanCubugu;               // İlerleme çubuğu
    public TMP_Text zamanMetni;              // "Düşünün... 3s" gibi metin
    public TMP_Text ipucuMetni;              // "Cevabınızı düşünün" gibi ipucu

    [Header("Ayarlar")]
    public string[] ipucuMesajlari = {
        "Cevabınızı düşünün...",
        "Hangi yaklaşım daha uygun?",
        "Danışanın duygularını göz önünde bulundurun...",
        "En iyi tepki ne olabilir?"
    };

    private Coroutine aktifGosterim;

    public void GecikmeGosteriminiBaslat(float gecikmeZamani)
    {
        if (aktifGosterim != null)
        {
            StopCoroutine(aktifGosterim);
        }

        aktifGosterim = StartCoroutine(GecikmeGosterimi(gecikmeZamani));
    }

    public void GecikmeGosteriminiDurdur()
    {
        if (aktifGosterim != null)
        {
            StopCoroutine(aktifGosterim);
            aktifGosterim = null;
        }

        gecikmePanel.SetActive(false);
    }

    private IEnumerator GecikmeGosterimi(float toplamZaman)
    {
        // Panel ve elemanları aktif et
        gecikmePanel.SetActive(true);
        
        // Rastgele ipucu seç
        if (ipucuMesajlari.Length > 0)
        {
            string rastgeleIpucu = ipucuMesajlari[Random.Range(0, ipucuMesajlari.Length)];
            ipucuMetni.text = rastgeleIpucu;
        }

        // Zamanı takip et
        float gecenZaman = 0f;

        while (gecenZaman < toplamZaman)
        {
            gecenZaman += Time.deltaTime;
            
            // Progress bar güncelle
            if (zamanCubugu != null)
            {
                zamanCubugu.value = gecenZaman / toplamZaman;
            }

            // Zaman metni güncelle
            if (zamanMetni != null)
            {
                int kalanSaniye = Mathf.CeilToInt(toplamZaman - gecenZaman);
                zamanMetni.text = $"Seçenekler {kalanSaniye}s sonra gelecek...";
            }

            yield return null;
        }

        // Gösterimi kapat
        gecikmePanel.SetActive(false);
        aktifGosterim = null;
    }

    // Hızlandırma butonu için
    public void GecikmeHizlandir()
    {
        if (aktifGosterim != null)
        {
            StopCoroutine(aktifGosterim);
            aktifGosterim = null;
        }

        gecikmePanel.SetActive(false);
        
        // DiyalogYoneticisi'ne seçenekleri hemen göstermesini söyle
        DiyalogYoneticisi yonetici = FindObjectOfType<DiyalogYoneticisi>();
        if (yonetici != null)
        {
            yonetici.SeçenekleriHemenGöster();
        }
    }
}