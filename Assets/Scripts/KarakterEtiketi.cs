using UnityEngine;

public class KarakterEtiketi : MonoBehaviour
{
    [Header("Karakter Adı (StreamingAssets içinde .json dosya ismi)")]
    public string karakterAdi = "ece";

    [Header("2-5 Arası Seanslar (TextAsset olarak inspector'a ekle)")]
    public TextAsset[] digerSeanslar;
}