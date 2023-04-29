using System.Collections.Generic;
using Fusion;
using GameStatus;
using Types;
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
        
        [Networked] private TickTimer delay { get; set; }

        private void Awake()
        {
            _level = 1;
            _baseWeaponStat = new BaseStat<WeaponStat>(1, 1);
            _projectileList = new List<NetworkObject>();
            _baseWeaponStat.AddStat(new Stat<WeaponStat>(WeaponStat.Velocity, 20, 0));
            _baseWeaponStat.AddStat(new Stat<WeaponStat>(WeaponStat.Range, 10, 0));
            
            WeaponTransform = gameObject.transform;
            Target = gameObject.transform.forward;
        }

        private void Start()
        {
            if(!_projectileObject.TryGetComponent<NetworkProjectileBase>(out _))
            {
                Debug.LogError("Holder 연결 에러");
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasInputAuthority) return;
            
            Attack();
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

        protected abstract void Attack();

        #region 레벨

        public void IncreaseLevel()
        {
            if (_baseWeaponStat.GetStat(WeaponStat.MaxLevel).Total > _level) _level++;
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