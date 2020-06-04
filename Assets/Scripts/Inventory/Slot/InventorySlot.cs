using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : Slot, IPointerClickHandler, IBeginDragHandler,IDragHandler, IEndDragHandler, IDropHandler
{ 
    // 인벤토리 관련 UI
    [SerializeField] private Image itemIcon;
    [SerializeField] private Text itemCount;

    // 아이템 설명창 UI
    [SerializeField] private Text itemName;
    [SerializeField] private Text itemDescription;
    [SerializeField] private Image descriptionIcon;

    // 슬롯에 아이템 추가
    public override void AddItem(Item target, bool alreadyHaveItem, int count = 1) 
    {
        isEmpty = false; // 슬롯에 아이템이 추가 됬으니 빈상태가 아니다.
        this.item = target;     
        itemIcon.sprite = target.data.sprite;

        // 소지중인 아이템이고 무기가 아니라면 슬롯에 갯수를 표시한다
        if (alreadyHaveItem && target.data.itemType != ItemData.ItemType.Weapon)
        {
            itemCount.text = "x" + count;
            return;
        }
        else // 소지중인 아이템이 아니고 
        {
            if (ItemData.ItemType.Use == this.item.data.itemType ||
    ItemData.ItemType.Ammo == this.item.data.itemType) // 소모품이면 갯수 나오게
            {
                itemCount.text = "x" + this.item.data.count.ToString();
            }
            else
            {
                itemCount.text = "";
            }
        }
    }
    // 슬롯 삭제
    public override void Clear()
    {
        isEmpty = true;
        item = null;
        itemCount.text = "";
        itemName.text = "";
        itemDescription.text = "";
        itemIcon.sprite = emptySlotSprite;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("BeginDrag");
        if (isEmpty)
            return;

        DragSlot.instance.dragSlot = this;
        DragSlot.instance.SetDragSlot(item);
        DragSlot.instance.transform.position = eventData.position;
    }

    // 마우스 드래그 이벤트 함수
    public void OnDrag(PointerEventData eventData)
    {
        if (isEmpty)
            return;

        DragSlot.instance.transform.position = eventData.position;
    }

    // 마우스 드랍시 이벤트 함수
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop");
        if (DragSlot.instance.dragSlot != null && DragSlot.instance.item != null)
        {
            ChangeSlot();
        }
    }

    // 마우스 드래그를 종료했을때 이벤트 함수
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag");
        DragSlot.instance.SetColor(0);
        DragSlot.instance.dragSlot = null;
        DragSlot.instance.item = null;
    }

    // 슬롯 변경 함수
    private void ChangeSlot()
    {
        bool isHaveItem;

        if (isEmpty) // 드랍 한 슬롯이 비었으면
        {
            Debug.Log("현재 드랍한 슬롯은 비었다!");
            isHaveItem = false;

            AddItem(DragSlot.instance.item, isHaveItem, DragSlot.instance.item.data.count);

            DragSlot.instance.dragSlot.Clear(); 
            
        }
        else // 비지 않았다면
        {
            Debug.Log("현재 드랍한 슬롯은 비지않았다!");
            if (item == DragSlot.instance.item) // 드래그한 아이템이 현재 슬롯 아이템과 같다면
            {
                isHaveItem = true;
                AddItem(DragSlot.instance.item, isHaveItem, DragSlot.instance.item.data.count);  
            }
            else
            {
                isHaveItem = false;
                AddItem(DragSlot.instance.item, isHaveItem, DragSlot.instance.item.data.count);
                DragSlot.instance.dragSlot.Clear();
            }
        }
    }

    // 슬롯 클릭시 이벤트 함수
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log("인벤토리 슬롯 우클릭");
            if (item == null)
                return;

            switch (item.data.itemType)
            {
                case ItemData.ItemType.Weapon:
                    Debug.Log("무기장착!");
                    Inventory.Instance.EuipWeapon(item);
                    Clear();
                    break;
                case ItemData.ItemType.Ammo:
                    Debug.Log("총알장착!");
                    AmmoPack ammo = item.GetComponent<AmmoPack>();

                    if (ammo != null)
                    {
                        Inventory.Instance.EquipAmmo(ammo);
                    }
                    break;
                case ItemData.ItemType.Use:
                    Debug.Log("소모품 사용!");
                    break;
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log("인벤토리 슬롯 좌클릭");
            if (isEmpty)
                return;

            itemName.text = item.data.itemName;
            itemDescription.text = item.data.description;
            descriptionIcon.sprite = item.data.sprite;
        }
    }

}
