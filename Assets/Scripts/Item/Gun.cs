using System;
using System.Collections;
using UnityEngine;

public class Gun : Item
{
    // 무기 타입
    public enum GunType
    {
        Pistol, // 권총
        Rifle, // 소총
    }
    public GunType gunType;

    // 무기 상태
    public enum State
    {
        Ready, // 대기
        Empty, // 탄약이 없는 상태
        Reloading // 재장전
    }
    public State state { get; protected set; }

    private PlayerShooter playerShooter;
    private LineRenderer bulletLineRenderer; // 총알 궤적

    public float fireDistance = 100f; // 무기 사거리
    public float damage;// 데미지

    public AmmoPack ammoPack; // 현재 가진 총알
    public int ammoRemain; // 현재 가방에 보유한 모든 탄약
    public int magCapacity = 30; // 현재 총의 전체 탄창수
    public int magAmmo; // 현재 무기의 남은 총알의 수

    protected float lastFireTime; // 마지막 발사 시점
    public float timeBetFire = 0.12f; // 연사력
    public float reloadTime = 1.8f; // 재장전시간
    protected float currentSpread; // 현재 탄퍼짐의 정도값
    protected float currentSpreadVelocity; // 현재 탄퍼짐 변화량

    [Range(0f, 10f)] public float maxSpread = 3f; // 탄착군 최대 범위(반동 최대범위)
    [Range(1f, 10f)] public float stability = 1f; // 총기의 안정성 (반동) (높을수록 반동증가)
    [Range(0.01f, 3f)] public float restoreFromRecoilSpeed = 2f; // 탄착군이 좁아지는데까지 걸리는 속도 (높을수록 안정성 증가)


    // Audio 관련 변수
    private AudioSource gunAudioPlayer;
    public AudioClip shotClip;
    public AudioClip reloadClip;

    // 이펙트
    public ParticleSystem muzzleFlashEffect;
    public ParticleSystem shellEjectEffect;

    private LayerMask targetLayer;

    // 총구 위치와 왼손 위치
    [SerializeField] private Transform fireTransform;
    public Transform leftHandMount;

    private void Awake()
    {
        ammoRemain = ammoPack.data.count;

        magAmmo = magCapacity;
        gunAudioPlayer = GetComponent<AudioSource>();
        bulletLineRenderer = GetComponent<LineRenderer>();

        bulletLineRenderer.positionCount = 2;
        bulletLineRenderer.enabled = false;
    }

    public void Setup(PlayerShooter holder) // 총 초기화    
    {
        // 총 소유자가 누구인지, 총알을 맞아선 안되는 대상들 설정
        playerShooter = holder;
        targetLayer = holder.excludeTarget;
    }

