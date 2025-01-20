using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using Cinemachine;

public class Player : MonoBehaviour, IGravityControl
{
    #region Variables
    [Header("Component")] //플레이어 외부 컴포넌트 변수
    public Animator anim;
    public CharacterController controller; // 이건  IGravityControl 에 있음 
    public GameObject windKey; // 내 태엽
    public GameObject[] gravityPrefebs;  // 던질 중력반전, // 던질 중력장
    public float duration = 6f;
    public float windupDuration = 120f;  
    public float timeScaleMultiplier = 0.005f; // 시간 계수. 거의 멈춤 
    public Transform itemPointTransform; // 탬 생성 위치
    public PlayerTimer timer;
    public Vector3 respawnPosition; // 리스폰 위치
    public Vector3 respawnPositionOrignal; // 리스폰 위치 

    [Header("UI")] //플레이어 외부 컴포넌트 변수
    public CamController camController; // CamController 참조
    public GameObject damageFX;

    [Header("PhysicsValue")] //플레이어 물리 효과 컨트롤 변수
    public float jumpForce = 8f;
    public float movSpeed = 5f;
    public float rotSpeed = 300f;
    public float pushPower = 0.03f;

    [Header("InputValue")] //플레이어 이동 사용 변수
    Vector3 moveDirection;
    private float inputV;
    private float inputH;
    private float rotateX;
    private bool inputWalk;
    private bool inputJump;
    private float directionInput;

    [Header("Item")] //플레이어 아이템 사용 변수
    private bool inputInteraction; // interaction키 down. 아이템 줍는 입력 e 
    private bool inputKeyButton1; // 템 스왑 1
    private bool inputKeyButton2; // 템 스왑 2
    private bool inputKeyButton3; // 템 스왑 3
    private bool inputKeyF; // 탬사용

    [Header("Sound")]
    public float footstepInterval = 0.2f; // 발소리 재생 간격
    private float footstepTimer = 0f;     // 발소리 타이머

    bool isSwaping = false; //사용하고 있는 변수. Console에 사용하지 않는다고 뜬다. 

    //플레이어 애니메이션 작동 변수
    //character status
    bool isJumping = false;
    bool isDodging = false;
    bool isWinding = false;

    //플레이어 상태 변수
    public bool isAlive = true;
    public bool isBlackHoling; // 블랙홀에 잡혀있는 중
    public bool isInShield = false;

    //주변 아이템 변수
    [SerializeField]
    public GameObject nearObject;
    public GameObject equipItem; // 현재 손에 들고있는 아이템 
    public int equipItemIndex = -1; // 현재 선택된 템 번호 
    
    public float shieldDuration = 12f;

    // 아이템 습득 UI 관련  
    public TextMeshProUGUI interactionText; // interaction 안내 UI 
    public GameObject[] equipItems; // 손에 드는 아이템들 
    public bool[] hasItems; // 아이템 가졌는지
    public Item.Type[] inventory; // 가진 아이템 목록

    private Vector3 blackholeVector = Vector3.zero; // 블랙홀 힘 저장 

    #endregion

    public bool IsInRange { get; set; }

    public float Gravity { get; set; }

    // 플레이어를 싱글톤으로??
    #region SingleTon Pattern
    public static Player Instance { get; private set; }
    private void Awake()
    {
        // If an instance already exists and it's not this one, destroy this one
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        // Set this as the instance and ensure it persists across scenes
        Instance = this;

        respawnPositionOrignal = respawnPosition;
        DontDestroyOnLoad(this.gameObject);
    }

    #endregion

    private void Start()
    {
        camController = GameObject.FindWithTag("MainCamera").transform.GetComponent<CamController>();
        inventory = new Item.Type[] { Item.Type.Null, Item.Type.Null, Item.Type.Null }; // 인벤토리 용량이 3
        AntiGravityEnd();
    }

    public void AntiGravity() // 중력 반전 함수 
    {
        IsInRange = true;
        Gravity = 9.81f;
        //Invoke("AntiGravity_End", 3f); // 3초뒤 해제 
        Debug.Log("AntiGravity On.");
    }
    public void AntiGravityEnd() //중력 반전 종료 함수
    {
        IsInRange = false;
        Gravity = -9.81f; // 반전 해제 
        Debug.Log("AntiGravity Off.");
    }
    /// </summary>


