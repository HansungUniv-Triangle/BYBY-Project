using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Fusion;
using GameStatus;
using Types;
using UnityEngine;
using UIHolder;
using Utils;

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
            if (NowHp < 0)
            {
                _gameManager.NetworkManager.EndedRound();
            }
            
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
            if (!HasStateAuthority)
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
                            _gameManager.NetworkManager.PlayerCharacter.OnDamaged(networkObject);
                        }
                        _gameManager.AddBehaviourEventCount(BehaviourEvent.피격, 1);
                    }
                    tick = data.Tick;
                }
                _characterHitLastTick = tick;
            }
        }

        public void AddCharacterHitData(NetworkObject networkObject, int damage, bool isMainWeapon)
        {
            if (CharacterHitList.Count == CharacterHitList.Capacity)
            {
                CharacterHitList.Remove(CharacterHitList.Get(0));
            }
            
            if (isMainWeapon)
            {
                _gameManager.hitCount++;
                _gameManager.AddBehaviourEventCount(BehaviourEvent.피해, damage);
            }

            CharacterHitList.Add(new CharacterHitData
            {
                Tick = Runner.Tick,
                NetworkId = networkObject
            });
        }

        private void OnDamaged(NetworkObject projectile)
        {
            var damage = projectile.GetComponent<NetworkProjectileBase>().DamageSave;
            var armor = StatConverter.ConversionStatValue(_baseCharStat.GetStat(CharStat.Armor));
            var calcDamage = damage * armor;
            NowHp -= calcDamage;

            if (projectile.TryGetComponent<ICollisionCharacterEvent>(out var collisionEvent))
            {
                collisionEvent.CollisionCharacterEvent(this);
            }
        }
    }

    public partial class NetworkPlayer
    {
        public struct BehaviorData : INetworkStruct
        {
            public int HitRate;
            public int DodgeRate;
            public int Accuracy;
            public int Damage;
            public int Special;
            public int DestroyBullet;
            public int Reload;
        }

        [Networked]
        public BehaviorData CharacterBehaviorData { get; set; }

        public void ConversionBehaviorData()
        {
            var netBehaviorData = new BehaviorData();
            var behaviorData = _gameManager.GetBehaviourEventCount();
            
            foreach (var (key, value) in behaviorData)
            {
                switch (key)
                {
                    case BehaviourEvent.피격:
                        netBehaviorData.HitRate = value;
                        break;
                    case BehaviourEvent.회피:
                        netBehaviorData.DodgeRate = value;
                        break;
                    case BehaviourEvent.명중:
                        netBehaviorData.Accuracy = value;
                        break;
                    case BehaviourEvent.피해:
                        netBehaviorData.Damage = value;
                        break;
                    case BehaviourEvent.특화:
                        netBehaviorData.Special = value;
                        break;
                    case BehaviourEvent.파괴:
                        netBehaviorData.DestroyBullet = value;
                        break;
                    case BehaviourEvent.장전:
                        netBehaviorData.Reload = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            CharacterBehaviorData = netBehaviorData;
        }
    }

    // 시너지
    public partial class NetworkPlayer
    {
        public List<Synergy> synergyList = new List<Synergy>();

        [Networked(OnChanged = nameof(OnSynergyChange)), Capacity(50)]
        [UnitySerializeField]
        private NetworkLinkedList<int> NetworkSynergyList => default;

        public static void OnSynergyChange(Changed<NetworkPlayer> changed)
        {
            changed.Behaviour.OnSynergyChange();
        }
        
        private void OnSynergyChange()
        {
            synergyList.Clear();
            foreach (var num in NetworkSynergyList)
            {
                if (_gameManager.GetSynergy(num, out var synergy))
                {
                    synergyList.Add(synergy);
                }
                else
                {
                    throw new Exception("시너지 범위 초과");
                }
            }
        }

        public void AddSynergy(int index)
        {
            NetworkSynergyList.Add(index);
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
            Healing(999f);
        }

        private void InitialCharacterStatus()
        {
            _baseCharStat.ClearStatList();

            foreach (var synergy in synergyList)
            {
                _baseCharStat.AddStatList(synergy.charStatList);
            }
            
            MaxHp = NowHp = StatConverter.ConversionStatValue(_baseCharStat.GetStat(CharStat.Health));
        }

        private void InitialWeaponStatus()
        {
            var weaponList = GetComponentsInChildren<NetworkProjectileHolder>();
            foreach (var weapon in weaponList)
            {
                weapon.ClearWeaponStat();

                foreach (var synergy in synergyList)
                {    
                    weapon.AddWeaponStatList(synergy.weaponStatList);
                }
            }
        }

        public Stat<CharStat> GetCharStat(CharStat type)
        {
            return _baseCharStat.GetStat(type);
        }

        public BaseStat<CharStat> GetCharBaseStat()
        {
            return _baseCharStat;
        }
        
        public Stat<WeaponStat> GetWeaponStat(WeaponStat type)
        {
            return GetWeaponBaseStat().GetStat(type);
        }
        
        public BaseStat<WeaponStat> GetWeaponBaseStat()
        {
            var weaponList = GetComponentsInChildren<NetworkProjectileHolder>();
            return (from networkProjectileHolder in weaponList where networkProjectileHolder.WeaponData.isMainWeapon select networkProjectileHolder.GetWeaponBaseStat()).FirstOrDefault();
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

        public void ToggleShooting()
        {
            isShooting = !isShooting;
        }

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
            _gunRay.origin = nph.GetShootPointTransform();

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
            var targetPoint = _gunRay.origin + _gunRay.direction * _shootDistance;

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
        private Transform _gunPos;
        private Transform _shootPoint;
        public bool IsCameraFocused { get; private set; } = false;
        private RectTransform _crossHairOrigin;

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

        private Vector3 _targetPoint;
        private Vector3 _moveDir;
        private Vector3 _jumpDir;
        private Vector3 _lastMoveDir;
        private Vector3 _initPos;

        private float _gravity = 15.0f;
        private float _jumpForce = 7.0f;
        private float _shakeDodgeThreshold = 2.0f;

        private bool _reverseHorizontalMove = false;
        private bool _isWalk = true;
        private bool _isJump = false;
        private bool _isDodge = false;
        private bool _isDodgeReady = true;
        #endregion

        #region 애니메이션 관련 변수
        private CatController _catController;
        [Networked(OnChanged = nameof(UpdatesAnimation))] 
        private int AnimationIdx { get; set; }
        [Networked(OnChanged = nameof(UpdatesRotate))] 
        private Quaternion CatRotate { get; set; }
        #endregion

        private CanvasManager _canvasManager;
        private GameManager _gameManager;
        private GameUI _gameUI;
        private Camera _camera;

        private List<NetworkId> _networkObjectCheckList = new List<NetworkId>();

        public static NetworkPlayer PlayerCharacter;
        public static NetworkPlayer EnemyCharacter;

        public override void Spawned()
        {
            _initPos = transform.position;
            _moveDir = Vector3.zero;
            _lastMoveDir = transform.forward;
            _gunPos = transform;
            _shootPoint = transform;
            _gunPos = transform.GetChild(2).transform;
            _moveDir = Vector3.zero;
            
            _baseCharStat = new BaseStat<CharStat>(1, 1);
            _characterController = GetComponent<CharacterController>();
            _catController = GetComponentInChildren<CatController>();
            
            _gameManager = GameManager.Instance;
            _gameUI = _gameManager.UIHolder as GameUI;
            _canvasManager = _gameUI.canvasManager;
            _joystick = _gameUI.joystick;
            _camera = Camera.main;
            
            if (Runner.ActivePlayers.Count() == 1)
            {
                _target = GameObject.Find("허수아비");
                _camera.GetComponent<PlayerCamera>().AddPlayer(transform);
                _camera.GetComponent<PlayerCamera>().AddEnemy(_target.transform);
                _gameUI.crossHair.GetComponent<SubCrosshair>().SetNetworkPlayer(this);
            }
            else
            {
                SetNetworkPlayer();
            }

            if (HasStateAuthority)
            {
                gameObject.layer = LayerMask.NameToLayer("Player");
                _canvasManager.SwitchUI(CanvasType.GameMoving);
            }
            else
            {
                gameObject.layer = LayerMask.NameToLayer("Enemy");
                _gameManager.NetworkManager.EnemyCharacter = this;
            }
        }

        public void SetGunPos(Transform transform)
        {
            _gunPos = transform;
            _shootPoint = _gunPos.GetChild(1);
        }

        private void SetNetworkPlayer()
        {
            if (HasStateAuthority)
            {
                PlayerCharacter = this;
            }
            else
            {
                EnemyCharacter = this;
            }

            if (PlayerCharacter == null || EnemyCharacter == null) return;
            
            PlayerCharacter.SetTarget();
        }

        private void SetTarget()
        {
            _camera.GetComponent<PlayerCamera>().AddPlayer(transform);
            _camera.GetComponent<PlayerCamera>().AddEnemy(EnemyCharacter.transform);
            _target = EnemyCharacter.gameObject;
        }
        
        public void ChangeGunPos(bool isLeft)
        {
            if (isLeft)
            {
                if (_gunPos.localPosition.x > 0)
                    _gunPos.localPosition = new Vector3(-_gunPos.localPosition.x, _gunPos.localPosition.y, _gunPos.localPosition.z);
            }
            else
            {
                if (_gunPos.localPosition.x < 0)
                    _gunPos.localPosition = new Vector3(-_gunPos.localPosition.x, _gunPos.localPosition.y, _gunPos.localPosition.z);
            }
        }
        
        public override void FixedUpdateNetwork()
        {
            if(!HasStateAuthority || !GameManager.Instance.NetworkManager.CanControlCharacter) return;

            var shakeMagnitude = Input.acceleration.magnitude;
            if (shakeMagnitude > _shakeDodgeThreshold)    //if (Input.GetKeyDown(KeyCode.Space) && !isDodge)
            {
                Dodge();
            }
        
            ShootAllWeapons(AttackType.Basic);

            if (_joystick is not null && _target is not null)
            {
                CharacterMove();
                CatRotate = _catController.Rotation;
            }
            
            var hitColliders = Physics.OverlapSphere(transform.position, 10f, (int)Layer.Enemy);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.TryGetComponent(out NetworkObject id))
                {
                    if (!_networkObjectCheckList.Contains(id))
                    {
                        _networkObjectCheckList.Add(id);
                        _gameManager.AddBehaviourEventCount(BehaviourEvent.회피, 1);
                    }
                }
            }
        }
        
        private static void UpdatesAnimation(Changed<NetworkPlayer> changed)
        {
            changed.Behaviour.UpdatesAnimation();
        }
        
        private void UpdatesAnimation()
        {
            var speed = StatConverter.ConversionStatValue(_baseCharStat.GetStat(CharStat.Speed));
            _catController.UpdateAnimation(AnimationIdx, speed * 0.5f);
        }

        private static void UpdatesRotate(Changed<NetworkPlayer> changed)
        {
            changed.Behaviour.UpdatesRotate();
        }
        
        private void UpdatesRotate()
        {
            if (!HasStateAuthority)
            {
                _catController.UpdateRotation(CatRotate);
            }
        }
        
        private void CharacterMove()
        {
            var oh = _reverseHorizontalMove ? -_joystick.Horizontal : _joystick.Horizontal;
            var ov = _joystick.Vertical;

            var rotatedInput = PlayerCamera.GetRotatedCoordinates(oh, ov);
            var h = rotatedInput.x;
            var v = rotatedInput.y;

            var speed = StatConverter.ConversionStatValue(_baseCharStat.GetStat(CharStat.Speed));

            if (_isDodge)
            {
                var dodgeDir = _lastMoveDir;
                var _dodgeForce = _baseCharStat.GetStat(CharStat.Rolling).Total;

                dodgeDir.x *= _dodgeForce;
                dodgeDir.z *= _dodgeForce;
                
                _characterController.Move(dodgeDir * Runner.DeltaTime);
                _catController.RotateModel(_lastMoveDir, 18f);
            }
            else
            {
                // move
                if (_characterController.isGrounded)
                {
                    if (_isWalk)
                    {
                        if (h == 0 && v == 0)
                            AnimationIdx = 1;
                        else
                            AnimationIdx = 18;
                    }

                    _moveDir = new Vector3(h, 0, v);
                    _moveDir = transform.TransformDirection(_moveDir);
                    
                    if (_moveDir.magnitude >= 0.3f)
                        _lastMoveDir = _moveDir;
                    
                    _moveDir *= speed;
                }
                else
                {
                    _jumpDir = new Vector3(h, 0, v);
                    _jumpDir = transform.TransformDirection(_jumpDir);
                    
                    if (_jumpDir.magnitude >= 0.3f)
                        _lastMoveDir = _jumpDir;
                    
                    _jumpDir *= (speed * 0.7f);

                    _moveDir.x = _jumpDir.x;
                    _moveDir.z = _jumpDir.z;
                    _moveDir.y -= _gravity * Runner.DeltaTime;
                }
                
                if (_isJump)
                {
                    AnimationIdx = 9;
                    _moveDir.y = _jumpForce;
                    _isJump = false;
                    _isWalk = true;
                }
                _characterController.Move(_moveDir * Runner.DeltaTime);
            }

            if (IsCameraFocused == false)
            {
                RotateToTarget(transform, _target.transform.position, 8f, true);
            }

            if (_isWalk)
            {
                RotateCharacterMoveDir(h, v);
            }
        }

        public void Dodge()
        {
            if (!_isDodge && _isDodgeReady)
            {
                AnimationIdx = 8;
                _isDodge = true;
                _isDodgeReady = false;
                DOTween.Sequence()
                    .AppendInterval(0.15f)  // 이동 시간
                    .AppendCallback(() =>
                    {
                        _isDodge = false;
                        _isWalk = true;
                        
                    })
                    .AppendInterval(0.5f)   // 대기 시간
                    .OnComplete(() =>
                    {
                        AnimationIdx = 1;
                        _isDodgeReady = true;
                    });
            }
        }

        public void Jump()
        {
            if (_characterController.isGrounded && !_isJump)
            {
                _isJump = true;
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
            var characterRotateSpeed = 18f;
            
            characterDir = transform.TransformDirection(characterDir);
            
            if (characterDir.magnitude < 0.5f)
            {
                characterDir = _target.transform.position - _catController.Position;
                characterDir.y = 0;
            }

            if (!_characterController.isGrounded)
            {
                //characterRotateSpeed = 3f;
            }

            _catController.RotateModel(characterDir, characterRotateSpeed);
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

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.TryGetComponent(out ICollisionObjectEvent collisionObject))
            {
                collisionObject.CollisionObjectEvent(Object);
            }
        }
    }

    public partial class NetworkPlayer
    {
        public void GetUlt()
        {
            _joystick.OnPointerUp(null);    // joystick 입력값 초기화
            IsCameraFocused = !IsCameraFocused;
            _canvasManager.SwitchUI(CanvasType.GameAiming);

            var weapon = FindObjectOfType<NetworkSniperRifle>();
            if (weapon != null)
            {
                weapon.SnipingMode(true);
            }
        }

        public void EndUlt()
        {
            //Shoot(AttackType.Ultimate, UltLine);
            IsCameraFocused = false;
            _canvasManager.SwitchUI(CanvasType.GameMoving);
            
            var weapon = FindObjectOfType<NetworkSniperRifle>();
            if (weapon != null)
            {
                weapon.SnipingMode(false);
            }
        }
    }

    // 설정 UI
    public partial class NetworkPlayer
    {
        private List<Stat<CharStat>> _settingsStatList = new();
        
        public string IncreaseSpeed()
        {
            _settingsStatList.Add(new Stat<CharStat>(CharStat.Speed, 1, 0));
            return AdditionalWork(CharStat.Speed);
        }
        
        public string DecreaseSpeed()
        {
            _settingsStatList.Add(new Stat<CharStat>(CharStat.Speed, -1, 0));
            return AdditionalWork(CharStat.Speed);
        }

        private string AdditionalWork(CharStat type)
        {
            InitialStatus();

            foreach (var s in _settingsStatList)
            {
                _baseCharStat.AddStat(s);
            }
        
            var total = _baseCharStat.GetStat(type).Total;

            return total.ToString();
        }
        
        public string IncreaseJump() { return (++_jumpForce).ToString(); }
        public string DecreaseJump() { return (--_jumpForce).ToString(); }

        public string IncreaseDodge(){
            _settingsStatList.Add(new Stat<CharStat>(CharStat.Rolling, 1, 0).SetRatio(0));
            return AdditionalWork(CharStat.Rolling);
        }
        public string DecreaseDodge(){
            _settingsStatList.Add(new Stat<CharStat>(CharStat.Rolling, -1, 0).SetRatio(0));
            return AdditionalWork(CharStat.Rolling);
        }

        public string IncreaseCalm()
        {
            _settingsStatList.Add(new Stat<CharStat>(CharStat.Calm, 1, 0).SetRatio(0));
            return AdditionalWork(CharStat.Calm);
        }

        public string DecreaseCalm()
        {
            _settingsStatList.Add(new Stat<CharStat>(CharStat.Calm, -1, 0).SetRatio(0));
            return AdditionalWork(CharStat.Calm);
        }

        public string IncreaseShakeSensitivity()
        {
            return (_shakeDodgeThreshold += 0.1f).ToString("F1");
        }
        public string DecreaseShakeSensitivity()
        {
            return (_shakeDodgeThreshold -= 0.1f).ToString("F1");
        }
        
        public string IncreaseShootDistance() { return (_shootDistance += 5).ToString(); }
        public string DecreaseShootDistance() { return (_shootDistance -= 5).ToString(); }

        public void ToggleReverseHorizontalMove(bool state) { _reverseHorizontalMove = state; }
    }
}