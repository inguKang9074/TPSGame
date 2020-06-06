using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DragSlot : MonoBehaviour
{
    public static DragSlot instance;
    public Slot dragSlot;
    public Item item;

    [SerializeField] private Image imageItem; // 아이템 이미지.

    void Start()
    {
        instance = this;
    }
    
    public void SetDragSlot(Item target)
    {
        item = target;
        imageItem.sprite = item.data.sprite;
        SetColor(1);
    }

    public void SetColor(float _alpha)
    {
        Color color = imageItem.color;
        color.a = _alpha;
        imageItem.color = color;
    }
}
