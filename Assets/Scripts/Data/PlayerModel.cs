using System;

[Serializable]
public class PlayerModel
{
    public string PlayerName;
    public int Day;
    public int Gold;
    public BuildGridData BuildGridData = new BuildGridData(); // ★ 추가
}
