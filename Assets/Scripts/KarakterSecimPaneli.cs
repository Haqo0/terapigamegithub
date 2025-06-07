using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class KarakterSecimPaneli : MonoBehaviour
{
    [System.Serializable]
    public class KarakterGirdi
    {
        public string karakterAdi;
        public string gorunenIsim;
    }

    [Header("Veriler")]
    public List<KarakterGirdi> karakterler;

    [Header("UI Bileşenleri")]
    public GameObject butonPrefab;
    public Transform butonParent;
    public ProfilGosterici profilGosterici;

    void Start()
    {
        ButonlariOlustur();
    }

    void ButonlariOlustur()
    {
        foreach (KarakterGirdi karakter in karakterler)
        {
            GameObject yeniButon = Instantiate(butonPrefab, butonParent);
            TMP_Text buttonText = yeniButon.GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
                buttonText.text = karakter.gorunenIsim;

            // 🔧 Yakalama hatasını engelle
            string dosyaAdi = karakter.karakterAdi;
            yeniButon.GetComponent<Button>().onClick.AddListener(() =>
            {
                profilGosterici.karakterDosyaAdi = dosyaAdi;
                profilGosterici.ProfilPanelAc();
            });
        }
    }
}