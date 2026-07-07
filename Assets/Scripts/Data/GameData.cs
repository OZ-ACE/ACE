using System;
using UnityEngine;

[Serializable]
public class GameDataBase
{
    public string ID;
}

[Serializable]
public class Dialogue : GameDataBase
{
    public string NextID;
    public string Speaker;
    public string Content;
    public string Background;
    public string BGM;
    public string SFX;
}