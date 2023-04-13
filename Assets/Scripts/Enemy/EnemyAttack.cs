using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    //공격중인지 확인하는 변수
    public bool isAttack = false;

    //적 화염구 프리팹
    public GameObject FireBall;
    //화염구 발사좌표
    public Transform firePos;

    //공격 모드 변수
    public int attackType = 1;

    //AudioSource 컴포넌트를 저장할 변수
    AudioSource _audio;
    //폭발음 오디오 클립
    public AudioClip[] enemyAttack;

    //Animator 컴포넌트 저장
    Animator animator;
    //플레이어 transform 컴포넌트
    Transform playrtTr;
    //적 transform 컴포넌트
    Transform enemyTr;


    //다음 발사할 시간 계산 변수
    float nextAttack = 0.0f;
    //화염구 발사 간격
    float attackTime = 3.8f;
    //주인공으을 향해 회전할 속도 계수
    float damping = 10.0f;

    //주인공의 정보를 저장할 변수
    GameObject player;
    Transform playerTr;
    float playerHp;
    float dist;

    public bool isTimerOn = false;
    private float timer = 0;

    void Start()
    {
        //컴포넌트 추출 및 변수 저장
        player = GameObject.FindGameObjectWithTag("PLAYER");
        playrtTr = player.transform;
        enemyTr = GetComponent<Transform>();
        animator = GetComponent<Animator>();
        _audio = GetComponent<AudioSource>();
        
    }

    void Awake()
    {
        //주인공 게임 오브젝트 추출
        GameObject player = GameObject.FindGameObjectWithTag("PLAYER");
        //주인공의 Transform 컴포넌트 추출
        if (player != null)
        {
            playerTr = player.transform;
        }
    }

        void Update()
    {
        if (isAttack)
        {
            dist = Vector3.Distance(playerTr.position, enemyTr.position);
            //현재 시간이 다음 발사 시간보다 큰지 확인
            if (Time.time >= nextAttack)
            {
                Attack(attackType);
                //다음 발사 시간 계산
                nextAttack = Time.time + attackTime + Random.Range(0.0f, 1.8f);
            }

            //주인공이 있는 위치까지의 회전각도 계산
            Quaternion rot = Quaternion.LookRotation(playrtTr.position - enemyTr.position);
            //보간 함수를 사용해 점진적으로 회전
            enemyTr.rotation = Quaternion.Slerp(enemyTr.rotation, rot, Time.deltaTime * damping);


            if (isTimerOn && (attackType == 3 || attackType == 4))
            {
                //공격 모드가 3,4일 경우 화염구 생성
                timer += Time.deltaTime;
                if (timer > 0.5f)
                {
                    GameObject fireball = Instantiate(FireBall, firePos.position, firePos.rotation);

                    //날아서 공격하면 공격 가능 횟수를 1 감소시킴
                    if (attackType == 4)
                    { 
                        GetComponent<EnemyAI>().flyballCount -= 1;
                    }

                    Destroy(fireball, 7.0f);
                    timer = 0;
                    isTimerOn = false;
                }
            }
        }
        
    }

    void Attack(int attackType)
    {
        switch (attackType) //공격 모드에 따라 분기 처리
        {
            //1_기본 물기 공격
            //사정거리 : 6이내, 효과음 재생
            case 1:
                animator.SetTrigger("BasicAttack");
                _audio.PlayOneShot(enemyAttack[0]);
                if (dist < 6 )
                {
                    player.GetComponent<Damage>().PlayerDamage(6.5f);
                }
                print (dist);
                break;

            //2_기본 꼬리 공격
            //사정거리 : 7.4이내, 효과음 재생
            case 2:
                animator.SetTrigger("TailAttack");
                _audio.PlayOneShot(enemyAttack[1]);
                if (dist < 7.4)
                {
                    player.GetComponent<Damage>().PlayerDamage(5.0f);
                }
                print(dist);
                break;

            //3_원거리 화염구 공격
            //효과음 재생, 화염구 생성
            case 3:
                animator.SetTrigger("FireBall");
                _audio.PlayOneShot(enemyAttack[2]);
                isTimerOn = true;
                break;

            //4_날면서 화염구 공격
            //효과음 재생, 화염구 생성
            case 4:
                animator.SetTrigger("FireBall_Fly");
                _audio.PlayOneShot(enemyAttack[2]);
                isTimerOn = true;
                break;

        }

    }
}
