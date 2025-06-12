using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class AnalizGosterici : MonoBehaviour
{
    public static AnalizGosterici instance;

    [Header("UI Panelleri")]
    public List<GameObject> paneller;

    [Header("Metin Alanları")]
    public TMP_Text analizMetni;
    
    [Header("🆕 Etiket ve Puan Gösterimi")]
    public TMP_Text etiketPuanMetni;  // Yeni eklenen alan - Etiket puanlarını gösterir
    public GameObject etiketPuanPaneli; // Opsiyonel: Ayrı panel
    
    [Header("Butonlar")]
    public GameObject yeniseansButonu;
    public GameObject kapatButonu;

    [Header("Gecikme")]
    public float analizGecikmesi = 2.0f;

    private List<string> analizKayitlari = new List<string>();
    private List<Dictionary<string, int>> gecmisPuanlar = new List<Dictionary<string, int>>(); // 🆕 Puan geçmişi
    private List<List<string>> gecmisSecimler = new List<List<string>>(); // 🆕 Seçim geçmişi

    private void Awake()
    {
        instance = this;
    }

    public void SonSeans()
    {
        if (DiyalogYoneticisi.instance != null && DiyalogYoneticisi.instance.mevcutSeansIndex >= 4)
        {
            Debug.LogWarning("DiyalogYoneticisi mevcut seans 4 veya daha yüksek - Son seans olarak işaretleniyor");
            if (kapatButonu != null) kapatButonu.SetActive(true);
            if (yeniseansButonu != null) yeniseansButonu.SetActive(false);
        }
        else
        {
            if (kapatButonu != null) kapatButonu.SetActive(true);
            if (yeniseansButonu != null) yeniseansButonu.SetActive(true);
        }
    }

    // 🎯 Seans sonunda çağrılır - GÜNCELLENMIŞ VERSİYON
    public void AnalizeGoster(AnalizSonucu analiz, Dictionary<string, int> puanlar, List<string> secimler)
    {
        foreach (GameObject panel in paneller)
        {
            panel.SetActive(false);
        }

        // Ana analiz metni
        if (analizMetni != null)
        {
            analizMetni.text = $"<b>{analiz.baslik}</b>\n\n{analiz.ozet}\n\n{analiz.detay}";
        }

        // 🆕 Etiket ve puan detaylarını göster
        GosterEtiketPuanDetaylari(puanlar, analiz, secimler);

        SonSeans();
        
        if (paneller.Count > 0)
        {
            paneller[0].SetActive(true); // örneğin AnalizPaneli
        }

        // ✅ Analiz ozetini, puanları ve seçimleri geçmişe ekle
        analizKayitlari.Add(analiz.ozet);
        gecmisPuanlar.Add(new Dictionary<string, int>(puanlar)); // 🆕 Puanları kaydet
        gecmisSecimler.Add(new List<string>(secimler)); // 🆕 Seçimleri kaydet

        // Cursor ayarları
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // 🆕 Etiket ve puan detaylarını gösteren yeni method
    private void GosterEtiketPuanDetaylari(Dictionary<string, int> puanlar, AnalizSonucu analiz, List<string> secimler)
    {
        if (etiketPuanMetni == null) return;

        string etiketMetni = "<b>📊 Seçim Analizi:</b>\n\n";

        // Seçim sayısı ve toplam puan bilgisi
        int toplamPuan = puanlar.Values.Sum();
        etiketMetni += $"<color=#4CAF50><b>Toplam Seçim: {secimler.Count}</b></color>\n";
        etiketMetni += $"<color=#4CAF50><b>Toplam Puan: {toplamPuan}</b></color>\n\n";

        // Yapılan seçimleri göster
        if (secimler.Count > 0)
        {
            etiketMetni += "<b>🎯 Yapılan Seçimler:</b>\n";
            for (int i = 0; i < secimler.Count; i++)
            {
                etiketMetni += $"<color=#2196F3>{i + 1}.) {secimler[i]}</color>\n";
            }
            etiketMetni += "\n";
        }

        // Her etiket için detay
        etiketMetni += "<b>🏷️ Etiket Dağılımı:</b>\n";
        
        if (puanlar.Count > 0)
        {
            foreach (var etiketPuan in puanlar.OrderByDescending(x => x.Value))
            {
                string etiketAdi = EtiketAdiGüzelleştir(etiketPuan.Key);
                int puan = etiketPuan.Value;
                
                // Puan seviyesine göre renk
                string renk = GetPuanRengi(puan);
                string puanCubugu = GetPuanCubugu(puan);
                
                etiketMetni += $"<color={renk}>• {etiketAdi}: {puan} puan {puanCubugu}</color>\n";
            }
        }
        else
        {
            etiketMetni += "<color=#FFC107><i>Bu seansta puana dayalı seçim yapılmadı.</i></color>\n";
        }

        // Analiz kriterlerini göster
        if (analiz.gerekenEtiketler != null && analiz.gerekenEtiketler.Count > 0)
        {
            etiketMetni += "\n<b>🎯 Analiz Kriterleri:</b>\n";
            etiketMetni += $"<color=#9C27B0>Minimum Puan: {analiz.minPuan}</color>\n";
            etiketMetni += $"<color=#9C27B0>Gereken Etiketler: {string.Join(", ", analiz.gerekenEtiketler.Select(EtiketAdiGüzelleştir))}</color>\n";
        }

        etiketPuanMetni.text = etiketMetni;

        // Eğer ayrı panel varsa onu da aktif et
        if (etiketPuanPaneli != null)
        {
            etiketPuanPaneli.SetActive(true);
        }
    }

    // 🆕 Etiket adlarını daha okunabilir hale getir
    private string EtiketAdiGüzelleştir(string etiket)
    {
        return etiket.Replace("_", " ")
                    .Replace("yumusak", "Yumuşak")
                    .Replace("hissetme", "Hissetme")
                    .Replace("icten", "İçten")
                    .Replace("gulumseme", "Gülümseme")
                    .Replace("sessizlik", "Sessizlik")
                    .Replace("kabul", "Kabul")
                    .Replace("duygu", "Duygu")
                    .Replace("kontrol", "Kontrol")
                    .Replace("empati", "Empati")
                    .Replace("gecmis", "Geçmiş")
                    .Replace("analiz", "Analiz")
                    .Replace("gozlem", "Gözlem")
                    .Replace("ifade", "İfade")
                    .Replace("bedensel", "Bedensel")
                    .Replace("bozulma", "Bozulma")
                    .Replace("rutin", "Rutin")
                    .Replace("sistem", "Sistem")
                    .Replace("tasma", "Taşma")
                    .Replace("korkusu", "Korkusu")
                    .Replace("belirsizlik", "Belirsizlik")
                    .Replace("zihin", "Zihin");
    }

    // 🆕 Puan seviyesine göre renk döndür
    private string GetPuanRengi(int puan)
    {
        if (puan >= 4) return "#4CAF50";      // Yeşil - Çok Yüksek
        else if (puan >= 3) return "#8BC34A"; // Açık Yeşil - Yüksek
        else if (puan >= 2) return "#FF9800"; // Turuncu - Orta
        else if (puan >= 1) return "#2196F3"; // Mavi - Düşük
        else return "#9E9E9E";                // Gri - Sıfır
    }

    // 🆕 Puan seviyesine göre görsel çubuk
    private string GetPuanCubugu(int puan)
    {
        if (puan >= 4) return "████";
        else if (puan >= 3) return "███○";
        else if (puan >= 2) return "██○○";
        else if (puan >= 1) return "█○○○";
        else return "○○○○";
    }

    // DevamEt butonu - Normal analiz akışını sürdürür
    public void DevamEt()
    {
        foreach (GameObject panel in paneller)
        {
            panel.SetActive(false);
        }

        if (etiketPuanPaneli != null)
        {
            etiketPuanPaneli.SetActive(false);
        }

        // Normal akışı sürdür - SeansiGecir metodunu çağır
        if (KarakterYonetici.instance != null)
        {
            KarakterVerisi aktifKarakter = KarakterYonetici.instance.GetAktifKarakter();
            if (aktifKarakter != null)
            {
                DiyalogYoneticisi diyalogYoneticisi = aktifKarakter.karakterPrefab.GetComponentInChildren<DiyalogYoneticisi>();
                if (diyalogYoneticisi != null)
                {
                    Debug.Log("Normal akış sürdürülüyor - Seans geçiş paneli gösterilecek");
                    diyalogYoneticisi.SeansiGecir(); // Normal geçiş (seans geçiş paneli ile)
                }
                else
                {
                    Debug.LogError("DiyalogYoneticisi bulunamadı!");
                }
            }
        }

        // Cursor ayarlarını normale döndür
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // 🎯 Not defterine tıklanınca çağrılır - GÜNCELLENMIŞ VERSİYON
    public void GecmisAnalizleriGoster()
    {
        foreach (GameObject panel in paneller)
        {
            panel.SetActive(false);
        }

        if (paneller.Count > 0)
        {
            paneller[0].SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Ana analiz metni
        if (analizMetni != null)
        {
            analizMetni.text = "<b>📋 Geçmiş Seans Analizleri</b>\n\n";

            if (analizKayitlari.Count == 0)
            {
                analizMetni.text += "Henüz analiz kaydı bulunmuyor.";
            }
            else
            {
                for (int i = 0; i < analizKayitlari.Count; i++)
                {
                    analizMetni.text += $"<b>Seans {i + 1}:</b>\n";
                    analizMetni.text += "• " + analizKayitlari[i] + "\n\n";
                }
            }
        }

        // 🆕 Geçmiş puanları ve seçimleri göster
        if (etiketPuanMetni != null)
        {
            string gecmisMetin = "<b>📊 Geçmiş Puan ve Seçim Dağılımları:</b>\n\n";
            
            if (gecmisPuanlar.Count == 0)
            {
                gecmisMetin += "Henüz puan kaydı bulunmuyor.";
            }
            else
            {
                for (int i = 0; i < gecmisPuanlar.Count; i++)
                {
                    gecmisMetin += $"<b><color=#2196F3>Seans {i + 1}:</color></b>\n";
                    
                    // Seçimleri göster
                    if (i < gecmisSecimler.Count && gecmisSecimler[i].Count > 0)
                    {
                        gecmisMetin += $"<color=#FF9800>Seçim Sayısı: {gecmisSecimler[i].Count}</color>\n";
                    }
                    
                    // Puanları göster
                    var puanlar = gecmisPuanlar[i];
                    if (puanlar.Count > 0)
                    {
                        int toplamPuan = puanlar.Values.Sum();
                        gecmisMetin += $"<color=#4CAF50>Toplam Puan: {toplamPuan}</color>\n";
                        
                        foreach (var etiketPuan in puanlar.OrderByDescending(x => x.Value))
                        {
                            string etiketAdi = EtiketAdiGüzelleştir(etiketPuan.Key);
                            string renk = GetPuanRengi(etiketPuan.Value);
                            gecmisMetin += $"<color={renk}>  • {etiketAdi}: {etiketPuan.Value}</color>\n";
                        }
                    }
                    else
                    {
                        gecmisMetin += "  <i>Puan kaydı yok</i>\n";
                    }
                    gecmisMetin += "\n";
                }
            }
            
            etiketPuanMetni.text = gecmisMetin;
            
            if (etiketPuanPaneli != null)
            {
                etiketPuanPaneli.SetActive(true);
            }
        }

        if (kapatButonu != null) kapatButonu.SetActive(true);
        if (yeniseansButonu != null) yeniseansButonu.SetActive(false);
    }

    public void AnalizPaneliniKapat()
    {
        foreach (GameObject panel in paneller)
        {
            panel.SetActive(false);
        }

        if (etiketPuanPaneli != null)
        {
            etiketPuanPaneli.SetActive(false);
        }

        // Normal akışı sürdür - SeansiGecir metodunu çağır
        if (KarakterYonetici.instance != null)
        {
            KarakterVerisi aktifKarakter = KarakterYonetici.instance.GetAktifKarakter();
            if (aktifKarakter != null)
            {
                DiyalogYoneticisi diyalogYoneticisi = aktifKarakter.karakterPrefab.GetComponentInChildren<DiyalogYoneticisi>();
                if (diyalogYoneticisi != null)
                {
                    Debug.Log("Normal akış sürdürülüyor - Seans geçiş paneli gösterilecek");
                    diyalogYoneticisi.SeansiGecir(); // Normal geçiş (seans geçiş paneli ile)
                }
                else
                {
                    Debug.LogError("DiyalogYoneticisi bulunamadı!");
                }
            }
        }

        // Cursor ayarlarını normale döndür
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        KarakterYonetici.instance.SeansSonuCutsceneBaslat();
    }

    public void YeniSeansaBasla()
    {
        foreach (GameObject panel in paneller)
        {
            panel.SetActive(false);
        }

        if (etiketPuanPaneli != null)
        {
            etiketPuanPaneli.SetActive(false);
        }

        // Karakteri sıfırla ve idle moda dön
        if (KarakterYonetici.instance != null)
        {
            KarakterYonetici.instance.IdleModaDon();
        }

        Debug.Log("Yeni seans için idle moda döndü");
    }

    // Analiz kayıtlarını temizleme (debug için)
    [ContextMenu("Analiz Kayıtlarını Temizle")]
    public void AnalizKayitlariniTemizle()
    {
        analizKayitlari.Clear();
        gecmisPuanlar.Clear(); // 🆕 Puan kayıtlarını da temizle
        gecmisSecimler.Clear(); // 🆕 Seçim kayıtlarını da temizle
        Debug.Log("Tüm analiz kayıtları temizlendi");
    }

    [ContextMenu("Kayıtlı Analiz Sayısını Göster")]
    public void KayitliAnalizSayisiniGoster()
    {
        Debug.Log($"Toplam kayıtlı analiz sayısı: {analizKayitlari.Count}");
        Debug.Log($"Toplam kayıtlı puan sayısı: {gecmisPuanlar.Count}");
        Debug.Log($"Toplam kayıtlı seçim sayısı: {gecmisSecimler.Count}");
    }

    // 🆕 Debug için - Inspector'da çağırılabilir
    [ContextMenu("Test Etiket Gösterimi")]
    public void TestEtiketGosterimi()
    {
        var testPuanlar = new Dictionary<string, int>
        {
            {"yumusak_hissetme", 3},
            {"icten_gulumseme", 2},
            {"sessizlik_kabul", 1},
            {"duygu", 2},
            {"empati", 4}
        };

        var testSecimler = new List<string>
        {
            "Bu 'yumuşak' kelimesi sende neyi çağrıştırdı?",
            "İlk defa kendine mi içten gülümsedin?",
            "Sessiz kal"
        };

        var testAnaliz = new AnalizSonucu
        {
            baslik = "Test Analizi",
            ozet = "Bu bir test analizidir - Ece yumuşak hissetmeye başladı",
            detay = "Detaylı açıklama: Hastanın duygu dünyasında olumlu gelişmeler gözlemlendi.",
            gerekenEtiketler = new List<string> {"yumusak_hissetme", "icten_gulumseme"},
            minPuan = 3
        };

        AnalizeGoster(testAnaliz, testPuanlar, testSecimler);
    }
}