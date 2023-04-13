using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCtrl : MonoBehaviour
{

    //이동속도, 회전 속도
    public float moveSpeed = 15;
    public float rotSpeed = 100;

    Vector3 AXIS_X = new Vector3(1, 0, 0);
    Vector3 AXIS_Y = new Vector3(0, 1, 0);
    Vector3 AXIS_Z = new Vector3(0, 0, 1);

    Transform tr;
    Rigidbody rb;

    Animator animator;

    //드레곤 오브젝트의 Mesh 저장
    public GameObject dragonMesh;
    //드레곤 오브젝트 저장
    public GameObject dragon;

    public bool isDie = false;

    bool isWalk = false;

    //걷는 소리를 통제하는 변수
    bool toggle = true;

    //update 함수 내에서 일정시간 반복할 기능을 통제할 변수
    //플레이어가 공격 중일 때 타이머 켜짐
    public bool isTimerOn = false;
    private float timer;

    //플레이어의 공격 가능 거리
    public float attackDist = 5.76f;

    AudioSource _audio;
    //플레이어 효과음 오디오 클립
    public AudioClip[] playerSnd;

    //적 캐릭터의 위치를 저장할 변수
    Transform enemyTr;


    void Start()
    {
        tr = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();

        animator = GetComponent<Animator>();
        _audio = GetComponent<AudioSource>();

    }

    void Awake()
    {
        //적(드레곤)의 Transform 컴포넌트 추출
        GameObject enemy = GameObject.FindGameObjectWithTag("ENEMY");
        if (enemy != null)
        {
            enemyTr = enemy.transform;
        }
    }

    void Update()
    {
        PlayerMove();
        Rotate();
        AttackTime();
    }



    void PlayerMove()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        //공격하지 않을 때 키보드 방향키, WASD를 누르면 조작가능, 플레이어의 상하좌우 이동 담당
        if (!isTimerOn)
        {
            Vector3 moveDir = (AXIS_X * h + AXIS_Z * v).normalized;
            tr.Translate(moveDir * moveSpeed * Time.deltaTime, Space.Self);
        }
        

        //플레이어의 공격1 >> 마우스 좌클릭
        //공격1에 해당하는 효과음과 애니메이션 재생
        if (Input.GetMouseButtonDown(0))
        {
            isWalk = false;
            _audio.Stop();
            _audio.PlayOneShot(playerSnd[1]);
            animator.SetTrigger("Attack"); // 공격
            isTimerOn = true;
            //적과 플레이어의 거리가 공격 범위에 속하면 적이 데미지를 입음
            float dist = Vector3.Distance(transform.position, enemyTr.position);
            if (dist < attackDist)
            {
                dragon.GetComponent<EnemyDamage>().EnemyDamageF(Random.Range(6, 8));
            }

        }
        //플레이어의 공격2 >> 마우스 우클릭
        //공격2에 해당하는 효과음과 애니메이션 재생
        else if (Input.GetMouseButtonDown(1))
        {
            isWalk = false;
            _audio.Stop();
            _audio.PlayOneShot(playerSnd[2]);
            animator.SetTrigger("SpecialSkill");
            isTimerOn = true;
            float dist = Vector3.Distance(transform.position, enemyTr.position);
            if (dist < attackDist)
            {
                dragon.GetComponent<EnemyDamage>().EnemyDamageF(Random.Range(8, 10));
            }
        }
        //플레이어의 이동 >> 키보드 입력값을 기준으로 동작
        else if (v != 0 || h != 0)
        {
            //걷는 소리가 여러번 출력되서 겹치지 않도록 한번만 재생
            if (isWalk == false)
            {
                if (_audio.isPlaying == false)
                {
                    isWalk = true;
                    _audio.PlayOneShot(playerSnd[0]);
                    toggle = true;
                }
            }

            animator.SetBool("IsMove", true); // 걷는 동작 출력 

        }

        else
        {
            //idle 상태일떄 걷는 소리 멈춤
            if (toggle == true && isWalk == true)
            {
                isWalk = false;
                _audio.Stop();
                toggle = false;
            }

            animator.SetBool("IsMove", false); // 정지시 idle
        }

    }

    //공격시간동안 플레이어의 위치를 고정시킴, 공격시간이 끝나면 타이머를 초기화시키고 타이머를 끔
    void AttackTime()
    {
        if (isTimerOn)
        {
            timer += Time.deltaTime;
            if (timer < 1.7f )
            {
                //타이머가 켜져있을 때 공격범위내에서 공격하면 적 캐릭터가 데미지를 입음을 표시하는 코드
                //적(드레곤)의 Mesh를 일정시간동안 깜박이도록 빨간색으로 변화시켜 데미지를 받았음을 나타냄 
                float dist = Vector3.Distance(transform.position, enemyTr.position);
                if (dist < attackDist)
                {
                    if (timer < 0.5 && timer > 0.3)
                    {
                        dragonMesh.GetComponent<SkinnedMeshRenderer>().material.color = new Color(1, 0, 0);
                    }
                    else if (timer < 0.7)
                    {
                        dragonMesh.GetComponent<SkinnedMeshRenderer>().material.color = new Color(1, 1, 1);
                    }
                    else if (timer < 0.9)
                    {
                        dragonMesh.GetComponent<SkinnedMeshRenderer>().material.color = new Color(1, 0, 0);
                    }
                    else if (timer < 1.1)
                    {
                        dragonMesh.GetComponent<SkinnedMeshRenderer>().material.color = new Color(1, 1, 1);
                    }
                }
                
                transform.position = transform.position;
            }
            else
            {
                timer = 0;
                isTimerOn = false;
            }
        }
       
    }


        void Rotate()
    {
        //플레이어의 회전 >> 마우스의 이동
        //월드 좌표계 Y축 기준 회전만 적용 -> 플레이어 좌우 시점 변경
        float mx = Input.GetAxis("Mouse X");
        Vector3 rotY = AXIS_Y * mx * rotSpeed * Time.deltaTime;
        tr.Rotate(rotY, Space.World);

    }




    void OnDisable()
    {
        //해당 스크립트가 비활성화 됐을때 실행되는 함수
        rb.velocity = Vector3.zero;
    }

}