    void Update() //플레이어 상태 관리 함수
    {
        GetInput();
        if (!isAlive) return;
        if (isWinding) return; //플레이어가 살아있지 않거나, 플레이어가 윈드 업 중일 때에는 동작 불가능

        SetDir();
        if (inputInteraction) Interaction(); //interaction item이 주변에 있을 때 상호작용 활성화
        if (inputKeyButton1 || inputKeyButton2 || inputKeyButton3) Swap(); //input key 버튼으로 아이템 선택 활성화
        if (inputKeyF)
        {
            UseItem();
        }
        //Debug.Log("R"+inputKeyR); Debug.Log("F" + inputKeyF); //디버깅
    }
    void FixedUpdate() //플레이어 행동 관리 함수
    {
        if (!isAlive) return;
        if (isWinding) return; //플레이어가 살아있지 않거나, 플레이어가 윈드 업 중일 때에는 동작 불가능

        Rotate();
        Move();

    }
    void GetInput() //인풋 받는 함수
    {
        inputH = Input.GetAxisRaw("Horizontal");
        inputV = Input.GetAxisRaw("Vertical");
        rotateX = Input.GetAxis("Mouse X");
        inputWalk = Input.GetButton("Walk"); // -> Dash
        inputJump = Input.GetButton("Jump");

        inputInteraction = Input.GetButtonDown("Interaction"); //e
        inputKeyButton1 = Input.GetButtonDown("Swap1");
        inputKeyButton2 = Input.GetButtonDown("Swap2");
        inputKeyButton3 = Input.GetButtonDown("Swap3");
        inputKeyF = Input.GetButtonDown("Effect2"); //f
    }

    void SetDir() //이동 함수
    {
        float targetDirectionInput = 0f;

        if (inputV > 0.1 && inputH == 0) targetDirectionInput = 0f;
        else if (inputV > 0.1 && inputH < 0) targetDirectionInput = 0.125f;
        else if (inputV == 0 && inputH < 0) targetDirectionInput = 0.25f;
        else if (inputV < -0.1 && inputH > 0) targetDirectionInput = 0.375f;
        else if (inputV < -0.1 && inputH == 0) targetDirectionInput = 0.5f;
        else if (inputV < -0.1 && inputH < 0) targetDirectionInput = 0.625f;
        else if (inputV == 0 && inputH > 0) targetDirectionInput = 0.75f;
        else if (inputV > 0.1 && inputH > 0) targetDirectionInput = 0.875f;

        // Smoothly interpolate the current value to the target value
        float smoothness = 0.1f; // Adjust this value for the desired smoothness
        directionInput = Mathf.Lerp(directionInput, targetDirectionInput, smoothness);

        anim.SetFloat("blendvarRun", directionInput);
    }

    void Move() //이동 함수
    {
        if (controller.isGrounded || isBlackHoling)
        {
            isJumping = false;
            moveDirection = new Vector3(inputH * movSpeed * (inputWalk ? 0.3f : 1f), 0f, inputV * movSpeed * (inputWalk ? 0.3f : 1f));

            if (inputJump)
            {
                AudioManager.instance.PlaySfx(AudioManager.SFX.SFX_JumpSound);

                moveDirection.y += jumpForce;
                isJumping = true;
                anim.SetTrigger("doJump");
            }
        }
        else
        {
            // 공중에 있을 때의 움직임 조정, Time.deltaTime을 곱하여 일관된 속도 유지
            moveDirection.x = inputH * movSpeed * (inputWalk ? 0.3f : 1f);
            moveDirection.z = inputV * movSpeed * (inputWalk ? 0.3f : 1f);

            // 대각선 노말라이제이션 

            // 추가 중력 적용
            moveDirection.y += Gravity * 2 * Time.unscaledDeltaTime; // 중력 적용에도 Time.deltaTime을 사용
        }
        moveDirection = transform.TransformDirection(moveDirection);
        controller.Move(moveDirection * Time.unscaledDeltaTime); // 이동 명령에 Time.deltaTime 적용

        // 이동 중인 상태 확인
        bool isRunning = Mathf.Abs(inputH) > 0f || Mathf.Abs(inputV) > 0f; // 입력에 따른 달리기 상태 결정
        anim.SetBool("isRunning", isRunning); // Animator에 달리기 상태 전달

        // 발소리 재생
        if (isRunning && controller.isGrounded) // 캐릭터가 이동 중이고 지면에 있을 때
        {
            footstepTimer += Time.deltaTime;

            if (footstepTimer >= footstepInterval)
            {
                Debug.Log("발소리 ");

                AudioManager.instance.PlaySfx(AudioManager.SFX.SFX_PlayerWalkSound);
                footstepTimer = 0f; // 타이머 리셋
            }
        }

        // 끌어당김 상태 업데이트
        isBlackHoling = false;
    }

