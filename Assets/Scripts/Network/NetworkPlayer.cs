using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.Tweening;
using Fusion;
using GameStatus;
using Types;
using UnityEngine;
using UIHolder;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;

namespace Network
{
    // 블록 히트 데이터
    public partial class NetworkPlayer
    {
        [Networked(OnChanged = nameof(UpdateBlockHitList)), Capacity(10)]
        private NetworkLinkedList<BlockHitData> BlockHitList => default;
        private int _blockHitLastTick = 0;

        private struct BlockHitData : INetworkStruct
        {
            public int Tick { get; set; }
            [Networked] public Vector3 BlockPos { get; set; }
            [Networked] public float Damage { get; set; }
        }
        
        public static void UpdateBlockHitList(Changed<NetworkPlayer> changed)
        {
            changed.Behaviour.UpdateBlockHitList();
        }

        private void UpdateBlockHitList()
        {
            if (!Object.HasStateAuthority)
            {
                int tick = 0;
                for (var i = 0; i < BlockHitList.Count; i++)
                {
                    var data = BlockHitList.Get(i);
                    tick = data.Tick;
                    if (_blockHitLastTick < data.Tick)
                    {
                        WorldManager.Instance.GetWorld().HitBlock(data.BlockPos, (int)data.Damage);
                    }
                }

                _blockHitLastTick = tick;
            }
        }

        public void AddBlockHitData(Vector3 pos, float damage)
        {
            if (BlockHitList.Count == BlockHitList.Capacity)
            {
                BlockHitList.Remove(BlockHitList.Get(0));
            }

            BlockHitList.Add(new BlockHitData
            {
                Tick = Runner.Tick,
                BlockPos = pos,
                Damage = damage
            });
        }
    }
    
    // 캐릭터 히트 데이터
    public partial class NetworkPlayer
    {
        [Networked(OnChanged = nameof(UpdateCharacterHit)), Capacity(10)]
        private NetworkLinkedList<CharacterHitData> CharacterHitList => default;
        private int _characterHitLastTick = 0;

        private struct CharacterHitData : INetworkStruct
        {
            public int Tick { get; set; }
            public NetworkId NetworkId { get; set; }
        }
        
        public static void UpdateCharacterHit(Changed<NetworkPlayer> changed)
        {
            changed.Behaviour.UpdateCharacterHit();
        }

        private void UpdateCharacterHit()
        {
            if (!Object.HasStateAuthority)
            {
                int tick = 0;
                for (var i = 0; i < CharacterHitList.Count; i++)
                {
                    var data = CharacterHitList.Get(i);
                    
                    if (_characterHitLastTick < data.Tick)
                    {
                        var networkObject = _gameManager.NetworkManager.FindNetworkObject(data.NetworkId);
                        if (networkObject is null)
                        {
                            Debug.LogError("해당 오브젝트가 리스트에 없음");
                        }
                        else
                        {
                            OnHit(networkObject);
                        }
                    }
                    
                    tick = data.Tick;
                }

                _characterHitLastTick = tick;
            }
        }

        private void AddCharacterHitData(NetworkObject networkObject)
        {
            if (CharacterHitList.Count == CharacterHitList.Capacity)
            {
                CharacterHitList.Remove(CharacterHitList.Get(0));
            }

            CharacterHitList.Add(new CharacterHitData
            {
                Tick = Runner.Tick,
                NetworkId = networkObject
            });
        }

        public void OnHit(NetworkObject projectile)
        {
            var damage = projectile.GetComponent<NetworkProjectileBase>().Damage;
            var armor = _baseCharStat.GetStat(CharStat.Armor).Total;
            var calcDamage = damage * (100 / (100 + armor));
            _nowHP -= calcDamage;
        }
        
        public void NetworkOnHit(NetworkObject networkObject)
        {
            OnHit(networkObject);
            AddCharacterHitData(networkObject);
        }
    }

    // 시너지
    public partial class NetworkPlayer
    {
        public List<Synergy> synergyList = new List<Synergy>();

        [Networked(OnChanged = nameof(OnSynergyChange)), Capacity(30)]
        [UnitySerializeField]
        public NetworkLinkedList<int> NetworkSynergyList => default;

        public static void OnSynergyChange(Changed<NetworkPlayer> changed)
        {
            changed.Behaviour.OnSynergyChange();
        }
        
        private void OnSynergyChange()
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
    }

    // 스탯
    public partial class NetworkPlayer
    {
        private BaseStat<CharStat> _baseCharStat;
        
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

            _maxHP = _nowHP = _baseCharStat.GetStat(CharStat.Health).Total;
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

