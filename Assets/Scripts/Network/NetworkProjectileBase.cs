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
            
            _projectileHolder = holder;
            _additionalVelocity = 0;
            _additionalScale = 0;
            _additionalDamage = 0;
            
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

        protected NetworkPlayer GetNetworkPlayer(GameObject character)
        {
            if (character.TryGetComponent<NetworkPlayer>(out var networkPlayer))
            {
                return networkPlayer;
            }
            else
            {
                throw new Exception("히트 플레이어, 게임오브젝트에서 네트워크 플레이어를 찾을 수 없었음");
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        protected void RPCHitPlayer()
        {
            IsHit = true;
        }
        
        #region 오버라이드 메소드 (abstract, virtual)
        // 총알 파괴 조건
        protected abstract bool IsExpirationProjectile();
        // 총알이 어떻게 움직이는가
        protected abstract void UpdateProjectile();
        #endregion
    }
}