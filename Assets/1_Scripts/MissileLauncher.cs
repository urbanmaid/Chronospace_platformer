using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileLauncher : MonoBehaviour
{
    public float delay = 10f; // 미사일 소환 지연 시간
    public GameObject missileObj; // 미사일 프리팹
    public GameObject missileSpawnPosObj; // 미사일 소환 위치
    public GameObject missileTarget;
    public float elapsedTime = 0f;
    public Vector3 spawningOffset = new Vector3(0f, 0.5f, 0f);
    public bool isPlayerInZone;

    void Start()
    {
        // 초기화 작업이 필요하면 여기에 추가
    }

    void Update()
    {
        // 플레이어가 영역에 있을 때만 미사일 발사
        if (isPlayerInZone)
        {
            elapsedTime += Time.deltaTime; // 경과 시간 업데이트

            if (elapsedTime >= delay)
            {
                // 지연 시간이 경과했으면 미사일 발사
                FireMissile();
                elapsedTime = 0f; // 경과 시간 초기화
            }
        }
        else{
            elapsedTime = 0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy") && !other.CompareTag("Bullet"))
        {
            if (other.CompareTag("Player"))
            {
                missileTarget = other.gameObject;
                if(elapsedTime == 0f && missileTarget){
                    FireMissile();
                }
                // 플레이어가 영역에 들어올 경우
                isPlayerInZone = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 플레이어가 영역을 나갈 경우
            isPlayerInZone = false;
        }
    }

    private void FireMissile()
    {
        // 미사일 소환
        GameObject missile = Instantiate(missileObj, missileSpawnPosObj.transform.position + spawningOffset, missileSpawnPosObj.transform.rotation);
        
        // 미사일의 Missile 컴포넌트를 가져와서 타겟을 설정
        Missile missileComponent = missile.GetComponent<Missile>();
        if (missileComponent != null && missileTarget)
        {
            // 플레이어를 타겟으로 지정
            missileComponent.target = missileTarget;
        }
    }
}
