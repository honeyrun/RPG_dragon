using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyDamage : MonoBehaviour
{
    //적 생명 게이지
    public float hp = 100.0f;

    //생명 게이지 위치보정 오프셋
    public Vector3 hpBarOffset = new Vector3(0, 6.2f, 0);

    //부모 canvas객체
    private Canvas uiCanvas;
    //생명 수치에 따라 fillAmount 속성을 변경할 Image
    public Image hpBarImage;

    public GameObject hpBar;
    public Image gameclear;

    AudioSource _audio;
    public AudioClip[] enemyDmg;

    EnemyAI AI;

    //update함수 내에서 사운드를 한번만 재생할 통제 변수
    bool playOnce = false;

    //생명 게이지 처음 색상 == 흰색에 가까움
    Color initColor = new Vector4(0.8f, 0.7f, 0.7f, 1.0f);
    Color currColor;

    void Start()
    {
       
        //생명게이지 생성 및 초기화
        SetHpBar();

        //생명 게이지의 초기 색상 설정
        hpBarImage.color = initColor;
        currColor = initColor;

        AI = GetComponent<EnemyAI>();

        _audio = GetComponent<AudioSource>();
    }

    void Update()
    {
        //드래곤이 죽을 떄
        if (AI.isDie)
        {
            if (playOnce == false)
            {
                //게임 클리어 효과음 출력
                _audio.PlayOneShot(enemyDmg[1]);
                playOnce = true;
            } 
            EnemyDie();
        }
    }

    void SetHpBar()
    {
        uiCanvas = GameObject.Find("UICanvas").GetComponent<Canvas>();

        //hpBarImage -> 초기 흰색 체력 바
        hpBarImage = hpBar.GetComponentsInChildren<Image>()[1];
        //생명 게이지가 따라가야할 대상과 오프셋값 설정
        EnemyHpBar bar = hpBar.GetComponent<EnemyHpBar>();
        bar.targetTr = gameObject.transform;
        bar.offset = hpBarOffset;
    }

    void EnemyDie()
    {
        if (hp <= 0.0f)
        {
            //게임 클리어 텍스트 화면에 띄움
            gameclear.enabled = true;

            //적 캐릭터가 사망한 이후 생명 게이지를 투명처리
            
            //GetComponentsInParent<Image>()[1] -> 부모 이미지 -> 검정색 체력바
            hpBarImage.GetComponentsInParent<Image>()[1].color = Color.clear;
            //GetComponentsInChildren<Image>()[2] -> 체력바 프레임
            hpBar.GetComponentsInChildren<Image>()[2].color = Color.clear;
        }
    }

    public void EnemyDamageF(float damage)
    {
        //데미지를 입는 효과음 출력
        _audio.PlayOneShot(enemyDmg[0]);

        hp -= damage;
        Debug.Log("Player HP = " + hp.ToString());

        DisplayHpbar();
    }

    void DisplayHpbar()
    {
        float ratio = hp / 100.0f;
        // 생명 수치가 감소함에 따라 점점 빨간색으로 변경

        currColor.g = ratio * 0.7f;
        currColor.b = ratio * 0.7f;

        hpBarImage.color = currColor;
        hpBarImage.fillAmount = ratio;
    }


}
