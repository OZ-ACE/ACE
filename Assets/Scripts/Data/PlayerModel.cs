using NUnit.Framework;
using System;
using System.Collections.Generic;

[Serializable]
public class PlayerModel
{
    public string PlayerName;
    public int Day;
    public int Gold;
    public int MemoryFragment;

    public BuildGridData BuildGridData = new BuildGridData();
    public List<ItemModel> Inventory = new List<ItemModel>();
    public List<ShopStockData> ShopStocks = new List<ShopStockData>();
    public List<HeroProgressModel> HeroProgressList = new List<HeroProgressModel>();
}

[Serializable]
public class ItemModel
{
    public string ItemID;
    public int ItemCount;
}


[Serializable]
public class ShopStockData
{
    public string ItemID;
    public int RemainStock;
}

[Serializable]
public class HeroProgressModel
{
    public string HeroId;
    public int BattleParticipateCount; //누적 전투 참여 횟수 (승패 무관), 프라임 레벨 산정 기준
}
