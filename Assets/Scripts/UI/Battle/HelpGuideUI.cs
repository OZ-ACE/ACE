using UnityEngine;

// 상단 도움말 버튼으로 여닫는 전투 설명 패널
public class HelpGuideUI : MonoBehaviour
{
    [Header("도움말 버튼")]
    [SerializeField] private RectTransform RectTransform_ToggleButton; //도움말 버튼 영역 - 외부 클릭 판정에서 제외

    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
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

    //버튼 클릭할 때마다 열림/닫힘 반전
    public void ToggleGuide()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    private void CloseGuide()
    {
        gameObject.SetActive(false);
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