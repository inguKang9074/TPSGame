using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using UnityEngine.UI;
using System;

public class ShopManager : MonoBehaviour
{
    PlayerInput playerInput;
    private CinemachineFreeLook cinemachineFreeLook;

    private bool active = false;

    [SerializeField] private ShopSlot[] shopSlots; // 상점 슬롯들
    [SerializeField] private Transform shopSlotParent;
    [SerializeField] private Transform tabSlotParent;

    [SerializeField] private Item[] menu; // 판매 목록
    [SerializeField] private List<Item> currentTabItemList; // 현재 탭 아이템 목록
    [SerializeField] private Image[] tabs; // Tab 이미지

    // tab 상태에 따른 Sprite
    [SerializeField] private Sprite idleTab;
    [SerializeField] private Sprite selectTab;

    private void Start()
    {
        cinemachineFreeLook = GameObject.Find("Follow Cam").GetComponent<CinemachineFreeLook>();
        playerInput = FindObjectOfType<PlayerInput>();

        shopSlots = shopSlotParent.GetComponentsInChildren<ShopSlot>();
        tabs = tabSlotParent.GetComponentsInChildren<Image>();

        AddMenu();
    }

    // 상점에 메뉴 추가
    public void AddMenu()
    {
        for (int i = 0; i < shopSlots.Length; i++)
        {
            shopSlots[i].AddItem(menu[i], false, menu[i].data.count);
        }
    }

    // 상점 열기
    public void OnTriggerStay(Collider other)
    {
        if (playerInput.OpenShop)
        {
            active = !active;

            UIManager.Instance.OpenShop(active);
            cinemachineFreeLook.enabled = !active;
            playerInput.IgnoreKeyInputWhenInventory(active);
            Cursor.visible = active;
        }
    }

    // 현재 탭에 따라 아이템 표시
    public void TabClickShowMenu(string type)
    {
        ClearSlot();
        // 현재 탭 상태가 전체 라면
        if (type == "All")
        {
            foreach (var item in menu)
            {
                currentTabItemList.Add(item);
            }

            UpdateShopSlot();   
            return;
        }

        // 현재 탭에 맞는 아이템의 정보를 슬롯에 보여줌
        foreach (var item in menu)
        {
            if (item.data.itemType ==
                (ItemData.ItemType)Enum.Parse(typeof(ItemData.ItemType), type))
            {
                currentTabItemList.Add(item);
            }
        }
        UpdateShopSlot();
    }

    // 현재 탭 목록에 맞는 아이템 정보를 보여줌
    private void UpdateShopSlot()
    {
        // 슬롯 끄고 켜기
        for (int i = 0; i < shopSlots.Length; i++)
        {
            shopSlots[i].gameObject.SetActive(i < currentTabItemList.Count);
        }

        // 슬롯 정보 초기화
        for (int i = 0; i < currentTabItemList.Count; i++)
        {
            shopSlots[i].itemIcon.sprite = currentTabItemList[i].data.sprite;
            shopSlots[i].itemName.text = currentTabItemList[i].data.itemName;
            shopSlots[i].itemDescription.text = currentTabItemList[i].data.description;
        }
    }

    private void ClearSlot()
    {
        currentTabItemList.Clear();
    }
}
