using UnityEngine;

/// <summary>
/// 방 버튼 하나가 자신의 roomId를 기억하고, 클릭 시 SelectRoom을 호출하기 위한 핸들러.
/// </summary>
public class RoomButtonHandler : MonoBehaviour
{
    private BuildGridViewModel _viewModel;
    private string _roomId;

    public void Setup(BuildGridViewModel viewModel, string roomId)
    {
        _viewModel = viewModel;
        _roomId = roomId;
    }

    public void OnClickSelect()
    {
        if (_viewModel != null)
        {
            _viewModel.SelectRoom(_roomId);
            Debug.Log($"[RoomButtonHandler] 방 선택: {_roomId}");
        }
    }
}