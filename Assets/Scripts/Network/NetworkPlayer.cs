using System.Collections.Generic;
using DG.Tweening;
using Fusion;
using GameStatus;
using Types;
using UnityEngine;

namespace Network
{
    public class NetworkPlayer : NetworkBehaviour
    {
        #region 전투 관련
        private BaseStat<CharStat> _baseCharStat;
        public List<Synergy> synergyList = new List<Synergy>();

        [Networked(OnChanged = nameof(OnSynergyChange)), Capacity(30)]
        [UnitySerializeField]
        public NetworkLinkedList<int> NetworkSynergyList => default;

        public static void OnSynergyChange(Changed<NetworkPlayer> changed)
        {
            changed.Behaviour.OnSynergyChange();
        }
        
        public void OnSynergyChange()
        {
            synergyList.Clear();
            foreach (var num in NetworkSynergyList)
            {
                if (GameManager.Instance.GetSynergy(num, out var synergy))
                {
                    synergyList.Add(synergy);
                }
                else
                {
                    Debug.Log("시너지 범위 초과");
                }
            }

            InitialStatus();
        }

        #endregion
        
        #region 에임
        
        public Transform GunPos;
        public static bool isCameraFocused = false;
        public LineRenderer ShotLine;
        public LineRenderer UltLine;
        public RectTransform CrossHairTransform;
        
        #endregion

        #region 타겟 지정 관련 변수
        public GameObject target;
        private bool _isTargetNotNull;
        private RaycastHit _raycast;
        public float maxDistance = 10.0f;
        #endregion
        
        #region 움직임 관련 변수
        private VariableJoystick _joystick;
        private Transform _transform;
        private Vector3 moveDir;
        private float gravity = 15.0f;
        private float jumpForce = 7.0f;
        private float dodgeForce = 4.0f;
        public bool ReverseHorizontalMove = false;
        private bool isJump = false;
        private bool isDodge = false;
        private float shakeDodgeThreshold = 2.0f;
        private NetworkCharacterControllerPrototype _characterController;
        #endregion
        
        private GameManager _gameManager;
        private RoomUI RoomUI;

        public override void Spawned()
        {
            _isTargetNotNull = true;
            moveDir = Vector3.zero;
            CrossHairTransform = GameObject.Find("SubCrosshair").GetComponent<RectTransform>();
            _joystick = GameObject.Find("Variable Joystick").GetComponent<VariableJoystick>();
            target = GameObject.Find("허수아비");
            _transform = gameObject.transform;
            _baseCharStat = new BaseStat<CharStat>(1, 1);
            _characterController = GetComponent<NetworkCharacterControllerPrototype>();
            GunPos = transform.GetChild(0).transform;

            ShotLine = Instantiate(ShotLine);
            UltLine = Instantiate(UltLine);

            if (HasInputAuthority)
            {
                //_joystick = RoomUI.Joystick;
                _gameManager = GameManager.Instance;
                RoomUI = GameManager.Instance.UIHolder as RoomUI;
            }
            
            InitialStatus();
        }

        public override void FixedUpdateNetwork()
        {
            if (HasInputAuthority)
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
            
                // 임시 자동공격
                if (!isCameraFocused)
                {
                    Shoot(AttackType.Basic, ShotLine);
                    //Debug.DrawLine(GunPos.position, target.transform.position, Color.red);
                    //_weapon.Attack();
                }
                
                if (_joystick is not null)
                {
                    CharacterMove();
                }
            }
        }
        
        private void Shoot(AttackType attackType, LineRenderer lineRenderer)    // 라인렌더러는 임시
        {
            RaycastHit hit;
            Ray gunRay;
            
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
        
        private void CharacterMove()
        {
            var h = ReverseHorizontalMove ? -_joystick.Horizontal : _joystick.Horizontal;
            var v = _joystick.Vertical;

            var speed = _baseCharStat.GetStat(CharStat.Speed).Total;

            // move
            if (_characterController.IsGrounded)
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

            moveDir.y -= gravity * Runner.DeltaTime;

            _characterController.Move(moveDir * Runner.DeltaTime);

            if (_isTargetNotNull == false)
            {
                _transform.rotation = Quaternion.Lerp(_transform.rotation, Quaternion.LookRotation(moveDir), Runner.DeltaTime * (speed * 0.1f));
            }
            else if (isCameraFocused == false)
            {
                var relativePosition = target.transform.position - transform.position;
                relativePosition.y = 0; // y축은 바라보지 않도록 함
                var targetRotation = Quaternion.LookRotation(relativePosition);

                _transform.rotation = Quaternion.Lerp(_transform.rotation, targetRotation, Runner.DeltaTime * 8f);
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.gameObject.layer == LayerMask.NameToLayer("World") && hit.normal.y == 0)
            {
                if (_characterController.IsGrounded)
                {
                    isJump = true;
                }
            }
        }
        
        public void EndUlt()
        {
            //_weapon.Attack();
            Shoot(AttackType.Ultimate, UltLine);
            isCameraFocused = false;
            CanvasManager.Instance.SwitchUI(CanvasType.GameMoving);
        }
        

        #region 스탯
        
        public void InitialStatus()
        {
            InitialCharacterStatus();
            InitialWeaponStatus();
        }

        private void InitialCharacterStatus()
        {
            _baseCharStat.ClearStatList();
            foreach (var synergy in synergyList)
            {
                _baseCharStat.AddStatList(synergy.charStatList);
            }
        }

        private void InitialWeaponStatus()
        {
            var weaponList = GetComponentsInChildren<NetworkProjectileHolder>();
            foreach (var weapon in weaponList)
            {
                foreach (var synergy in synergyList)
                {
                    _baseCharStat.AddStatList(synergy.charStatList);
                    weapon.AddWeaponStatList(synergy.weaponStatList);
                }
            }
        }
        
        #endregion
    }
}