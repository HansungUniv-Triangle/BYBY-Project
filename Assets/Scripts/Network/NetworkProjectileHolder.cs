using System;
using System.Collections.Generic;
using DG.Tweening;
using Fusion;
using GameStatus;
using TMPro;
using Types;
using UIHolder;
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

        public int GetBullet()
        {
            return _remainBullet;
        }
        
        public void SetBullet()
        {
            if (WeaponData.isMainWeapon)
            {
                _remainBullet = (int)GetWeaponStat(WeaponStat.Bullet).Total;
                _maxBullet = (int)GetWeaponStat(WeaponStat.Bullet).Total;
            }
        }

        protected void UpdateBullet(int value)
        {
            _remainBullet += value;
            
            if (WeaponData.isMainWeapon)
            {
                GameManager.Instance.NetworkManager.UpdateBullet(_remainBullet, _maxBullet);
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasInputAuthority || !IsAttacking || !GameManager.Instance.NetworkManager.CanControlCharacter)
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
            if (!IsDoneShootAction)
            {
                return false;
            }
            
            if (_remainBullet <= 0 && _weaponData.isMainWeapon)
            {
                ReloadBullet();
                return false;
            }
            
            if (delay.ExpiredOrNotRunning(Runner))
            {
                SetDelayTimer();
                return true;
            }
            
            return false;
        }

        protected void SetDelayTimer()
        {
            var delayValue = StatConverter.ConversionStatValue(_baseWeaponStat.GetStat(WeaponStat.Interval));
            delay = TickTimer.CreateFromSeconds(Runner, delayValue);
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
            if (IsDoneShootAction && !attackMode && WeaponData.isMainWeapon)
            {
                ReloadBullet();
            }
        }

        protected void ReloadBullet()
        {
            var gameUI = GameManager.Instance.UIHolder as GameUI ?? FindObjectOfType<GameUI>();
            
            _reloadSequence.Kill();
            
            _reloadSequence = DOTween.Sequence();

            _reloadSequence
                .OnStart(() =>
                {
                    gameUI.UpdateCircleReload(true);
                    IsDoneShootAction = false;
                })
                .OnComplete(() =>
                {
                    gameUI.UpdateCircleReload(false);
                    IsDoneShootAction = true;
                });

            var now = _remainBullet;
            var max = GetWeaponStat(WeaponStat.Bullet).Total;
            var time = GetWeaponStat(WeaponStat.Reload).Total;
            var separateTime = max / time;
            
            GameManager.Instance.AddBehaviourEventCount(BehaviourEvent.장전, (int)(separateTime * 100));
            
            for (int i = now; i < max; i++)
            {
                _reloadSequence
                    .AppendCallback(() =>
                    {
                        UpdateBullet(+1);
                    })
                    .AppendInterval(separateTime);
            }
        }

        protected abstract void Attack();
        
        public void ForcedAttack()
        {
            delay = TickTimer.CreateFromSeconds(Runner, 0);
        }

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