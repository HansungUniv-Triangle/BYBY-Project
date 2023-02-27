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

    private void Update()
    {
        var inputX = Input.GetAxis("Horizontal");
        var inputZ = Input.GetAxis("Vertical");
        var inputSpace = Input.GetButton("Jump");
        
        // move
        _characterRigidbody.velocity = new Vector3(inputX * _base.Speed, 0, inputZ * _base.Speed);
        
        // fire gun
        _fireCoolTime += Time.deltaTime;
        if (inputSpace && _fireCoolTime > _base.FireRate)
        {
            _gun.Shoot(_base.ShotAtOnce + 5);
            _fireCoolTime = 0;
        }
        
        // target
        if(_isTargetNotNull) transform.LookAt(target.transform);
        
        // Todo: 그냥 테스트용 코드
        TestUpdate();
    }
}
