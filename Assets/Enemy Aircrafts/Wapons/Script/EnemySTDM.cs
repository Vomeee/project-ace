using MGAssets.AircraftPhysics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySTDM : MonoBehaviour
{
    public WarningController wc;
    public TagController tagController;
    public Transform target; // 추적할 타겟

    [Header("Attributes")]
    public float turningForce; // 회전 속도
    public float maxSpeed; // 최대 속도
    public float accelAmount; // 가속량
    public float lifetime; // 미사일의 수명
    public float speed; // 현재 속도
    public int damage; //미사일의 대미지

    public float boresightAngle; //한계 추적 각도. 90도 base


    [Space]
    [Header("Effect and sounds")]
    [SerializeField] private GameObject enemyHitEffect; //적기 명중시 폭파효과
    [SerializeField] private GameObject groundHitEffect;

    [SerializeField] Rigidbody rb;
    [SerializeField] CapsuleCollider mslCollider;

    //[SerializeField] GameObject MissileIndicatorPrefab;
    //[SerializeField] GameObject mslIndicator = null;
    //[SerializeField] GameObject parent;


    public void Launch(Transform target, float launchSpeed, WarningController warningController)
    {
        wc = warningController;

        this.target = target;
        if (wc != null)
        {
            // 타겟이 존재할 때만 할당
            if (target != null)
            {
                

                wc.TrackingMissileCount(1);

                MissileIndicatorController indicatorController = warningController.GetComponentInChildren<MissileIndicatorController>();
                if (indicatorController != null) //target is player
                {
                    indicatorController.AddMissileIndicator(this);
                }

                //mslIndicator = Instantiate(MissileIndicatorPrefab); //ui....
                //MissileIndicator newMissileIndicator = mslIndicator.GetComponent<MissileIndicator>();

                //newMissileIndicator.transform.SetParent(target.transform);
                //newMissileIndicator.InitalizeReference(this);
            }
        }
        Debug.Log("Missile instantiated");
        // 발사 속도를 설정
        speed = launchSpeed;
    }

    void LookAtTarget()
    {
        // 타겟이 존재할 때만 추적
        if (target == null)
            return;

        Vector3 targetDir = target.position - transform.position;
        float angle = Vector3.Angle(targetDir, transform.forward);

        if (angle > boresightAngle)
        {
            Debug.Log("evaded");
            target = null;

            //경보 해제
            //Destroy(mslIndicator.gameObject);
            //mslIndicator = null;
            if(wc != null)
            {
                wc.TrackingMissileCount(-1);
            }
            
            return;
        }

        Quaternion lookRotation = Quaternion.LookRotation(targetDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, turningForce * Time.deltaTime);
    }

    void Start()
    {
        // 수명이 끝나면 미사일을 제거
        Destroy(gameObject, lifetime);
        rb = GetComponent<Rigidbody>();
        mslCollider = GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        // 속도가 maxSpeed를 넘지 않도록 가속
        if (speed < maxSpeed)
        {
            speed += accelAmount * Time.deltaTime;
        }

        // 타겟이 없으면 직진만 수행
        if (target == null)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
        else
        {
            // 타겟이 있으면 추적
            LookAtTarget();
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }

    void OnCollisionEnter(Collision collision) //땅이든, 적이든... 파괴.
    {
        Debug.Log(collision.gameObject.name);
        if (collision.gameObject.CompareTag("Player"))
        {
            if(wc != null)
            {
                wc.TrackingMissileCount(-1);
                // 적기에 부딪혔을 때 효과 생성
                Instantiate(enemyHitEffect, transform.position, Quaternion.identity);

                Aircraft playerAircraft = collision.gameObject.GetComponent<Aircraft>();
                if (playerAircraft != null)
                {
                    playerAircraft.playerHP -= damage;
                }

                target = null;

                StartCoroutine(WaitForIndicator());
            }
            

            Destroy(gameObject);
        }

        // 충돌한 오브젝트의 태그가 "Ground"일 경우
        if (collision.gameObject.CompareTag("Ground"))
        {
            // 땅에 닿았을 때 효과 생성
            Instantiate(groundHitEffect, transform.position, Quaternion.identity);

            
        }

        // 총알 파괴
        
    }

    private IEnumerator WaitForIndicator()
    {
        yield return new WaitForSeconds(0.3f);
    }
}
