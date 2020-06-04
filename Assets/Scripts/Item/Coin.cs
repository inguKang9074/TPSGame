using UnityEngine;

public class Coin : Item
{
    public int amout = 200;

    public override void Use(GameObject target)
    {
        Debug.Log("°ñµå È¹µæ");
    }
}