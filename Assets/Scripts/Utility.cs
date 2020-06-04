using UnityEngine;
using UnityEngine.AI;

public static class Utility
{
    public static Vector3 GetRandomPointOnNavMesh(Vector3 center, float distance, int areaMask)
    {
        // center위치 에서 distance 만큼의 반지름내에 랜덤한 위치를 받는다. 
        var randomPos = Random.insideUnitSphere * distance + center;
        
        NavMeshHit hit;

        // areaMask 해당하는 네비메시 중에 
        // distance내에 randomPos에 가장 가까운 점을 out해줌 
        NavMesh.SamplePosition(randomPos, out hit, distance, areaMask);
        
        return hit.position;
    }
    
    public static float GetRandomNormalDistribution(float mean, float standard)
    {
        var x1 = Random.Range(0f, 1f);
        var x2 = Random.Range(0f, 1f);
        return mean + standard * (Mathf.Sqrt(-2.0f * Mathf.Log(x1)) * Mathf.Sin(2.0f * Mathf.PI * x2));
    }
}