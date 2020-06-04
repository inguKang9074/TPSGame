using UnityEngine;

public class PlayerHealth : LivingEntity
{
    private Animator animator;
    private AudioSource playerAudioPlayer;

    public AudioClip deathClip;
    public AudioClip hitClip;

    private void Awake()
    {
        playerAudioPlayer = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    protected override void OnEnable()
    {      
        base.OnEnable();
        UpdateUI();
    }
    
    public override void RestoreHealth(float newHealth)
    {
        base.RestoreHealth(newHealth);
        UpdateUI();
    }

    private void UpdateUI()
    {
        UIManager.Instance.UpdateHealthText(dead ? 0f : health, startingHealth);

    }
    
    public override bool ApplyDamage(DamageMessage damageMessage)
    {
        if (!base.ApplyDamage(damageMessage))
            return false;

        // 데미지 적용하는데 성공했다면 이펙트 재생, hitSound재생, UI를 갱신함
        EffectManager.Instance.PlayHitEffect(damageMessage.hitPoint, damageMessage.hitNormal, this.transform, EffectManager.EffectType.Flesh);

        playerAudioPlayer.PlayOneShot(hitClip);

        UpdateUI();
        CoroutineManager.Instance.Run("ShowDamageScreen", UIManager.Instance.ShowDamageScreen());

        return true;
    }
    
    public override void Die()
    {
        base.Die();

        // deathSound, Die애니메이션 재생, UI 갱신
        playerAudioPlayer.PlayOneShot(deathClip);
        animator.SetTrigger("Die");

        UpdateUI();
    }
}