using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 건설 시스템 설정값. GameManager 인스펙터에서 조절
/// </summary>
[Serializable]
public class BuildConfig
{
    [Header("셀 크기")]
    public float CellWidth = 1f;
    public float CellHeight = 1f;

    [Header("그리드 범위")]
    public int MinFloor = -10;
    public int MaxFloor = 1;
    public int MinColumn = 0;
    public int MaxColumn = 19;

    [Header("초기 해금 최저 층")]
    public int InitialMinFloor = -3;

    [Header("건설 가능한 방 목록")]
    public List<string> BuildableRoomIds = new List<string>();
}