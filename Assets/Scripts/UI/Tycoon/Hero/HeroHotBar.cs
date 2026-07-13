using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class HeroHotBar : MonoBehaviour
{
    [SerializeField] private Button Button_Rest;
    [SerializeField] private Button Button_Sun;

    private HeroViewModel _targetVM;

    private void Awake()
    {
        Button_Rest.onClick.AddListener(OnClickRest);
        Button_Sun.onClick.AddListener(OnClickSun);
    
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_targetVM != null)
        {
            _targetVM.PropertyChanged -= OnPropertyChanged_View;
        }
    }

    public void OpenHotBar(HeroViewModel heroVM)
    {
        if (_targetVM != null)
        {
            _targetVM.PropertyChanged -= OnPropertyChanged_View;
        }

        _targetVM = heroVM;
        _targetVM.PropertyChanged += OnPropertyChanged_View;

        gameObject.SetActive(true);

        _targetVM.InvokeOnceOnInit();
    }

    private void OnPropertyChanged_View(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_targetVM.Affection):
                // 호감도 반영
                break;

            case nameof(_targetVM.Satisfaction):
                // 불만도 반영
                break;
        }
    }

    private void OnClickRest()
    {
        // 호감도, 불만도 등 반영
        // _targetVM.AddSatisfaction(10);
    }

    private void OnClickSun()
    {
        // 호감도, 불만도 등 반영
        // _targetVM.AddSatisfaction(10);
    }
}
