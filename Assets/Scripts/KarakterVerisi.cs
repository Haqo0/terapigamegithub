using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class KarakterVerisi
{
    public string karakterAdi;                  // Örn: "Ece", "Mert"
    public GameObject karakterPrefab;           // Sahnedeki karakter objesi
    public PlayableDirector girisCutscene;      // Karakterin giriş sahnesi (Timeline)
    public PlayableDirector cikisCutscene;      // Karakterin çıkış sahnesi (Timeline)
    public TextAsset[] seansJsonlar;            // 5 adet JSON dosyası (Seans1 - Seans5)
}