using System;
using GameStatus;
using Type;
using UnityEngine;

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
        private Stat<WeaponStat> _range;
        private Stat<WeaponStat> _velocity;
        private Stat<WeaponStat> _bulletSize;
        private Stat<WeaponStat> _damage;
    
        // 변동 스탯
        protected Vector3 Direction;
        protected float Distance;
        protected float AddVelocity;
        protected float AddScale;
        protected float AddDamage;
    
        // 기본 + 변동 스탯
        public float TotalVelocity => _velocity.Total + AddVelocity;
        public float TotalBulletSize => _bulletSize.Total + AddScale;
        public float TotalDamage => _damage.Total + AddDamage;
        protected float MaxRange => _range.Total;
        
        
        // 스탯 초기화
        public void Initialized(ProjectileHolder<T> holder)
        {
            InitializedStat();
            SetHolder(holder);
            AddBulletSize(_bulletSize.Total);
        }
        
        private void SetHolder(ProjectileHolder<T> holder)
        {
            _projectileHolder = holder;
        }

        // 스탯 지정
        public ProjectileBase<T> AddBaseStat(Stat<WeaponStat> stat)
        {
            switch (stat.Type)
            {
                case WeaponStat.Range:
                    _range.AddStat(stat);
                    break;
                case WeaponStat.Damage:
                    _damage.AddStat(stat);
                    break;
                case WeaponStat.Velocity:
                    _velocity.AddStat(stat);
                    break;
                case WeaponStat.BulletSize:
                    _bulletSize.AddStat(stat);
                    break;
                case WeaponStat.Interval:
                case WeaponStat.ShotAtOnce:
                case WeaponStat.Reload:
                case WeaponStat.Bullet:
                case WeaponStat.ShieldBreak:
                case WeaponStat.Special:
                case WeaponStat.Pierce:
                case WeaponStat.Guided:
                case WeaponStat.MaxLevel:
                default:
                    throw new ArgumentOutOfRangeException(nameof(stat), stat, null);
            }
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
                if (_projectileHolder is null)
                {
                    throw new Exception("홀더 지정 에러");
                }
                _projectileHolder.RemoveProjectile(this);
            }
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
            _bulletSize.AddAmount(AddScale);
            var size = TotalBulletSize;
            transform.localScale = new Vector3(size, size, size);
            AddScale = 0;
        }

        #region 오버라이드 메소드 (abstract, virtual)
    
        // 총알 파괴 조건
        protected abstract bool CheckDestroy();

        // 총알이 닿았을 때
        protected abstract void OnCollisionEnter(Collision collision);

        // 총알이 어떻게 움직이는가
        protected abstract void MoveProjectile();

        protected virtual void InitializedStat()
        {
            _range = new Stat<WeaponStat>(WeaponStat.Range, 1);
            _velocity = new Stat<WeaponStat>(WeaponStat.Velocity, 0.1f);
            _bulletSize = new Stat<WeaponStat>(WeaponStat.BulletSize, 1);
            _damage = new Stat<WeaponStat>(WeaponStat.Damage, 1);
        
            Distance = 0;
            Direction = Vector3.forward;
            AddVelocity = 0;
            AddScale = 0;
            AddDamage = 0;
        }
        
        #endregion
    }
}
