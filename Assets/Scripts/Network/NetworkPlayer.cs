using System;
using System.Collections.Generic;
using Fusion;
using GameStatus;
using Types;
using UnityEngine;
using UnityEngine.SceneManagement;
using Weapon;

namespace Network
{
    public class NetworkPlayer : NetworkBehaviour
    {
        #region 타겟 지정 관련 변수
        public GameObject target;
        private bool _isTargetNotNull;
        private RaycastHit _raycast;
        public float maxDistance = 10.0f;
        #endregion

        #region 움직임 관련 변수
        public FixedJoystick _joystick;
        private Transform _transform;
        private NetworkCharacterControllerPrototype _characterController;
        private Vector3 moveDir;
        private float jumpForce = 5.0f;
        public float yVelocity = 0.0f;
        #endregion

        #region 전투 관련
        private BaseStat<CharStat> _baseCharStat;
        public List<Synergy> synergyList = new List<Synergy>();
        #endregion

        private GameManager _gameManager;
        private Vector3 _forward;

        private void Awake()
        {
            _transform = gameObject.transform;
            _forward = Vector3.forward;
            _baseCharStat = new BaseStat<CharStat>(1, 1);
        }

        private void Start()
        {
            _characterController = GetComponent<NetworkCharacterControllerPrototype>();
            _joystick = GameObject.Find("Fixed Joystick").GetComponent<FixedJoystick>();
            _gameManager = GameManager.Instance;

            InitialStatus();
        }
        
        private void InitialStatus()
        {
            _baseCharStat.ClearStatList();
            foreach (var synergy in synergyList)
            {
                _baseCharStat.AddStatList(synergy.charStatList);
            }
        }

        public void InitialWeapon()
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

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasInputAuthority) return;
            
            CharacterMove();
        }

        private void CharacterMove()
        {
            // move
            var speed = 10f;
            var horizontal = _joystick.Horizontal;
            var vertical = _joystick.Vertical;

            moveDir = new Vector3(horizontal, 0, vertical);
            moveDir.Normalize();

            // if (_isTargetNotNull == false)
            // {
            //     if (!(horizontal == 0 && vertical == 0))
            //     {
            //         _transform.rotation = Quaternion.Lerp(_transform.rotation, Quaternion.LookRotation(moveDir),
            //             Runner.DeltaTime * (speed * 0.1f));
            //     }
            // }

            _characterController.Move(speed * moveDir * Runner.DeltaTime);
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
    }
}