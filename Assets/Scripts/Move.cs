using System.Collections.Generic;
using UnityEngine;
using Status;

public class Move : MonoBehaviour
{
    #region Test Code
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
    
    private BaseStat _base;
    [SerializeField]
    private WeaponData _weapon;
    [SerializeField]
    private WeaponData Autorifle;
    [SerializeField]
    private WeaponData Cannon;
    [SerializeField]
    private WeaponData Shotgun;
    [SerializeField]
    private WeaponData Handgun;
    private float _fireCoolTime = 0f;

    #region Target
    public GameObject target;
    private bool _isTargetNotNull;
    #endregion

    #region Start
    private Rigidbody _characterRigidbody;
    private Gun _gun;
    #endregion

    private GameManager _gameManager;
    public List<Synergy> synergyList = new List<Synergy>();
    private int BtnStatus = 0;

    private void Awake()
    {
        _base = new BaseStat();
        _gameManager = GameManager.Instance;
        _isTargetNotNull = target is not null;
        _characterRigidbody = GetComponent<Rigidbody>();
        _gun = transform.GetComponentInChildren<Gun>();
    }

    private void Start()
    {
        SetupWeapon();
        InitialStatus();
        ApplyStatToBulletData();
    }

    private void SetupWeapon()
    {
        _gameManager.weaponList.Add(Autorifle);
        _gameManager.weaponList.Add(Cannon);
        _gameManager.weaponList.Add(Shotgun);
        _gameManager.weaponList.Add(Handgun);
    }

    private void InitialStatus()
    {
        _base.AddRatioStat(_weapon.statList);
        foreach (var synergy in synergyList)
        {
            _base.AddRatioStat(synergy.statList);
        }
    }

    private void ApplyStatToBulletData()
    {
        _gameManager.PlayerBulletData.maxRange = _base.Range;
        _gameManager.PlayerBulletData.size = _base.Size;
        _gameManager.PlayerBulletData.damage = _base.Damage;
        _gameManager.PlayerBulletData.shield = _base.Shield;
        _gameManager.PlayerBulletData.velocity = _base.Velocity;
    }

    public void ChangeGun1()
    {
        if (BtnStatus != 1)
        {
            _base.ClearStatList();
            _weapon = _gameManager.weaponList.Find(x => x.weaponName == "라이플");
            InitialStatus();
            ApplyStatToBulletData();
            BtnStatus = 1;
        }
    }

    public void ChangeGun2()
    {
        if (BtnStatus != 2)
        {
            _base.ClearStatList(); 
            _weapon = _gameManager.weaponList.Find(x => x.weaponName == "캐논");
            InitialStatus();
            ApplyStatToBulletData();
            BtnStatus = 2;
        }
    }

    public void ChangeGun3()
    {
        if (BtnStatus != 3)
        {
            _base.ClearStatList();
            _weapon = _gameManager.weaponList.Find(x => x.weaponName == "샷건");
            InitialStatus();
            ApplyStatToBulletData();
            BtnStatus = 3;
        }
    }

    public void ChangeGun4()
    {
        if (BtnStatus != 4)
        {
            _base.ClearStatList();
            _weapon = _gameManager.weaponList.Find(x => x.weaponName == "핸드건");
            InitialStatus();
            ApplyStatToBulletData();
            BtnStatus = 4;
        }
    }
    public void GetUlt()
    {
        Debug.Log("Ult");
    }

    public void CharacterMove(Vector2 inputVector)
    {
        float h, v;
        Vector2 moveInput = inputVector;
        h = moveInput.x;
        v = moveInput.y;

        // move
        _characterRigidbody.velocity = new Vector3(h * _base.Speed, 0, v * _base.Speed);
    }

    private void Update()
    {
        var inputSpace = Input.GetButton("Jump");
        // fire gun
        _fireCoolTime += Time.deltaTime;
        if (inputSpace && _fireCoolTime > _base.FireRate)
        {
            _gun.Shoot(_base.ShotAtOnce);
            _fireCoolTime = 0;
        }
        
        // target
        if(_isTargetNotNull) transform.LookAt(target.transform);
        
        // Todo: 그냥 테스트용 코드
        TestUpdate();
    }
}
