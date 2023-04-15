using System;
using UnityEngine;

using Fusion;
using Types;
using Utils;

namespace Network
{
    public abstract class NetworkProjectileBase : NetworkBehaviour
    {
        private NetworkProjectileHolder _projectileHolder;
        private NetworkObject _projectileNetworkObject;

        // 기본 스탯
        private float _baseStat(WeaponStat weaponStat) => _projectileHolder.GetWeaponStatTotal(weaponStat);

        // 변동 스탯
        protected float Distance;
        private float _additionalVelocity;
        private float _additionalScale;
        private float _additionalDamage;
    
        // 기본 + 변동 스탯
        public float TotalVelocity => _baseStat(WeaponStat.Velocity) + _additionalVelocity;
        public float TotalScale => _baseStat(WeaponStat.BulletSize) + _additionalScale;
        public float TotalDamage => _baseStat(WeaponStat.Damage) + _additionalDamage;
        protected float MaxRange => _baseStat(WeaponStat.Range);
        
        // 초기화
        public void Initialized(NetworkProjectileHolder holder)
        {
            if (_projectileHolder is not null)
            {
                Debug.LogError("ProjectileBase가 2번 초기화 되었습니다.");
            }
            
            _projectileHolder = holder;
            _projectileNetworkObject = GetComponent<NetworkObject>();
            _additionalVelocity = 0;
            _additionalScale = 0;
            _additionalDamage = 0;
            
            transform.localScale = new Vector3(TotalScale, TotalScale, TotalScale);
        }

        public override void Spawned()
        {
            if (!Object.HasStateAuthority)
                GetComponent<Rigidbody>().isKinematic = true;
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasInputAuthority) return;
            
            Distance += Runner.DeltaTime * TotalVelocity;
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
            _projectileHolder.RemoveProjectile(_projectileNetworkObject);
        }

        protected void AddScale(float scale)
        {
            _additionalScale += scale;
            transform.localScale = new Vector3(TotalScale, TotalScale, TotalScale);
        }
        
        protected void AddVelocity(float velocity)
        {
            _additionalVelocity += velocity;
        }
        
        protected void AddDamage(float damage)
        {
            _additionalDamage += damage;
        }
        
        #region 오버라이드 메소드 (abstract, virtual)
        // 총알 파괴 조건
        protected abstract bool IsExpirationProjectile();
        // 총알이 어떻게 움직이는가
        protected abstract void UpdateProjectile();
        #endregion
    }
}