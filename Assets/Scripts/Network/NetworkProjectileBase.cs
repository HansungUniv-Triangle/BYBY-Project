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
        protected NetworkProjectileHolder _projectileHolder;
        protected Rigidbody _rigidbody;

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
        protected float TotalDamage => StatConverter.ConversionStatValue(_baseStat(WeaponStat.Attack)) + IndividualDamage;
        public float IndividualDamage;

        // 네트워크 관련
        [Networked(OnChanged = nameof(HitEffect))] protected NetworkBool IsHit { get; set; }
        [Networked] protected NetworkBool IsEnemyHit { get; set; }
        [Networked] protected NetworkBool NetworkActive { get; set; } = true;
        [Networked] private int NetWeaponData { get; set; } = -1;

        private bool _isSendDestroy = false;

        private static void HitEffect(Changed<NetworkProjectileBase> changed)
        {
            changed.Behaviour.HitEffect();
        }
        private void HitEffect()
        {
            var particle = IsEnemyHit ? WeaponData.bulletHitToPlayer : WeaponData.bulletHit;
            EffectManager.Instance.PlayEffect(particle, transform.position, -transform.forward);

            SoundManager.Instance.Play(WeaponData.hitSoundPath, Sound.Effect);
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
            if (!HasStateAuthority)
            {
                gameObject.layer = LayerMask.NameToLayer("Bullet");
            }

            GameManager.Instance.NetworkManager.AddNetworkObjectInList(Object);
            EffectManager.Instance.PlayEffect(WeaponData.bulletShoot, transform.position, -transform.forward);
            SoundManager.Instance.Play(WeaponData.shootSoundPath, Sound.Effect);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            GameManager.Instance.NetworkManager.RemoveDeSpawnNetworkObject(Object);
        }

        public override void FixedUpdateNetwork()
        {
            if (NetworkActive != gameObject.activeSelf)
            { 
                gameObject.SetActive(NetworkActive);
            }
            
            if (NetworkActive && HasStateAuthority)
            {
                Distance += Runner.DeltaTime * TotalVelocity;
                Damage = TotalDamage;
            
                UpdateProjectile();
            
                if (IsExpirationProjectile())
                {
                    if (!_isSendDestroy && _projectileHolder.WeaponData.isMainWeapon && Distance > MaxRange)
                    {
                        _isSendDestroy = true;
                        GameManager.Instance.CheckBulletBetweenEnemyAndMe(transform.position);
                    }
                
                    DestroyProjectile();
                }
            }
            
            DamageSave = Damage;
        }
        
        public void DestroyProjectile()
        {
            NetworkActive = false;
        }
        
        protected virtual void OnTriggerEnter(Collider other)
        {
            if(IsHit || !HasStateAuthority) return;
            
            if (other.gameObject.TryGetComponent(out ICollisionObjectEvent collisionObject))
            {
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