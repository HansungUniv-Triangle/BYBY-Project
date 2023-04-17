using System.Collections.Generic;
using DG.Tweening;
using GameStatus;
using TMPro;
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
    public static float targetDistance;

    private RaycastHit _hit;
    private Ray _gunRay;
    #endregion

    #region 움직임 관련 변수
    public Joystick Joystick;

    private CharacterController _characterController;
    private Vector3 moveDir;
    private float gravity = 15.0f;
    private float jumpForce = 7.0f;
    private float dodgeForce = 4.0f;
    private bool ReverseHorizontalMove = false;
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
    private float _shootDistance = 30f;
    #endregion

    #region UI Settings
    private CanvasManager _canvasManager;
    private List<Stat<CharStat>> _statlist = new();

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

    public void IncreaseSpeed(GameObject text)
    {
        _statlist.Add(new Stat<CharStat>(CharStat.Speed, 1).SetRatio(0));
        AdditionalWork(text, CharStat.Speed);
    }

    public void DecreaseSpeed(GameObject text)
    {
        _statlist.Add(new Stat<CharStat>(CharStat.Speed, -1).SetRatio(0));
        AdditionalWork(text, CharStat.Speed);
    }

    private void AdditionalWork(GameObject text, CharStat type)
    {
        InitialStatus();

        foreach (var s in _statlist)
        {
            _baseCharStat.AddStat(s);
        }
        
        var total = _baseCharStat.GetStat(type).Total;
        text.GetComponent<TextMeshProUGUI>().text = total.ToString();
    }

    public void IncreaseJump(GameObject text) { text.GetComponent<TextMeshProUGUI>().text = (++jumpForce).ToString(); }
    public void DecreaseJump(GameObject text) { text.GetComponent<TextMeshProUGUI>().text = (--jumpForce).ToString(); }

    public void IncreaseDodge(GameObject text){ text.GetComponent<TextMeshProUGUI>().text = (++dodgeForce).ToString(); }
    public void DecreaseDodge(GameObject text){ text.GetComponent<TextMeshProUGUI>().text = (--dodgeForce).ToString(); }

    public void IncreaseShootDistance(GameObject text) { text.GetComponent<TextMeshProUGUI>().text = (_shootDistance += 5).ToString(); }
    public void DecreaseShootDistance(GameObject text) { text.GetComponent<TextMeshProUGUI>().text = (_shootDistance -= 5).ToString(); }

    public void IncreaseShakeSensitivity(GameObject text)
    {
        text.GetComponent<TextMeshProUGUI>().text = (shakeDodgeThreshold += 0.1f).ToString("F1");
    }
    public void DecreaseShakeSensitivity(GameObject text)
    {
        text.GetComponent<TextMeshProUGUI>().text = (shakeDodgeThreshold -= 0.1f).ToString("F1");
    }
    public void ToggleReverseHorizontalMove() { ReverseHorizontalMove = !ReverseHorizontalMove; }

    private JoystickSettingType joystickType = JoystickSettingType.Floating;
    [Space(5f)]
    public VariableJoystick variableJoystick;
    public FloatingJoystick floatingJoystick;
    public void ChangeJoystick(GameObject text)
    {
        switch (joystickType) 
        {
            case JoystickSettingType.Variable:
                floatingJoystick.gameObject.SetActive(true);
                floatingJoystick.GetComponent<CanvasController>().canvasType = CanvasType.GameMoving;

                joystickType = JoystickSettingType.Floating;
                Joystick = floatingJoystick;

                variableJoystick.GetComponent<CanvasController>().canvasType = CanvasType.None;
                variableJoystick.gameObject.SetActive(false);

                text.GetComponent<TextMeshProUGUI>().text = "floating";
                break;

            case JoystickSettingType.Floating:
                variableJoystick.gameObject.SetActive(true);
                variableJoystick.GetComponent<CanvasController>().canvasType = CanvasType.GameMoving;

                joystickType = JoystickSettingType.Variable;
                Joystick = variableJoystick;

                floatingJoystick.gameObject.SetActive(false);
                floatingJoystick.GetComponent<CanvasController>().canvasType = CanvasType.None;

                text.GetComponent<TextMeshProUGUI>().text = "variable";
                break;
        }
    }

    private Vector3 _initPos;
    public void InitPosition()
    {
        _characterController.enabled = false;
        transform.position = _initPos;
        _characterController.enabled = true;
    }

    private bool attacking = true;
    public void ToggleAttacking()
    {
        attacking = !attacking;
    }

    public void vibrate()
    {
        long[] pattern = 
            { 0, 60, 20, 30, 20, 5};
        int[] amplitudes = 
            { 0, 2, 0, 1, 0, 1 };

        RDG.Vibration.Vibrate(pattern, amplitudes, -1, true);

        if (isVibrateBeat)
        {
            isVibrateBeat = false;
            vibrateBeat();
        }
    }

    private bool isVibrateBeat = false;

    public void vibrateBeat()
    {
        if (isVibrateBeat)
        {
            RDG.Vibration.Cancel();
            isVibrateBeat = false;
        }
        else
        {
            long[] pattern = { 1000, 20, 1000, 20 };
            int[] amplitudes = { 0, 1 };

            RDG.Vibration.Vibrate(pattern, amplitudes, 0);
            isVibrateBeat = true;
        }
    }

    #endregion

    private GameManager _gameManager;

    public TextMeshProUGUI HitDamageTextMesh;

    private void Awake()
    {
        _btnStatus = 1;
        _initPos = transform.position;

        _baseCharStat = new BaseStat<CharStat>();
        _gunRay = new Ray();

        _gameManager = GameManager.Instance;
        _canvasManager = CanvasManager.Instance;
        
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
        _canvasManager.SwitchUI(CanvasType.GameMoving);
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

    public void GetUlt()
    {
        isCameraFocused = !isCameraFocused;
        _canvasManager.SwitchUI(CanvasType.GameAiming);
    }

    public void EndUlt()
    {
        //_weapon.Attack();
        Shoot(AttackType.Ultimate, UltLine);
        isCameraFocused = false;
        _canvasManager.SwitchUI(CanvasType.GameMoving);
    }

    public float GetSpeed()
    {
        return _baseCharStat.GetStat(CharStat.Speed).Total;
    }

    private void CharacterMove()
    {
        if (target)
        {
            // y값은 고려하지 않음
            var playerPos = new Vector3(transform.position.x, 0, transform.position.z);
            var targetPos = new Vector3(target.transform.position.x, 0, target.transform.position.z);

            targetDistance = Vector3.Distance(playerPos, targetPos);
        }
        
        var h = ReverseHorizontalMove ? -Joystick.Horizontal : Joystick.Horizontal;
        var v = Joystick.Vertical;
        v = (targetDistance < 2f && v > 0) ? 0 : v;

        var speed = GetSpeed();

        // move
        if (_characterController.isGrounded)
        {
            moveDir = new Vector3(h, 0, v);
            moveDir = transform.TransformDirection(moveDir);
            moveDir *= speed;
        }
        else
        {
            var tmp = new Vector3(h, 0, v);
            tmp = transform.TransformDirection(tmp);
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

        if (isCameraFocused == false)
        {
            var relativePosition = target.transform.position - transform.position;
            relativePosition.y = 0; // y축은 바라보지 않도록 함
            var targetRotation = Quaternion.LookRotation(relativePosition);

            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * speed);
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
            if (attacking)
                Shoot(AttackType.Basic, ShotLine);
            //_weapon.Attack();
        }

        // Todo: 그냥 테스트용 코드
        TestUpdate();
    }

    public int damage = 1;

    // ReSharper disable Unity.PerformanceAnalysis
    private void Shoot(AttackType attackType, LineRenderer lineRenderer)    // 라인렌더러는 임시
    {
        var aimRay = Camera.main.ScreenPointToRay(GetCrosshairPointInScreen());
        
        _gunRay.origin = GunPos.position;
        // 화면 중앙으로 쏘는 레이는 원점이 플레이어 앞에서 시작되어야 한다.
        // 그렇지 않으면 플레이어는 크로스헤어에는 걸렸지만, 뒤에 있는 물체를 부수게 된다.
        // 발사하는 주체는 제외
        if (Physics.Raycast(aimRay.origin + aimRay.direction * 10, aimRay.direction, out _hit, _shootDistance) && _hit.transform.gameObject != gameObject)
        {
            _gunRay.direction = (_hit.point - GunPos.position).normalized;
        }
        else
        {
            _gunRay.direction = ((aimRay.origin + aimRay.direction * 10 + aimRay.direction * _shootDistance) - GunPos.position).normalized;
        }

        lineRenderer.SetPosition(0, _gunRay.origin);
        lineRenderer.SetPosition(1, _gunRay.origin + _gunRay.direction * _shootDistance);

        if (Physics.Raycast(_gunRay, out _hit, _shootDistance))
        {
            var point = _hit.point - _hit.normal * 0.01f;

            if (_hit.transform.gameObject == target.gameObject)
            {
                bool isCritical = false;
                if (_hit.point.y - (target.transform.position.y - 1) > 1.25f)
                    isCritical = true;

                HitDamageTextMesh.GetComponent<HitDamage>().HitDamageAnimation(damage, isCritical);
            }

            switch (attackType)
            {
                case AttackType.Basic:
                    WorldManager.Instance.GetWorld().HitBlock(point, 1);
                    break;

                case AttackType.Ultimate:
                    WorldManager.Instance.GetWorld().ExplodeBlocks(point, 3, 3);
                    vibrate();
                    break;
            }

            lineRenderer.SetPosition(1, _hit.point);
        }
    }

    private Vector3 GetCrosshairPointInScreen()
    {
        return new Vector3(CrossHairTransform.transform.position.x, CrossHairTransform.transform.position.y, 0);
    }
}
