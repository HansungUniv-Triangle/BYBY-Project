using System;
using UnityEngine;
using Fusion;
using GameStatus;
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
        protected Rigidbody _rigidbody;
        
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

        // 기본 스탯
        protected Stat<WeaponStat> _baseStat(WeaponStat weaponStat) => _projectileHolder.GetWeaponStat(weaponStat);
        protected float MaxRange => StatConverter.ConversionStatValue(_baseStat(WeaponStat.Range));
        protected float Distance;
        
        // 기본 + 변동 스탯
        protected float TotalVelocity => StatConverter.ConversionStatValue(_baseStat(WeaponStat.Velocity)) + IndividualVelocity;
        public float IndividualVelocity;
        protected float TotalDamage => StatConverter.ConversionStatValue(_baseStat(WeaponStat.Damage)) + IndividualDamage;
        public float IndividualDamage;

        // 네트워크 관련
        [Networked(OnChanged = nameof(HitEffect))] 
        protected NetworkBool IsHit { get; set; }
        [Networked] 
        protected NetworkBool IsEnemyHit { get; set; }

        private static void HitEffect(Changed<NetworkProjectileBase> changed)
        {
            changed.Behaviour.HitEffect();
        }
        private void HitEffect()
        {
            var particle = IsEnemyHit ? WeaponData.bulletHitToPlayer : WeaponData.bulletHit;
            EffectManager.Instance.PlayEffect(particle, transform.position, -transform.forward);
            SoundManager.Instance.Play3DSound("hit", Sound.Effect, transform.position);
        }
        
        [Networked] public float Damage { get; set; }
        public float DamageSave;

        // 초기화
        public void Initialized(NetworkProjectileHolder holder, int netWeaponData)
        {
            _projectileHolder = holder;
            
            NetWeaponData = netWeaponData;
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
            EffectManager.Instance.PlayEffect(WeaponData.bulletShoot, transform.position, transform.forward);
            SoundManager.Instance.Play3DSound("rifle_shoot", Sound.Effect, transform.position);
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
                if (_projectileHolder.WeaponData.isMainWeapon && Distance > MaxRange)
                {
                    GameManager.Instance.CheckBulletBetweenEnemyAndMe(transform.position);
                }
                
                DestroyProjectile();
            }
        }
        
        public void DestroyProjectile()
        {
            NetworkActive = false;
        }
        
        protected virtual void OnTriggerEnter(Collider other)
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