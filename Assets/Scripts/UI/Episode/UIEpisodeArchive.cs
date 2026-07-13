using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIEpisodeArchive : UIBase
{
    [SerializeField] private Transform Transform_EpisodeList;
    [SerializeField] private UIEpisodeArchiveSlot Prefab_EpisodeSlot;

    [SerializeField] private TextMeshProUGUI Text_EpisodeName;
    [SerializeField] private TextMeshProUGUI Text_EpisodeDescription;
    [SerializeField] private TextMeshProUGUI Text_Category;

    [SerializeField] private GameObject Panel_Detail;

    [SerializeField] private Button Button_Play;
    [SerializeField] private Button Button_Close;

    private readonly Dictionary<string, UIEpisodeArchiveSlot> _slotDict = new Dictionary<string, UIEpisodeArchiveSlot>();

    private EpisodeArchiveViewModel _viewModel;

    private void OnEnable()
    {
        Button_Play.onClick.AddListener(OnClickPlay);
        Button_Close.onClick.AddListener(OnClickClose);
    }

    private void OnDisable()
    {
        Button_Play.onClick.RemoveListener(OnClickPlay);
        Button_Close.onClick.RemoveListener(OnClickClose);
    }

    public void Bind(EpisodeArchiveViewModel viewModel)
    {
        _viewModel = viewModel;

        RefreshArchive();
    }

    private void RefreshArchive()
    {
        ClearSlots();

        if (_viewModel == null)
        {
            ClearDetail();
            return;
        }

        _viewModel.RefreshEpisodes();

        IReadOnlyList<EpisodeArchiveItemViewModel> episodeItems = _viewModel.EpisodeItems;

        for (int i = 0; i < episodeItems.Count; i++)
        {
            CreateSlot(episodeItems[i]);
        }

        ClearDetail();
    }

    private void CreateSlot(EpisodeArchiveItemViewModel itemViewModel)
    {
        if (itemViewModel == null || Prefab_EpisodeSlot == null)
        {
            return;
        }

        UIEpisodeArchiveSlot slot = Instantiate(Prefab_EpisodeSlot, Transform_EpisodeList);

        if (slot == null)
        {
            return;
        }

        slot.Bind(itemViewModel, SelectEpisode);

        _slotDict.Add(itemViewModel.EpisodeDataId, slot);
    }

    private void SelectEpisode(string episodeDataId)
    {
        if (_viewModel == null)
        {
            return;
        }

        _viewModel.SelectEpisode(episodeDataId);

        RefreshDetail();
        RefreshSlots();
    }

    private void RefreshDetail()
    {
        EpisodeArchiveItemViewModel selectedEpisode = _viewModel.SelectedEpisode;

        if (selectedEpisode == null)
        {
            ClearDetail();
            return;
        }

        Panel_Detail.SetActive(true);

        Text_EpisodeName.text = selectedEpisode.EpisodeName;
        Text_EpisodeDescription.text = selectedEpisode.EpisodeDescription;
        Text_Category.text = selectedEpisode.Category.ToString();
    }

    private void RefreshSlots()
    {
        foreach (KeyValuePair<string, UIEpisodeArchiveSlot> pair in _slotDict)
        {
            pair.Value.Refresh();
        }
    }

    private void ClearSlots()
    {
        foreach (KeyValuePair<string, UIEpisodeArchiveSlot> pair in _slotDict)
        {
            if (pair.Value == null)
            {
                continue;
            }

            Destroy(pair.Value.gameObject);
        }

        _slotDict.Clear();
    }

    private void ClearDetail()
    {
        Panel_Detail.SetActive(false);

        Text_EpisodeName.text = string.Empty;
        Text_EpisodeDescription.text = string.Empty;
        Text_Category.text = string.Empty;
    }

    private void OnClickPlay()
    {
        if (_viewModel == null || _viewModel.SelectedEpisode == null)
        {
            return;
        }

        Debug.Log($"UIEpisodeArchive - 재생 요청 : {_viewModel.SelectedEpisode.EpisodeName}");

        // TODO: 다음 단계에서 DialogueUI와 연결
    }

    private void OnClickClose()
    {
        UIManager.Inst.CloseEpisodeArchive();
    }
}