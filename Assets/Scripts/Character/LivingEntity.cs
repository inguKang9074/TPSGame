using System;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable
{
    public float startingHealth = 100f; // 최대 체력
    public float health { get; protected set; } // 현재 체력
    public bool dead { get; protected set; } // 죽었는지 체크
    
    public event Action OnDeath; // 죽었을때 이벤트를 실행할 변수
    
    private const float minTimeBetDamaged = 0.1f; // 데미지 적용 간격
    private float lastDamagedTime; // 마지막 피격 시점

    protected bool IsInvulnerabe // 데미지를 받을수 있는 상태인지 체크
    {
        get
        {
            if (Time.time >= lastDamagedTime + minTimeBetDamaged) return false;

            return true;
        }
    }
    

    protected virtual void OnEnable()
    {
        // 생명체 상태를 리셋
        dead = false;
        health = startingHealth;
    }

    // 데미지를 받는 함수
    public virtual bool ApplyDamage(DamageMessage damageMessage)
    {
        // 데미지를 받을수없는 상태거나, 공격한 대상이 자기 자신이거나, 죽은 상태면 데미지 받는것이 불가능함
        if (IsInvulnerabe || damageMessage.damager == gameObject || dead)
            return false;

        // 마지막 피격 시점을 기억하고 데미지 적용
        lastDamagedTime = Time.time;
        health -= damageMessage.amount;
        
        // 체력이 0이하면 죽음 이벤트 발생
        if (health <= 0)
            Die();

        return true;
    }
    
    // 체력회복 함수
    public virtual void RestoreHealth(float newHealth)
    {
        if (dead)
            return;
        
        health += newHealth;
    }
    
    // 죽음 이벤트 함수
    public virtual void Die()
    {
        if (OnDeath != null)
            OnDeath();
        
        dead = true;
    }
}