using System;
using GameStatus;
using Types;
using UnityEngine;
using Utils;

/* ProjectileBase
 * 투사체에 사용하는 클래스
 * 사용법은 BasicBullet 참조바람.
 * 구현할 때 오버라이드 메소드에 주의하세요.
 * 기본 스탯을 따로 설정했는데, BaseStat<WeaponStat>으로 해도 될 듯
 */
namespace Weapon
{
    public abstract class ProjectileBase<T> : MonoBehaviour where T : ProjectileBase<T>
    {
        private ProjectileHolder<T> _projectileHolder;

        // 기본 스탯
        private BaseStat<WeaponStat> _base;

        // 변동 스탯
        protected Vector3 Direction;
        protected float Distance;
        protected float AddVelocity;
        protected float AddScale;
        protected float AddDamage;
    
        // 기본 + 변동 스탯
        public float TotalVelocity => _base.GetStat(WeaponStat.Velocity).Total + AddVelocity;
        public float TotalBulletSize => _base.GetStat(WeaponStat.BulletSize).Total + AddScale;
        public float TotalDamage => _base.GetStat(WeaponStat.Damage).Total + AddDamage;
        protected float MaxRange => _base.GetStat(WeaponStat.Range).Total;
        
        // 초기화
        public void Initialized(ProjectileHolder<T> holder)
        {
            SetHolder(holder);
            InitializedStat();
            AddBulletSize(TotalBulletSize);
        }
        
        private void InitializedStat()
        {
            _base = new BaseStat<WeaponStat>(0, 0);
            Distance = 0;
            Direction = Vector3.forward;
            AddVelocity = 0;
            AddScale = 0;
            AddDamage = 0;
        }
        
        private void SetHolder(ProjectileHolder<T> holder)
        {
            _projectileHolder = holder;
        }

        // 스탯 지정
        public ProjectileBase<T> AddBaseStat(Stat<WeaponStat> stat)
        {
            _base.AddStat(stat);
            return this;
        }

        protected void AddBulletSize(float size)
        {
            AddScale = size;
        }
    
        private void Update()
        {
            if (CheckDestroy())
            {
                DestroyProjectile();
            }
        }

        protected void DestroyProjectile()
        {
            if (_projectileHolder is null)
            {
                throw new Exception($"{nameof(T)} : {Message.CantAssignHolder}");
            }
            _projectileHolder.RemoveProjectile(this);
        }

        // 이동 거리 계산
        private void FixedUpdate()
        {
            Distance += Time.deltaTime * TotalVelocity;
            MoveProjectile();
            ChangeScale();
        }

        private void ChangeScale()
        {
            if (AddScale == 0) return;

            var sizeStat = new Stat<WeaponStat>(WeaponStat.BulletSize, AddScale, 0);
            _base.AddStat(sizeStat);
            var size = TotalBulletSize;
            transform.localScale = new Vector3(size, size, size);
            AddScale = 0;
        }

        private void GuidedTarget(float guided)
        {
            // var ray = Physics.SphereCastAll(transform.position, guided, transform.forward, 0, (int)Layer.Entity);
            //
            // foreach (var raycastHit in ray)
            // {
            //     if (raycastHit.transform.gameObject.name == "허수아비")
            //     {
            //         transform.LookAt(raycastHit.transform);
            //     }
            // }
        }

        #region 오버라이드 메소드 (abstract, virtual)
    
        // 총알 파괴 조건
        protected abstract bool CheckDestroy();

        // 총알이 어떻게 움직이는가
        protected abstract void MoveProjectile();

        #endregion
    }
}
