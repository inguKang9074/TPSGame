using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "itemData", menuName = "itemFile")]
public class ItemData : ScriptableObject
{
    public string itemName; // 아이템 이름
    public string id; // 아이템의 고유번호
    public string description;  // 아이템의 설명
    public int count; // 아이템 갯수
    public Sprite sprite;  // 아이템의 아이콘

    public ItemType itemType;
    public enum ItemType            // 아이템의 타입
    {
        Weapon,                     // 무기류
        Use,                //  소모품
        Quest,             // 퀘스트 아이템류
        Ammo // 총알류 
    }
}

public abstract class Item : MonoBehaviour
{
    public ItemData data; 

    public abstract void Use(GameObject target);
}
