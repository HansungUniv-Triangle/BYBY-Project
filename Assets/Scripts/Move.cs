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
    public VariableJoystick Joystick;

    RaycastHit _raycast;
    public float maxDistance = 10.0f;

    #region Target
    public Transform target;
    public Transform enemy;
    private bool _isTargetNotNull;
    #endregion

    #region Start
    public Transform _transform;
    public Transform _puppet;
    private CharacterController _characterController;
    private Gun _gun;
    #endregion
 
    private GameManager _gameManager;
    public List<Synergy> synergyList = new List<Synergy>();
    private int BtnStatus = 0;
    private bool isBtnClicked = false;

    private Vector3 moveDir;
    private float gravity = -10.0f;
    private float jumpForce = 5.0f;
    public float yVelocity = 0.0f;

    Vector3 viewPos;
    Camera _camera;

    private void Awake()
    {
        _base = new BaseStat();
        _gameManager = GameManager.Instance;
        //_isTargetNotNull = target is not null;
        _isTargetNotNull = false;
        _characterController = GetComponent<CharacterController>();
        _gun = transform.GetComponentInChildren<Gun>();
        _camera = GameObject.Find("Main Camera").GetComponent<Camera>();
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
        if (isBtnClicked == false)
        {
            Debug.Log(viewPos);
            isBtnClicked = true;
        }
        else
        {
            isBtnClicked = false;
        }
    }

    public void EnemyAppear()
    {
        _isTargetNotNull = true;
    }

    public void EnemyDismiss()
    {
        target = null;
        _isTargetNotNull = false;
    }

    private void CharacterMove()
    {
        float h, v;
        h = Joystick.Horizontal;
        v = Joystick.Vertical;

        // move
        moveDir = new Vector3(h, 0, v);
        moveDir = _transform.TransformDirection(moveDir);
        moveDir *= _base.Speed;
        
        if(_isTargetNotNull == false)
        {
            if (!(h == 0 && v == 0))
            {
                _transform.rotation = Quaternion.Lerp(_transform.rotation, Quaternion.LookRotation(moveDir), Time.deltaTime * _base.Speed);
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

    void Update()
    {
        CharacterMove();
        
        viewPos = _camera.WorldToViewportPoint(enemy.position);
        Collider[] cols = Physics.OverlapSphere(transform.position, 10f);
      
        if (isBtnClicked == true)
        {
            if (viewPos.x > 0.3f && viewPos.x < 0.7f)
            {
                if (cols.Length > 0)
                {
                    for (int i = 0; i < cols.Length; i++)
                    {
                        if (cols[i].tag == "Player")
                        {
                            target = cols[i].gameObject.transform;
                            EnemyAppear();
                        }
                    }
                }   
            }
        }
        else
        {
            EnemyDismiss();
        }

        var inputSpace = Input.GetButton("Jump");
        // fire gun
        _fireCoolTime += Time.deltaTime;
        if (inputSpace && _fireCoolTime > _base.FireRate)
        {
            _gun.Shoot(_base.ShotAtOnce);
            _fireCoolTime = 0;
        }

        // target
        //if(_isTargetNotNull) transform.LookAt(target.transform);
        

        // Todo: 그냥 테스트용 코드
        TestUpdate();
    }

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 dir = target.position - transform.position;
            transform.LookAt(dir);
        }
    }
}
