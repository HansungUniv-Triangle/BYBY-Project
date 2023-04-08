using System.Collections.Generic;
using DG.Tweening;
using GameStatus;
using Types;
using UnityEngine;
using UnityEngine.UI;
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
    public bool ReverseHorizontalMove = false;
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

    public Transform GunPos;
    public static bool isCameraFocused = false;

    public LineRenderer ShotLine;
    public LineRenderer UltLine;
    
    public RectTransform CrossHairTransform;
    #endregion

    private GameManager _gameManager;

    private void Awake()
    {
        _btnStatus = 1;
        _transform = gameObject.transform;
        _baseCharStat = new BaseStat<CharStat>(1, 1);
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
        //_weapon.Attack();
        Shoot(AttackType.Ultimate, UltLine);
        isCameraFocused = false;
        CanvasManager.Instance.SwitchUI(CanvasType.GameMoving);
    }

    private void CharacterMove()
    {
        var h = ReverseHorizontalMove ? -Joystick.Horizontal : Joystick.Horizontal;
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
            relativePosition.y = 0; // y축은 바라보지 않도록 함
            var targetRotation = Quaternion.LookRotation(relativePosition);

            _transform.rotation = Quaternion.Lerp(_transform.rotation, targetRotation, Time.deltaTime * 8f);
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
        CharacterMove();

        // 임시 자동공격
        if (!isCameraFocused)
        {
            Shoot(AttackType.Basic, ShotLine);
            //Debug.DrawLine(GunPos.position, target.transform.position, Color.red);
            //_weapon.Attack();
        }

        // Todo: 그냥 테스트용 코드
        TestUpdate();
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void Shoot(AttackType attackType, LineRenderer lineRenderer)    // 라인렌더러는 임시
    {
        RaycastHit hit;
        Ray gunRay;

        //var screenCenter = new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2);        // 화면 중앙 (크로스헤어)
        //var aimRay = Camera.main.ScreenPointToRay(screenCenter);
        var aimRay = Camera.main.ScreenPointToRay(GetCrosshairPointInScreen());
        var aimDistance = 30f;

        // 화면 중앙으로 쏘는 레이는 원점이 플레이어 앞에서 시작되어야 한다.
        // 그렇지 않으면 플레이어는 크로스헤어에는 걸렸지만, 뒤에 있는 물체를 부수게 된다.
        // 발사하는 주체는 제외
        if (Physics.Raycast(aimRay.origin + aimRay.direction * 10, aimRay.direction, out hit, aimDistance) && hit.transform.gameObject != gameObject)
        {
            gunRay = new Ray(GunPos.position, (hit.point - GunPos.position).normalized);
        }
        else
        {
            gunRay = new Ray(GunPos.position, ((aimRay.origin + aimRay.direction * 10 + aimRay.direction * aimDistance) - GunPos.position).normalized);
        }

        lineRenderer.SetPosition(0, gunRay.origin);
        lineRenderer.SetPosition(1, gunRay.origin + gunRay.direction * aimDistance);

        if (Physics.Raycast(gunRay, out hit, aimDistance))
        {
            var point = hit.point - hit.normal * 0.01f;

            switch (attackType)
            {
                case AttackType.Basic:
                    WorldManager.Instance.GetWorld().HitBlock(point, 1);
                    break;

                case AttackType.Ultimate:
                    WorldManager.Instance.GetWorld().ExplodeBlocks(point, 3, 3);
                    break;
            }

            lineRenderer.SetPosition(1, hit.point);
        }
    }

    private Vector3 GetCrosshairPointInScreen()
    {
        return new Vector3(CrossHairTransform.transform.position.x, CrossHairTransform.transform.position.y, 0);
    }
}
