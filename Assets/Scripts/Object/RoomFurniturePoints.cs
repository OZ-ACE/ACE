using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomFurniturePoints : MonoBehaviour
{
    [Serializable]
    private class FurniturePointEntry
    {
        [SerializeField] private string _pointId;
        [SerializeField] private Transform _point;

        public string PointId
        {
            get { return _pointId; }
        }

        public Transform Point
        {
            get { return _point; }
        }
    }

    [SerializeField]
    private List<FurniturePointEntry> _points = new List<FurniturePointEntry>();

    public bool TryGetPoint(string pointId, out Transform point)
    {
        point = null;

        if (string.IsNullOrEmpty(pointId) == true)
        {
            Debug.LogWarning("RoomFurniturePoints - 설치 포인트 ID가 비어있습니다.");
            return false;
        }

        for (int i = 0; i < _points.Count; i++)
        {
            FurniturePointEntry entry = _points[i];

            if (entry == null)
            {
                continue;
            }

            if (entry.PointId != pointId)
            {
                continue;
            }

            if (entry.Point == null)
            {
                Debug.LogWarning($"RoomFurniturePoints - Transform 이 연결되지 않은 포인트 : {pointId}");
                return false;
            }

            point = entry.Point;
            return true;
        }

        Debug.LogWarning($"RoomFurniturePoints - 설치 포인트를 찾을 수 없습니다: {pointId}");

        return false;
    }

    public Transform GetPoint(string pointId)
    {
        if (TryGetPoint(pointId, out Transform point))
        {
            return point;
        }

        return null;
    }
}