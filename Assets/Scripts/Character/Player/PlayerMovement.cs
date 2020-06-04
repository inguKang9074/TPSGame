using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;
    private PlayerInput playerInput;
    private PlayerShooter playerShooter;
    private Animator animator;
    
    private Camera followCam;
    
    public float speed = 6f; // 이동속도
    public float jumpVelocity = 20f; // 점프 속도
    [Range(0.01f, 1f)] public float airControlPercent; 
    // 공중에 있는 동안 몇 %의 속도로 움직일수있는지 ( 1에 가까울수록 원래 속도 )

    public float speedSmoothTime = 0.1f; // 움직임 지연시간
    public float turnSmoothTime = 0.1f; // 회전 지연시간
    
    private float speedSmoothVelocity; // 속도 값의 속도 변화량
    private float turnSmoothVelocity; // 회전 속도 값의 변화량
    
    private float currentVelocityY; // 현재 Y방향의 속도
    
    public float currentSpeed =>
        new Vector2(characterController.velocity.x, characterController.velocity.z).magnitude;
    // 지면상의 현재 속도

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        playerShooter = GetComponent<PlayerShooter>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        followCam = Camera.main;
    }

    private void FixedUpdate()
    {
        // 총을 쏘거나 움직이는 경우라면 플레이어 캐릭터와 카메라 방향을 일치시킴
        if (currentSpeed > 0.2f || playerInput.Fire || playerShooter.aimState == PlayerShooter.AimState.HipFire)           
            Rotate();

        Move(playerInput.MoveInput);

        if (playerInput.Jump)
            Jump();
    }

    private void Update()
    {
        UpdateAnimation(playerInput.MoveInput);
    }

    public void Move(Vector2 moveInput)
    {
        var targetSpeed = speed * moveInput.magnitude;
        // 움직임 속도
        // magnitude 를 사용한 이유
        // 게임패드의 스틱을 살짝 밀었을 경우 moveInput의 벡터 길이가 1보다 작게 들어올수도 있음
        // 패드스틱을 미는 정도에 따라 속도 조절

        var moveDiection = Vector3.Normalize(transform.forward * moveInput.y + transform.right * moveInput.x);
        // 이동 방향벡터(앞 뒤 좌 우)
        // moveInput 값에 따라 방향 속도 조절
        // 방향이 1이 아닌 경우가 발생할 경우를 대비 Normalize 사용

        var smoothTime = characterController.isGrounded ? speedSmoothTime : 
            speedSmoothTime/ airControlPercent;
        // 공중에 떠있는 상태에선 조작이 잘 먹히지 않게 하기 위한 smoothTime

        targetSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, smoothTime);
        // 현재 속도, 목표값, 변화량, 지연시간

        currentVelocityY += Time.deltaTime * Physics.gravity.y; // 중력 (현재 y방향의 속도)

        var velocity = moveDiection * targetSpeed + Vector3.up * currentVelocityY; // 최종 이동 거리값

        characterController.Move(velocity * Time.fixedDeltaTime); // 이동

        if (characterController.isGrounded) // 현재 바닥에 있다면 중력값을 0으로
            currentVelocityY = 0f;
    }

    public void Rotate()
    {
        var targetRotation = followCam.transform.eulerAngles.y;

        targetRotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);
        // 스무딩 적용
        transform.eulerAngles = Vector3.up * targetRotation;
    }

    public void Jump()
    {
        if (!characterController.isGrounded)
            return;
        
        currentVelocityY = jumpVelocity;
    }

    private void UpdateAnimation(Vector2 moveInput)
    {
        var animationSpeedPercent = currentSpeed / speed;

        animator.SetFloat("Vertical Move", moveInput.y * animationSpeedPercent, 0.05f, Time.deltaTime);
        animator.SetFloat("Horizontal Move", moveInput.x * animationSpeedPercent, 0.05f, Time.deltaTime);
    }
}