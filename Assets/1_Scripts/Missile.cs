using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 20f;
    public float inertiaFactor = 0.1f; // 관성 계수
    private Vector3 velocity; // 현재 속도


    public GameObject target;
    private Rigidbody rb;
    public GameObject fxObject;

    public bool IsInRange { get; set; }

    // 인터페이스 구현 
    public float Gravity { get; set; }

    void Start()
    {
        Debug.Log("드론 소환됨");
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = false; // 중력 영향을 받지 않도록 설정

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        Move();
    }

    void Move() //moves the missile into target
    {
        if (target != null)
        {
            // 목표 방향 계산
            Vector3 direction = (target.transform.position - transform.position).normalized;

            // 현재 속도에 관성을 적용
            velocity = Vector3.Lerp(velocity, direction * speed, inertiaFactor);
            
            // 미사일 이동
            transform.position += velocity * Time.deltaTime;

            // 미사일의 본인 바라보도록 설정하기
            transform.LookAt(target.transform, Vector3.up);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Enemy") && !other.CompareTag("Bullet")){
            if(other.CompareTag("Player")){
                Debug.Log("총알이 플레이어에 닿았기 때문에 사라졌습니다");
            }
            Debug.Log("총알이 다른 오브젝트에 닿았거나 처음부터 오브젝트와 겹치기 때문에 사라졌습니다");
            Destroy(gameObject);
        }
    }
}