using System.Collections.Generic;
using DG.Tweening;
using GameStatus;
using Type;
using UnityEngine;
using Weapon;

public class Move : MonoBehaviour
{
    #region 테스트용 코드 (스탯 적용)
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
    private float gravity = 15.0f;
    private float jumpForce = 7.0f;
    private float dodgeForce = 4.0f;
    private bool isJump = false;
    private bool isDodge = false;
    private float shakeDodgeThreshold = 2.0f;
    #endregion

    #region 전투 관련
    private BaseStat<CharStat> _baseCharStat;
    public List<Synergy> synergyList = new List<Synergy>();
    public List<WeaponBase> weapons = new List<WeaponBase>();
    private WeaponBase _weapon => weapons[_btnStatus - 1];
    private int _btnStatus = 0;

    public LineRenderer ShotLine;
    public LineRenderer UltLine;
    #endregion

    public bool isCameraFocused = false;
    public Transform CameraObj;
    public Transform CameraPos;
    public Transform CameraFocusPos;
    public Transform GunPos;

    private GameManager _gameManager;
    
    private void Awake()
    {
        _btnStatus = 1;
        _transform = gameObject.transform;
        _baseCharStat = new BaseStat<CharStat>();
        _gameManager = GameManager.Instance;
        _isTargetNotNull = true;

        _characterController = GetComponent<CharacterController>();
        
        weapons.Add(gameObject.AddComponent<HandGun>());
        weapons.Add(gameObject.AddComponent<ShieldGenerator>());
        // 임시 칸 채우기 용도
        weapons.Add(gameObject.AddComponent<HandGun>());
        weapons.Add(gameObject.AddComponent<ShieldGenerator>());
    }

    private void Start()
    {
        moveDir = Vector3.zero;
        InitialStatus();
        CanvasManager.Instance.SwitchUI(CanvasType.GameMoving);
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
    
    public void ChangeGun3()
    {
        InitialStatus();
        _btnStatus = 3;
    }
    
    public void ChangeGun4()
    {
        InitialStatus();
        _btnStatus = 4;
    }
    
    public void GetUlt()
    {
        isCameraFocused = !isCameraFocused;
        CanvasManager.Instance.SwitchUI(CanvasType.GameAiming);
    }

    public void EndUlt()
    {
        UltShoot();
        //_weapon.Attack();
        isCameraFocused = false;
        CanvasManager.Instance.SwitchUI(CanvasType.GameMoving);
    }

    private void UltShoot()
    {
        // ultimate shot
        RaycastHit hit;
        Ray gunRay;

        var screenCenter = new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2);        // 화면 중앙 (크로스헤어)
        var aimRay = Camera.main.ScreenPointToRay(screenCenter);
        var aimDistance = 30f;
        
        if (Physics.Raycast(aimRay, out hit, aimDistance) && hit.transform.gameObject != gameObject)    // 발사하는 주체는 제외
        {
            gunRay = new Ray(GunPos.position, (hit.point - GunPos.position).normalized);
        }
        else
        {
            gunRay = new Ray(GunPos.position, ((aimRay.origin + aimRay.direction * aimDistance) - GunPos.position).normalized);
        }
        
        UltLine.SetPosition(0, gunRay.origin);
        UltLine.SetPosition(1, gunRay.origin + gunRay.direction * aimDistance);
        //Debug.DrawRay(gunRay.origin, gunRay.direction * aimDistance, Color.cyan, 5f);

        if (Physics.Raycast(gunRay, out hit, aimDistance))   //if (Physics.Raycast(gunRay, out RaycastHit hit, aimDistance, (int)Layer.World))
        {
            var point = hit.point - hit.normal * 0.1f;
            WorldManager.Instance.GetWorld().ExplodeBlocks(point, 3, 3);

            UltLine.SetPosition(1, hit.point);
            //Debug.DrawLine(gunRay.origin, hit.point, Color.cyan, 5f);
        }
    }

    private void CameraMove()
    {
        if (isCameraFocused)
        {
            CameraObj.position = Vector3.Lerp(CameraObj.position, CameraFocusPos.position, Time.deltaTime * 4f);
        }
        else
        {
            CameraObj.position = Vector3.Lerp(CameraObj.position, CameraPos.position, Time.deltaTime * 4f);
        }
    }

    private void CharacterMove()
    {
        var h = Joystick.Horizontal;
        var v = Joystick.Vertical;

        var speed = _baseCharStat.GetStat(CharStat.Speed).Total;

        // move
        if (_characterController.isGrounded)
        {    
            moveDir = new Vector3(h, 0, v);
            moveDir = _transform.TransformDirection(moveDir);
            moveDir *= speed;
        }
        else
        {
            var tmp = new Vector3(h, 0, v);
            tmp = _transform.TransformDirection(tmp);
            tmp *= (speed * 0.7f);

            moveDir.x = tmp.x;
            moveDir.z = tmp.z;
        }

        if (isJump)
        {
            moveDir.y = jumpForce;
            isJump = false;
        }

        if (isDodge)
        {
            moveDir.x *= dodgeForce;
            moveDir.z *= dodgeForce;
        }

        moveDir.y -= gravity * Time.deltaTime;

        _characterController.Move(moveDir * Time.deltaTime);

        if (_isTargetNotNull == false)
        {
            _transform.rotation = Quaternion.Lerp(_transform.rotation, Quaternion.LookRotation(moveDir), Time.deltaTime * (speed * 0.1f));
        }
        else if (isCameraFocused == false)
        {
            var relativePosition = target.transform.position - transform.position;
            var targetRotation = Quaternion.LookRotation(relativePosition);

            _transform.rotation = Quaternion.Lerp(_transform.rotation, targetRotation, Time.deltaTime * 4f);
            //_transform.localRotation = Quaternion.Lerp(_transform.localRotation, Quaternion.Euler(h * 20, 0, v * 20), Time.deltaTime * 4f);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.layer == LayerMask.NameToLayer("World") && hit.normal.y == 0)
        {
            if (_characterController.isGrounded)
            {
                isJump = true;
            }
        }
    }

    private void Update()
    {
        var shakeMagnitude = Input.acceleration.magnitude;

        if (shakeMagnitude > shakeDodgeThreshold && !isDodge)    //if (Input.GetKeyDown(KeyCode.Space) && !isDodge)
        {
            isDodge = true;
            DOTween.Sequence()
                .AppendInterval(0.1f)
                .OnComplete(() =>
                {
                    isDodge = false;
                });
        }
        CameraMove();
        CharacterMove();
        
        // 임시 자동공격
        if (!isCameraFocused)
        {
            BasicShoot();
            //Debug.DrawLine(GunPos.position, target.transform.position, Color.red);
            //_weapon.Attack();
        }

        // Todo: 그냥 테스트용 코드
        TestUpdate();
    }

    private void BasicShoot()
    {
        ShotLine.SetPosition(0, GunPos.position);
        ShotLine.SetPosition(1, target.transform.position);

        var gunRay = new Ray(GunPos.position, target.transform.position - GunPos.position);
        if (Physics.Raycast(gunRay, out RaycastHit hit, 50))
        {
            var point = hit.point - hit.normal * 0.01f;
            WorldManager.Instance.GetWorld().HitBlock(point, 1);

            ShotLine.SetPosition(1, hit.point);
            //Debug.DrawLine(gunRay.origin, hit.point, Color.cyan, 5f);
        }
    }
}
