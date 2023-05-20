﻿using System;
using System.Collections.Generic;
using DG.Tweening;
using Fusion;
using GameStatus;
using TMPro;
using Types;
using UIHolder;
using UnityEngine;

namespace Network
{
    public abstract class NetworkProjectileHolder : NetworkBehaviour
    {
        private List<NetworkObject> _projectileList;
        protected BaseStat<WeaponStat> _baseWeaponStat;
        protected Transform WeaponTransform;
        protected Transform ShootPointTransform;
        protected Vector3 Target;
        protected bool IsDoneShootAction;
        protected int RemainBullet;
        protected TextMeshProUGUI BulletText;
        protected TickTimer delay;

        [Networked] private int NetWeaponData { get; set; } = -1;

        private Weapon _weaponData;
        public Weapon WeaponData
        {
            get
            {
                if (_weaponData is null && NetWeaponData != -1)
                {
                    _weaponData = GameManager.Instance.WeaponList[NetWeaponData];
                }

                return _weaponData;
            }
            private set => _weaponData = value;
        }

        private void Awake()
        {
            _baseWeaponStat = new BaseStat<WeaponStat>(1, 1);
            _projectileList = new List<NetworkObject>();

            _baseWeaponStat.AddStat(new Stat<WeaponStat>(WeaponStat.Damage, 10, 0));
            _baseWeaponStat.AddStat(new Stat<WeaponStat>(WeaponStat.Velocity, 20, 0));
            _baseWeaponStat.AddStat(new Stat<WeaponStat>(WeaponStat.Range, 10, 0));
            _baseWeaponStat.AddStat(new Stat<WeaponStat>(WeaponStat.Special, 6, 0));
            _baseWeaponStat.AddStat(new Stat<WeaponStat>(WeaponStat.Bullet, 10, 0));
            _baseWeaponStat.AddStat(new Stat<WeaponStat>(WeaponStat.Reload, 10, 0));
            
            WeaponTransform = gameObject.transform;
            
            var shootPoint = transform.Find("ShootPoint");
            if (shootPoint == null)
            {
                shootPoint = WeaponTransform;
            }
            
            ShootPointTransform = shootPoint;

            Target = gameObject.transform.forward;
            IsDoneShootAction = true;
        }

        private void Start()
        {
            if (WeaponData.isMainWeapon)
            {
                RemainBullet = (int)GetWeaponStat(WeaponStat.Bullet).Total;
            }
            //BulletText = (GameManager.Instance.UIHolder as GameUI).bulletText;
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasInputAuthority || GameManager.Instance.NetworkManager.GameRoundState != RoundState.RoundStart)
            {
                return;
            }
            
            Attack();

            //BulletText.text = RemainBullet.ToString();
        }

        public void InitialHolder(Weapon weaponData)
        {
            WeaponData = weaponData;
            NetWeaponData = GameManager.Instance.WeaponList.FindIndex(data => data.Equals(weaponData));
        }

        public void SetTarget(Vector3 target)
        {
            Target = target;
        }
        
        public Vector3 GetTarget()
        {
            return Target;
        }
        
        public Vector3 GetShootPointTransform()
        {
            return ShootPointTransform.position;
        }
        
        private void InitializeProjectile(NetworkRunner runner, NetworkObject obj)
        {
            var objInit = obj.GetComponent<NetworkProjectileBase>();
            objInit.Initialized(this, NetWeaponData);
        }

        protected NetworkObject SpawnProjectile(Transform transform)
        {
            var position = transform.position;
            var obj = Runner.Spawn(
                WeaponData.bulletPrefabRef, 
                position, //+ position.TransformDirection(Vector3.forward), 
                Quaternion.LookRotation(Target - position), 
                Runner.LocalPlayer,
                InitializeProjectile
            );
            _projectileList.Add(obj);
            return obj;
        }
        
        protected virtual bool CanAttack()
        {
            if (!IsDoneShootAction)
            {
                return false;
            }
            
            if (RemainBullet == 0)
            {
                ReloadBullet();
                return false;
            }
            
            if (delay.ExpiredOrNotRunning(Runner))
            {
                delay = TickTimer.CreateFromSeconds(Runner, _baseWeaponStat.GetStat(WeaponStat.Interval).Total);
                return true;
            }
            
            return false;
        }

        public void ChangeIsDone(bool value)
        {
            IsDoneShootAction = value;
        }

        protected void ReloadBullet()
        {
            Sequence reloadSequence = DOTween.Sequence();

            reloadSequence
                .OnStart(() =>
                {
                    IsDoneShootAction = false;
                    GameManager.Instance.ActiveLoadingUI();
                })
                .OnComplete(() =>
                {
                    IsDoneShootAction = true;
                    GameManager.Instance.DeActiveLoadingUI();
                });

            var max = GetWeaponStat(WeaponStat.Bullet).Total;
            var time = GetWeaponStat(WeaponStat.Reload).Total;
            var separateTime = (50 + time) / (50 * max);
            
            GameManager.Instance.AddBehaviourEventCount(BehaviourEvent.장전, (int)(max * separateTime));
            
            for (int i = 0; i < max; i++)
            {
                reloadSequence
                    .AppendCallback(() => RemainBullet++)
                    .AppendInterval(separateTime);
            }

            reloadSequence.Play();
        }

        protected abstract void Attack();

        #region 스탯

        protected void AddWeaponAdditionStat(WeaponStat weaponStat, float add)
        {
            GetWeaponStat(weaponStat).AddAddition(add);
        }
        
        protected void AddCharAdditionStat(CharStat charStat, float add)
        {
            GetCharStat(charStat).AddAddition(add);
        }
        
        protected Stat<CharStat> GetCharStat(CharStat stat)
        {
            var localCharacter = GameManager.Instance.NetworkManager.PlayerCharacter;
            return localCharacter.GetCharStat(stat);
        }
        
        public void AddWeaponStat(Stat<WeaponStat> stat)
        {
            _baseWeaponStat.AddStat(stat);
        }
        
        public void AddWeaponStatList(List<Stat<WeaponStat>> statList)
        {
            _baseWeaponStat.AddStatList(statList);
        }
        
        public Stat<WeaponStat> GetWeaponStat(WeaponStat stat)
        {
            return _baseWeaponStat.GetStat(stat);
        }
        
        public float GetWeaponStatTotal(WeaponStat stat)
        {
            return _baseWeaponStat.GetStat(stat).Total;
        }

        public void ClearWeaponStat()
        {
            _baseWeaponStat.ClearStatList();
        }

        #endregion
    }
}