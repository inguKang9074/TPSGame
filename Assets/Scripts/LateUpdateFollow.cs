using UnityEngine;

// 매 프레임마다 대상의 위치와 회전으로 변경
public class LateUpdateFollow : MonoBehaviour
{
    public Transform targetToFollow; 

    private void LateUpdate()
    {
        transform.position = targetToFollow.position;
        transform.rotation = targetToFollow.rotation;
    }
}