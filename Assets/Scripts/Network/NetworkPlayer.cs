using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Fusion;
using GameStatus;
using Types;
using UnityEngine;
using UIHolder;

namespace Network
{
    // HP canvas
    public partial class NetworkPlayer
    {
        [Networked] private float MaxHp { get; set; }
        [Networked(OnChanged = nameof(OnHpChanged))] private float NowHp { get; set; }

        private static void OnHpChanged(Changed<NetworkPlayer> changed)
        {
            changed.Behaviour.OnHpChanged();
        }

        private void OnHpChanged()
        {
            if (HasStateAuthority)
            {
                _gameUI.playerHpBarImage.DOFillAmount(NowHp / MaxHp, 0.5f);
            }
            else
            {
                _gameUI.enemyHpBarImage.DOFillAmount(NowHp / MaxHp, 0.5f);
            }
        }

        public void Healing(float point)
        {
            // Todo: 힐링 파티클 있으면 좋을 듯?
            NowHp += point;
            if (MaxHp < NowHp)
            {
                NowHp = MaxHp;
            }
            Debug.Log("힐링 되엇ㅅ브니다");
        }

        public float GetNowHp()
        {
            return NowHp;
        }
        
        public float GetMaxHp()
        {
            return MaxHp;
        }
    }
    
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
            public int Damage { get; set; }
            public NetworkBool Explode { get; set; }
            public int Radius { get; set; }
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
                        if (data.Explode)
                        {
                            WorldManager.Instance.GetWorld().ExplodeBlocks(data.BlockPos, data.Radius, data.Damage);
                        }
                        else
                        {
                            WorldManager.Instance.GetWorld().HitBlock(data.BlockPos, data.Damage);
                        }
                    }
                }

                _blockHitLastTick = tick;
            }
        }

        public void AddBlockHitData(Vector3 pos, int radius, int damage)
        {
            if (BlockHitList.Count == BlockHitList.Capacity)
            {
                BlockHitList.Remove(BlockHitList.Get(0));
            }

            if (radius > 0)
            {
                BlockHitList.Add(new BlockHitData
                {
                    Tick = Runner.Tick,
                    BlockPos = pos,
                    Radius = radius,
                    Explode = true,
                    Damage = damage
                });
            }
            else
            {
                BlockHitList.Add(new BlockHitData
                {
                    Tick = Runner.Tick,
                    BlockPos = pos,
                    Damage = damage,
                    Explode = false
                });
            }
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
                            _gameManager.NetworkManager.LocalCharacter.OnHit(networkObject);
                        }
                    }
                    
                    tick = data.Tick;
                }

                _characterHitLastTick = tick;
            }
        }

        public void AddCharacterHitData(NetworkObject networkObject)
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

        private void OnHit(NetworkObject projectile)
        {
            var damage = projectile.GetComponent<NetworkProjectileBase>().DamageSave;
            var armor = _baseCharStat.GetStat(CharStat.Armor).Total;
            var calcDamage = damage * (100 / (100 + armor));
            NowHp -= calcDamage;

            if (projectile.TryGetComponent<ICollisionCharacterEvent>(out var collisionEvent))
            {
                collisionEvent.CollisionCharacterEvent(this);
            }
        }
        
        public void OnHitDebugging(float damage)
        {
            var armor = _baseCharStat.GetStat(CharStat.Armor).Total;
            var calcDamage = damage * (100 / (100 + armor));
            NowHp -= calcDamage;
        }
        
        public void RotateDebugging()
        {
            GetComponent<NetworkTransform>().TeleportToRotation(new Quaternion(5f, 5f, 5f, 5f));
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
            
            MaxHp = NowHp = _baseCharStat.GetStat(CharStat.Health).Total * 100;
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

        public NetworkProjectileHolder[] GetProjectileHolderList()
        {
            return GetComponentsInChildren<NetworkProjectileHolder>();
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
                { 0, 60, 0, 30, 0, 30};

            RDG.Vibration.Vibrate(pattern, amplitudes, -1);
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
                long[] pattern = { 0, 3, 100, 3, 1000, 0 };
                int[] amplitudes = { 0, 1 };

                isVibrateBeat = true;
                RDG.Vibration.Vibrate(pattern, amplitudes, 0);
            }
        }
    }

    // 공격
    public partial class NetworkPlayer
    {
        private Ray _gunRay;
        private RaycastHit _hit;
        private float _shootDistance = 30f;
        private bool isShooting = true;
        private const int shootRayMask = (int)Layer.Enemy | (int)Layer.World;

        public void ToggleShooting() { isShooting = !isShooting; }

        private void ShootAllWeapons(AttackType attackType) {
            var weapon = GetComponentsInChildren<NetworkProjectileHolder>();

            foreach (var networkProjectileHolder in weapon)
            {
                Shoot(networkProjectileHolder, attackType);
            }
        }

        private void Shoot(NetworkProjectileHolder nph, AttackType attackType)
        {
            if (!isShooting) return;

            var aimRay = _camera.ScreenPointToRay(GetCrossHairPointInScreen());
            // 조준점으로 쏘는 레이의 원점이 플레이어 앞에서 시작되어야 한다.
            // 그렇지 않으면, 플레이어의 총알은 플레이어의 뒤에 있지만, 조준점에는 걸린 물체로 날아가게 된다. 한마디로 뒤로 쏘게 된다.
            var distCam = Vector3.Distance(_camera.transform.position, transform.position);
            var aimRayOrigin = aimRay.origin + aimRay.direction * distCam;

            /* 총알이 날아갈 지점 구하기 */
            _gunRay.origin = nph.ShootPointTransform.position;

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

            if (Physics.Raycast(_gunRay, out _hit, _shootDistance, shootRayMask))
            {
                targetPoint = _hit.point;
            }

            RotateToTarget(nph.transform, targetPoint, 8f, false);
            nph.SetTarget(targetPoint);
        }

        // private void LaserBeam(Ray gunRay, float aimDistance, AttackType attackType, LineRenderer lineRenderer)
        // {
        //     lineRenderer.SetPosition(0, gunRay.origin);
        //
        //     /* 실제 총알이 날아가는 경로 */
        //     if (Physics.Raycast(gunRay, out _hit, aimDistance, shootRayMask))
        //     {
        //         var point = _hit.point - _hit.normal * 0.01f;
        //
        //         if (_hit.transform.gameObject == _target.gameObject)
        //         {
        //             // 임시 헤드 판정
        //             var isCritical = _hit.point.y - (_target.transform.position.y - 1) > 1.25f;
        //             _gameUI.hitDamageText.GetComponent<HitDamage>().HitDamageAnimation(_damage, isCritical);
        //         }
        //         
        //         switch (attackType)
        //         {
        //             case AttackType.Basic:
        //                 AddBlockHitData(point, 1);
        //                 break;
        //
        //             case AttackType.Ultimate:
        //                 WorldManager.Instance.GetWorld().ExplodeBlocks(point, 3, 3);
        //                 VibrateUlt();
        //                 break;
        //             default:
        //                 throw new ArgumentOutOfRangeException(nameof(attackType), attackType, null);
        //         }
        //
        //         targetPoint = _hit.point;
        //     }
        //     
        //     GetComponentInChildren<NetworkProjectileHolder>().target = targetPoint;
        //     lineRenderer.SetPosition(1, targetPoint);
        // }

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
        public Transform ShootPoint;
        public static bool isCameraFocused = false;
        public LineRenderer ShotLine;
        public LineRenderer UltLine;
        public RectTransform _crossHairOrigin;

        public RectTransform _crossHair
        {
            get
            {
                if (_gameUI != null)
                {
                    _crossHairOrigin = _gameUI.crossHair;
                }
                return _crossHairOrigin;
            }
        }
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
        private bool isDodgeReady = true;
        #endregion

        #region 애니메이션 관련 변수
        private Animator _animator;
        [Networked(OnChanged = nameof(UpdatesAnimation))] 
        private int AnimationIdx { get; set; }
        #endregion

        private CanvasManager _canvasManager;
        private GameManager _gameManager;
        [SerializeField]
        private GameUI _gameUI;
        private Camera _camera;

        public override void Spawned()
        {
            _initPos = transform.position;
            _gameManager = GameManager.Instance;
            _baseCharStat = new BaseStat<CharStat>(1, 1);
            _gameUI = _gameManager.UIHolder as GameUI;

            if (_gameUI == null)
            {
                throw new Exception("ui holder가 null임");
            }
            
            InitialStatus();
            
            _gameUI = _gameManager.UIHolder as GameUI;
            if (_gameUI == null)
            {
                throw new Exception("ui holder가 null임");
            }
            
            _canvasManager = _gameUI.canvasManager;
            _joystick = _gameUI.joystick;
            _camera = Camera.main;
            _characterController = GetComponent<CharacterController>();
            _characterTransform = transform.Find("Cat");
            _animator = _characterTransform.GetComponentInChildren<Animator>();
            
            if (HasStateAuthority)
            {
                _camera.GetComponent<PlayerCamera>().AddPlayer(transform);
                _target = GameObject.Find("허수아비");
                _joystick = _gameUI.joystick;
                
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
                Camera.main.GetComponent<PlayerCamera>().AddEnemy(_target.transform);
                gameObject.layer = LayerMask.NameToLayer("Enemy");
                return;
            }
            
            moveDir = Vector3.zero;
            lastMoveDir = transform.forward;
            ShootPoint = transform;
            ShotLine = Instantiate(ShotLine);
            UltLine = Instantiate(UltLine);

            // 자신은 타겟팅 되지 않기 위해 레이어 변경
            gameObject.layer = LayerMask.NameToLayer("Player");
        }

        public void SetGunPos(Transform transform)
        {
            GunPos = transform;
            ShootPoint = GunPos.GetChild(1);
        }
        
        public void ChangeGunPos(bool isLeft)
        {
            if (isLeft)
            {
                if (GunPos.localPosition.x > 0)
                    GunPos.localPosition = new Vector3(-GunPos.localPosition.x, GunPos.localPosition.y, GunPos.localPosition.z);
            }
            else
            {
                if (GunPos.localPosition.x < 0)
                    GunPos.localPosition = new Vector3(-GunPos.localPosition.x, GunPos.localPosition.y, GunPos.localPosition.z);
            }
        }

        public override void FixedUpdateNetwork()
        {
            if(!HasStateAuthority) return;
            
            /*
            if (Input.GetKeyDown(KeyCode.Space))
                ScreenCapture.CaptureScreenshot("test.png");
            */

            var shakeMagnitude = Input.acceleration.magnitude;

            if (shakeMagnitude > shakeDodgeThreshold)    //if (Input.GetKeyDown(KeyCode.Space) && !isDodge)
            {
                Dodge();
            }
        
            // 임시 자동공격
            if (!isCameraFocused)
            {
                ShootAllWeapons(AttackType.Basic);
                //Shoot(AttackType.Basic, ShotLine);
            }
            
            if (_joystick is not null)
            {
                CharacterMove();
            }
        }

        private static void UpdatesAnimation(Changed<NetworkPlayer> changed)
        {
            changed.Behaviour.UpdatesAnimation();
        }
        
        private void UpdatesAnimation()
        {
            switch (AnimationIdx)
            {
                case 8:
                    _animator.Rebind();
                    _animator.Update(0f);
                    break;
                case 18:
                    _animator.SetFloat("walkSpeed", _baseCharStat.GetStat(CharStat.Speed).Total * 0.5f);
                    break;
                default:
                    break;
            }

            _animator.SetInteger("animation", AnimationIdx);
        }

        private void CharacterMove()
        {
            var oh = ReverseHorizontalMove ? -_joystick.Horizontal : _joystick.Horizontal;
            var ov = _joystick.Vertical;

            var camAngle = _camera.transform.eulerAngles.z * Mathf.Deg2Rad;

            var h = oh * Mathf.Cos(camAngle) - ov * Mathf.Sin(camAngle);
            var v = oh * Mathf.Sin(camAngle) + ov * Mathf.Cos(camAngle);

            var speed = _baseCharStat.GetStat(CharStat.Speed).Total;
            speed = speed > 0 ? speed : 0;

            if (isDodge)
            {
                var dodgeDir = lastMoveDir;
                var dodgeForce = _baseCharStat.GetStat(CharStat.Rolling).Total;

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
                            AnimationIdx = 1;
                        else
                            AnimationIdx = 18;
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
                    AnimationIdx = 9;
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

            if (isWalk)
                RotateCharacterMoveDir(h, v);
        }

        public void Dodge()
        {
            if (!isDodge && isDodgeReady)
            {
                AnimationIdx = 8;
                isDodge = true;
                isDodgeReady = false;
                DOTween.Sequence()
                    .AppendInterval(0.15f)  // 이동 시간
                    .AppendCallback(() =>
                    {
                        isDodge = false;
                        isWalk = true;
                        
                    })
                    .AppendInterval(0.5f)   // 대기 시간
                    .OnComplete(() =>
                    {
                        AnimationIdx = 1;
                        isDodgeReady = true;
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
            //Shoot(AttackType.Ultimate, UltLine);
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

        public string IncreaseDodge(){ 
            _settingsStatlist.Add(new Stat<CharStat>(CharStat.Rolling, 1, 0).SetRatio(0));
            return AdditionalWork(CharStat.Rolling);
        }
        public string DecreaseDodge(){
            _settingsStatlist.Add(new Stat<CharStat>(CharStat.Rolling, -1, 0).SetRatio(0));
            return AdditionalWork(CharStat.Rolling);
        }

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