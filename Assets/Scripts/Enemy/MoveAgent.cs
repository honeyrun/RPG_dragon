using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveAgent : MonoBehaviour
{

    //드래곤의 스폰위치
    public Vector3 dragonSpawn;
    Quaternion dragonRot;

    float normalSppeed = 5f; // 스폰 포인트로 이동하는 속도
    float traceSpeed = 9.0f; // 추적 속도

    //NavMeshAgent 컴포넌트를 저장할 변수
    NavMeshAgent agent;

    //추적 대상의 위치를 저장하는 변수
    Vector3 traceTaget;

    void Start()
    {
        dragonSpawn = transform.position;
        dragonRot = transform.rotation;

        agent = GetComponent<NavMeshAgent>();
        agent.speed = normalSppeed;
        SetIdle();
    }

    public void SetIdle()
    {
        Stop();
    }

    public void SetTraceTarget(Vector3 pos)
    {
        traceTaget = pos;
        agent.speed = traceSpeed;
        agent.angularSpeed = 360;
        TraceTarget(traceTaget);
    }



    //스폰 지점까지 이동명령을 내리는 함수
    public void MoveToSpawn()
    {
        agent.speed = normalSppeed;
        agent.angularSpeed = 120;

        //최던거리 경로 계산이 끝나지 않았으면 다음을 수행하지 않음
        if (agent.isPathStale) return;
        //목적지를 spawn point로 지정
        agent.destination = dragonSpawn;
        //네비게이션 기능을 활성화해서 이동을 시작함
        agent.isStopped = false;

    }

    //주인공 추적시 이동 함수
    void TraceTarget(Vector3 pos)
    {
        if (agent.isPathStale)
            return;
        agent.destination = pos;
        agent.isStopped = false;
    }

    //순찰 및 추적을 정지시키는 함수
    public void Stop()
    {
        agent.isStopped = true;
        //바로 정지하기 위해 속도를 0으로 설정
        agent.velocity = Vector3.zero;
    }

}
