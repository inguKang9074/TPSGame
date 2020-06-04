using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class SwapWeapon : MonoBehaviour
{
    private PlayerShooter playerShooter;

    // 무기 중복 교체 실행 방지
    public static bool weaponChange = false;

    // 무기교체까지 걸리는 시간
    public float weaponSwapDelayTime;

    // 무기 슬롯 관리를 위한 딕셔너리( 0 = 주무기, 1= 보조무기 )
    private Dictionary<string, Gun> weaponSlotDictionary = new Dictionary<string, Gun>();

    // 현재 선택된 총의 인덱스
    private string currentIndex;

    void Start()
    {
        playerShooter = FindObjectOfType<PlayerShooter>();

        for (int i = 0; i < playerShooter.gunSlot.Length; i++)
        {
            weaponSlotDictionary.Add(i.ToString(), playerShooter.gunSlot[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!weaponChange && playerShooter.equipGun.state != Gun.State.Reloading)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) // 주무기
            {
                string weaponIndex = "0";

                CoroutineManager.Instance.Run("SwapCoroutine", Swap(weaponIndex, weaponSwapDelayTime));
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2)) // 보조무기
            {
                string weaponIndex = "1";

                CoroutineManager.Instance.Run("SwapCoroutine", Swap(weaponIndex, weaponSwapDelayTime));
            }
        }
    }

    IEnumerator Swap(string index, float delayTime)
    {
        if (weaponSlotDictionary[index] == null)
            yield break; 

        weaponChange = true;
        playerShooter.enabled = false;
        playerShooter.equipGun = weaponSlotDictionary[index];
        playerShooter.enabled = true;

        UIManager.Instance.UpdateGunNameText(playerShooter.equipGun.data.itemName); // 이름 갱신
        UIManager.Instance.UpdateGunSprite(playerShooter.equipGun.data.sprite); // 무기 이미지 갱신

        yield return new WaitForSeconds(delayTime);

        weaponChange = false;
    }

}
