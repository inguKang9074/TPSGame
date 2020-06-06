using UnityEngine;
using UnityEngine.AI;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    private Animator animator;

    public AudioClip itemPickupClip;
    private AudioSource playerAudioPlayer;

    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    private PlayerShooter playerShooter;
    private PlayerInput playerInput;
    private CinemachineFreeLook cinemachineFreeLook;

    private bool active = false;

    private void Start()
    {
        cinemachineFreeLook = GameObject.Find("Follow Cam").GetComponent<CinemachineFreeLook>();
        playerMovement = GetComponent<PlayerMovement>();
        playerShooter = GetComponent<PlayerShooter>();
        playerHealth = GetComponent<PlayerHealth>();
        playerAudioPlayer = GetComponent<AudioSource>();
        playerInput = GetComponent<PlayerInput>();

        playerHealth.OnDeath += HandleDeath;

        Cursor.visible = false;

        SetupInventory();
    }

    private void Update()
    {
        if (playerInput.OpenInventory)
        {
            active = !active;

            UIManager.Instance.OpenInventory(active);
            cinemachineFreeLook.enabled = !active;
            playerInput.IgnoreKeyInputWhenInventory(active);
            Cursor.visible = active;
        }
    }


    // 플레이어 사망시 실행
    private void HandleDeath()
    {
        playerHealth.enabled = false;
        playerShooter.enabled = false;

        GameManager.Instance.EndGame();

        Cursor.visible = true;
    }

    // 플레이어 리스폰
    public void Respawn()
    {
        gameObject.SetActive(false);
        transform.position = Utility.GetRandomPointOnNavMesh(transform.position, 5f, NavMesh.AllAreas);
        playerHealth.enabled = true;
        playerShooter.enabled = true;
        gameObject.SetActive(true);

        foreach (var gun in playerShooter.haveGuns)
        {
            gun.ammoRemain = 150;
        }

        UIManager.Instance.SetActiveGameoverUI(false);
        GameManager.Instance.RestartGame();
        Cursor.visible = false;
    }

    // 아이템 먹는 처리
    private void OnTriggerEnter(Collider other)
    {
        if (playerHealth.dead)
        {
            return;
        }

        var item = other.GetComponent<Item>();

        if (item != null)
        {
            item.Use(gameObject);
            playerAudioPlayer.PlayOneShot(itemPickupClip);
        }
    }

    // 인벤토리 초기화
    private void SetupInventory()
    {
        // 현재 가진 총, 총알을 인벤토리로
        foreach (var gun in playerShooter.haveGuns)
        {
            //Inventory.Instance.GetItem(gun);
            Inventory.Instance.AddItem(gun.ammoPack,
                gun.ammoRemain);
        }

        // 무기 장착 슬롯 초기화
        foreach (var slot in playerShooter.gunSlot)
        {
            Inventory.Instance.EuipWeapon(slot); // 무기 슬롯에 현재 총 장착
            Inventory.Instance.EquipAmmo(slot.ammoPack); // 무기 슬롯에 현재 탄 장착
        }
    }
}