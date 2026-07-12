using JetBrains.Annotations;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[Serializable]
public class PlayerModel
{
    public string PlayerName;
    public int Day;
    public int Gold;
    public int MemoryFragment;
    public int TodayMemoryFragment; //오늘 하루 동안 전투로 획득한 기억의파편 누적량, 마감 정산 시 리셋
    public bool IsBattleDoneToday;

    public BuildGridData BuildGridData = new BuildGridData();
    public List<ItemModel> Inventory = new List<ItemModel>();
    public List<ShopStockData> ShopStocks = new List<ShopStockData>();
    public List<HeroProgressModel> HeroProgressList = new List<HeroProgressModel>();
    public List<HeroStat> HeroStats = new List<HeroStat>();
    public List<EpisodeProgressModel> EpisodeProgressList = new List<EpisodeProgressModel>();
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

[Serializable]
public class HeroStat
{
    public string HeroID;
    public int Affection;
    public int Satisfaction;
}
