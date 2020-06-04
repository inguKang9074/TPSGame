using UnityEngine;
public class PlayerInput : MonoBehaviour
{
    private bool IgnoreKeyInput = false;

    public string fireButtonName = "Fire1";
    public string jumpButtonName = "Jump";
    public string moveHorizontalAxisName = "Horizontal";
    public string moveVerticalAxisName = "Vertical";
    public string reloadButtonName = "Reload";
    public string inventoryButtonName = "Inventory";
    public string startWaveButtonName = "StartWave";
    public string ShopButtonName = "Shop";

    public Vector2 MoveInput { get; private set; }
    public bool Fire { get; private set; }
    public bool Reload { get; private set; }
    public bool Jump { get; private set; }
    public bool StartWave { get; private set; }
    public bool OpenInventory { get; private set; }
    public bool OpenShop { get; private set; }

    private void Update()
    {
        // 게임 오버 상태면 입력 불가
        if (GameManager.Instance != null && GameManager.Instance.isGameover)
        {
            MoveInput = Vector2.zero;
            Fire = false;
            Reload = false;
            Jump = false;
            StartWave = false;
            OpenInventory = false;
            OpenShop = false;
            return;
        }

        if (IgnoreKeyInput) // 인벤토리가 열린 상태면 입력 불가
        {
            MoveInput = Vector2.zero;
            Fire = false;
            Reload = false;
            Jump = false;
            StartWave = false;
            OpenInventory = Input.GetButtonDown(inventoryButtonName);
            OpenShop = Input.GetButtonDown(ShopButtonName);
            return;
        }

        MoveInput = new Vector2(Input.GetAxis(moveHorizontalAxisName), Input.GetAxis(moveVerticalAxisName));
        if (MoveInput.sqrMagnitude > 1)
            MoveInput = MoveInput.normalized; // 길이가 1인 벡터로

        // 입력 감지
        Jump = Input.GetButtonDown(jumpButtonName);
        Fire = Input.GetButton(fireButtonName);
        Reload = Input.GetButtonDown(reloadButtonName);
        StartWave = Input.GetButtonDown(startWaveButtonName);
        OpenInventory = Input.GetButtonDown(inventoryButtonName);
        OpenShop = Input.GetButtonDown(ShopButtonName);
    }

    // 키입력 무시
    public void IgnoreKeyInputWhenInventory(bool active)
    {
        IgnoreKeyInput = active;
       
    }
}