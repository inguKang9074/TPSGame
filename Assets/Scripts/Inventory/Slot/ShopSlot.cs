using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ShopSlot : Slot
{
    // UI
    [SerializeField] private Image slotStateIcon; // 아이템 상태 표시 이미지

    public Image itemIcon; // 아이템 이미지
    public Text itemName; // 아이템 이름
    public Text itemDescription; // 아이템 설명

    // 슬롯 상태 표시 UI
    [SerializeField] private Sprite selecStatetImage;
    [SerializeField] private Sprite idleStateImage;

    public void Start()
    {
        //AddItem(item, false, item.data.count);
    }

    public override void AddItem(Item target, bool already, int count = 1)
    {
        if (already)
            return;

        isEmpty = false;

        item = target;
        itemIcon.sprite = item.data.sprite;
        itemName.text = item.data.itemName;
        itemDescription.text = item.data.description;
    }

    public override void Clear()
    {
        item = null;

        slotStateIcon = null;
        itemIcon = null;
        itemName.text = "";
        itemDescription.text = "";
    }
}
