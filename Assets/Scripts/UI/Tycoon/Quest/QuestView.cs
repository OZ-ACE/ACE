using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class QuestView : ViewBase
{
    [Header("슬롯 프리팹 / 부모")]
    [SerializeField] private GameObject Prefab_QuestSlot;
    [SerializeField] private Transform Transform_SlotParent;

    [SerializeField] private GameObject GameObject_HeroRequestRoot;
    [SerializeField] private HeroRequestSlot HeroRequestSlot;

    private QuestViewModel _viewModel;
    private HeroRequestService _heroRequestService;
    private List<QuestSlot> _activeSlots = new List<QuestSlot>();
    private PlayerModel _playerModel = SaveManager.Inst.CurrentPlayerModel;

    // 뷰모델 바인딩
    public void Bind(QuestViewModel viewModel)
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _viewModel.OnChangeQuestProgress -= OnChangeQuestProgress;
        }

        _viewModel = viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        _viewModel.OnChangeQuestProgress += OnChangeQuestProgress;

        _viewModel.InvokeOnceOnInit();
    }

    private void OnEnable()
    {
        if (_viewModel == null)
        {
            QuestViewModel vm = GameManager.Inst.Services.QuestService.GetQuestViewModel();
            if (vm == null)
            {
                return;
            }
            Bind(vm);
        }
        else
        {
            _viewModel.InvokeOnceOnInit();
        }

        _heroRequestService = GameManager.Inst.Services.HeroRequestService;

        if (_heroRequestService != null)
        {
            _heroRequestService.OnHeroRequestChanged += RefreshHeroRequest;
        }

        RefreshHeroRequest();
    }

    private void OnDisable()
    {
        if (_heroRequestService != null)
        {
            _heroRequestService.OnHeroRequestChanged -= RefreshHeroRequest;
        }
    }

    private void OnDestroy()
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _viewModel.OnChangeQuestProgress -= OnChangeQuestProgress;
        }
    }

    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(QuestViewModel.QuestList))
        {
            CreateSlots();
        }
    }

    // 진행도·보상 수령 시 기존 슬롯 상태만 갱신
    private void OnChangeQuestProgress()
    {
        CreateSlots();
    }

    private void CreateSlots()
    {
        ClearSlots();

        List<QuestData> quests = _viewModel.QuestList;
        foreach (QuestData quest in quests)
        {
            QuestState state = _viewModel.GetState(quest.ID);
            //수령 완료(Rewarded)했거나 선행 미완료로 잠긴(Locked) 퀘스트는 표시 안 함
            if (state == QuestState.Rewarded || state == QuestState.Locked)
            {
                continue;
            }
            GameObject slotObj = Instantiate(Prefab_QuestSlot, Transform_SlotParent);
            slotObj.name = $"Slot_{quest.ID}";
            QuestSlot slot = slotObj.GetComponent<QuestSlot>();
            if (slot == null)
            {
                continue;
            }
            _activeSlots.Add(slot);
            slot.SetSlotData(quest, _viewModel);
        }

        Debug.Log($"[QuestView] 퀘스트 슬롯 {_activeSlots.Count}개 생성");
    }

    private void ClearSlots()
    {
        foreach (QuestSlot slot in _activeSlots)
        {
            if (slot != null)
            {
                Destroy(slot.gameObject);
            }
        }
        _activeSlots.Clear();
    }

    private void RefreshAllSlots()
    {
        foreach (QuestSlot slot in _activeSlots)
        {
            if (slot == null)
            {
                continue;
            }

            slot.UpdateState();
        }
    }

    private void RefreshHeroRequest()
    {
        HeroRequestService service = GameManager.Inst.Services.HeroRequestService;

        if (service == null)
        {
            GameObject_HeroRequestRoot.SetActive(false);
            return;
        }

        HeroRequestModel requestModel = service.GetCurrentRequest();
        HeroRequestData requestData = service.GetCurrentRequestData();

        bool hasRequest = requestModel != null && requestData != null;

        GameObject_HeroRequestRoot.SetActive(hasRequest);

        if (hasRequest == false)
        {
            return;
        }

        GameObject_HeroRequestRoot.transform.SetAsFirstSibling();
        HeroRequestSlot.SetData(requestModel, requestData, service.CurrentHour);

        HeroRequestSlot.OnClickShortcut = OnClickHeroRequestShortcut;
    }

    private void OnClickHeroRequestShortcut()
    {
        HeroRequestModel request = _heroRequestService.GetCurrentRequest();

        if (request == null)
        {
            return;
        }

        UIBase ui = UIManager.Inst.GetOpenedUI(UIRootType.Main, UIType.TycoonMainUI);

        if (ui is TycoonMainUI tycoonUI)
        {
            tycoonUI.OpenHeroSchedule(request.HeroId);
        }
    }
}