    void Rotate()
    {
        transform.Rotate((Vector3.up * rotateX * rotSpeed * Time.unscaledDeltaTime));
    }

    public void Die()
    {
        AudioManager.instance.PlaySfx(AudioManager.SFX.SFX_PlayerDieSound);


        // 사망 처리
        isAlive = false;

        // 3인칭 전환
        camController.ToggleCamera(3);

        // 죽음 애니메이션 설정 
        anim.SetBool("isDie", true);

        // 사망 후 일정 시간 후에 시작 위치로 이동
        StartCoroutine(Respawn());
    }

    // 저장 위치에서 부활 
    public IEnumerator Respawn()
    {
        yield return new WaitForSeconds(4f); // 사망 후 4초 대기 (옵션)

        // 플레이어 위치 초기화
        PlayerInit();

        // 죽음 -> idle 변경 
        anim.SetBool("isDie", false);

        // 플레이어 상태 초기화
        isAlive = true;

        // 3인칭 전환
        camController.ToggleCamera(1);

        //
        AudioManager.instance.PlaySfx(AudioManager.SFX.SFX_RespawnSound);


    }


    //아이템 사용 함수
    #region Item

    void Swap() //아이템 스왑 함수
    {
        Debug.Log("스왑 눌림");
        AudioManager.instance.PlaySfx(AudioManager.SFX.SFX_ItemEquipSound);

        // 현재 장착된 아이템 비활성화
        if (equipItem != null) 
        {
            //equipItem.SetActive(false);
            Debug.Log("아이템 없음");
        }
            
        // 각 키에 맞는 아이템 선택 및 활성화
        if (inputKeyButton1 && inventory[0] != Item.Type.Null)
        {
            EquipItem(inventory[0], 0);
            equipItemIndex = 0;
        }
        else if (inputKeyButton2 && inventory[1] != Item.Type.Null)
        {
            EquipItem(inventory[1], 1);
            equipItemIndex = 1;
        }
        else if (inputKeyButton3 && inventory[2] != Item.Type.Null)
        {
            EquipItem(inventory[2], 2);
            equipItemIndex = 2;
        }

        isSwaping = true;
        Invoke("SwapOut", 0.4f); // 스왑 후 추가 로직 수행을 위해 0.4초 대기

    }

    void EquipItem(Item.Type itemType, int itemIndex)
    {
        // inventory[i] 에 있는 아이템을, 내 손의 탬 중에서 찾아서 장비해야 함 
        equipItem = equipItems[(int)itemType]; // 0중력 1 시간 2태엽 3블랙홀 4쉴드 
        equipItem.SetActive(true);

        // 아이템 UI에 장착 장비 표시
        UIManager.instance.equipItemUI(itemType, itemIndex);
    }


    void SwapOut() //스왑 끝내기
    {
        isSwaping = false;
    }

    // 플레이어 상태 초기화 
    public void PlayerInit()
    {

        moveDirection = Vector3.zero; // 이 값 임의로 초기화 
        controller.enabled = false; // 잠시 끄고 

        transform.position = respawnPosition;  // 현재 체크포인트에서 시작. 처음엔 처음 위치임 

        controller.enabled = true; // 다시 켠다 

        // 걍 설정 
        Application.targetFrameRate = 50; // 서주민 전용코드 

        Gravity = -9.81f;

        //controller.detectCollisions = false; // 이거 끄면.. 플레이어가 발판을 못 밟음 
        timer.TimerUIInit();

        if (StageManager.Instance.currentStageName == "stage0") timer.isPlaying = false;
        else timer.isPlaying = true;


    }

    public void SetCheckpoint(Vector3 checkpointPosition)
    {
        respawnPosition = checkpointPosition + respawnPositionOrignal;
        Debug.Log(respawnPosition + " has set to new Spawning Location");
    }

