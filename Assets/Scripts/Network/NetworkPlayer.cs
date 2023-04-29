using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Fusion;
using GameStatus;
using Types;
using UnityEngine;
using UIHolder;
using UnityEngine.EventSystems;

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
        private float _shootDistance = 30f;
        private int _damage = 1;
        private bool isShooting = true;
        
        public void ToggleShooting() { isShooting = !isShooting; }

        private void Shoot(AttackType attackType, LineRenderer lineRenderer)    // 라인렌더러는 임시
        {
            if (!isShooting) return;
            
            var aimRay = Camera.main.ScreenPointToRay(GetCrossHairPointInScreen());
            _gunRay.origin = GunPos.position;

            // 화면 중앙으로 쏘는 레이는 원점이 플레이어 앞에서 시작되어야 한다.
            // 그렇지 않으면 플레이어는 크로스헤어에는 걸렸지만, 뒤에 있는 물체를 부수게 된다.
            // 발사하는 주체는 제외
            
            if (Physics.Raycast(aimRay.origin + aimRay.direction * 10, aimRay.direction, out var hit, _shootDistance) 
                && hit.transform.gameObject != gameObject)
            {
                _gunRay.direction = (hit.point - GunPos.position).normalized;
            }
            else
            {
                _gunRay.direction = ((aimRay.origin + aimRay.direction * 10 + aimRay.direction * _shootDistance) - GunPos.position).normalized;
            }
            
            targetPoint = _gunRay.origin + _gunRay.direction * _shootDistance;
            GetComponentInChildren<NetworkProjectileHolder>().target = targetPoint;
            LaserBeam(_gunRay, _shootDistance, attackType, lineRenderer);
        }

        private void LaserBeam(Ray gunRay, float aimDistance, AttackType attackType, LineRenderer lineRenderer)
        {
            RaycastHit hit;
            
            lineRenderer.SetPosition(0, gunRay.origin);
            lineRenderer.SetPosition(1, targetPoint);
            
            if (Physics.Raycast(gunRay, out hit, aimDistance, (int)(Layer.World | Layer.Enemy)))
            {
                var point = hit.point - hit.normal * 0.01f;

                if (hit.transform.gameObject == _target.gameObject)
                {
                    // 임시 헤드 판정
                    var isCritical = hit.point.y - (_target.transform.position.y - 1) > 1.25f;
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

                targetPoint = hit.point;
                lineRenderer.SetPosition(1, targetPoint);
            }
        }

        private Vector3 GetCrossHairPointInScreen()
        {
            var position = _crossHair.transform.position;
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
        private RaycastHit _raycast;
        public float maxDistance = 10.0f;
        #endregion
        
        #region 움직임 관련 변수
        private Joystick _joystick;
        private Transform _transform;
        private Vector3 moveDir;
        private float gravity = 15.0f;
        private float jumpForce = 7.0f;
        private float dodgeForce = 4.0f;
        public bool ReverseHorizontalMove = false;
        private bool isJump = false;
        private bool isDodge = false;
        private float shakeDodgeThreshold = 2.0f;
        private CharacterController _characterController;
        public Vector3 targetPoint;
        private Vector3 _initPos;
        #endregion

        private CanvasManager _canvasManager;
        private GameManager _gameManager;
        private GameUI _gameUI;

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
            
            if (HasStateAuthority)
            {
                Camera.main.GetComponent<PlayerCamera>().AddPlayer(transform);
                _target = GameObject.Find("허수아비");
                
                // 혼자 테스트용 코드
                if (Runner.ActivePlayers.Count() == 1)
                {
                    _target = GameObject.Find("허수아비");
                    Camera.main.GetComponent<PlayerCamera>().AddEnemy(_target.transform);
                }
                _canvasManager.SwitchUI(CanvasType.GameMoving);
            }
            else
            {
                _target = GameObject.Find("허수아비");
                Camera.main.GetComponent<PlayerCamera>().AddEnemy(_target.transform);
                return;
            }
            
            moveDir = Vector3.zero;
            GunPos = transform; // 총 위치로 수정해야함.

            _transform = gameObject.transform;
            _characterController = GetComponent<CharacterController>();

            ShotLine = Instantiate(ShotLine);
            UltLine = Instantiate(UltLine);

            // 자신은 타겟팅 되지 않기 위해 레이어 변경
            gameObject.layer = LayerMask.NameToLayer("Player");
        }
        
        public override void FixedUpdateNetwork()
        {
            if(!HasStateAuthority) return;
            
            var shakeMagnitude = Input.acceleration.magnitude;

            if (shakeMagnitude > shakeDodgeThreshold && !isDodge)    //if (Input.GetKeyDown(KeyCode.Space) && !isDodge)
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

            moveDir.y -= gravity * Runner.DeltaTime;
            _characterController.Move(moveDir * Runner.DeltaTime);

            if (isCameraFocused == false)
            {
                var relativePosition = _target.transform.position - transform.position;
                relativePosition.y = 0; // y축은 바라보지 않도록 함
                var targetRotation = Quaternion.LookRotation(relativePosition);

                _transform.rotation = Quaternion.Lerp(_transform.rotation, targetRotation, Runner.DeltaTime * 8f);
            }
        }

        private void Dodge()
        {
            isDodge = true;
            DOTween.Sequence()
                .AppendInterval(0.1f)
                .OnComplete(() =>
                {
                    isDodge = false;
                });
        }

        public void InitPosition()
        {
            _characterController.enabled = false;
            transform.position = _initPos;
            _characterController.enabled = true;
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
    }

    public partial class NetworkPlayer
    {
        public void GetUlt()
        {
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