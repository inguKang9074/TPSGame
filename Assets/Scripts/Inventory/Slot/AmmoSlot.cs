using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AmmoSlot : Slot
{
    [SerializeField] private Image ammoIcon; // 장착한 총알 아이콘
    [SerializeField] private Text itemCount; // 장착한 총알의 갯수

    // 장착 슬롯에 탄약 장착
    public override void AddItem(Item target, bool alreadyEquip, int count = 1)
    {
        if (!alreadyEquip)
        {     
            return;
        }
        this.item = target;

        ammoIcon.sprite = this.item.data.sprite;
        itemCount.text = "x" + count.ToString();
    }

    public override void Clear()
    {
        ammoIcon.sprite = emptySlotSprite;
        itemCount.text = "";
    }

}
