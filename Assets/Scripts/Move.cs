using System;
using System.Collections.Generic;
using GameStatus;
using UnityEngine;
using Type;
using Weapon;

public class Move : MonoBehaviour
{
    #region 인스펙터에 버튼 만들기 귀찮아서 만들어둔 테스트코드입니다.
    public bool testButton = false;
    private void TestUpdate()
    {
        if (testButton)
        {
            InitialStatus();
            testButton = false;
        }
    }
    #endregion

    private BaseStat<CharStat> _baseCharStat;
    [SerializeField]
    private Joystick _joystick;
    
    public float _jumpForce = 7.0f;
    public float _fallMultiplier = 2.0f;
    [SerializeField]
    private bool _isOnGround;
    
    private bool _isTargetNotNull;
    private Rigidbody _characterRigidbody;
    private int _btnStatus;
    
    public GameObject target;
    public List<Synergy> synergyList = new List<Synergy>();

    public List<WeaponBase> weapons = new List<WeaponBase>();
    
    
    private WeaponBase _weapon => weapons[_btnStatus - 1];
    
    private void Awake()
    {
        _btnStatus = 1;
        _baseCharStat = new BaseStat<CharStat>();
        _isTargetNotNull = target is not null;
        _characterRigidbody = GetComponent<Rigidbody>();

        weapons.Add(gameObject.AddComponent<HandGun>());
        weapons.Add(gameObject.AddComponent<ShieldGenerator>());
    }

    private void Start()
    {
        InitialStatus();
    }

    private void InitialStatus()
    {
        _baseCharStat.ClearStatList();
        foreach (var synergy in synergyList)
        {
            _baseCharStat.AddStatList(synergy.charStatList);
            _weapon.AddWeaponStatList(synergy.weaponStatList);
        }
    }

    public void ChangeGun1()
    {
        InitialStatus();
        _btnStatus = 1;
    }
    
    public void ChangeGun2()
    {
        InitialStatus();
        _btnStatus = 2;
    }
    
    //
    // public void ChangeGun3()
    // {
    //     if (BtnStatus != 3)
    //     {
    //         _base.ClearStatList();
    //         _weapon = _gameManager.weaponList.Find(x => x.weaponName == "샷건");
    //         InitialStatus();
    //         ApplyStatToBulletData();
    //         BtnStatus = 3;
    //     }
    // }
    //
    // public void ChangeGun4()
    // {
    //     if (BtnStatus != 4)
    //     {
    //         _base.ClearStatList();
    //         _weapon = _gameManager.weaponList.Find(x => x.weaponName == "핸드건");
    //         InitialStatus();
    //         ApplyStatToBulletData();
    //         BtnStatus = 4;
    //     }
    // }
    
    public void GetUlt()
    {
        Debug.Log("Ult");
    }

    public void CharacterMove()
    {
        var speed = _baseCharStat.GetStat(CharStat.Speed).Total;
        _characterRigidbody.AddForce(new Vector3(_joystick.Horizontal, 0, _joystick.Vertical) * speed, ForceMode.Acceleration);
    }

    private void Update()
    {
        var inputSpace = Input.GetButton("Jump");
;
        if (inputSpace && _isOnGround)
        {
            _characterRigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        }
    }

    private void FixedUpdate()
    {
        if(_isTargetNotNull) transform.LookAt(target.transform);
        CharacterMove();
        TestUpdate();

        if (_characterRigidbody.velocity.y < 0)
        {
            //_characterRigidbody.AddForce(Vector3.down * _fallMultiplier, ForceMode.Impulse);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("World"))
        {
            _isOnGround = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("World"))
        {
            _isOnGround = false;
        }
    }

    public void Attack()
    {
        _weapon.Attack();
    }
}
