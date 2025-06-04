using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SecimSistemi : MonoBehaviour
{
    public GameObject butonPrefab;             // Buton prefabı (içinde TMP_Text olan)
    public Transform butonParent;              // Butonların yerleştirileceği panel

    private List<GameObject> aktifButonlar = new List<GameObject>();

    // Seçenekleri göster ve her butona tıklanıldığında ilgili ID'yi işle
    public void SecenekleriGoster(List<Secenek> secenekler, Action<string> secimYapildiginda)
    {
        // Eski butonları temizle
        SecenekleriTemizle();

        foreach (Secenek secenek in secenekler)
        {
            GameObject eniButon = Instantiate(butonPrefab, butonParent);
            
            TMP_Text buttonText = eniButon.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = secenek.metin;
            }
            else
            {
                Debug.LogError("Buton prefab içinde TMP_Text bileşeni bulunamadı!");
            }

            // Tıklanıldığında ilgili sonraki ID'yi gönder
            eniButon.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                secimYapildiginda.Invoke(secenek.sonrakiID);
            });

            aktifButonlar.Add(eniButon);
        }
    }

    // Tüm önceki butonları sil
    public void SecenekleriTemizle()
    {
        foreach (GameObject buton in aktifButonlar)
        {
            Destroy(buton);
        }

        aktifButonlar.Clear();
    }
}