        public Stat<CharStat> GetCharStat(CharStat type)
        {
            return _baseCharStat.GetStat(type);
        }
    }

    // 진동
    public partial class NetworkPlayer
    {
        private bool isVibrateBeat = false;
        
        private void VibrateUlt()
        {
            long[] pattern = 
                { 0, 60, 20, 30, 20, 5};
            int[] amplitudes = 
                { 0, 2, 0, 1, 0, 1 };

            RDG.Vibration.Vibrate(pattern, amplitudes, -1, true);
        }
        
        public void VibrateHeartBeat()
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
    }

    // 공격
    public partial class NetworkPlayer
    {
        private Ray _gunRay;
        private RaycastHit _hit;
        private float _shootDistance = 30f;
        private int _damage = 1;
        private bool isShooting = true;
        private const int shootRayMask = (int)Layer.Enemy | (int)Layer.World;

        public void ToggleShooting() { isShooting = !isShooting; }

        private void Shoot(AttackType attackType, LineRenderer lineRenderer)    // 라인렌더러는 임시
        {
            if (!isShooting) return;
            
            var aimRay = _camera.ScreenPointToRay(GetCrossHairPointInScreen());
            // 조준점으로 쏘는 레이의 원점이 플레이어 앞에서 시작되어야 한다.
            // 그렇지 않으면, 플레이어의 총알은 플레이어의 뒤에 있지만, 조준점에는 걸린 물체로 날아가게 된다. 한마디로 뒤로 쏘게 된다.
            var distCam = Vector3.Distance(_camera.transform.position, transform.position);
            var aimRayOrigin = aimRay.origin + aimRay.direction * distCam;
            
            /* 총알이 날아갈 지점 구하기 */
            _gunRay.origin = GunPos.position;
            
            //Debug.DrawRay(aimRayOrigin, aimRay.direction * _shootDistance, Color.blue, 0.3f);
            if (Physics.Raycast(aimRayOrigin, aimRay.direction, out _hit, _shootDistance, shootRayMask))
            {
                _gunRay.direction = (_hit.point - _gunRay.origin).normalized;
            }
            else
            {
                _gunRay.direction = ((aimRayOrigin + aimRay.direction * _shootDistance) - _gunRay.origin).normalized;
            }
            //Debug.DrawRay(_gunRay.origin, _gunRay.direction * _shootDistance, Color.magenta, 0.3f);
            targetPoint = _gunRay.origin + _gunRay.direction * _shootDistance;
            LaserBeam(_gunRay, _shootDistance, attackType, lineRenderer);

            RotateToTarget(GunPos, targetPoint, 8f, false);
        }

        private void LaserBeam(Ray gunRay, float aimDistance, AttackType attackType, LineRenderer lineRenderer)
        {
            lineRenderer.SetPosition(0, gunRay.origin);

            /* 실제 총알이 날아가는 경로 */
            if (Physics.Raycast(gunRay, out _hit, aimDistance, shootRayMask))
            {
                var point = _hit.point - _hit.normal * 0.01f;

                if (_hit.transform.gameObject == _target.gameObject)
                {
                    // 임시 헤드 판정
                    var isCritical = _hit.point.y - (_target.transform.position.y - 1) > 1.25f;
                    _gameUI.hitDamageText.GetComponent<HitDamage>().HitDamageAnimation(_damage, isCritical);
                }
                
                switch (attackType)
                {
                    case AttackType.Basic:
                        WorldManager.Instance.GetWorld().HitBlock(point, 1);
                        AddBlockHitData(point, 1);
                        break;

                    case AttackType.Ultimate:
                        WorldManager.Instance.GetWorld().ExplodeBlocks(point, 3, 3);
                        VibrateUlt();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(attackType), attackType, null);
                }

                targetPoint = _hit.point;
            }
            
            GetComponentInChildren<NetworkProjectileHolder>().target = targetPoint;
            lineRenderer.SetPosition(1, targetPoint);
        }

