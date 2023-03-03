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
    public Autorifle autorifle;
    public Cannon cannon;
    public Shotgun shotgun;
    public Handgun handgun;
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
        InitialStatus();
        ApplyStatToBulletData();
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
            _base = new BaseStat();
            autorifle = GameObject.Find("Autorifle").GetComponent<Autorifle>();
            _weapon = autorifle._weapon;
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
            _base = new BaseStat();
            cannon = GameObject.Find("Cannon").GetComponent<Cannon>();
            _weapon = cannon._weapon;
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
            _base = new BaseStat();
            shotgun = GameObject.Find("Shotgun").GetComponent<Shotgun>();
            _weapon = shotgun._weapon;
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
            _base = new BaseStat();
            handgun = GameObject.Find("Handgun").GetComponent<Handgun>();
            _weapon = handgun._weapon;
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
