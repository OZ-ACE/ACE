using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAdmissionPopup : UIBase, IClosablePopup
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
    private BuildGridViewModel _buildGridViewModel;
    private string _pendingHeroId;
    private long _pendingRoomInstanceId;
    private int _currentPaperIndex;

    private void OnEnable()
    {
        BindButtonEvents();
    }

    private void OnDisable()
    {
        UnbindButtonEvents();
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
        RequestClose();
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

    public void RequestClose()
    {
        UIManager.Inst.ClosePopup(this);
    }

    public void CloseImmediately()
    {
        CleanupAdmissionSelection();

        gameObject.SetActive(false);
    }

    public void Initialize()
    {
        _viewModel = new AdmissionPopupViewModel();
        _viewModel.Initialize();

        _buildGridViewModel = GameManager.Inst.Services.BuildService.GetBuildGridViewModel();

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
            Debug.LogWarning("입소 신청서 ID가 비어있음.");
            return;
        }

        if (string.IsNullOrEmpty(_pendingHeroId) == false)
        {
            Debug.LogWarning("방 선택하는 중.");
            return;
        }

        RoomAssignmentService roomAssignmentService = GameManager.Inst.Services.RoomAssignmentService;

        List<PlacedRoomData> emptyRooms = roomAssignmentService.GetEmptyRooms();

        if (emptyRooms.Count <= 0)
        {
            Debug.LogWarning("빈 침실 없음.");
            return;
        }

        _pendingHeroId = heroId;

        SetAdmissionButtonsLocked(true);

        _buildGridViewModel.OnRoomSelected -= OnRoomSelected;
        _buildGridViewModel.OnRoomSelected += OnRoomSelected;

        _buildGridViewModel.BeginRoomSelection(emptyRooms);

        Debug.Log($"[{heroId}] 방 선택 시작");
    }

    private void OnDestroy()
    {
        CleanupAdmissionSelection();

        UnbindButtonEvents();
        ClearPaperSlots();
    }

    private void OnRoomSelected(long roomInstanceId)
    {
        _buildGridViewModel.OnRoomSelected -= OnRoomSelected;
        _buildGridViewModel.EndRoomSelection();

        _pendingRoomInstanceId = roomInstanceId;

        OpenAdmissionConfirmPopup();
    }

    private void SetAdmissionButtonsLocked(bool isLocked)
    {
        for (int i = 0; i < _paperSlots.Count; i++)
        {
            UIAdmissionPaperSlot paperSlot = _paperSlots[i];

            if (paperSlot == null)
            {
                continue;
            }

            paperSlot.SetSelectionLocked(isLocked);
        }
    }

    private void OpenAdmissionConfirmPopup()
    {
        HeroData heroData = GameDataManager.Inst.GetData<HeroData>(_pendingHeroId);

        if (heroData == null)
        {
            Debug.LogWarning($"확인 팝업에 표시할 영웅 데이터를 찾을 수 없음. ID : {_pendingHeroId}");
            ResetPendingAdmission();
            return;
        }

        UIBase popupUI = UIManager.Inst.OpenPopup(UIType.AdmissionConfirmPopup);
        UIAdmissionConfirmPopup confirmPopup = popupUI as UIAdmissionConfirmPopup;

        if (confirmPopup == null)
        {
            Debug.LogError("UIAdmissionConfirmPopup 을 열 수 없음.");
            ResetPendingAdmission();
            return;
        }

        confirmPopup.Open(heroData.HeroName, "선택한 침실", ConfirmAdmission, CancelAdmission);
    }

    private void ConfirmAdmission()
    {
        if (string.IsNullOrEmpty(_pendingHeroId) == true)
        {
            Debug.LogWarning("입소할 영웅 ID 가 비어있음.");
            ResetPendingAdmission();
            return;
        }

        if (_pendingRoomInstanceId <= 0)
        {
            Debug.LogWarning("선택된 방 ID 가 유효하지 않음.");
            ResetPendingAdmission();
            return;
        }

        string admittedHeroId = _pendingHeroId;

        bool isAdmitted = AdmissionManager.Inst.TryAdmitHero(admittedHeroId, _pendingRoomInstanceId);

        if (isAdmitted == false)
        {
            Debug.LogWarning($"입소 처리에 실패함. HeroId : {admittedHeroId}");
            ResetPendingAdmission();
            return;
        }

        UIAdmissionPaperSlot admittedPaperSlot = FindPaperSlotByHeroId(admittedHeroId);

        if (admittedPaperSlot != null)
        {
            admittedPaperSlot.SetAdmittedState();
        }

        ResetPendingAdmission();
    }

    private void CancelAdmission()
    {
        ResetPendingAdmission();
    }

    private void ResetPendingAdmission()
    {
        _pendingHeroId = null;
        _pendingRoomInstanceId = 0;

        SetAdmissionButtonsLocked(false);
    }

    private UIAdmissionPaperSlot FindPaperSlotByHeroId(string heroId)
    {
        for (int i = 0; i < _paperSlots.Count; i++)
        {
            UIAdmissionPaperSlot paperSlot = _paperSlots[i];

            if (paperSlot == null)
            {
                continue;
            }

            if (paperSlot.GetHeroId() == heroId)
            {
                return paperSlot;
            }
        }

        return null;
    }

    private void CleanupAdmissionSelection()
    {
        if (_buildGridViewModel != null)
        {
            _buildGridViewModel.OnRoomSelected -= OnRoomSelected;
            _buildGridViewModel.EndRoomSelection();
        }

        ResetPendingAdmission();
    }
}