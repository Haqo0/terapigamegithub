using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SecimSistemi : MonoBehaviour
{
    public GameObject butonPrefab;
    public Transform butonParent;

    private List<GameObject> aktifButonlar = new List<GameObject>();

    // Gelişmiş seçenek gösterme - artık Secenek objesini direkt alıyor
    public void SecenekleriGoster(List<Secenek> secenekler, Action<Secenek> secimYapildiginda)
    {
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

            // Tıklanıldığında tüm secenek objesini gönder
            eniButon.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                secimYapildiginda.Invoke(secenek);
            });

            aktifButonlar.Add(eniButon);
        }
    }

    public void SecenekleriTemizle()
    {
        foreach (GameObject buton in aktifButonlar)
        {
            Destroy(buton);
        }

        aktifButonlar.Clear();
    }
}