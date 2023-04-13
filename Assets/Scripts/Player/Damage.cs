using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Damage : MonoBehaviour
{
    //플레이어 체력
    public float currHp = 100;

    //체력바 이미지 저장
    public Image hpBar;

    public Image gameover;

    public bool isPlayerDie = false;

    //생명 게이지 처음 색상 == 녹색
    Color initColor = new Vector4(0, 1.0f, 0.0f, 0.7f);
    Color currColor;

    AudioSource _audio;

    //오디오 클립
    public AudioClip[] playerDmg;


    void Start()
    {
        //생명 게이지의 초기 색상 설정
        hpBar.color = initColor;
        currColor = initColor;

        _audio = GetComponent<AudioSource>();
    }

    void Update()
    {
        //Playert의 생명이 0 이하이면 사망처리
        if (currHp <= 0.0f && isPlayerDie == false)
        {
            //게임 오버 효과음 출력
            _audio.PlayOneShot(playerDmg[1]);
            isPlayerDie = true;
            PlayerDie();
        }

    }


    //플레이어가 데미지를 입을 때 마다 호출되는 함수
    //데미지 정보를 받아서 체력에서 깎는다
    public void PlayerDamage (float damage)
    {
        //데미지를 입는 효과음 출력
        _audio.PlayOneShot(playerDmg[0]);
        currHp -= damage;
        Debug.Log("Player HP = " + currHp.ToString());

        DisplayHpbar();
    }

    //충돌한 Collider의 IsTrigger 옵션이 체크됐을 떄 발생
    void OnTriggerEnter(Collider coll)
    {
        //충돌한 Collider의 태그가FIREBALL이면 Player의 curHp를 차감
        if (coll.tag == "FIREBALL")
        {
            //데미지를 입는 효과음 출력
            _audio.PlayOneShot(playerDmg[0]);
            Destroy(coll.gameObject);
            currHp -= 10.0f;
            Debug.Log("Player HP = " + currHp.ToString());
            
            DisplayHpbar();
        }
    }

    void PlayerDie()
    {
        //주인공 죽는 애니메이션 출력
        GetComponent<Animator>().SetTrigger("PlayerDie");
        //플레이어 조작 불가
        GetComponent<PlayerCtrl>().enabled = false;
        gameover.enabled = true;
    }


    void DisplayHpbar()
    {
        float ratio = currHp / 100.0f;
        // 생명 수치가 50%초과일때는 노란색으로 변경
        if (ratio > 0.5f)
        {
            currColor.r = (1 - ratio) * 2.0f;
        }
        else // 생명 수치가 0%전까지는 때까지는 빨간색으로 변경
        {
            currColor.g = ratio * 2.0f;
        } 
        hpBar.color = currColor;
        hpBar.fillAmount = ratio;
    }

}
