using System;
using System.Collections.Generic;
using DG.Tweening;
using Fusion;
using GameStatus;
using TMPro;
using Types;
using UnityEngine;
using Utils;

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
        
        private int _maxBullet;
        private int _remainBullet;
        protected int RemainBullet
        {
            get
            {
                if (WeaponData.isMainWeapon)
                {
                    GameManager.Instance.NetworkManager.UpdateBullet(_remainBullet, _maxBullet);
                }
                return _remainBullet;
            }
            set => _remainBullet = value;
        }

        protected bool IsAttacking;
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

        private Sequence _reloadSequence;

        private void Awake()
        {
            _baseWeaponStat = new BaseStat<WeaponStat>(10, 1);
            _projectileList = new List<NetworkObject>();
            WeaponTransform = gameObject.transform;
            Target = gameObject.transform.forward;
            IsDoneShootAction = true;
            
            var shootPoint = transform.Find("ShootPoint");
            ShootPointTransform = shootPoint ? shootPoint : WeaponTransform;
        }
        
        public void SetBullet()
        {
            if (WeaponData.isMainWeapon)
            {
                RemainBullet = (int)GetWeaponStat(WeaponStat.Bullet).Total;
                _maxBullet = (int)GetWeaponStat(WeaponStat.Bullet).Total;
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasInputAuthority || !IsAttacking || !IsDoneShootAction || !GameManager.Instance.NetworkManager.CanControlCharacter)
            {
                return;
            }
            
            Attack();
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

        protected NetworkObject SpawnProjectile(Vector3 position, bool basicRotate = false)
        {
            var obj = Runner.Spawn(
                WeaponData.bulletPrefabRef, 
                position, //+ position.TransformDirection(Vector3.forward), 
                basicRotate ? null : Quaternion.LookRotation(Target - position), 
                Runner.LocalPlayer,
                InitializeProjectile
            );
            _projectileList.Add(obj);
            
            if (_weaponData.isMainWeapon)
            {
                GameManager.Instance.shootCount++;
            }
            
            return obj;
        }

        protected virtual bool CanAttack()
        {
            if (RemainBullet == 0 && _weaponData.isMainWeapon)
            {
                ReloadBullet();
                return false;
            }
            
            if (delay.ExpiredOrNotRunning(Runner))
            {
                var delayValue = StatConverter.ConversionStatValue(_baseWeaponStat.GetStat(WeaponStat.Interval));
                delay = TickTimer.CreateFromSeconds(Runner, delayValue);
                return true;
            }
            
            return false;
        }

        public void ChangeIsDone(bool value)
        {
            IsDoneShootAction = value;
        }

        public void ChangeIsAttacking(bool value)
        {
            IsAttacking = value;
        }

        public void CallReload(bool attackMode)
        {
            if (!attackMode && WeaponData.isMainWeapon)
            {
                ReloadBullet();
            }
        }

        protected void ReloadBullet()
        {
            _reloadSequence.Kill();
            
            _reloadSequence = DOTween.Sequence();

            _reloadSequence
                .OnStart(() =>
                {
                    IsDoneShootAction = false;
                })
                .OnComplete(() =>
                {
                    IsDoneShootAction = true;
                });

            var now = RemainBullet;
            var max = GetWeaponStat(WeaponStat.Bullet).Total;
            var time = GetWeaponStat(WeaponStat.Reload).Total;
            var separateTime = max / time;
            
            GameManager.Instance.AddBehaviourEventCount(BehaviourEvent.장전, (int)separateTime * 100);
            
            for (int i = now; i < max; i++)
            {
                _reloadSequence
                    .AppendCallback(() =>
                    {
                        RemainBullet++;
                    })
                    .AppendInterval(separateTime);
            }

            _reloadSequence.Play();
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
        
        public BaseStat<WeaponStat> GetWeaponBaseStat()
        {
            return _baseWeaponStat;
        }
        
        public float GetWeaponStatTotal(WeaponStat stat)
        {
            return _baseWeaponStat.GetStat(stat).Total;
        }

        public void ClearWeaponStat()
        {
            _baseWeaponStat.ClearStatList();
            foreach (var stat in _weaponData.basicWeaponStat)
            {
                _baseWeaponStat.AddStat(stat);
            }
        }

        #endregion
    }
}