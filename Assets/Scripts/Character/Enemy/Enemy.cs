using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Enemy : LivingEntity
{
    private enum State
    {
        Patrol, // 정찰
        Tracking, // 추적
        AttackBegin, // 공격을 시도하는 상태
        Attacking // 공격하는 상태 ( 실제 데미지 적용 )
    }
    private State state;

    private NavMeshAgent agent;
    private Animator animator;

    public Transform attackRoot; // 공격 범위 시작 점
    public Transform eyeTransform; // 시야 기준점

    private AudioSource audioPlayer;
    public AudioClip hitClip;
    public AudioClip deathClip;

    public float runSpeed = 6f; // 이동 속도
    [Range(0.01f, 2f)] public float turnSmoothTime = 0.1f; // 회전 지연시간
    private float turnSmoothVelocity; // 현재 실시간 회전 변화량

    public float damage = 30f; // 공격력
    public float attackRadius = 2f; // 공격반경
    private float attackDistance; // 공격 거리

    public float fieldOfView = 50f; // 시야각
    public float viewDistance = 15f; // 시야 감지 거리
    public float patrolSpeed = 3f; // 정찰 속도

    public int dropGold// 드랍 골드량
    {
        private set { gold = value; }
        get { return gold; }
    }
    [SerializeField] private int gold;


    [HideInInspector] public LivingEntity targetEntity; // 공격 대상
    public LayerMask whatIsTarget; // 적을 감지할때 사용할 필터

    private RaycastHit[] hits = new RaycastHit[10]; // 범위기반 공격을 위한 배열
    private List<LivingEntity> lastAttackedTargets = new List<LivingEntity>(); // 마지막 공격 대상(공격이 2번이상 똑같은 대상에게 적용되지 않기 위함)

    private bool hasTarget => targetEntity != null && !targetEntity.dead;
    // 추적할 대상이 존재하는지 알려주는 프로퍼티

#if UNITY_EDITOR
    // 공격 범위를 표시하기 위한 기즈모
    private void OnDrawGizmosSelected()
    {
        if (attackRoot != null)
        {
            Gizmos.color = new Color(1f, 0f, 1f);
            Gizmos.DrawSphere(attackRoot.position, attackRadius);
        }

        if (eyeTransform != null)
        {
            var leftEyeRotation = Quaternion.AngleAxis(-fieldOfView * 0.5f, Vector3.up);
            var leftRayDirection = leftEyeRotation * transform.forward;

            Handles.color = new Color(1f, 1f, 1f, 0.2f);
            Handles.DrawSolidArc(eyeTransform.position, Vector3.up, leftRayDirection, fieldOfView, viewDistance);
        }
    }

#endif

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioPlayer = GetComponent<AudioSource>();

        attackDistance = Vector3.Distance(transform.position,
                             new Vector3(attackRoot.position.x, transform.position.y, attackRoot.position.z)) +
                         attackRadius;

        agent.stoppingDistance = attackDistance;
        agent.speed = patrolSpeed;
    }

    // enemy 스펙 초기화
    public void Setup(float health, float damage,
        float runSpeed, float patrolSpeed)
    {                                          
        this.startingHealth = health;     
        this.health = health;
        this.damage = damage;
        this.runSpeed = runSpeed;
        this.patrolSpeed = patrolSpeed;

        agent.speed = patrolSpeed;
    }

    private void Start()
    {
        StartCoroutine(UpdatePath());
    }

    private void Update()
    {
        if (dead)
        {
            return;
        }

        if (state == State.Tracking &&
            Vector3.Distance(targetEntity.transform.position, transform.position) <= attackDistance)
        {
            BeginAttack();
        }

        // agent.desiredVelocity : 실제이동하려는 속도가 아니라 
        //이동하려고 하는 속도
        animator.SetFloat("Speed", agent.desiredVelocity.magnitude);
    }

    private void FixedUpdate()
    {
        if (dead)
            return;

        // 공격 하려는 대상을 부드럽게 바라본다.
        if (state == State.AttackBegin || state == State.Attacking)
        {
            // Enemy의 방향을 추적대상으로 변경
            var lookRotation = Quaternion.LookRotation(targetEntity.transform.position - transform.position);

            var targetAngleY = lookRotation.eulerAngles.y;

            // 회전값을 부드럽게 변환
            targetAngleY = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngleY, ref turnSmoothVelocity, turnSmoothTime);

            // 회전 적용
            transform.eulerAngles = Vector3.up * targetAngleY;
        }

        if (state == State.Attacking)
        {
            // 공격의 방향
            var direction = transform.forward;

            // Time.fixedDeltaTime 동안 이동하는 거리를 계산
            var deltaDistance = agent.velocity.magnitude * Time.fixedDeltaTime;

            // 감지된 콜라이더의 갯수를 반환
            var size = Physics.SphereCastNonAlloc(attackRoot.position, attackRadius, direction, hits, deltaDistance, whatIsTarget);

            for (int i = 0; i < size; i++)
            {
                var attackTargetEntity = hits[i].collider.GetComponent<LivingEntity>();

                if (attackTargetEntity != null && !lastAttackedTargets.Contains(attackTargetEntity))
                {
                    var message = new DamageMessage();
                    message.damager = this.gameObject;
                    message.amount = damage;

                    // 휘두르자마자 겹쳐진 콜라이더가 있어서
                    // hits.distance가 0가 나오는 경우는 부딧힌 위치를 그냥 공격 시작 지점으로 설정한다.
                    if (hits[i].distance <= 0f)
                        message.hitPoint = attackRoot.position;
                    else // 휘두르는 도중에 상대방 콜라이더 감지됬다면
                    {
                        message.hitPoint = hits[i].point;
                    }

                    message.hitNormal = hits[i].normal;

                    attackTargetEntity.ApplyDamage(message);
                    lastAttackedTargets.Add(attackTargetEntity);
                    break;
                }
            }
        }
    }

    // 0.2초 마다 주기적으로 타겟이 있는지 체크하고 갱신하는 코루틴
    private IEnumerator UpdatePath()
    {
        while (!dead)
        {
            if (hasTarget)
            {
                if (state == State.Patrol)
                {
                    state = State.Tracking;
                    agent.speed = runSpeed;
                }
                // 목표지점 설정
                agent.SetDestination(targetEntity.transform.position);
            }
            else
            {
                if (targetEntity != null)
                    targetEntity = null;

                if (state != State.Patrol)
                {
                    state = State.Patrol;
                    agent.speed = patrolSpeed;
                }

                // enemy가 목표지점에 도착하면
                if (agent.remainingDistance <= attackDistance)
                {
                    // 랜덤한 정찰 위치 생성
                    var patrolTargetPosition = Utility.GetRandomPointOnNavMesh(transform.position, 20f, NavMesh.AllAreas);

                    agent.SetDestination(patrolTargetPosition);
                }

                // eyeTransform위치에서 viewDistance내에 
                //whatIsTarget 인 모든 콜라이더를 가져옴
                
                var colliders = Physics.OverlapSphere(eyeTransform.position, viewDistance, whatIsTarget);
                // 탐색한 콜라이더들을 돌면서 
                foreach (var collider in colliders)
                {
                    // 시야 내에 없으면 건너뛰고
                    if (!IsTargetOnSight(collider.transform))
                    {
                        continue;
                    }

                    // 시야 내에 존재하면 생명체인지 확인하고 루프를 종료시키고
                    //타겟을 설정한다.
                    var livingEntity = collider.GetComponent<LivingEntity>();
                    if (livingEntity != null && !livingEntity.dead)
                    {
                        Debug.Log("Target 찾음!");
                        targetEntity = livingEntity;
                        break;
                    }
                }
            }

            yield return new WaitForSeconds(0.05f);
        }
    }

    public override bool ApplyDamage(DamageMessage damageMessage)
    {
        if (!base.ApplyDamage(damageMessage))
            return false;

        // 공격을 받았는데 현재 공격중인 대상이 없다면
        if (targetEntity == null)
        {
            // 자신을 공격한 대상을 공격 타겟을 변경
            targetEntity = damageMessage.damager.GetComponent<LivingEntity>();
        }

        EffectManager.Instance.PlayHitEffect(damageMessage.hitPoint, damageMessage.hitNormal, this.transform, EffectManager.EffectType.Flesh);

        audioPlayer.PlayOneShot(hitClip);

        return true;
    }

    // 공격을 시작하는 시점
    // 애니메이션을 시작하고 추적을 중단
    public void BeginAttack()
    {
        state = State.AttackBegin;

        agent.isStopped = true;
        animator.SetTrigger("Attack");
    }

    // 공격이 들어가는 시점
    // 마지막 공격 대상을 지움
    public void EnableAttack()
    {
        state = State.Attacking;

        lastAttackedTargets.Clear();
    }

    // 공격이 종료되는 시점
    // 추적 상태로 변경
    public void DisableAttack()
    {
        if (dead)
            return;

        // 공격이 종료됬을떄 공격할 대상이 있다면 추적상태로
        if (hasTarget)
        {
            state = State.Tracking;
        }
        // 공격 대상이 없다면 정찰 상태로
        else
        {
            state = State.Patrol;
        }
        agent.isStopped = false;
    }

    private bool IsTargetOnSight(Transform target)
    {
        // 눈의 위치에서 타겟까지의 거리를 계산
        var direction = target.position - eyeTransform.position;
        direction.y = eyeTransform.forward.y;

        // 시야에서 벗어났다면
        if (Vector3.Angle(direction, eyeTransform.forward) > fieldOfView * 0.5f)
        {
            return false;
        }


        // 장애물에 가려졌을경우를 대비
        // 눈의 위치에서 타겟까지의 거리로 Ray를 쏴서 타겟을 체크
        direction = target.position - eyeTransform.position;

        RaycastHit hit;

        if (Physics.Raycast(eyeTransform.position + Vector3.up, direction, out hit, viewDistance))
        {
            // 부딧힌 대상이 타겟이 맞다면 true 반환
            if (hit.transform == target)
            {
                return true;
            }
        }

        return false;
    }

    public override void Die()
    {
        base.Die();

        GetComponent<Collider>().enabled = false;

        // angent를 isStopped가 아닌 enabled 해준 이유는
        // 완전히 비활성화 해주지 않으면 시체를 피해 경로를 계산하기 때문
        agent.enabled = false;

        animator.applyRootMotion = true;
        animator.SetTrigger("Die");

        audioPlayer.PlayOneShot(deathClip);     
    }
}