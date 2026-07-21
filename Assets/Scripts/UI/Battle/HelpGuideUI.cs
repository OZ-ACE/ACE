using System;
using UnityEngine;
using UnityEngine.UI;

// 상단 도움말 버튼으로 여닫는 전투 설명 패널
public class HelpGuideUI : MonoBehaviour
{
    [Header("도움말 버튼")]
    [SerializeField] private RectTransform RectTransform_ToggleButton; //도움말 버튼 영역 - 외부 클릭 판정에서 제외

    [Header("패널 내부")]
    [SerializeField] private Toggle Toggle_DontShowAgain;
    [SerializeField] private Button Button_CloseGuide;

    private const string DontShowAgainKey = "BattleHelpDontShowAgain";
    private const int DontShowAgainOnValue = 1;
    private const int DontShowAgainOffValue = 0;

    private RectTransform _rectTransform;
    private Action _onGuideClosed;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

        if (Button_CloseGuide != null)
        {
            Button_CloseGuide.onClick.AddListener(OnClickCloseGuide);
        }
    }

    private void OnDestroy()
    {
        if (Button_CloseGuide != null)
        {
            Button_CloseGuide.onClick.RemoveListener(OnClickCloseGuide);
        }
    }

    private void Update()
    {
        if (gameObject.activeSelf == false)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) && IsPointerInsideGuide() == false && IsPointerInsideToggleButton() == false)
        {
            CloseGuide();
        }
    }

    //전투 시작 직전 자동 표시. '다시 보지 않기'가 켜져 있으면 띄우지 않고 곧바로 다음 단계로 넘긴다
    public void ShowGuideOnBattleStart(Action onGuideClosed)
    {
        _onGuideClosed = onGuideClosed;

        if (IsDontShowAgainOn() == true)
        {
            NotifyGuideClosed();
            return;
        }

        OpenGuide();
    }

    //버튼 클릭할 때마다 열림/닫힘 반전
    public void ToggleGuide()
    {
        if (gameObject.activeSelf == true)
        {
            CloseGuide();
            return;
        }

        OpenGuide();
    }

    //전투 리셋 등에서 콜백 실행 없이 조용히 닫을 때 사용한다
    public void CloseGuideSilently()
    {
        _onGuideClosed = null;
        gameObject.SetActive(false);
    }

    private void OpenGuide()
    {
        gameObject.SetActive(true);

        if (Toggle_DontShowAgain != null)
        {
            Toggle_DontShowAgain.isOn = IsDontShowAgainOn();
        }
    }

    private void OnClickCloseGuide()
    {
        CloseGuide();
    }

    private void CloseGuide()
    {
        SaveDontShowAgain();
        gameObject.SetActive(false);
        NotifyGuideClosed();
    }

    //대기 중인 닫힘 콜백을 한 번만 실행한다 (중복 호출로 전투 루프가 두 번 시작되는 것 방지)
    private void NotifyGuideClosed()
    {
        Action callback = _onGuideClosed;
        _onGuideClosed = null;

        if (callback == null)
        {
            return;
        }

        callback();
    }

    private bool IsDontShowAgainOn()
    {
        return PlayerPrefs.GetInt(DontShowAgainKey, DontShowAgainOffValue) == DontShowAgainOnValue;
    }

    private void SaveDontShowAgain()
    {
        if (Toggle_DontShowAgain == null)
        {
            return;
        }

        PlayerPrefs.SetInt(DontShowAgainKey, Toggle_DontShowAgain.isOn ? DontShowAgainOnValue : DontShowAgainOffValue);
        PlayerPrefs.Save();
    }

    //패널 영역 안을 클릭했는지 판별한다
    private bool IsPointerInsideGuide()
    {
        return RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, Input.mousePosition, null);
    }

    //도움말 버튼 영역 안을 클릭했는지 판별한다 (버튼 자체 클릭은 여기서 걸러서 자체 충돌 방지)
    private bool IsPointerInsideToggleButton()
    {
        if (RectTransform_ToggleButton == null)
        {
            return false;
        }

        return RectTransformUtility.RectangleContainsScreenPoint(RectTransform_ToggleButton, Input.mousePosition, null);
    }
}