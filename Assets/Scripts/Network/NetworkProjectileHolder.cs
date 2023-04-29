﻿using System.Collections.Generic;
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
        private BaseStat<WeaponStat> _baseWeaponStat;
        private int _level;
        private List<NetworkObject> _projectileList;
        [SerializeField]
        private NetworkObject _projectileObject;

        protected Transform WeaponTransform;
        protected Vector3 Target;
        protected bool IsDoneShootAction;
        protected int RemainBullet;
        protected TextMeshProUGUI BulletText;
        
        [Networked] private TickTimer delay { get; set; }

        private void Awake()
        {
            _level = 1;
            _baseWeaponStat = new BaseStat<WeaponStat>(1, 1);
            _projectileList = new List<NetworkObject>();
            _baseWeaponStat.AddStat(new Stat<WeaponStat>(WeaponStat.Velocity, 20, 0));
            _baseWeaponStat.AddStat(new Stat<WeaponStat>(WeaponStat.Range, 10, 0));
            _baseWeaponStat.AddStat(new Stat<WeaponStat>(WeaponStat.Special, 12, 0));
            _baseWeaponStat.AddStat(new Stat<WeaponStat>(WeaponStat.Bullet, 4, 0));
            
            WeaponTransform = gameObject.transform;
            Target = gameObject.transform.forward;
            IsDoneShootAction = true;
        }

        private void Start()
        {
            if(!_projectileObject.TryGetComponent<NetworkProjectileBase>(out _))
            {
                Debug.LogError("Holder 연결 에러");
            }
            
            RemainBullet = (int)GetWeaponStat(WeaponStat.Bullet).Total;
            BulletText = (GameManager.Instance.UIHolder as GameUI).bulletText;
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasInputAuthority) return;
            
            if (RemainBullet == 0)
            {
                ReloadBullet();
            }
            else
            {
                Attack();
            }

            BulletText.text = RemainBullet.ToString();
        }

        public void SetTarget(Vector3 target)
        {
            Target = target;
        }
        
        private void InitializeProjectile(NetworkRunner runner, NetworkObject obj)
        {
            var objInit = obj.GetComponent<NetworkProjectileBase>();
            objInit.Initialized(this);
        }

        public void RemoveProjectile(NetworkObject projectile, bool networkActive)
        {
            if (networkActive) return;
            _projectileList.Remove(projectile);
        }

        protected void SpawnProjectile(Transform position)
        {
            var obj = Runner.Spawn(
                _projectileObject, 
                position.position + Vector3.forward, 
                Quaternion.LookRotation(Target - gameObject.transform.position), 
                Runner.LocalPlayer,
                InitializeProjectile
            );
            _projectileList.Add(obj);
        }
        
        protected virtual bool CanAttack()
        {
            if (!IsDoneShootAction)
            {
                return false;
            }
            
            if (delay.ExpiredOrNotRunning(Runner))
            {
                delay = TickTimer.CreateFromSeconds(Runner, _baseWeaponStat.GetStat(WeaponStat.Interval).Total);
                return true;
            }
            else
            {
                return false;
            }
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

            var max = (int)GetWeaponStat(WeaponStat.Bullet).Total;
            var time = (int)GetWeaponStat(WeaponStat.Reload).Total;
            
            for (int i = 0; i < max; i++)
            {
                reloadSequence
                    .AppendCallback(() => RemainBullet++)
                    .AppendInterval(0.5f);
            }

            reloadSequence.Play();
        }

        protected abstract void Attack();

        #region 레벨

        public void IncreaseLevel()
        {
            if (4 > _level) _level++;
        }
        
        public void DecreaseLevel()
        {
            if (_level > 1) _level--;
        }

        public int GetLevel()
        {
            return _level;
        }

        #endregion

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
            var localCharacter = GameManager.Instance.NetworkManager.LocalCharacter;
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