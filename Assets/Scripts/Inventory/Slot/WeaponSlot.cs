using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WeaponSlot : Slot, IPointerClickHandler
{
    // 인벤토리 관련 UI
    [SerializeField] private Text itemName; // 장착한 무기 이름
    [SerializeField] private Image weaponIcon; // 장착한 무기 아이콘

    // 아이템 설명창 UI
    [SerializeField] private Text timeBetFireText; // 연사력
    [SerializeField] private Text reloadText; // 재장전 시간 
    [SerializeField] private Text damageText; // 데미지
    [SerializeField] private Image ammoSlotImage; // 총알 슬롯 이미지

    // 장착 슬롯에 아이템 장착
    public override void AddItem(Item target, bool alreadyEquipWeapon, int count = 1)
    {
        if (alreadyEquipWeapon) // 이미 장착한 무기라면
        {
            return;
        }

        isEmpty = false;

        item = target;
        Gun gun = item.GetComponent<Gun>();

        if (gun == null)
            return;

        if (item.data.itemType == ItemData.ItemType.Weapon)
        {
            weaponIcon.sprite = gun.data.sprite;
            itemName.text = gun.data.itemName;
            timeBetFireText.text = "연사력: " + gun.timeBetFire.ToString() + " sec";
            damageText.text = "데미지: " + gun.damage.ToString();
            reloadText.text = "재장전 시간: " + gun.reloadTime.ToString() + " sec";
        }
    }

    public override void Clear()
    {
        item = null;
        weaponIcon.sprite = emptyStateImage;
        ammoSlotImage.sprite = emptyStateImage;

        itemName.text = "무기명:";
        timeBetFireText.text = "연사력:";
        reloadText.text = "재장전시간:";
        damageText.text = "공격력";
    }

    // 슬롯 클릭시 이벤트 함수
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // 무기 장착 해제
            Inventory.Instance.ClearAmmoSlot(item); // 장착 총알 슬롯 지우기
            Inventory.Instance.AddItem(item); // 인벤토리로 장착중인 총 옮기기
            Clear(); // 무기 슬롯 지우기
            Debug.Log("무기장착 해제!");
        }
    }
}
