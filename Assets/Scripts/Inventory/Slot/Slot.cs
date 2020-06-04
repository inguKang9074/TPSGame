using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public abstract class Slot : MonoBehaviour
{
    public Item item;
    public bool isEmpty = true;

    [SerializeField] protected Sprite emptySlotSprite; // 빈슬롯 이미지

    // 슬롯 채우기
    public abstract void AddItem(Item target, bool alreadyEquipWeapon, int count = 1);

    // 슬롯 비우기
    public abstract void Clear();

}
