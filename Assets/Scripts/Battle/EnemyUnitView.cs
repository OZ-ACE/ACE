using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUnitView : MonoBehaviour
{
    [Header("Unit Info")]
    [SerializeField] private TMP_Text Text_Name;
    [SerializeField] private TMP_Text Text_Hp;
    [SerializeField] private Image Image_HpFill;

    [Header("VFX")]
    [SerializeField] private Transform _vfxPoint;

    public Transform VfxPoint
    {
        get
        {
            if (_vfxPoint != null)
            {
                return _vfxPoint;
            }

            return transform;
        }
    }

    public void Initialize(string unitName, int currentHp, int maxHp)
    {
        if (Text_Name != null)
        {
            Text_Name.text = unitName;
        }

        RefreshHp(currentHp, maxHp);
    }

    public void RefreshHp(int currentHp, int maxHp)
    {
        int safeMaxHp = Mathf.Max(1, maxHp);
        int safeCurrentHp = Mathf.Clamp(currentHp, 0, safeMaxHp);

        if (Text_Hp != null)
        {
            Text_Hp.text = $"{safeCurrentHp} / {safeMaxHp}";
        }

        if (Image_HpFill != null)
        {
            Image_HpFill.fillAmount = (float)safeCurrentHp / safeMaxHp;
        }
    }
}
