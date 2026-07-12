using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//액션 큐에 표시되는 개별 유닛의 행동 슬롯을 관리하는 컴포넌트
public class BattleActionSlot : MonoBehaviour
{
    [Serializable]
    public struct ActionTypeVisual
    {
        public ActionType Type;
        public Sprite Icon;
        public Sprite FrameSprite;
    }

    [Header("초상화")]
    [SerializeField] private Image Image_UnitPortrait;

    [Header("액션슬롯")]
    [SerializeField] private Image ActionTypeFrame;
    [SerializeField] private Image Image_ActionType;
    [SerializeField] private List<ActionTypeVisual> _actionTypeVisualList;

    ///<summary> 슬롯에 유닛 초상화와 액션타입 표시를 채운다 </summary>
    ///SetSlotData가 진입점이고, 안에서 SetUnitPortrait/SetActionTypeVisual 두 개를 각각 호출하는 구조
    public void SetSlotData(BattleUnitModel unit, ActionType actionType)
    {
        if (unit == null)
        {
            Debug.LogWarning("[BattleActionSlot] 유닛 데이터 없음");
            return;
        }

        SetUnitPortrait(unit.ID);
        SetActionTypeVisual(actionType);
    }

    private void SetUnitPortrait(string unitId)
    {
        if (Image_UnitPortrait == null)
        {
            return;
        }

        HeroData heroData = GameDataManager.Inst.GetData<HeroData>(unitId);

        if (heroData == null || string.IsNullOrEmpty(heroData.ProfileImage) == true)
        {
            return;
        }

        ResourceManager.Inst.LoadSprite(heroData.ProfileImage, SetPortraitSprite);
    }

    private void SetPortraitSprite(Sprite sprite)
    {
        if (Image_UnitPortrait == null || sprite == null)
        {
            return;
        }

        Image_UnitPortrait.sprite = sprite;
    }

    private void SetActionTypeVisual(ActionType actionType)
    {
        foreach (ActionTypeVisual visual in _actionTypeVisualList)
        {
            if (visual.Type != actionType)
            {
                continue;
            }

            if (Image_ActionType != null)
            {
                Image_ActionType.sprite = visual.Icon;
            }

            if (ActionTypeFrame != null)
            {
                ActionTypeFrame.sprite = visual.FrameSprite;
            }

            return;
        }

        Debug.LogWarning($"[BattleActionSlot] 매칭되는 ActionType 비주얼 없음: {actionType}");
    }

}
