using UnityEngine;

public class HealthPack : MonoBehaviour, IItem
{
    public float health = 50;

    public void Use(GameObject target)
    {
        var lvingEntity = target.GetComponent<LivingEntity>();

        // 플레이어가 죽지않았다면
        if(lvingEntity != null && lvingEntity.dead == false)
        {
            // 체력을 회복하고 오브젝트를 파괴
            lvingEntity.RestoreHealth(health);
            Destroy(this.gameObject);
        }
    }
}