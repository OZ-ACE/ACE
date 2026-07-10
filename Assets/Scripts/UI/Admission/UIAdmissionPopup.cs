using System.Collections.Generic;
using UnityEngine;

public class UIAdmissionPopup : UIBase
{
    [Header("Paper")]
    [SerializeField] private RectTransform Rect_PaperRoot;
    [SerializeField] private UIAdmissionPaperSlot Prefab_PaperSlot;

    [Header("Paper Position")]
    [SerializeField]
    private Vector2 _stackedStartPosition = Vector2.zero;

    [SerializeField]
    private Vector2 _stackedOffset = new Vector2(18f, -8f);

    [SerializeField]
    private Vector2 _flippedStartPosition = new Vector2(-420f, 0f);

    [SerializeField]
    private Vector2 _flippedOffset = new Vector2(-18f, -8f);

    private readonly List<UIAdmissionPaperSlot> _paperSlots = new List<UIAdmissionPaperSlot>();

    private AdmissionPopupViewModel _viewModel;
    private int _currentPaperIndex;

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

        Debug.Log($"입소 신청서 개수 : {candidateCount}");

        for (int i = 0; i < candidateCount; i++)
        {
            HeroData heroData = _viewModel.GetHeroData(i);

            if (heroData == null)
            {
                Debug.LogWarning($"영웅 데이터를 찾을 수 없습니다. Index : {i}");
                continue;
            }

            UIAdmissionPaperSlot paperSlot = Instantiate(Prefab_PaperSlot, Rect_PaperRoot);

            if (paperSlot == null)
            {
                Debug.LogWarning($"입소 신청서 생성에 실패했습니다. Index : {i}");
                continue;
            }

            Vector2 stackedPosition = _stackedStartPosition + (_stackedOffset * i);
            Vector2 flippedPosition = _flippedStartPosition + (_flippedOffset * i);

            Vector3 stackedRotation = new Vector3(0f, 0f, GetStackedRotationZ(i));
            Vector3 flippedRotation = new Vector3(0f, 0f, 8f);

            paperSlot.Initialize(heroData, i);
            paperSlot.SetPaperLayout(stackedPosition, flippedPosition, stackedRotation, flippedRotation);

            BindPaperSlotEvents(paperSlot);

            _paperSlots.Add(paperSlot);
        }

        RefreshPaperLayout();
    }

    private void BindPaperSlotEvents(UIAdmissionPaperSlot paperSlot)
    {
        paperSlot.OnClickNext += RequestMoveNextPaper;
        paperSlot.OnClickPrev += RequestMovePreviousPaper;

        paperSlot.OnFlipComplete += CompleteMoveNextPaper;
        paperSlot.OnReturnComplete += CompleteMovePreviousPaper;

        paperSlot.OnClickAdmit += AdmitHero;
    }

    private void UnbindPaperSlotEvents(UIAdmissionPaperSlot paperSlot)
    {
        paperSlot.OnClickNext -= RequestMoveNextPaper;
        paperSlot.OnClickPrev -= RequestMovePreviousPaper;

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

    private void RequestMoveNextPaper(int paperIndex)
    {
        if (_currentPaperIndex >= _paperSlots.Count)
        {
            return;
        }

        if (paperIndex != _currentPaperIndex)
        {
            Debug.LogWarning($"현재 종이가 아닙니다. Current : {_currentPaperIndex}, Clicked : {paperIndex}");
            return;
        }

        UIAdmissionPaperSlot currentPaper = _paperSlots[_currentPaperIndex];

        currentPaper.PlayFlipToLeft();
    }

    private void CompleteMoveNextPaper(int paperIndex)
    {
        if (paperIndex != _currentPaperIndex)
        {
            Debug.LogWarning($"완료된 종이 순서가 일치하지 않습니다. Current : {_currentPaperIndex}, Completed : {paperIndex}");
            return;
        }

        _currentPaperIndex++;

        RefreshPaperLayout();
    }

    private void RequestMovePreviousPaper(int paperIndex)
    {
        if (_currentPaperIndex <= 0)
        {
            return;
        }

        int previousPaperIndex = _currentPaperIndex - 1;

        if (paperIndex != previousPaperIndex)
        {
            Debug.LogWarning($"되돌릴 수 있는 종이가 아닙니다. Expected : {previousPaperIndex}, Clicked : {paperIndex}");
            return;
        }

        UIAdmissionPaperSlot previousPaper = _paperSlots[previousPaperIndex];

        previousPaper.PlayReturnToStack();
    }

    private void CompleteMovePreviousPaper(int paperIndex)
    {
        int previousPaperIndex = _currentPaperIndex - 1;

        if (paperIndex != previousPaperIndex)
        {
            Debug.LogWarning($"되돌아온 종이 순서가 일치하지 않습니다. Expected : {previousPaperIndex}, Completed : {paperIndex}");
            return;
        }

        _currentPaperIndex--;

        RefreshPaperLayout();
    }

    private void RefreshPaperLayout()
    {
        RefreshPaperStates();
        RefreshPaperOrder();
    }

    private void RefreshPaperStates()
    {
        for (int i = 0; i < _paperSlots.Count; i++)
        {
            UIAdmissionPaperSlot paperSlot = _paperSlots[i];

            if (i < _currentPaperIndex)
            {
                bool canReturn = i == _currentPaperIndex - 1;

                paperSlot.ApplyState(AdmissionPaperState.Flipped, canReturn);
                continue;
            }

            if (i == _currentPaperIndex)
            {
                paperSlot.ApplyState(AdmissionPaperState.Viewing, false);
                continue;
            }

            paperSlot.ApplyState(AdmissionPaperState.Stacked, false);
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

    private void AdmitHero(string heroId)
    {
        if (string.IsNullOrEmpty(heroId) == true)
        {
            Debug.LogWarning("입소 신청서의 영웅 ID가 비어있습니다.");
            return;
        }

        bool isSuccess = AdmissionManager.Inst.TryAdmitHero(heroId);

        if (isSuccess == false)
        {
            Debug.LogWarning("입소 처리에 실패했습니다.");
            return;
        }

        Debug.Log($"{heroId} 입소 확정 완료!");
    }

    private void OnDestroy()
    {
        ClearPaperSlots();
    }
}