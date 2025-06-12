using System;
using System.Collections.Generic;

[Serializable]
public class DiyalogData
{
    public List<DiyalogAdimi> adimlar;
    public List<AnalizSonucu> analizSonuclari;
}

[Serializable]
public class DiyalogAdimi
{
    public string id;
    public string anlatim;
    public List<Secenek> secenekler;
    public bool seansSonu;
}

[Serializable]
public class Secenek
{
    public string metin;
    public string sonrakiID;
    public string etiket;
    public int puan;
}

[Serializable]
public class AnalizSonucu
{
    public string baslik;
    public string aciklama;
    public string detay;
    public string etiket;
    public string puan;
    public string ozet;

    public List<string> gerekenEtiketler;
    public int minPuan;
}