        private Vector3 GetCrossHairPointInScreen()
        {
            var position = _crossHair.position;
            return new Vector3(position.x, position.y, 0);
        }
    }
    
    public partial class NetworkPlayer : NetworkBehaviour
    {
        #region 에임
        public Transform GunPos;
        public static bool isCameraFocused = false;
        public LineRenderer ShotLine;
        public LineRenderer UltLine;
        private RectTransform _crossHair;
        #endregion

        #region 타겟 지정 관련 변수
        private GameObject _target;
        #endregion
        
        #region 움직임 관련 변수
        private Joystick _joystick;
        private CharacterController _characterController;
        private Transform _characterTransform;

        public Vector3 targetPoint;
        private Vector3 moveDir;
        private Vector3 jumpDir;
        private Vector3 lastMoveDir;
        private Vector3 _initPos;

        private float gravity = 15.0f;
        private float jumpForce = 7.0f;
        private float dodgeForce = 4.0f;
        private float shakeDodgeThreshold = 2.0f;

        public bool ReverseHorizontalMove = false;
        private bool isWalk = true;
        private bool isJump = false;
        private bool isDodge = false;
        #endregion

        private CanvasManager _canvasManager;
        private GameManager _gameManager;
        private GameUI _gameUI;
        private Camera _camera;
        private Animator _animator;
        
        public float _maxHP;
        public float _nowHP;

        public override void Spawned()
        {
            _initPos = transform.position;
            _gameManager = GameManager.Instance;
            _baseCharStat = new BaseStat<CharStat>(1, 1);
            InitialStatus();
            
            _gameUI = _gameManager.UIHolder as GameUI;
            if (_gameUI == null)
            {
                throw new Exception("ui holder가 null임");
            }
            
            _canvasManager = _gameUI.canvasManager;
            _crossHair = _gameUI.crossHair;
            _joystick = _gameUI.joystick;
            _camera = Camera.main;
            
            if (HasStateAuthority)
            {
                _camera.GetComponent<PlayerCamera>().AddPlayer(transform);
                _target = GameObject.Find("허수아비");
                
                // 혼자 테스트용 코드
                if (Runner.ActivePlayers.Count() == 1)
                {
                    _target = GameObject.Find("허수아비");
                    _camera.GetComponent<PlayerCamera>().AddEnemy(_target.transform);
                }
                _canvasManager.SwitchUI(CanvasType.GameMoving);
            }
            else
            {
                _target = GameObject.Find("허수아비");
                _camera.GetComponent<PlayerCamera>().AddEnemy(_target.transform);
                return;
            }
            
            moveDir = Vector3.zero;
            GunPos = transform; // 총 위치로 수정해야함.
            
            _characterController = GetComponent<CharacterController>();
            _characterTransform = transform.Find("Cat");
            _animator = _characterTransform.GetComponentInChildren<Animator>();

            ShotLine = Instantiate(ShotLine);
            UltLine = Instantiate(UltLine);

            // 자신은 타겟팅 되지 않기 위해 레이어 변경
            gameObject.layer = LayerMask.NameToLayer("Player");
        }

        public void SetGunPos(Transform transform)
        {
            GunPos = transform;
        }
        
        public override void FixedUpdateNetwork()
        {
            if(!HasStateAuthority) return;
            
            var shakeMagnitude = Input.acceleration.magnitude;

            if (shakeMagnitude > shakeDodgeThreshold)    //if (Input.GetKeyDown(KeyCode.Space) && !isDodge)
            {
                Dodge();
            }
        
            // 임시 자동공격
            if (!isCameraFocused)
            {
                Shoot(AttackType.Basic, ShotLine);
            }
            
            if (_joystick is not null)
            {
                CharacterMove();
            }
        }

        private void CharacterMove()
        {
            var h = ReverseHorizontalMove ? -_joystick.Horizontal : _joystick.Horizontal;
            var v = _joystick.Vertical;

            var speed = _baseCharStat.GetStat(CharStat.Speed).Total;

            if (isDodge)
            {
                var dodgeDir = lastMoveDir;

                dodgeDir.x *= dodgeForce;
                dodgeDir.z *= dodgeForce;
                
                _characterController.Move(dodgeDir * Runner.DeltaTime);

                LerpLookRotation(_characterTransform, lastMoveDir, 18f);
            }
            else
            {
                // move
                if (_characterController.isGrounded)
                {
                    if (isWalk)
                    {
                        if (h == 0 && v == 0)
                            _animator.SetInteger("animation", 1);
                        else
                        {
                            _animator.SetInteger("animation", 18);
                            _animator.SetFloat("walkSpeed", speed * 0.5f);
                        }
                    }

                    moveDir = new Vector3(h, 0, v);
                    moveDir = transform.TransformDirection(moveDir);
                    
                    if (moveDir.magnitude >= 0.3f)
                        lastMoveDir = moveDir;
                    
                    moveDir *= speed;
                }
                else
                {
                    jumpDir = new Vector3(h, 0, v);
                    jumpDir = transform.TransformDirection(jumpDir);
                    
                    if (jumpDir.magnitude >= 0.3f)
                        lastMoveDir = jumpDir;
                    
                    jumpDir *= (speed * 0.7f);

                    moveDir.x = jumpDir.x;
                    moveDir.z = jumpDir.z;
                }
                
                if (isJump)
                {
                    _animator.SetInteger("animation", 9);
                    moveDir.y = jumpForce;
                    isJump = false;
                    isWalk = true;
                }

                moveDir.y -= gravity * Runner.DeltaTime;
                _characterController.Move(moveDir * Runner.DeltaTime);
            }

            if (isCameraFocused == false)
            {
                RotateToTarget(transform, _target.transform.position, 8f, true);
            }

            if (!isDodge)
                RotateCharacterMoveDir(h, v);
        }

        public void Dodge()
        {
            if (!isDodge)
            {
                _animator.Rebind();
                _animator.SetInteger("animation", 8);
                isDodge = true;
                DOTween.Sequence()
                    .AppendInterval(0.1f)
                    .OnComplete(() =>
                    {
                        isDodge = false;
                        isWalk = true;
                    });
            }
        }

        public void Jump()
        {
            if (_characterController.isGrounded && !isJump)
            {
                isJump = true;
            }
        }

        public void InitPosition()
        {
            _characterController.enabled = false;
            transform.position = _initPos;
            _characterController.enabled = true;
        }

        private void RotateToTarget(Transform origin, Vector3 targetPos, float speed, bool lockAxisY)
        {
            var relativePosition = targetPos - origin.position;
            if (lockAxisY)
                relativePosition.y = 0; // y축은 바라보지 않도록 함

            LerpLookRotation(origin, relativePosition, speed);
        }

        private void RotateCharacterMoveDir(float h, float v)
        {
            // 캐릭터를 이동 방향대로 회전
            var characterDir = new Vector3(h, 0, v);
            var characterRotateSpeed = 8f;
            
            characterDir = transform.TransformDirection(characterDir);
            
            if (characterDir.magnitude < 0.5f)
            {
                characterDir = _target.transform.position - _characterTransform.position;
                characterDir.y = 0;
            }

            if (!_characterController.isGrounded)
            {
                characterRotateSpeed = 3f;
            }

            LerpLookRotation(_characterTransform, characterDir, characterRotateSpeed);
        }

        private void LerpLookRotation(Transform origin, Vector3 dir, float speed)
        {
            var targetRotation = Quaternion.LookRotation(dir);
            origin.rotation = Quaternion.Lerp(origin.rotation, targetRotation, Runner.DeltaTime * speed);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.gameObject.layer == LayerMask.NameToLayer("World") && hit.normal.y == 0)
            {
                Jump();
            }
        }
    }

    public partial class NetworkPlayer
    {
        public void GetUlt()
        {
            _joystick.OnPointerUp(null);    // joystick 입력값 초기화
            isCameraFocused = !isCameraFocused;
            _canvasManager.SwitchUI(CanvasType.GameAiming);
        }

        public void EndUlt()
        {
            Shoot(AttackType.Ultimate, UltLine);
            isCameraFocused = false;
            _canvasManager.SwitchUI(CanvasType.GameMoving);
        }
    }

    // 설정 UI
    public partial class NetworkPlayer
    {
        private List<Stat<CharStat>> _settingsStatlist = new();
        
        public string IncreaseSpeed()
        {
            _settingsStatlist.Add(new Stat<CharStat>(CharStat.Speed, 1, 0).SetRatio(0));
            return AdditionalWork(CharStat.Speed);
        }
        
        public string DecreaseSpeed()
        {
            _settingsStatlist.Add(new Stat<CharStat>(CharStat.Speed, -1, 0).SetRatio(0));
            return AdditionalWork(CharStat.Speed);
        }
        
        private string AdditionalWork(CharStat type)
        {
            InitialStatus();

            foreach (var s in _settingsStatlist)
            {
                _baseCharStat.AddStat(s);
            }
        
            var total = _baseCharStat.GetStat(type).Total;

            return total.ToString();
        }
        
        public string IncreaseJump() { return (++jumpForce).ToString(); }
        public string DecreaseJump() { return (--jumpForce).ToString(); }

        public string IncreaseDodge(){ return (++dodgeForce).ToString(); }
        public string DecreaseDodge(){ return (--dodgeForce).ToString(); }

        public string IncreaseShakeSensitivity()
        {
            return (shakeDodgeThreshold += 0.1f).ToString("F1");
        }
        public string DecreaseShakeSensitivity()
        {
            return (shakeDodgeThreshold -= 0.1f).ToString("F1");
        }
        
        public string IncreaseShootDistance() { return (_shootDistance += 5).ToString(); }
        public string DecreaseShootDistance() { return (_shootDistance -= 5).ToString(); }

        public void ToggleReverseHorizontalMove(bool state) { ReverseHorizontalMove = state; }
    }
}