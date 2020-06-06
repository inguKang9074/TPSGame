using UnityEngine;

// 구조체로 선언한 이유는 클래스와 달리 구조체는 value 타입이기때문에
// DamageMessage를 전달 받은측에서 수정해도 다른 곳에 영향을 미치지 않기떄문 
public struct DamageMessage
{
    public GameObject damager; // 공격 가한측
    public float amount; // 데미지 

    public Vector3 hitPoint; // 데미지 위치
    public Vector3 hitNormal; // 공격이 가해진 방향
}