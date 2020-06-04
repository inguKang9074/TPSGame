using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    private static Inventory instance;

    public static Inventory Instance
    {
        get
        {
            return instance;
        }
    }
    // to do: Make SoundManager
    private AudioSource audioSource;
    /*public AudioClip keySound;
    public AudioClip enterSound;
    public AudioClip cancelSound;
    public AudioClip openSound;
    public AudioClip beepSound; */

    private InventorySlot[] inventorySlots; // 인벤토리 슬롯들
    private WeaponSlot[] weaponSlots; // 무기 장착 슬롯들
    private AmmoSlot[] ammoSlots; // 탄약 장착 슬롯들

    [SerializeField] private Transform invntorySlotParent; // 인벤토리의 부모 트렌스폼
    [SerializeField] private Transform weaponSlotParent; // 무기 슬롯의 부모 트렌스폼

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(transform.root.gameObject);
        inventorySlots = invntorySlotParent.GetComponentsInChildren<InventorySlot>();
        weaponSlots = weaponSlotParent.GetComponentsInChildren<WeaponSlot>();
        ammoSlots = weaponSlotParent.GetComponentsInChildren<AmmoSlot>();
    }

    // Use this for initialization
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        instance = this;
    }


    // 인벤토리 갱신하는 함수
    public void AddItem(Item target, int count = 0)
    {
        bool alreadyHaveItem = false; // 이미 아이템을 가지고 있는지 체크

        for (int i = 0; i < inventorySlots.Length; i++) // 슬롯 전체를 돌면서
        {
            if (!inventorySlots[i].isEmpty) // 현재 슬롯이 빈슬롯이 아니고
            {
                // 추가 하려는 아이템과 같은 아이템이 있다면
                if (target.data.id == inventorySlots[i].item.data.id)
                {
                    // 아이템 갯수를 증가
                    alreadyHaveItem = true;
                    inventorySlots[i].AddItem(target, alreadyHaveItem, count);
                    break;
                }
            }
            else // 현재 슬롯이 빈 슬롯이라면
            {
                // 현재 슬롯을 제외한 남은 슬롯(i+1) 중에
                for (int j = i+1 ; j < inventorySlots.Length; j++)
                {
                    // 같은 아이템이 있다면
                    if (inventorySlots[j].item != null && inventorySlots[j].item.data.id == target.data.id)
                    {
                        alreadyHaveItem = true;
                        inventorySlots[j].AddItem(target, alreadyHaveItem, count); // 갯수만 증가
                        break;
                    }
                    alreadyHaveItem = false;
                }

                if (!alreadyHaveItem)
                {
                    inventorySlots[i].AddItem(target, alreadyHaveItem, count); // 갯수만 증가
                }
                break; 
            }
        }
    }



    // 장비 착용 함수
    public void EuipWeapon(Item target)
    {
        int weaponSlotIndex;
        Gun gun = target.GetComponent<Gun>();
        
        switch (gun.gunType)
        {
            case Gun.GunType.Rifle:
                // 0번 슬롯에 들어갈 무기
                weaponSlotIndex = 0;
                IsEmptyCheckWeaponSlot(gun, weaponSlotIndex);
                break;
            case Gun.GunType.Pistol:
                // 1번 슬롯에 들어갈 무기
                weaponSlotIndex = 1;
                IsEmptyCheckWeaponSlot(gun, weaponSlotIndex);
                break;
        }
    }

    // 탄약 착용 함수
    public void EquipAmmo(AmmoPack target)
    {
        bool alreadyEquip;
       
        for (int i = 0; i < weaponSlots.Length; i++) // 무기 슬롯을 돌면서
        {
            if (weaponSlots[i].item != null) // 현재 무기 슬롯이 비지 않았고
            {
                Gun gun = weaponSlots[i].item as Gun;

                if (gun != null)
                {
                    // 현재 무기 슬롯의 총알과 add 하려는 총알이 같다면
                    if (gun.ammoPack.data.id == target.data.id)
                    {
                        alreadyEquip = true;
                        ammoSlots[i].AddItem(target, alreadyEquip, target.data.count); 
                        // 총알 갯수 추가
                        break;
                    }
                }
            }
        }
    }


    // 장비슬롯이 비어있는지 체크하는 함수
    private void IsEmptyCheckWeaponSlot(Gun target, int index)
    {
        bool alreadyEquipWeapon;
        // 장비 슬롯이 비었는지 체크
        if (weaponSlots[index].item == target) // 현재 index슬롯에 target이 있는지
        {
            alreadyEquipWeapon = true;
            weaponSlots[index].AddItem(target.ammoPack, alreadyEquipWeapon);
        }
        else
        {
            alreadyEquipWeapon = false;
            weaponSlots[index].AddItem(target, alreadyEquipWeapon);
        }
    }

    public void ClearAmmoSlot(Item target) // 총알 슬롯 비우기
    {
        AmmoPack ammo = target.GetComponent<Gun>().ammoPack;

        if (ammo == null)
            return;

        foreach (var slot in ammoSlots)
        {
            if (ammo.data.id == slot.item.data.id)
            {
                slot.Clear();
            }
        }
    }
}
