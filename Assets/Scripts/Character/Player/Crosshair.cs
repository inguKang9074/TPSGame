using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    public Image aimPointReticle; // 조준위치
    public Image hitPointReticle; // 실제 맞는 위치

    public float smoothTime = 0.2f; // 조준점 지연시간
    
    private Camera screenCamera; // 실제로 총알이 맞게 되는 곳을 위치를 알기 위한 카메라
    private RectTransform crossHairRectTransform; // 총알이 실제 맞게 되는 위치

    private Vector2 currentHitPointVelocity; // 스무딩에 사용할 값의 변화량
    private Vector2 targetPoint; // worldPoin를 화면상의 위치로 변환한 위치

    private void Awake()
    {
        screenCamera = Camera.main;
        crossHairRectTransform = hitPointReticle.GetComponent<RectTransform>();
    }

    // 조준점을 활성화하거나 비활성화할 메소드
    public void SetActiveCrosshair(bool active)
    {
        hitPointReticle.enabled = active;
        aimPointReticle.enabled = active;
    }

    // worldPoint를 scrrenPoint로 변경
    public void UpdatePosition(Vector3 worldPoint)
    {
        if(screenCamera == null)
            screenCamera = Camera.main;

        targetPoint = screenCamera.WorldToScreenPoint(worldPoint);
    }

    private void Update()
    {
        if (!hitPointReticle.enabled)
            return;

        // crossHair위치를 targerPoint로 부드럽게 이동
        crossHairRectTransform.position = Vector2.SmoothDamp(crossHairRectTransform.position, targetPoint,
         ref currentHitPointVelocity, smoothTime * Time.deltaTime);
    }
}