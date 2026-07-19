using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

public class ConstructSlot : MonoBehaviour
{
    [Header("버튼")]
    [SerializeField] private Button Button_Select;

    [Header("방 정보")]
    [SerializeField] private Image Image_RoomIcon;
    [SerializeField] private TextMeshProUGUI Text_RoomName;
    [SerializeField] private TextMeshProUGUI Text_Description;
    [SerializeField] private TextMeshProUGUI Text_Cost;

    private BuildGridViewModel _viewModel;
    private string _roomId;

    // 슬롯에 방 데이터를 채운다
    public void SetSlotData(RoomData roomData, BuildGridViewModel viewModel)
    {
        if (roomData == null || viewModel == null)
        {
            Debug.LogWarning("[ConstructSlot] 데이터 또는 뷰모델 없음");
            return;
        }

        _viewModel = viewModel;
        _roomId = roomData.ID;

        SetRoomIcon(roomData.ID).Forget();

        Text_RoomName.text = roomData.Name;
        Text_Description.text = roomData.Description;
        Text_Cost.text = $"{roomData.BuildCost} G";

        // 비활성 부모에서 Instantiate되면 Awake가 안 도니 여기서 등록
        Button_Select.onClick.RemoveListener(OnClickSelect);
        Button_Select.onClick.AddListener(OnClickSelect);
    }

    // 방 아이콘 로드
    private async UniTask SetRoomIcon(string roomId)
    {
        if (Image_RoomIcon == null)
        {
            return;
        }

        Sprite sprite = await ResourceManager.Inst.LoadSprite($"Image/Room/{roomId}");

        if (sprite == null)
        {
            Debug.LogWarning($"[ConstructSlot] 아이콘 없음: {roomId}");
            return;
        }
        Image_RoomIcon.sprite = sprite;
    }

    // 방 선택
    private void OnClickSelect()
    {
        if (_viewModel == null)
        {
            return;
        }

        _viewModel.SelectRoom(_roomId);
        Debug.Log($"[ConstructSlot] 방 선택: {_roomId}");
    }
}