using System;
using System.Collections.Generic;

[Serializable]
public class DiyalogAdimi
{
    public string id;
    public string konusmaci;
    public string anlatim;
    public List<Secenek> secenekler;
    public bool seansSonu = false; // Bu adım seans sonunu mu işaret ediyor
}

[Serializable]
public class Secenek
{
    public string metin;
    public string sonrakiID;
    public string etiket; // Seçimin psikolojik kategorisi (empati, analitik, destekleyici, vb.)
    public int puan = 1; // Bu seçimin ağırlığı
}

[Serializable]
public class AnalizSonucu
{
    public string baslik;
    public string aciklama;
    public string detayliAnaliz;
    public List<string> gerekenEtiketler; // Bu analiz için hangi etiketler gerekli
    public int minPuan = 0; // Minimum gerekli puan
}

[Serializable]
public class DiyalogData
{
    public List<DiyalogAdimi> adimlar;
    public List<AnalizSonucu> analizSonuclari;
}