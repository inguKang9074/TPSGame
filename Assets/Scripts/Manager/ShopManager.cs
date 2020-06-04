using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class ShopManager : MonoBehaviour
{
    PlayerInput playerInput;
    private CinemachineFreeLook cinemachineFreeLook;

    private bool active = false;

    private void Start()
    {
        cinemachineFreeLook = GameObject.Find("Follow Cam").GetComponent<CinemachineFreeLook>();
        playerInput = FindObjectOfType<PlayerInput>();
    }

    public void OnTriggerStay(Collider other)
    {
        if (playerInput.OpenShop)
        {
            active = !active;

            UIManager.Instance.OpenShop(active);
            cinemachineFreeLook.enabled = !active;
            playerInput.IgnoreKeyInputWhenInventory(active);
            Cursor.visible = active;
        }
    }


}
