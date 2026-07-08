using NUnit.Framework;
using TMPro;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Text_SelectItem;
    [SerializeField] private TextMeshProUGUI Text_SelectDescription;

    [SerializeField] private Transform Transform_SlotParent;
    [SerializeField] private GameObject InventorySlot;

    private InventoryViewModel _inventoryVM;
    private List<In>

    public void BindViewModel()
    {

    }
}
