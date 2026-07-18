using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIAdmissionPopup : UIBase
{
    [Header("Button")]
    [SerializeField] private Button Button_Background;
    [SerializeField] private Button Button_Next;
    [SerializeField] private Button Button_Prev;

    [Header("Paper")]
    [SerializeField] private RectTransform Rect_PaperRoot;
    [SerializeField] private UIAdmissionPaperSlot Prefab_PaperSlot;
    [SerializeField] private Vector2 _stackedStartPosition = Vector2.zero;

    [Header("Offset")]
    [SerializeField] private Vector2 _stackedOffset = new Vector2(18f, -8f);
    [SerializeField] private Vector2 _flippedStartPosition = new Vector2(-420f, 0f);
    [SerializeField] private Vector2 _flippedOffset = new Vector2(-18f, -8f);

    private readonly List<UIAdmissionPaperSlot> _paperSlots = new List<UIAdmissionPaperSlot>();

    private AdmissionPopupViewModel _viewModel;
    private int _currentPaperIndex;

    private void OnEnable()
    {
        BindButtonEvents();
    }

    private void OnDisable()
    {
        UnbindButtonEvents();
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ClosePopup();
        }
    }

    private void BindButtonEvents()
    {
        Button_Background.onClick.RemoveListener(OnClickBackgroundButton);
        Button_Next.onClick.RemoveListener(OnClickNextButton);
        Button_Prev.onClick.RemoveListener(OnClickPrevButton);

        Button_Background.onClick.AddListener(OnClickBackgroundButton);
        Button_Next.onClick.AddListener(OnClickNextButton);
        Button_Prev.onClick.AddListener(OnClickPrevButton);
    }

    private void UnbindButtonEvents()
    {
        Button_Background.onClick.RemoveListener(OnClickBackgroundButton);
        Button_Next.onClick.RemoveListener(OnClickNextButton);
        Button_Prev.onClick.RemoveListener(OnClickPrevButton);
    }

    private void OnClickBackgroundButton()
    {
        ClosePopup();
    }

    private void OnClickNextButton()
    {
        if (_currentPaperIndex >= _paperSlots.Count)
        {
            return;
        }

        _paperSlots[_currentPaperIndex].PlayFlipToLeft();
    }

    private void OnClickPrevButton()
    {
        if (_currentPaperIndex <= 0)
        {
            return;
        }

        _paperSlots[_currentPaperIndex - 1].PlayReturnToStack();
    }

    private void ClosePopup()
    {
        UIManager.Inst.CloseAdmissionPopup();
    }

    public void Initialize()
    {
        _viewModel = new AdmissionPopupViewModel();
        _viewModel.Initialize();

        _currentPaperIndex = 0;

        ClearPaperSlots();
        CreatePaperSlots();
    }

    private void CreatePaperSlots()
    {
        int candidateCount = _viewModel.CandidateModels.Count;

        for (int i = 0; i < candidateCount; i++)
        {
            HeroData heroData = _viewModel.GetHeroData(i);
            AdmissionCandidateModel candidateModel = _viewModel.CandidateModels[i];

            if (heroData == null)
            {
                Debug.LogWarning($"영웅 데이터를 찾을 수 없음. Index : {i}");
                continue;
            }

            UIAdmissionPaperSlot paperSlot = Instantiate(Prefab_PaperSlot, Rect_PaperRoot);

            if (paperSlot == null)
            {
                Debug.LogWarning($"입소 신청서 생성에 실패함. Index : {i}");
                continue;
            }

            Vector2 stackedPosition = _stackedStartPosition + (_stackedOffset * i);
            Vector2 flippedPosition = _flippedStartPosition + (_flippedOffset * i);

            Vector3 stackedRotation = new Vector3(0f, 0f, GetStackedRotationZ(i));
            Vector3 flippedRotation = new Vector3(0f, 0f, 8f);

            paperSlot.Initialize(heroData, i, candidateModel.IsAdmitted);
            paperSlot.SetPaperLayout(stackedPosition, flippedPosition, stackedRotation, flippedRotation);

            BindPaperSlotEvents(paperSlot);

            _paperSlots.Add(paperSlot);
        }

        RefreshPaperLayout();
    }

    private void BindPaperSlotEvents(UIAdmissionPaperSlot paperSlot)
    {
        paperSlot.OnFlipComplete += CompleteMoveNextPaper;
        paperSlot.OnReturnComplete += CompleteMovePreviousPaper;
        paperSlot.OnClickAdmit += AdmitHero;
    }

    private void UnbindPaperSlotEvents(UIAdmissionPaperSlot paperSlot)
    {
        paperSlot.OnFlipComplete -= CompleteMoveNextPaper;
        paperSlot.OnReturnComplete -= CompleteMovePreviousPaper;
        paperSlot.OnClickAdmit -= AdmitHero;
    }

    private void ClearPaperSlots()
    {
        for (int i = 0; i < _paperSlots.Count; i++)
        {
            UIAdmissionPaperSlot paperSlot = _paperSlots[i];

            if (paperSlot == null)
            {
                continue;
            }

            UnbindPaperSlotEvents(paperSlot);
            Destroy(paperSlot.gameObject);
        }

        _paperSlots.Clear();
    }

    private void CompleteMoveNextPaper(int paperIndex)
    {
        if (paperIndex != _currentPaperIndex)
        {
            Debug.LogWarning($"완료된 종이 순서가 일치하지 않음. Current : {_currentPaperIndex}, Completed : {paperIndex}");
            return;
        }

        _currentPaperIndex++;

        RefreshPaperLayout();
    }

    private void CompleteMovePreviousPaper(int paperIndex)
    {
        int previousPaperIndex = _currentPaperIndex - 1;

        if (paperIndex != previousPaperIndex)
        {
            Debug.LogWarning($"되돌아온 종이 순서가 일치하지 않음. Expected : {previousPaperIndex}, Completed : {paperIndex}");
            return;
        }

        _currentPaperIndex--;

        RefreshPaperLayout();
    }

    private void RefreshPaperLayout()
    {
        RefreshPaperStates();
        RefreshPaperOrder();
        RefreshButtonState();
    }

    private void RefreshPaperStates()
    {
        for (int i = 0; i < _paperSlots.Count; i++)
        {
            UIAdmissionPaperSlot paperSlot = _paperSlots[i];

            if (i < _currentPaperIndex)
            {
                paperSlot.ApplyState(AdmissionPaperState.Flipped);
                continue;
            }

            if (i == _currentPaperIndex)
            {
                paperSlot.ApplyState(AdmissionPaperState.Viewing);
                continue;
            }

            paperSlot.ApplyState(AdmissionPaperState.Stacked);
        }
    }

    private void RefreshPaperOrder()
    {
        int siblingIndex = 0;

        for (int i = 0; i < _currentPaperIndex; i++)
        {
            _paperSlots[i].transform.SetSiblingIndex(siblingIndex);
            siblingIndex++;
        }

        for (int i = _paperSlots.Count - 1; i >= _currentPaperIndex; i--)
        {
            _paperSlots[i].transform.SetSiblingIndex(siblingIndex);
            siblingIndex++;
        }
    }

    private float GetStackedRotationZ(int index)
    {
        if (index == 0)
        {
            return 0f;
        }

        if (index % 2 == 0)
        {
            return -1.5f;
        }

        return 1.5f;
    }

    private void RefreshButtonState()
    {
        Button_Next.gameObject.SetActive(_currentPaperIndex < _paperSlots.Count);
        Button_Prev.gameObject.SetActive(_currentPaperIndex > 0);

        Button_Next.interactable = _currentPaperIndex < _paperSlots.Count;
        Button_Prev.interactable = _currentPaperIndex > 0;
    }

    private void AdmitHero(string heroId)
    {
        if (string.IsNullOrEmpty(heroId))
        {
            Debug.LogWarning("입소 신청서 ID 가 비어있음.");
            return;
        }

        RoomAssignmentService roomAssignmentService = GameManager.Inst.Services.RoomAssignmentService;

        List<PlacedRoomData> emptyRooms = roomAssignmentService.GetEmptyRooms();

        if (emptyRooms.Count <= 0)
        {
            Debug.LogWarning("빈 침실 없음.");
            return;
        }

        bool isSuccess = AdmissionManager.Inst.TryAdmitHero(heroId);

        if (isSuccess == false)
        {
            Debug.LogWarning("입소 처리 실패함.");
            return;
        }

        bool isAssigned = roomAssignmentService.AssignRoom(heroId, emptyRooms[0].RoomInstanceId);

        if (isAssigned == false)
        {
            Debug.LogWarning("방 배정에 실패함.");
            return;
        }

        if (_currentPaperIndex < 0 || _currentPaperIndex >= _paperSlots.Count)
        {
            return;
        }

        UIAdmissionPaperSlot currentPaperSlot = _paperSlots[_currentPaperIndex];
        currentPaperSlot.SetAdmittedState();
    }

    private void OnDestroy()
    {
        UnbindButtonEvents();
        ClearPaperSlots();
    }
}