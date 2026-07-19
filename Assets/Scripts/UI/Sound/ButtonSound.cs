using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        SoundManager.Inst.PlaySFX("Click");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SoundManager.Inst.PlaySFX("ButtonHover");
    }
}