    void Interaction() //아이템 줍기
    {
        if (nearObject != null && nearObject.tag == "Item")
        {
            if (interactionText != null)
            {
                interactionText.enabled = false; // ui 끄기 
            }

            AudioManager.instance.PlaySfx(AudioManager.SFX.SFX_UI_ClickSound);  // 버튼 클릭 소리 재생

            Item.Type itemType = nearObject.GetComponent<Item>().type;
            bool enabled = nearObject.GetComponent<Item>().enabledToUse;
            int itemIndex = (int)itemType; // gravity 0, time 1, mag 2, shield 3 wind4  
            //Debug.Log("itemIndex" + itemIndex);

            hasItems[itemIndex] = true;  //아이템 인덱스 활성화(활성화된 인덱스에서의 아이템 꺼내기가 활성화됨)

            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i] == itemType || !enabled)
                {
                    // 이미 해당 아이템이 존재한다면
                    Debug.Log("아이템이 중복되거나 비활성화되어 있습니다.");
                    break;
                }
                if (inventory[i] == Item.Type.Null)
                {
                    // i 번째 슬롯이 비었다! 
                    inventory[i] = itemType; // 아이템 넣어주고~ 
                    UIManager.instance.hasItemUI(itemType, true, i);
                    Debug.Log(i + "빈슬롯 색칠~");
                    Destroy(nearObject); //지역에 떨어진 아이템 삭제하기
                    break;
                }

            }


        }
        else if (nearObject != null && nearObject.tag == "Switch")
        {
            if (interactionText != null)
            {
                interactionText.enabled = false; // ui 끄기 
            }
            SwitchTrigger targetSwitchScript = nearObject.GetComponent<SwitchTrigger>();
            if (targetSwitchScript != null)
            {
                targetSwitchScript.Activate();
            }
            else
            {
                Debug.LogError("Switch Object" + nearObject + "Has no script for functioning.");
            }
        }
    }

    private void UseItem()
    {
        GameObject itemObjResult;
        float elapsedTime = 0f;

        if (equipItemIndex < 0 || equipItemIndex >= inventory.Length)
        {
            Debug.LogError("EquipItemIndex is out of range.");
            return;
        }

        switch (inventory[equipItemIndex])
        {

            case Item.Type.Gravity: //중력(중력 적용장치 인스턴스를 생성한 다음 정해진 방향으로 투척
                
                AudioManager.instance.PlaySfx(AudioManager.SFX.SFX_ItemUseSound);

                if(itemPointTransform) {
                    itemObjResult = Instantiate(gravityPrefebs[0], itemPointTransform.position + itemPointTransform.forward, itemPointTransform.rotation);
                }
                else{
                    itemObjResult = Instantiate(gravityPrefebs[0], transform.position + transform.forward, transform.rotation);
                }
                
                //투척 후 시간 종료 시 아이템 삭제하기
                elapsedTime += Time.deltaTime;
                if (elapsedTime >= duration)
                {
                    Destroy(itemObjResult);
                }
                break;

            case Item.Type.TimeStop: //시간 정지(입력에 따라서 시간 속도를 조절함
                
                StartCoroutine(TweakTimeEffect(timeScaleMultiplier, 5));
                Debug.Log("Time speed has changed into " + timeScaleMultiplier + "x.");
                Debug.Log("Actual Time speed has changed into " + Time.timeScale + "x.");
                AudioManager.instance.PlaySfx(AudioManager.SFX.SFX_ItemUseSound);

                break;

            case Item.Type.Shield:

                Debug.Log("Using shield now");
                timer.isPlaying = false;
                isInShield = true;
                AudioManager.instance.PlaySfx(AudioManager.SFX.SFX_ItemUseSound);

                StartCoroutine(WaitAndExecute(shieldDuration));

                break;

            case Item.Type.WindKey: //윈드 키
                timer.TimeChange(windupDuration); // 30초 추가 
                AudioManager.instance.PlaySfx(AudioManager.SFX.SFX_ItemUseSound);
                break;

            case Item.Type.Magneticgrav: // 블랙홀
                AudioManager.instance.PlaySfx(AudioManager.SFX.SFX_ItemUseSound);
                if (itemPointTransform) {
                    itemObjResult = Instantiate(gravityPrefebs[1], itemPointTransform.position + itemPointTransform.forward, itemPointTransform.rotation);
                }
                else{
                    itemObjResult = Instantiate(gravityPrefebs[1], transform.position + transform.forward, transform.rotation);
                }

                elapsedTime += Time.deltaTime;
                if (elapsedTime >= duration)
                {
                    Destroy(itemObjResult);
                }
                break;
            
            case Item.Type.Null: //없음
                // Handle no item selected
                return;

            default: //모든 확인할 수 없는 아이템이 잡혀 있는 경우 에러 메시지 전달
                Debug.LogError("Unknown item type: " + equipItemIndex);
                return;

        }

        //사용 후 처리들 

        hasItems[equipItemIndex] = false;

        UIManager.instance.hasItemUI(inventory[equipItemIndex], false, equipItemIndex); // 아이콘과 슬롯 끄기 

        inventory[equipItemIndex] = Item.Type.Null;        // 사용시 사라진다

        UIManager.instance.equipItemUI(inventory[equipItemIndex], equipItemIndex); // itemFrame 꺼진다 . 몇번째 슬롯인지 받아온다 

        //equipItem.SetActive(false);
    }

    #endregion
    #region Time Tweak
    IEnumerator TweakTimeEffect(float scale, float duration)
    {
        TweakTimeStart(scale); //지정된 값만큼 시간 속도를 조절
        yield return new WaitForSecondsRealtime(duration); // Unscaled time을 사용해 실제 흐르는 시간을 측정하고 이후에 시간 조작 비활성화
        TweakTimeEnd();
    }
    public void TweakTimeStart(float timeScaleMultiplier) //시간 조작 활성화
    {
        Time.timeScale = timeScaleMultiplier;
        //Time.fixedDeltaTime = Time.timeScale * (1 / Application.targetFrameRate);
        OnCallBackTweakTimeStart();
    }
    public void TweakTimeEnd() //시간 조작 비활성화
    {
        Time.timeScale = 1.0f;

        OnCallBackTweakTimeEnd();
    }
    public void OnCallBackTweakTimeStart() // CallBack을 그냥 Method Call로 대체 
    {
        anim.speed = 1.0f / Time.timeScale; // 애니메이션 속도도 바꿔준다
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }
    public void OnCallBackTweakTimeEnd() // CallBack을 그냥 Method Call로 대체 
    {
        anim.speed = 1.0f;
        Time.fixedDeltaTime = 0.02f;
        // 추가적으로 처리해줘야 할 부분들
        // 추가 처리 더 없다면, 위 함수와 합쳐줘도 될듯 
    }


    //Winding
    public void WindKeyActivate() // 활성화되면서 
    {
        windKey.SetActive(true);
    }

    // 필수 조건 
    // 1. 둘 중에 하나에 무조건 rigidbody
    // 2. 둘 중에 하나에 무조건 isTrigger 체크

    #endregion
    #region Colli Interaction
    private void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Bullet") && !isInShield) 
        {
            // 피격 
            AudioManager.instance.PlaySfx(AudioManager.SFX.SFX_PlayerDmgSound);

            camController.ShakeCam();
            StartCoroutine(UIManager.instance.DmgFX());
        }

        /*
        if (other.CompareTag("CheckPoint"))
        {
            respawnPosition = other.transform.position;

            timer.SetCheckPointTime();

            Debug.Log("Checkpoint reached: " + respawnPosition);
        }
        */
    }

    private void OnTriggerStay(Collider other) 
    {
        if (other.CompareTag("Item"))
        {
            //UI 켜기
            if (interactionText != null)
            {
                interactionText.enabled = true; // ui 끄기 
            }
            nearObject = other.gameObject;
        }
        else if (other.CompareTag("Switch")) //스위치이면 활성화 준비
        {
            if (interactionText != null)
            {
                interactionText.enabled = true; // ui 끄기 
            }
            nearObject = other.gameObject;
        }
        else if (other.CompareTag("Bullet") && !isInShield) //총알이면 대미지 입기
        {
            if (Time.time % 1 == 0){
                AudioManager.instance.PlaySfx(AudioManager.SFX.SFX_PlayerDmgSound);

                camController.ShakeCam();
                StartCoroutine(UIManager.instance.DmgFX());
            }
        }
    }

    private void OnTriggerExit(Collider other) 
    {
        if (other.CompareTag("Item"))
        {
            if (interactionText != null)
            {
                interactionText.enabled = false; // ui 끄기 
            }
            nearObject = null;
        }
        else if (other.CompareTag("Switch")) 
        {
            if (interactionText != null)
            {
                interactionText.enabled = false; // ui 끄기 
            }
            nearObject = null;
        }
    }

    // 상자 밀기 
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        CharacterController hitController = hit.gameObject.GetComponent(typeof(CharacterController)) as CharacterController;

        if (hitController)
        {
            // hitController.SimpleMove(moveDirection);
            Vector3 pushDirection = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
            hitController.Move(pushDirection * pushPower); // * Time.deltaTime
        }
    }
    public void BlackHole(Vector3 fieldCenter)
    {
        isBlackHoling = true;
        blackholeVector = fieldCenter - transform.position; // + new Vector3(0f,1.3f,0f)
        //blackholeVector = Vector3.Normalize(blackholeVector); // 방향만 구함 
        controller.Move(blackholeVector * Time.deltaTime); //* blackholeStrength *Time.unscaledDeltaTime

    }

    private IEnumerator WaitAndExecute(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        Debug.Log("Stopped using shield");
        isInShield = false;
        timer.isPlaying = true;
    }

    #endregion
}