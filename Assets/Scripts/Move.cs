using System.Collections.Generic;
using Fusion;
using GameStatus;
using Types;
using UnityEngine;
using UnityEngine.UI;
using Weapon;

public class Move : MonoBehaviour
{
    #region 테스트 코드 (스탯 적용)
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
    
    #region 테스트 코드 (스탯 UI)

    private List<Stat<CharStat>> statlist1 = new List<Stat<CharStat>>();
    private List<Stat<WeaponStat>> statlist2 = new List<Stat<WeaponStat>>();
    
    public void IncreaseSpeed(GameObject text)
    {
        statlist1.Add(new Stat<CharStat>(CharStat.Speed, 1, 0));
        AdditionalWork(text, CharStat.Speed);
    }
    
    public void DecreaseSpeed(GameObject text)
    {
        statlist1.Add(new Stat<CharStat>(CharStat.Speed, -1, 0));
        AdditionalWork(text, CharStat.Speed);
    }
    
    public void IncreaseInterval(GameObject text)
    {
        statlist2.Add(new Stat<WeaponStat>(WeaponStat.Interval, 0.1f, 0));
        AdditionalWork(text, WeaponStat.Interval);
    }
    
    public void DecreaseInterval(GameObject text)
    {
        statlist2.Add(new Stat<WeaponStat>(WeaponStat.Interval, -0.1f, 0));
        AdditionalWork(text, WeaponStat.Interval);
    }
    
    public void IncreaseRange(GameObject text)
    {
        statlist2.Add(new Stat<WeaponStat>(WeaponStat.Range, 0.5f, 0));
        AdditionalWork(text, WeaponStat.Range);
    }
    
    public void DecreaseRange(GameObject text)
    {
        statlist2.Add(new Stat<WeaponStat>(WeaponStat.Range, -0.5f, 0));
        AdditionalWork(text, WeaponStat.Range);
    }

    public void AdditionalWork(GameObject text, CharStat type)
    {
        InitialStatus();
        
        foreach (var s1 in statlist1)
        {
            _baseCharStat.AddStat(s1);
        }
        
        foreach (var s2 in statlist2)
        {
            _weapon.AddWeaponStat(s2);
        }

        var total = _baseCharStat.GetStat(type).Total;
        text.GetComponent<Text>().text = total.ToString("F2");
    }
    
    public void AdditionalWork(GameObject text, WeaponStat type)
    {
        InitialStatus();
        
        foreach (var s1 in statlist1)
        {
            _baseCharStat.AddStat(s1);
        }
        
        foreach (var s2 in statlist2)
        {
            _weapon.AddWeaponStat(s2);
        }

        var total = _weapon.GetWeaponStat(type).Total;
        text.GetComponent<Text>().text = total.ToString("F2");
    }
    
    
    #endregion

    #region 타겟 지정 관련 변수
    public GameObject target;
    private bool _isTargetNotNull;
    private RaycastHit _raycast;
    public float maxDistance = 10.0f;
    #endregion

    #region 움직임 관련 변수
    public VariableJoystick Joystick;
    private Transform _transform;
    private CharacterController _characterController;
    private Vector3 moveDir;
    private float gravity = -10.0f;
    private float jumpForce = 5.0f;
    public float yVelocity = 0.0f;
    #endregion

    #region 전투 관련
    
    private BaseStat<CharStat> _baseCharStat;
    public List<Synergy> synergyList = new List<Synergy>();
    public List<WeaponBase> weapons = new List<WeaponBase>();
    private WeaponBase _weapon => weapons[_btnStatus - 1];
    private int _btnStatus = 0;
    #endregion

    private GameManager _gameManager;

    [Networked] 
    public bool Test { get; set; }
    
    private void Awake()
    {
        _btnStatus = 1;
        _transform = gameObject.transform;
        
        _baseCharStat = new BaseStat<CharStat>(1, 1);

        _gameManager = GameManager.Instance;
        _isTargetNotNull = false;
        _characterController = GetComponent<CharacterController>();
        
        weapons.Add(gameObject.AddComponent<HandGun>());
        weapons.Add(gameObject.AddComponent<ShieldGenerator>());
        weapons.Add(gameObject.AddComponent<GuidedGun>());
        
        // 임시 무기 채우기
        weapons.Add(gameObject.AddComponent<ShieldGenerator>());
    }

    private void Start()
    {
        InitialStatus();
    }

    private void InitialStatus()
    {
        _baseCharStat.ClearStatList();
        _weapon.ClearWeaponStat();
        foreach (var synergy in synergyList)
        {
            _baseCharStat.AddStatList(synergy.charStatList);
            _weapon.AddWeaponStatList(synergy.weaponStatList);
        }
    }

    public void ChangeGun1()
    {
        _btnStatus = 1;
        InitialStatus();
    }
    
    public void ChangeGun2()
    {
        _btnStatus = 2;
        InitialStatus();
    }
    
    public void ChangeGun3()
    {
        _btnStatus = 3;
        InitialStatus();
    }
    
    public void ChangeGun4()
    {
        _btnStatus = 4;
        InitialStatus();
    }
    
    public void GetUlt()
    {
        Debug.Log("Ult");
    }

    private void CharacterMove()
    {
        float h, v;
        h = Joystick.Horizontal;
        v = Joystick.Vertical;

        // move
        var speed = _baseCharStat.GetStat(CharStat.Speed).Total * 5;
        moveDir = new Vector3(h, 0, v);
        moveDir = _transform.TransformDirection(moveDir);
        moveDir *= speed;

        //_characterRigidbody.velocity = new Vector3(h * _base.Speed, 0, v * _base.Speed);
        if (_isTargetNotNull == false)
        {
            if (!(h == 0 && v == 0))
            {
                _transform.rotation = Quaternion.Lerp(_transform.rotation, Quaternion.LookRotation(moveDir), Time.deltaTime * (speed * 0.1f));
            }
        }
        
        yVelocity += (gravity * Time.deltaTime);
        moveDir.y = yVelocity;
        
        _characterController.Move(moveDir * Time.deltaTime);
    }

    private void OnCollisionEnter()
    {
        if (moveDir.x != 0 && moveDir.z != 0)
        {
            yVelocity = 0;  
            if (_characterController.isGrounded)
            {
                yVelocity = jumpForce;
            }
        }
    }

    public void Jump()
    {
        yVelocity = jumpForce;
    }

    public Transform canvasTarget;
    public Transform gun;
    
    private void Update()
    {
        CharacterMove();
        
        if (Physics.Raycast(transform.position, transform.forward, out _raycast, maxDistance))
        {
            if (_raycast.transform.gameObject.name == "허수아비")
            {
                _isTargetNotNull = true;
                target = _raycast.transform.gameObject;
            }
        }
        else
        {
            _isTargetNotNull = false;
            target = null;
        }
        
        // target
        if(_isTargetNotNull) transform.LookAt(target.transform);
        
        gun.LookAt(canvasTarget);
        
        
        // 임시 자동공격
        _weapon.Attack();

        // Todo: 그냥 테스트용 코드
        TestUpdate();
    }
}
