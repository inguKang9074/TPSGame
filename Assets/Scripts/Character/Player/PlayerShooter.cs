using UnityEngine;
using System.Collections.Generic;
// 플레이어 입력에 따라 총을 쏘거나 재장전
// 플레이어의 왼손 위치가 항상 총에 위치하도록 정의하는 클래스
public class PlayerShooter : MonoBehaviour
{
    public enum AimState
    {
        Idle, // 대기 상태
        HipFire // 조준없이 총을 쏘고 있는 상태
    }
    public AimState aimState { get; private set; }

    public Gun[] gunSlot; // 총 장착 슬롯 (1번 주무기, 2번 보조무기)
    public List<Gun> haveGuns; // 현재 가진 총들
    public Gun equipGun; // 현재 착용중인 총

    [SerializeField] private Transform muzzleTransform;
    public LayerMask excludeTarget; // 조준에서 제외할 LayerMask

    private PlayerInput playerInput;
    private Animator playerAnimator;
    private Camera playerCamera;

    private float waitingTimeForReleasingAim = 2.5f; // 발사 입력이 없으면 얼마뒤 대기 시간으로 바뀔지 정하는 변수
    private float lastFireInputTime; // 마지막 공격 입력 시간

    private Vector3 aimPoint; // 실제로 총알이 닿는 위치
    private bool linedUp => 
        !(Mathf.Abs( playerCamera.transform.eulerAngles.y - transform.eulerAngles.y) > 1f); // 카메라와 캐릭터 사이의 각도계산
    
    private bool hasEnoughDistance => !Physics.Linecast(transform.position + Vector3.up * muzzleTransform.position.y, muzzleTransform.position, ~excludeTarget); // 총이 겹쳐서 파묻혀 있는지 체크
    
    void Awake()
    {
        // Player가 Player를 공격할수없게 예외 처리
        if (excludeTarget != (excludeTarget | (1 << gameObject.layer)))
        {
            excludeTarget |= 1 << gameObject.layer;
        }

        gunSlot = GetComponentsInChildren<Gun>(true); // 무기 슬롯 초기화
        equipGun = gunSlot[0]; // 현재 사용중인 무기 초기화

        foreach (var gun in gunSlot) // 가지고 있는 무기 초기화
        {
            haveGuns.Add(gun);
        }
    }

    private void Start()
    {
        playerCamera = Camera.main;
        playerInput = GetComponent<PlayerInput>();
        playerAnimator = GetComponent<Animator>();

    }

    // PlayerShooter가 켜질때마다 gun을 켜주고
    // Gun을 초기화 시킴
    private void OnEnable()
    {
        UIManager.Instance.UpdateGunNameText(equipGun.data.itemName); // 이름 갱신
        UIManager.Instance.UpdateGunSprite(equipGun.data.sprite); // 무기 이미지 갱신
        aimState = AimState.Idle;

        equipGun.gameObject.SetActive(true);
        equipGun.Setup(this);
    }

    // PlayerShooter가 꺼질때마다 gun도 꺼줌
    private void OnDisable()
    {
        aimState = AimState.Idle;

        equipGun.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (playerInput.Fire)
        {
            lastFireInputTime = Time.time;
            Shoot();
        }
        else if (playerInput.Reload)
        {
            Reload();
        }
    }

    private void Update()
    {
        UpdateAimTarget();
        UpdateAimAngle();

        UpdateUI();
    }

    private void UpdateAimAngle()
    {
        var angle = playerCamera.transform.eulerAngles.x;

        if (angle > 270)
            angle -= 360f;

        angle = angle / -180f + 0.5f;

        playerAnimator.SetFloat("Angle", angle);

        if (!playerInput.Fire && Time.time >= lastFireInputTime + waitingTimeForReleasingAim)
        {
            aimState = AimState.Idle;
        }
    }

    public void Shoot()
    {
        if (aimState == AimState.Idle) // 현재 가만히 있는 상태고 Player와 카메라가 정렬이 되어있다면
        {
            if(linedUp)
                aimState = AimState.HipFire; // 발사 가능 상태로
        }
        else if (aimState == AimState.HipFire) // 발사 가능 상태이고
        {
            if (hasEnoughDistance) // 총이 벽과 파묻혀있지 않다면
            {
                if (equipGun.Fire(aimPoint)) // 발사를 시도하고 애니메이션 재생
                {
                    playerAnimator.SetTrigger("Shoot"); 
                }
            }
            else // 총이 파묻혀 있다면 대기상태로
            {
                aimState = AimState.Idle;
            }
        }
    }

    public void Reload()
    {
        if (equipGun.Reload())
        {
            UIManager.Instance.UpdateAmmoText(equipGun.magAmmo, equipGun.magCapacity, equipGun.ammoRemain); // 탄약 갱신
            playerAnimator.SetTrigger("Reload");
        }
    }

    private void UpdateAimTarget()
    {
        RaycastHit hit;

        var ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)); // 카메라 화면상의 정중앙 ray 생성

        if (Physics.Raycast(ray, out hit, equipGun.fireDistance, ~excludeTarget)) // 해당 ray에서 총돌되는 것이 있는지 체크하고
        {
            aimPoint = hit.point; // 1차 충돌위치를 저장

            // 그리고 2차적으로 player 총구의 위치에서 
            // 1차 충돌체크 지점까지 다른 물체가 있는지 체크
            if (Physics.Linecast(muzzleTransform.position, hit.point, out hit, ~excludeTarget))
            {
                aimPoint = hit.point; // 감지된 물체가 있다면 위치 수정
            }
        }
        else // 충돌한 물체가 없다면 현재 카메라 위치에서 앞쪽방향으로 총의 사거리만큼의 위치를 저장
        {
            aimPoint = playerCamera.transform.position + playerCamera.transform.forward * equipGun.fireDistance; 
        }
    }

    // 남은 탄의 UI를 갱신하고 조준점을 갱신
    private void UpdateUI()
    {
        if (equipGun == null || UIManager.Instance == null)
            return;
      
        UIManager.Instance.SetActiveCrosshair(hasEnoughDistance); // 정면에 충분한 공간이 있는지 체크하고 조준점 갱신
        UIManager.Instance.UpdateCrossHairPosition(aimPoint); // 실제 맞는 위치로 크로스헤어의 위치가 이동 
    }

    private void OnAnimatorIK(int layerIndex)
    {
        // 무기가 없거나, 재장전 중이면 IK를 갱신하지않음
        if (equipGun == null || equipGun.state == Gun.State.Reloading)
        {
            return;
        }

        // 왼손의 위치, 회전을 총에 맞게 갱신
        playerAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
        playerAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);

        playerAnimator.SetIKPosition(AvatarIKGoal.LeftHand, equipGun.leftHandMount.position);
        playerAnimator.SetIKRotation(AvatarIKGoal.LeftHand, equipGun.leftHandMount.rotation);
    }

}