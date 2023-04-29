using System;
using UnityEngine;

using Fusion;
using Types;
using Utils;

namespace Network
{
    public abstract class NetworkProjectileBase : NetworkBehaviour
    {
        [Networked(OnChanged = nameof(DeActiveNetworkObject))] 
        protected NetworkBool NetworkActive { get; set; } = true;

        private static void DeActiveNetworkObject(Changed<NetworkProjectileBase> changed)
        {
            changed.Behaviour.DeActiveNetworkObject();
        }
        
        private void DeActiveNetworkObject()
        {
            if(_projectileHolder)
                _projectileHolder.RemoveProjectile(Object, NetworkActive);
            gameObject.SetActive(NetworkActive);
        }
        
        private NetworkProjectileHolder _projectileHolder;

        // 기본 스탯
        protected float _baseStat(WeaponStat weaponStat) => _projectileHolder.GetWeaponStatTotal(weaponStat);
        protected float MaxRange => _baseStat(WeaponStat.Range);
        protected float Distance;
        
        // 기본 + 변동 스탯
        protected float TotalVelocity => _baseStat(WeaponStat.Velocity) + IndividualVelocity;
        protected float IndividualVelocity;
        protected float TotalScale => _baseStat(WeaponStat.BulletSize) + IndividualScale;
        protected float IndividualScale;
        protected float TotalDamage => _baseStat(WeaponStat.Damage) + IndividualDamage;
        protected float IndividualDamage;
        
        
        // 네트워크 관련
        [Networked] protected NetworkBool IsHit { get; set; }
        [Networked] public float Damage { get; set; }
         
        // 초기화
        public void Initialized(NetworkProjectileHolder holder)
        {
            if (_projectileHolder is not null)
            {
                Debug.LogError("ProjectileBase가 2번 초기화 되었습니다.");
            }

            IndividualVelocity = 0;
            IndividualScale = 0;
            IndividualDamage = 0;
            
            _projectileHolder = holder;
            transform.localScale = new Vector3(TotalScale, TotalScale, TotalScale);
        }

        public override void Spawned()
        {
            GameManager.Instance.NetworkManager.AddNetworkObjectInList(Object);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            GameManager.Instance.NetworkManager.RemoveDeSpawnNetworkObject(Object);
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasInputAuthority) return;
            
            Distance += Runner.DeltaTime * TotalVelocity;
            Damage = TotalDamage;
            UpdateProjectile();
            
            if (IsExpirationProjectile())
            {
                DestroyProjectile();
            }
        }
        
        private void DestroyProjectile()
        {
            if (_projectileHolder is null)
            {
                throw new Exception($"{nameof(gameObject)} : {Message.CantAssignHolder}");
            }

            NetworkActive = false;
        }

        #region 오버라이드 메소드 (abstract, virtual)
        // 총알 파괴 조건
        protected abstract bool IsExpirationProjectile();
        // 총알이 어떻게 움직이는가
        protected abstract void UpdateProjectile();
        #endregion
    }
}