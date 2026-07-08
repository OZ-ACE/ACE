using System;
using UnityEngine;

[Serializable]
public class GameDataBase
{
    public string ID;
}

// 임의로 넣어두었습니다.
[Serializable]
public class Hero : GameDataBase
{
    public string Name;
    public string Description;
}