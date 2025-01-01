using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGravityControl : MonoBehaviour, IGravityControl
{

    public bool IsInRange { get; set; }

    // 인터페이스 구현 
    public float Gravity { get; set; }
    public CharacterController controller; // 컨트롤러

    public Vector3 gravityVector;

    public float savedTimeScale = 1f;
    public Vector3 forceSavedVector;
    public bool hadTouchWhenTimeStopped = false;

    public void AntiGravity() // 중력 반전 함수 
    {
        IsInRange = true;
        Gravity = 9.81f; // 반전
        ApplyGravity();
        Debug.Log(name + " has set to AntiGravity On.");
    }
    public void AntiGravityEnd()
    {
        IsInRange = false;
        Gravity = -9.81f; // 반전 해제 
        ApplyGravity();
        Debug.Log(name + " has set to AntiGravity Off.");
    }

    private void Update()
    {
        if(IsInRange){
            Gravity = 9.81f;
        }
        else{
            Gravity = -9.81f;
        }
        
        ApplyGravity();
    }

    void ApplyGravity()
    {
        if(hadTouchWhenTimeStopped && (Time.timeScale > 0.98)){ 
            // 시간 멈춤 적용 뒤 멈췄을 때 준 물리량을 그대로 가져감
            Debug.Log("Somebody touched this object when time stopped");
            hadTouchWhenTimeStopped = false;
            controller.Move(forceSavedVector * -60f * Time.deltaTime);
        }
        else{ 
            // 평상시
            // 수직 방향으로 중력을 적용.
            gravityVector = new Vector3(0f, Gravity, 0f);

            // 경박스런 움직임. Lerp으로 퇴마  
            gravityVector = Vector3.Lerp(controller.velocity, gravityVector, Time.deltaTime);

            // 중력 벡터를 현재 위치에 적용
            controller.Move(gravityVector * Time.deltaTime);
        }
    }

    public void BlackHole(Vector3 fieldCenter)
    {
        Vector3 direction = fieldCenter - transform.position;
        direction = Vector3.Normalize(direction); // 방향만 구함 
        controller.Move(direction * Time.deltaTime); // lerp 로 움직여보자! 
    }

    private void OnTriggerEnter(Collider other){
        if(other.tag == "Player"){
            if((1.02 < Time.timeScale) || (Time.timeScale < 0.98)){
                forceSavedVector = other.transform.position - transform.position - new Vector3(0f, -0.25f, 0f);
                savedTimeScale = Time.timeScale;
                hadTouchWhenTimeStopped = true;
                Debug.Log("Relative Position: " + forceSavedVector);

                // 이후 controller 에게 특정한 물리량을 제공함
                // 해당 객체 형식에서는 캐릭터 컨트롤러를 프롭의 이동 시에만 사용하며, 캐릭터 조종에 사용하지 않음
                // 물리량 벡터 값은 relativePosition 참조
                //controller.Move(-1.5f * forceSavedVector.normalized * Time.deltaTime);
            }
        }
    }

    public void GiveForceAfterNormalTimeScale() {

    }

}
