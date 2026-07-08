using NUnit.Framework;
using System;
using System.Collections.Generic;

[Serializable]
public class PlayerModel
{
    public string PlayerName;
    public int Day;
    public int Gold;

    public BuildGridData BuildGridData = new BuildGridData();
    public List<ItemModel> Inventory = new List<ItemModel>();
}

[Serializable]
public class ItemModel
{
    public string ItemID;
    public int ItemCount;
}