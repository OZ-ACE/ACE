using System.ComponentModel;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class TutorialView : ViewBase
{
    [Header("내용")]
    [SerializeField] private TextMeshProUGUI Text_Title;
    [SerializeField] private Image Image_Tutorial;
    [SerializeField] private TextMeshProUGUI Text_Description;
    [Header("버튼/체크박스")]
    [SerializeField] private Button Button_Next;
    [SerializeField] private Button Button_Prev;
    [SerializeField] private Toggle Toggle_Ignore;
    private TutorialViewModel _viewModel;
    public void Bind(TutorialViewModel viewModel)
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }
        _viewModel = viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        Refresh();
    }
    private void OnEnable()
    {
        Button_Next.onClick.RemoveListener(OnClickNext);
        Button_Next.onClick.AddListener(OnClickNext);
        Button_Prev.onClick.RemoveListener(OnClickPrev);
        Button_Prev.onClick.AddListener(OnClickPrev);
        Toggle_Ignore.onValueChanged.RemoveListener(OnToggleIgnore);
        Toggle_Ignore.onValueChanged.AddListener(OnToggleIgnore);
        Bind(GameManager.Inst.Services.TutorialService.GetTutorialViewModel());
    }
    private void OnDestroy()
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }
    }
    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        Refresh();
    }
    private void Refresh()
    {
        if (_viewModel == null)
        {
            return;
        }
        if (Text_Description != null)
        {
            Text_Description.text = _viewModel.CurrentText;
        }
        SetImage(_viewModel.CurrentImagePath).Forget();
        if (Toggle_Ignore != null)
        {
            Toggle_Ignore.SetIsOnWithoutNotify(_viewModel.IsIgnoreChecked);
        }
        if (Text_Title != null)
        {
            Text_Title.text = _viewModel.CurrentTitle;
        }
    }
    private async UniTask SetImage(string path)
    {
        if (Image_Tutorial == null || string.IsNullOrEmpty(path) == true)
        {
            return;
        }
        Sprite sprite = await ResourceManager.Inst.LoadSprite(path);
        if (sprite != null)
        {
            Image_Tutorial.sprite = sprite;
        }
    }
    private void OnToggleIgnore(bool isOn)
    {
        _viewModel.IsIgnoreChecked = isOn;
    }
    // 다음 페이지, 마지막이면 무시 반영 후 닫기
    private void OnClickNext()
    {
        if (_viewModel.GoNextPage() == false)
        {
            _viewModel.ApplyIgnore();
            UIManager.Inst.CloseTutorialUI();
        }
    }

    private void OnClickPrev()
    {
        _viewModel.GoPrevPage();   // 첫 장이면 무시됨
    }


}