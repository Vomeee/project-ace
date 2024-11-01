using MGAssets.AircraftPhysics;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class WeaponSystem : MonoBehaviour
{
    public AircrafSimpleHUD infoGetter; //속도 높이 가져오는 instance
    
    #region weaponCounts variables
    [SerializeField] private int gunCount;
    [SerializeField] private int missileCount;
    //[SerializeField] private int specialWeaponCount;
    #endregion

    

    #region weapon prefabs
    public GameObject bulletPrefab; // 총알 프리팹
    public GameObject missilePrefab; // 미사일 프리팹
    public GameObject specialWeaponPrefab; // 특수 무기 프리팹

    #endregion

    #region gunVariables
    public float gunFireRate = 0.02f;
    public bool isGunFiring = false;
    private float fireCooldown;

    public Transform gunPointL; // 발사 위치 L
    public Transform gunPointR; // 발사 위치 R
    [SerializeField] Transform currentGunPoint; // 현재 발사 위치
    public AudioSource gunAudioSource; // AudioSource를 참조
    #endregion

    #region missileVariables
    public Transform leftMissileTransform;
    public Transform rightMissileTransform;
    #endregion

    public int weaponSelection = 0; // 0 : stdm, 1 : sp
    [SerializeField] AudioSource weaponChangeToSpecialWeaponSound;
    [SerializeField] AudioSource weaponChangeToSTDMSound;
    
    
    public Transform playerTransform; //무기 생성시 활용
    public Transform currentTargetTransform; //firemissile 시 활용

    public TagController tagController;
    public TargettingSystem targettingSystem; //현재 타겟 받아오는데 필요함.

    //[SerializeField] Image rightMissileCooldownFilling;
    //[SerializeField]

    public float aircraftSpeed; //기체 현재 속도

    void Start()
    {
        gunCount = 1600;
        missileCount = 125;
        //specialWeaponCount = 16;

        gunCountUIUpdate();
        stdmCountUIUpdate();
        specialWeaponCountUIUpdate();
    }


    [Space]
    #region STDM instances

    public float missileCoolDownTime;

    public float rightMissileCoolDown;
    public float leftMissileCoolDown;

    #endregion

    #region weaponUI instances
    [SerializeField] RectTransform weaponPointer; // 무기 ui 포인터
    [SerializeField] TextMeshProUGUI gunCountText; // 기총 잔량
    [SerializeField] TextMeshProUGUI missileCountText; // 기본미사일 잔량
    [SerializeField] TextMeshProUGUI specialWeaponCountText; // 특수무기 잔량
    #endregion


    void Update()
    {
        #region Weapon Change and Fire

        if (Input.GetMouseButtonDown(1))
        {
            switch (weaponSelection)
            {
                case 0:
                    FireMissile();
                    break;
                case 1:
                    FireSpecialWeapon();
                    break;
                  
            }
        }

        // 무기 전환 (우클릭)
        if (Input.GetMouseButtonDown(2))
        {
            
            
            if (weaponSelection == 0)
            {
                weaponSelection = 1;
                weaponChangeToSpecialWeaponSound.Play(); // 소리 재생
                
            }
            else if(weaponSelection == 1)
            {
                weaponSelection = 0;
                weaponChangeToSTDMSound.Play();
            }
            weaponPointerUpdate(); //무기 포인터 업데이트
            //Beep(); //무기 전환 소리
        }

        #endregion

        #region gunfire updates

        if (Input.GetMouseButton(0)) // H 키를 누르고 있는 동안
        {
            isGunFiring = true; // 총 발사 상태를 true로 설정
            if (!gunAudioSource.isPlaying) // 소리가 재생 중이지 않다면
            {
                gunAudioSource.Play(); // 소리 재생
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isGunFiring = false; // 총 발사 상태를 false로 설정
            gunAudioSource.Stop(); // 소리 정지
        }

        // 총기 연속 발사 처리
        if (isGunFiring && fireCooldown <= 0f)
        {
            FireGun();
            fireCooldown = gunFireRate;
        }

        // 쿨다운 타이머
        if (fireCooldown > 0f)
        {
            fireCooldown -= Time.deltaTime;
        }

        

        #endregion

        #region STDM updates

        STDMCoolDown(ref rightMissileCoolDown);
        STDMCoolDown(ref leftMissileCoolDown);

        aircraftSpeed = infoGetter.getSpeed();

        #endregion
    }

    

    void FireGun()
    {
        Debug.Log("gunfireTriggered");
        if (bulletPrefab != null && gunPointL != null && gunCount > 0)
        {
            if(gunCount % 2 == 0)
            {
                 currentGunPoint = gunPointL;
            }
            else
            {
                 currentGunPoint = gunPointR;
            }
            GameObject bullet = Instantiate(bulletPrefab, currentGunPoint.position, currentGunPoint.rotation);

            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.bulletSpeed = 1200f; // 원하는 속도로 설정
            }

            gunCount--;
            gunCountUIUpdate(); //잔탄 업데이트
            Debug.Log("Gun fired");
        }
    }


    

    #region stdmCodes
    void FireMissile()
    {
        if (missileCount <= 0)
        {
            return; // 잔탄 0.
        }
        if (leftMissileCoolDown > 0 && rightMissileCoolDown > 0)
        {
            return; // 재장전 안됨.
        }

        Vector3 missilePosition;

        if(missileCount % 2 == 1) // 남은 미사일 수가 홀수
        {
            missilePosition = rightMissileTransform.position;
            rightMissileCoolDown = missileCoolDownTime;
        }
        else // 짝수
        {
            missilePosition = leftMissileTransform.position;
            leftMissileCoolDown = missileCoolDownTime;
        }

        GameObject stdm = Instantiate(missilePrefab, missilePosition, playerTransform.rotation); //미사일 생성
        STDM missileScript = stdm.GetComponent<STDM>();

       
        currentTargetTransform = targettingSystem.currentTargetTransform;
        if(targettingSystem.IsInCone(currentTargetTransform))
        {
            missileScript.Launch(currentTargetTransform, infoGetter.getSpeed() / 10 + 20, tagController); ////////확인!!!!!
        }
        else
        {
            missileScript.Launch(null, infoGetter.getSpeed() / 5, tagController);
        }
        
        missileCount--;
        stdmCountUIUpdate();
    }

    void STDMCoolDown(ref float cooldown)
    {
        if (cooldown > 0)
        {
            cooldown -= Time.deltaTime;
            if (cooldown < 0) cooldown = 0;
        }
        else return;
    }

    #endregion

    void FireSpecialWeapon()
    {
        
    }

    #region weaponUI update funcs
    void weaponPointerUpdate()
    {
        if (weaponPointer != null)
        {
            if(weaponSelection == 0)
            {
                weaponPointer.anchoredPosition = new Vector3(-308, 446, 0);
            }
            else if(weaponSelection == 1)
            {
                weaponPointer.anchoredPosition = new Vector3(-308, 386, 0);
            }
        }
    }

    void gunCountUIUpdate()
    {
        string gunText = gunCount.ToString();
        gunCountText.text = "<align=left>GUN<line-height=0>" + "\n" + "<align=right>" + gunText + "<line-height=1em>";
    }

    void stdmCountUIUpdate()
    {
        string mslText = missileCount.ToString();
        missileCountText.text = "<align=left>MSL<line-height=0>" + "\n" + "<align=right>" + mslText + "<line-height=1em>";
    }

    void specialWeaponCountUIUpdate()
    {

    }

    void STDMFrameUpdate()
    {

    }
    #endregion
}
