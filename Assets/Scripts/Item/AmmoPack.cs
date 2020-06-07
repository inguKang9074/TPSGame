using UnityEngine;
using UnityEngine.UI;

public class AmmoPack : Item
{
    public Text itemNameText;

    private void Start()
    {
        itemNameText.text = data.itemName;
    }

    // 아이템을 먹었을때
    public override void Use(GameObject target)
    {
        var playerShooter = target.GetComponent<PlayerShooter>();
        var haveGuns = playerShooter.haveGuns;
        var equipGun = playerShooter.equipGun;

        // 총을 쏠수있는 상태인지, 총을 가지고 있는 상태인지 체크
        if (playerShooter != null && equipGun != null && haveGuns != null)
        {
            if (equipGun.ammoPack.data.id == data.id) // 장착중인 총과 먹은 총알이 같다면 총알 증가
            {
                equipGun.ammoRemain += data.count;

                Inventory.Instance.AddItem(equipGun.ammoPack, equipGun.ammoRemain); // 인벤토리에 총알 추가
                Inventory.Instance.EquipAmmo(equipGun.ammoPack);// 총알 장비 슬롯에 추가
            }
            else // 같지 않다면
            {
                foreach (var gun in haveGuns) // 보유한 총중에
                {
                    if (gun.ammoPack.data.id == data.id) // 맞는 총알이 있다면
                    {
                        gun.ammoRemain += data.count;

                        Inventory.Instance.AddItem(gun.ammoPack, gun.ammoRemain); 

                        Inventory.Instance.EquipAmmo(gun.ammoPack);
                        break;
                    }
                }
            }
        }

        UIManager.Instance.UpdateAmmoText(equipGun.magAmmo, equipGun.magCapacity, equipGun.ammoRemain);

        Destroy(this.gameObject);
    }

}