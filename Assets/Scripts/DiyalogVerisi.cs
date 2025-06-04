using System;
using System.Collections.Generic;

[Serializable]
public class DiyalogAdimi
{
    public string id;
    public string konusmaci;
    public string anlatim;
    public List<Secenek> secenekler;
}

[Serializable]
public class Secenek
{
    public string metin;
    public string sonrakiID;
}

[Serializable]
public class DiyalogData
{
    public List<DiyalogAdimi> adimlar;
}