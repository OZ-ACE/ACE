using TMPro;
using UnityEngine;

public class RoomView : MonoBehaviour
{
    [SerializeField] private TextMeshPro Text_Capacity;

    private PlacedRoomData _roomData;

    public void Bind(PlacedRoomData data)
    {
        if (_roomData != null)
        {
            _roomData.OnUserCountChanged -= UpdateText;
        }

        _roomData = data;

        if (_roomData != null)
        {
            _roomData.OnUserCountChanged += UpdateText;
            UpdateText();
        }
    }

    private void UpdateText()
    {
        if (Text_Capacity != null)
        {
            Text_Capacity.text = $"{_roomData.CurrentCount} / {_roomData.MaxCapacity}";
            Text_Capacity.color = (_roomData.CurrentCount >= _roomData.MaxCapacity) ? Color.red : Color.white;
        }
    }

    private void OnDestroy()
    {
        if (_roomData != null)
        {
            _roomData.OnUserCountChanged -= UpdateText;
        }
    }
}
