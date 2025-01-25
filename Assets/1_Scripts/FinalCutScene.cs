using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // DoTween 네임스페이스 추가
using Cinemachine; // 시네머신 넴스페이스 

public class FinalCutScene : MonoBehaviour
{
    public RectTransform topBox;
    public RectTransform bottomBox;
    public float animationDuration = 1.0f; // 애니메이션 시간

    public Transform targetObject; // 카메라가 바라보는 타겟
    public Transform leftPosition; // 왼쪽 위치
    public Transform rightPosition; // 오른쪽 위치
    public Transform centerPosition; // 중간 위치
    public float moveDuration = 1.0f; // 각 이동에 걸리는 시간

    // 컷씬 시네 캠 
    public CinemachineVirtualCamera cutSceneCamera;

    void AnimateBlackBoxes()
    {
        // 화면 해상도가 변하는 상황 이 있다면? 비율로 나타내야함. 
        
        // 위 박스 이동
        topBox.DOAnchorPos(new Vector2(0, 840), animationDuration).SetEase(Ease.InOutQuad);

        // 아래 박스 이동
        bottomBox.DOAnchorPos(new Vector2(0, -840), animationDuration).SetEase(Ease.InOutQuad);
    }


    void CutSceneCameraSet()
    {
        // 컷 씬 카메라 활성화
        cutSceneCamera.Priority = 10; // 더 높은 우선순위로 설정
    }

    void StartCameraMovement()
    {
        Sequence sequence = DOTween.Sequence();

        // Step 1: Move target to the left
        sequence.Append(targetObject.DOMove(leftPosition.position, moveDuration).SetEase(Ease.InOutQuad));

        // Step 2: Move target to the right
        sequence.Append(targetObject.DOMove(rightPosition.position, moveDuration).SetEase(Ease.InOutQuad));

        // Step 3: Move target back to the center
        sequence.Append(targetObject.DOMove(centerPosition.position, moveDuration).SetEase(Ease.InOutQuad));

        sequence.OnComplete(() =>
        {
            Debug.Log("Camera movement finished.");
        });
    }

    void EndCutScene()
    {
        // 엔딩 처리 (씬 전환 또는 페이드 아웃)
        Debug.Log("Cut Scene Finished");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Player 태그가 도착하면 
        {
            // caption box가 위아래를 채움 
            AnimateBlackBoxes();

            // 캠 변경
            CutSceneCameraSet();

            // 타겟 오브젝트 이동 -> 카메라 이동 
            StartCameraMovement();

        }
    }
}
