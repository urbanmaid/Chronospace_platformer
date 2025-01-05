using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening; // dot ween 사용 

public class InteractionObject : StageMechanicsController
{
    [Header("Interaction Target")]
    public GameObject[] targetObjects;

    [Header("Target Transition")]
    public float duration = 1.0f;
    public Vector3[] transitionLoc;
    public bool transitionRotToggle;
    public Vector3[] transitionRot;
    public bool transitionScaleToggle;
    public Vector3[] transitionScale;
    private Vector3[] originalLoc;
    private Vector3[] originalRot;
    private Vector3[] originalScale;

    [Header("Material Changes")]
    public GameObject[] selfMesh;
    public GameObject selfMeshLight;
    public int[] selfRecoloredMaterials;
    public int[] selfRecoloredMaterialsGlow;

    private MeshRenderer meshRenderer; //오브젝트의 메시 렌더링 시스템을 불러올 때 사용됨
    private Light meshRendererLight;
    private Material[] originalMaterials; //오브젝트의 머티리얼 리스트를 임시로 불러올 때 사용됨
    private Material importedMaterial; //스위치 오브젝트에서 받아온 머티리얼을 저장
    private Material importedMaterialGlow; //스위치 오브젝트에서 받아온 발광 머티리얼을 저장

    //other vars are granted from switch

    public override int Idx { get; set; }

    void Start()
    {
        // Initialize the arrays
        originalLoc = new Vector3[targetObjects.Length];
        originalRot = new Vector3[targetObjects.Length];
        originalScale = new Vector3[targetObjects.Length];

        for (int i = 0; i < targetObjects.Length; i++)
        {
            if (targetObjects[i] != null)
            {
                originalLoc[i] = targetObjects[i].transform.localPosition;
                originalRot[i] = targetObjects[i].transform.localRotation.eulerAngles;
                originalScale[i] = targetObjects[i].transform.localScale;
            }
        }
    }

    // Update is called once per frame

    public override void Trigger()
    {
        //AudioManager.instance.PlaySfx(AudioManager.SFX.SFX_DoorOpen);

        //Debug.Log("Trigger");
        for (int i = 0; i < targetObjects.Length; i++)
        {
            targetObjects[i].transform.DOLocalMove(originalLoc[i] + transitionLoc[i], duration);
            targetObjects[i].transform.DOLocalRotate(originalRot[i] + transitionRot[i], duration);
            targetObjects[i].transform.DOScale(transitionScale[i], duration);
        }
        if(selfMeshLight) {
            selfMeshLight.SetActive(true);
        }
        for (int i = 0; i<selfMesh.Length; i++){
            if(selfRecoloredMaterialsGlow[i] != -1) {
                meshRenderer = selfMesh[i].GetComponent<MeshRenderer>();
                originalMaterials = meshRenderer.sharedMaterials;
                originalMaterials[selfRecoloredMaterialsGlow[i]] = importedMaterialGlow;
                meshRenderer.sharedMaterials = originalMaterials;
            }
        }
    }

    public override void Exit()
    {
        AudioManager.instance.PlaySfx(AudioManager.SFX.SFX_DoorOpen);

        for (int i = 0; i < targetObjects.Length; i++)
        {
            targetObjects[i].transform.DOLocalMove(originalLoc[i], duration);
            targetObjects[i].transform.DOLocalRotate(originalRot[i], duration);
            targetObjects[i].transform.DOScale(originalScale[i], duration);
        }

        if(selfMeshLight) selfMeshLight.SetActive(false);
        
        for (int i = 0; i<selfMesh.Length; i++){
            if(selfRecoloredMaterialsGlow[i] != -1) {
                meshRenderer = selfMesh[i].GetComponent<MeshRenderer>();
                originalMaterials = meshRenderer.sharedMaterials;
                originalMaterials[selfRecoloredMaterialsGlow[i]] = importedMaterial;
                meshRenderer.sharedMaterials = originalMaterials;
            }
        }
    }

    public override void SetInitialColor(Material targetColor, Material targetColorGlow)
    {
        importedMaterial = targetColor;
        importedMaterialGlow = targetColorGlow;
        for (int i = 0; i<selfMesh.Length; i++){
            if(selfRecoloredMaterialsGlow[i] != -1 && selfRecoloredMaterials[i] != -1) {
                meshRenderer = selfMesh[i].GetComponent<MeshRenderer>();
                originalMaterials = meshRenderer.sharedMaterials;
                originalMaterials[selfRecoloredMaterials[i]] = importedMaterial;
                originalMaterials[selfRecoloredMaterialsGlow[i]] = importedMaterial;
                meshRenderer.sharedMaterials = originalMaterials;
            }
        }

        if (selfMeshLight != null)
        {
            meshRendererLight = selfMeshLight.GetComponent<Light>();
            meshRendererLight.color = importedMaterial.color;
            selfMeshLight.SetActive(false);
        }
    }
}