    private void OnEnable()
    {
        UIManager.Instance.UpdateAmmoText(magAmmo, magCapacity, ammoRemain); // 탄약 갱신
        currentSpread = 0f;
        lastFireTime = 0f;
        state = State.Ready;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public bool Fire(Vector3 aimTarget)
    {
        // 현재 총이 준비상태이고 
        // 현재 시간이 마지막 발사시점+ 발사간격을 더한 시간보다 크거나 같으면 발사 가능
        if (state == State.Ready && Time.time >= lastFireTime + timeBetFire && magAmmo > 0)
        {
            var fireDirection = aimTarget - fireTransform.position; // 목표지점 - 현재 총의 위치 = 목표까지 방향과 거리

            // 표준 편차 랜덤을 통한 랜덤 탄착군 형성(1번째 파라미터 = 기준값, 2번째 파라미터= 기준값과의 차이 확률)
            var precisionX = Utility.GetRandomNormalDistribution(0, currentSpread);
            var precisionY = Utility.GetRandomNormalDistribution(0, currentSpread);

            // 탄착군 정밀도
            fireDirection = Quaternion.AngleAxis(precisionY, Vector3.up) * fireDirection;
            fireDirection = Quaternion.AngleAxis(precisionX, Vector3.right) * fireDirection;

            currentSpread += 1f / stability; // 반동 증가

            lastFireTime = Time.time; // 발사시간 초기화
            Shot(fireTransform.position, fireDirection);
            UIManager.Instance.UpdateAmmoText(magAmmo, magCapacity, ammoRemain);

            return true;
        }

        return false;
    }

    private void Shot(Vector3 startPoint, Vector3 direction)
    {
        RaycastHit hit;
        Vector3 hitPosition;

        // 탄 적중 체크와 데미지 적용
        if (Physics.Raycast(startPoint, direction, out hit, fireDistance, ~targetLayer))
        {
            var target = hit.collider.GetComponent<IDamageable>();
            if (target != null)
            {
                DamageMessage damageMessage;

                damageMessage.damager = playerShooter.gameObject;
                damageMessage.amount = damage;
                damageMessage.hitPoint = hit.point;
                damageMessage.hitNormal = hit.normal;

                target.ApplyDamage(damageMessage);
            }
            else
            {
                EffectManager.Instance.PlayHitEffect(hit.point, hit.normal, hit.transform);
            }

            hitPosition = hit.point;
        }
        else
        {
            // 만약 fireDistance 만큼 날아갔는데 충돌한 대상이 없다면
            // 현재위치에서 direction방향으로 fireDistance만큼 간 거리를 hitPosition으로
            hitPosition = startPoint + direction * fireDistance;
        }

        // 이펙트 재생
        CoroutineManager.Instance.Run("ShotEffect", ShotEffect(hitPosition));

        // 탄창의 총알 수를 감소 시키고 탄이 없다면 빈 상태로 변경
        magAmmo--;

        if (magAmmo >= 0)
            state = State.Empty;

        state = State.Ready;
    }

    private IEnumerator ShotEffect(Vector3 hitPosition)
    {
        muzzleFlashEffect.Play();
        shellEjectEffect.Play();

        gunAudioPlayer.PlayOneShot(shotClip); // 속도 중첩을 위해 PlayOnShot 메소드 사용

        // 라인렌더러 초기화
        bulletLineRenderer.enabled = true;
        bulletLineRenderer.SetPosition(0, fireTransform.position); // 시작점 (총의 시작점)
        bulletLineRenderer.SetPosition(1, hitPosition); // 끝점 (맞은 위치)

        yield return new WaitForSeconds(0.03f);

        bulletLineRenderer.enabled = false;
    }

    public bool Reload()
    {
        // 현재 총의 상태가 재장전중이거나, 총 남은 탄약의 수가 없거나, 이미 탄창에 탄약이 가득한 경우)
        if (state == State.Reloading || ammoRemain <= 0 || magAmmo >= magCapacity)
        {
            return false;
        }

        // 재장전 코루틴
        CoroutineManager.Instance.Run("ReloadRoutine", ReloadRoutine());

        // 재장전 UI
        UIManager.Instance.ShowReloadUI(reloadTime);
        return true;
    }

    private IEnumerator ReloadRoutine()
    {
        state = State.Reloading;
        gunAudioPlayer.PlayOneShot(reloadClip);

        yield return new WaitForSeconds(reloadTime);

        var ammoToFill = Mathf.Clamp(magCapacity - magAmmo, 0, ammoRemain); // 현재 총의 탄창에 들어갈수 있는 탄약의 수
                                                                            // 채워야 하는 탄의 수를 보유한 총 탄의 수로 짜름
                                                                            // 총알을 초기화 하고 상태를 발사 대기 상태로
        magAmmo += ammoToFill;
        ammoRemain -= ammoToFill;
        UIManager.Instance.UpdateAmmoText(magAmmo, magCapacity, ammoRemain); // 탄약 갱신

        state = State.Ready;
    }

    private void Update()
    {
        // 매 프레임마다 반동을 줄임
        currentSpread = Mathf.Clamp(currentSpread, 0f, maxSpread);
        currentSpread
            = Mathf.SmoothDamp(currentSpread, 0f, ref currentSpreadVelocity, 1f / restoreFromRecoilSpeed);
        // 현재 값, 목표 값,현재 값의 변화량, 지연시간

    }

    public override void Use(GameObject target)
    {
        // 아이템 장착   
    }
}