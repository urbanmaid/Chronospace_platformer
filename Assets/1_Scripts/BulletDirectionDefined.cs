using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletDirectionDefined : MonoBehaviour
{
    public float speed = 20f;
    public float spreadScale = 0.075f;
    public float lifetime = 5f;
    private float timeDilation = 1f;
    public Vector3 initialDirection;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = false; // 중력 영향을 받지 않도록 설정

        SetInitialDirectionToPlayer();
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        timeDilation = Time.timeScale;
    }

    void FixedUpdate()
    {
        MoveBullet();
    }

    void SetInitialDirectionToPlayer() //sets the direction of the bullet into player
    {
            initialDirection.Normalize();
            // 총알을 타겟 방향으로 회전
            transform.rotation = Quaternion.LookRotation(initialDirection);
    }

    void MoveBullet() //moves the bullet into player
    {
        rb.velocity = initialDirection * speed * timeDilation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Enemy") && !other.CompareTag("Bullet")){
            if(other.CompareTag("Player")){
                Debug.Log("총알이 플레이어에 닿았기 때문에 사라졌습니다");
            }
            Destroy(gameObject);
        }
    }


}