using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SwitchTrigger : MonoBehaviour
{
    [Header("Interaction Object")]
    public StageMechanicsController[] targetObjects;
    public string targetObjectElement = "InteractionObject";
    public InteractionObject targetFuncScript;
    public bool activated;

    [Header("Timer")]
    public bool hasTimer = false;
    public float timerValue = 5f;
    public float delayValue = 0f;

    [Header("Material Changes")]
    public Color selfColor;
    public GameObject selfMesh; //childObj
    public GameObject selfMeshLight; //childObjLight
    public int[] selfRecoloredMaterials;
    public int[] selfRecoloredMaterialsGlow;
    public Material selfRefMaterial;
    public Material selfRefMaterialGlow;
    //private
    private MeshRenderer meshRenderer;
    private Light meshRendererLight;
    private Material[] originalMaterials;
    private Material newMaterial;
    private Material newGlowMaterial;

    void Start()
    {
        activated = false;
        meshRenderer = selfMesh.GetComponent<MeshRenderer>();

        RecolorMaterials();
        foreach (StageMechanicsController targetObject in targetObjects)
        {
            if (targetObject != null)
            {
                targetFuncScript = targetObject.GetComponent<InteractionObject>();
                //RecolorTargetObject(targetObject);
                targetObject.SetInitialColor(newMaterial, newGlowMaterial);
            }
        }
        targetFuncScript = null;
    }

    public void Activate()
    {
        AudioManager.instance.PlaySfx(AudioManager.SFX.SFX_SwitchPressed);

        if(!activated){
            OnSelfMesh();
        }
        else{
            OffSelfMesh();
        }

        // delayValue만큼 대기 후 다음 작업 수행하기
        StartCoroutine(ActivateAfterDelay());
    }

    private IEnumerator ActivateAfterDelay()
    {
        // delayValue만큼 대기
        yield return new WaitForSeconds(delayValue);

        if (!activated)
        {
            OnSwitchController();

            /*if(hasTimer)
            {
                Invoke("OffSwitchController", timerValue);
            }*/
        }
        else
        {
            OffSwitchController();
        }
    }

    private void OnSelfMesh(){
        foreach (int index in selfRecoloredMaterialsGlow)
        {
            if (index >= 0 && index < originalMaterials.Length)
            {
                originalMaterials[index] = newGlowMaterial;
            }
        }
        //originalMaterialsTarget[selfRecoloredMaterialsGlowTarget] = newGlowMaterial;
        meshRenderer.sharedMaterials = originalMaterials;
        selfMeshLight.SetActive(true);
    }

    private void OffSelfMesh(){
        foreach (int index in selfRecoloredMaterialsGlow)
        {
            if (index >= 0 && index < originalMaterials.Length)
            {
                originalMaterials[index] = newMaterial;
            }
        }
        //originalMaterialsTarget[recoloredMaterialsGlowTarget] = newMaterial;
        meshRenderer.sharedMaterials = originalMaterials;
        selfMeshLight.SetActive(false);
    }

    private void OnSwitchController()
    {
        /*
        foreach (StageMechanicsController targetObject in targetObjects)
        {
            if (targetObject != null)
            {
                //오브젝트 내 메시 오브젝트 지정
                targetFuncScript = targetObject.GetComponent<InteractionObject>();
                targetFuncScript.Trigger();
            }
        }
        */
        foreach (StageMechanicsController tObj in targetObjects) {
            if(tObj != null) {
                tObj.Trigger();
            }
        }
        
        //스위치 작동 활성화 기록
        activated = true;
    }

    private void OffSwitchController()
    {
        /* 
        if (targetObjects != null && targetObjects.Length > 0)
        {
            foreach (StageMechanicsController targetObject in targetObjects)
            {
                //오브젝트 내 메시 오브젝트 지정
                targetFuncScript = targetObject.GetComponent<InteractionObject>();
                targetFuncScript.Exit();
            }
        }
        */

        foreach (StageMechanicsController tObj in targetObjects) {
            if(tObj != null) {
                tObj.Exit();
            }
        }
        
        // Other actions when deactivated
        activated = false;
    }

    void RecolorMaterials()
    {
        if (selfMesh != null)
        {
            meshRenderer = selfMesh.GetComponent<MeshRenderer>();

            if (meshRenderer != null)
            {
                originalMaterials = meshRenderer.sharedMaterials;

                // Recolor main materials
                foreach (int index in selfRecoloredMaterials)
                {
                    if (index >= 0 && index < originalMaterials.Length)
                    {
                        newMaterial = new Material(selfRefMaterial);
                        newMaterial.color = selfColor;
                        originalMaterials[index] = newMaterial;
                    }
                }
                // Recolor glow materials
                foreach (int index in selfRecoloredMaterialsGlow)
                {
                    if (index >= 0 && index < originalMaterials.Length)
                    {
                        newGlowMaterial = new Material(selfRefMaterialGlow);
                        newGlowMaterial.color = selfColor;
                        newGlowMaterial.SetColor("_EmissionColor", selfColor); // Set emission color
                        originalMaterials[index] = newMaterial;
                    }
                }
                meshRenderer.sharedMaterials = originalMaterials;
            }
            else
            {
                Debug.LogError("MeshRenderer component not found on selfMesh.");
            }
        }
        else
        {
            Debug.LogError("selfMesh not assigned.");
        }

        meshRendererLight = selfMeshLight.GetComponent<Light>();
        meshRendererLight.color = selfColor;
        selfMeshLight.SetActive(false);
    }

}
