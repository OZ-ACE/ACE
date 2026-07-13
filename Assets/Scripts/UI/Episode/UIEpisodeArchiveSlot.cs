using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIEpisodeArchiveSlot : UIBase
{
    [SerializeField] private TextMeshProUGUI Text_EpisodeName;
    [SerializeField] private TextMeshProUGUI Text_Category;
    [SerializeField] private Image Image_New;
    [SerializeField] private Button Button_Select;

    private EpisodeArchiveItemViewModel _viewModel;
    private Action<string> _onSelectEpisode;

    private void OnEnable()
    {
        Button_Select.onClick.AddListener(OnClickSelect);
    }

    private void OnDisable()
    {
        Button_Select.onClick.RemoveListener(OnClickSelect);

        _viewModel = null;
        _onSelectEpisode = null;
    }

    public void Bind(EpisodeArchiveItemViewModel viewModel, Action<string> onSelectEpisode)
    {
        _viewModel = viewModel;
        _onSelectEpisode = onSelectEpisode;

        Refresh();
    }

    public void Refresh()
    {
        if (_viewModel == null)
        {
            return;
        }

        Text_EpisodeName.text = _viewModel.EpisodeName;
        Text_Category.text = _viewModel.Category.ToString();
        Image_New.gameObject.SetActive(_viewModel.IsNew);
    }

    private void OnClickSelect()
    {
        if (_viewModel == null)
        {
            return;
        }

        _onSelectEpisode?.Invoke(_viewModel.EpisodeDataId);
    }
}