using System;
using System.Collections.Generic;
using Fusion;
using GameStatus;
using Types;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util;
using Weapon;
using Random = UnityEngine.Random;

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

        #region 타겟 지정 관련 변수
        public GameObject target;
        private bool _isTargetNotNull;
        private RaycastHit _raycast;
        public float maxDistance = 10.0f;
        #endregion
        
        #region 움직임 관련 변수
        private FixedJoystick _joystick;
        private Vector3 moveDir;
        private float jumpForce = 5.0f;
        private float yVelocity = 0.0f;
        private NetworkCharacterControllerPrototype _characterController;
        #endregion

        #region 테스트 코드

        public bool check = false;

        #endregion

        private GameManager _gameManager;
        private RoomUI RoomUI;

        private void Awake()
        {
            _baseCharStat = new BaseStat<CharStat>(1, 1);
            _characterController = GetComponent<NetworkCharacterControllerPrototype>();
        }

        private void Start()
        {
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
            if (HasInputAuthority && _joystick is not null)
            {
                var speed = 10f;
                var horizontal = _joystick.Horizontal;
                var vertical = _joystick.Vertical;

                moveDir = new Vector3(horizontal, 0, vertical);
                moveDir.Normalize();
                _characterController.Move(speed * moveDir * Runner.DeltaTime);
            }
        }

        private void OnCollisionEnter()
        {
            if (moveDir.x != 0 && moveDir.z != 0)
            {
                yVelocity = 0;
                if (_characterController.IsGrounded)
                {
                    yVelocity = jumpForce;
                }
            }
        }

        public void Jump()
        {
            yVelocity = jumpForce;
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