using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    // 적 캐릭터의 상태를 표현하기 위한 열거형 변수 정의
    public enum State
    {
        MOVESPAWN,
        IDLE,
        TRACE,
        ATTACK,
        FIREATTACK,
        FLYATTACK,
        DIE
    }

    //상태를 저장할 변수
    public State state = State.IDLE;

    //근거리 공격 사정거리
    public float AttackDist = 9f;
    //원거리 공격 사정거리
    public float fireAttackDist = 13f;
    //추적 사정거리
    public float traceDist = 22.0f;
    //날아서 공격하는 횟수 제한
    public int flyballCount = 4;

    public bool isDie = false;

    //주인공(player) 오브젝트 저장
    public GameObject player;

    //주인공의 위치를 저장할 변수
    Transform playerTr;
    //적 캐릭터(자기자신)의 위치를 저장할 변수 
    Transform enemyTr;
    //코루틴에서 사용할 지연시간 변수
    WaitForSeconds ws;
    //이동을 제어하는 MoveAgent 클래스를 저장할 변수
    MoveAgent moveAgent;
    //Animator컴포넌트를 저장할 변수
    Animator animator;
    //총 발사를 제어하는 EnemyFire 클래스 저장 변수
    EnemyAttack enemyFire;

    //Start함수 전에 실행되는 함수 + 오브젝트가 비활성화 된 상태에서도 실행됨
    void Awake()
    {
        //주인공 게임 오브젝트 추출
        GameObject player = GameObject.FindGameObjectWithTag("PLAYER");
        //주인공의 Transform 컴포넌트 추출
        if (player != null)
        {
            playerTr = player.transform;
            // == playerTr  = player.GetComponent<Transform>();
        }
        //적 캐릭터의 Transform 컴포넌트 추출
        enemyTr = GetComponent<Transform>();
        //이동을 제어하는 MoveAgent2 클래스 추출
        moveAgent = GetComponent<MoveAgent>();
        //Animator 컴포넌트 추출
        animator = GetComponent<Animator>();
        //총 발사를 제어하는 EnemyFire 클래스 추출
        enemyFire = GetComponent<EnemyAttack>();


        //코루틴 지연시간 생성
        ws = new WaitForSeconds(0.3f);
    }

    void OnEnable()
    {
        //checkState 코루틴 함수 실행
        StartCoroutine(CheckState());
        //Action 코루틴 함수 실행
        StartCoroutine(Action());
    }

    IEnumerator CheckState()
    {
        //적 캐릭터가 사망하기 전까지 도는 무한로프, 0.3초마다 실행
        while (isDie == false)
        { 
            //상태가 사망이면 코루틴 함수를 종료시킴
            if (state == State.DIE) yield break;

            //주인공과 적 캐릭터 간의 거리를 계산
            float dist = Vector3.Distance(playerTr.position, enemyTr.position);
            
            if (dist <= AttackDist) //근접 공격 사정거리 이내
                state = State.ATTACK;
            else if (dist <= fireAttackDist) //원거리 공격 사정거리 이내
                state = State.FIREATTACK;
            else if (dist <= traceDist) //추척 사정거리 이내의 경우
                state = State.TRACE;
            else if ((moveAgent.dragonSpawn - transform.position).magnitude > 2.0f) //원래 스폰지점으로 되돌아감
                state = State.MOVESPAWN;
            else //idle상태
                state = State.IDLE;
            
            //hp가 0이면 죽음
            if (GetComponent<EnemyDamage>().hp <= 0)
            {
                state = State.DIE;
            }
            //hp가 35%이하이고 날아서 공격 가능한 횟수가 남았을때
            else if (GetComponent<EnemyDamage>().hp < 35 && flyballCount > 0)
            {
                //나는 상태가 아니면 날도록 변경, 이륙 애니매이션 한번 실행 후 나는 애니메이션과 공격 애니메이션 반복
                //4번의 화염구 공격 가능
                if (!animator.GetBool("IsFly"))
                {
                    animator.SetTrigger("Fly");
                    animator.SetBool("IsFly", true);
                }
                state = State.FLYATTACK;
            }

            //플레이어가 죽으면 idle상태로 변경
            if (player.GetComponent<Damage>().isPlayerDie)
            {
                state = State.IDLE;
            }

            //0.3초 대기하는 동안 제어권을 양보
            yield return ws;
        }
    }

    //상태에 따라 적 캐릭터의 행동을 처리하는 함수 코루틴
    IEnumerator Action()
    {
        while (!isDie)
        {
            yield return ws;

            switch (state) //상태에 따라 분기 처리
            {
                case State.MOVESPAWN:
                    //스폰 위치로 이동
                    enemyFire.isAttack = false;
                    moveAgent.MoveToSpawn() ;
                    animator.SetBool("IsMove", true);
                    animator.SetBool("IsFly", false);
                    break;

                case State.IDLE:
                    //대기 상태
                    enemyFire.isAttack = false;
                    moveAgent.SetIdle();
                    animator.SetBool("IsMove", false);
                    animator.SetBool("IsFly", false);
                    break;

                case State.TRACE:
                    //주인공의 위치를 넘겨 추적모드로 변경
                    enemyFire.isAttack = false;
                    moveAgent.SetTraceTarget(playerTr.position);
                    animator.SetBool("IsMove", true);
                    animator.SetBool("IsFly", false);
                    break;

                case State.ATTACK:
                    //근접 공격 - attackType : 1,2
                    moveAgent.Stop(); 
                    animator.SetBool("IsMove", false);
                    animator.SetBool("IsFly", false);
                    if (enemyFire.isAttack == false)
                    enemyFire.isAttack = true;
                    //1(물기),2(꼬리 공격) 중에 랜덤
                    enemyFire.attackType = UnityEngine.Random.Range(1, 3);
                    break;

                case State.FIREATTACK:
                    //파이어볼 공격 - attackType : 3
                    moveAgent.Stop(); 
                    animator.SetBool("IsMove", false);
                    animator.SetBool("IsFly", false);
                    if (enemyFire.isAttack == false)
                        enemyFire.isAttack = true;
                    enemyFire.attackType = 3;
                    break;

                case State.FLYATTACK:
                    //날아서 파이어볼 공격 - attackType : 4
                    moveAgent.Stop();
                    enemyFire.attackType = 4;
                    animator.SetBool("IsMove", false);
                    animator.SetBool("IsFly", true);
                    if (enemyFire.isAttack == false)
                        enemyFire.isAttack = true;
                    break;

                case State.DIE:
                    isDie = true;
                    enemyFire.isAttack = false;
                    //순찰 및 추적을 정지
                    moveAgent.Stop();
                    //사망 애니메이션 실행
                    animator.SetTrigger("Die");
                    
                    gameObject.tag = "Untagged";
                    break;

            }
        }
    }


    public void OnPlayerDie()
    {
        if (isDie == false)
        {
            moveAgent.Stop();
            enemyFire.isAttack = false;
            //모드 코루틴 함수를 종료시킴
            StopAllCoroutines();
            animator.SetTrigger("PlayerDie");
        }
    }


}
