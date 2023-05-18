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
            gameObject.SetActive(NetworkActive);
        }
        
        protected NetworkProjectileHolder _projectileHolder;
        private Rigidbody _rigidbody;

        // 기본 스탯
        protected float _baseStat(WeaponStat weaponStat) => _projectileHolder.GetWeaponStatTotal(weaponStat);
        protected float MaxRange => _baseStat(WeaponStat.Range);
        protected float Distance;
        
        // 기본 + 변동 스탯
        protected float TotalVelocity => _baseStat(WeaponStat.Velocity) + IndividualVelocity;
        public float IndividualVelocity;
        protected float TotalDamage => _baseStat(WeaponStat.Damage) + IndividualDamage;
        public float IndividualDamage;

        // 네트워크 관련
        [Networked] protected NetworkBool IsHit { get; set; }
        [Networked] public float Damage { get; set; }
        public float DamageSave;
         
        // 초기화
        public void Initialized(NetworkProjectileHolder holder)
        {
            if (_projectileHolder is not null)
            {
                Debug.LogError("ProjectileBase가 2번 초기화 되었습니다.");
            }
            
            _projectileHolder = holder;

            IndividualVelocity = 0;
            IndividualDamage = 0;
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
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
            DamageSave = Damage;

            if (!HasStateAuthority) return;
            
            Distance += Runner.DeltaTime * TotalVelocity;
            Damage = TotalDamage;
            
            UpdateProjectile();
            
            if (IsExpirationProjectile())
            {
                DestroyProjectile();
            }
        }
        
        public void DestroyProjectile()
        {
            NetworkActive = false;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if(IsHit || !HasStateAuthority) return;
            
            if (other.gameObject.TryGetComponent(out ICollisionObjectEvent collisionObject))
            { // 추후 해당 인터페이스로 변경할 것.
                collisionObject.CollisionObjectEvent(Object);
                if (!collisionObject.CollisionObjectIsHitCheck())
                {
                    IsHit = true;
                }
            }
        }
        
        #region 오버라이드 메소드 (abstract, virtual)
        // 총알 파괴 조건
        protected abstract bool IsExpirationProjectile();
        
        protected virtual void UpdateProjectile()
        {
            _rigidbody.velocity = transform.forward * TotalVelocity;
        }
        
        #endregion
    }
}