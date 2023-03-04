using GameStatus;
using Type;
using UnityEngine;

namespace Weapon
{
    public class ShieldGenerator : WeaponBase
    {
        private ProjectileHolder<Shield> _projectileHolder;

        protected override void Initialize()
        {
            _projectileHolder = new ProjectileHolder<Shield>("Shield");
            
            BaseWeaponStat = new BaseStat<WeaponStat>();
            BaseWeaponStat.GetStat(WeaponStat.BulletSize).AddRatio(-0.5f);
            BaseWeaponStat.GetStat(WeaponStat.Range).AddRatio(200.0f);
            BaseWeaponStat.GetStat(WeaponStat.Velocity).AddRatio(200.0f);
            
            Level = 0;
            CoolTime = 0;
            
            // 임시로 위치 얻어오기
            WeaponPos = GameObject.Find("Player").transform;
        }

        public override void Attack()
        {
            if (CheckCoolTime())
            {
                _projectileHolder.GetProjectile(WeaponPos)
                    .AddBaseStat(BaseWeaponStat.GetStat(WeaponStat.BulletSize))
                    .AddBaseStat(BaseWeaponStat.GetStat(WeaponStat.Range))
                    .AddBaseStat(BaseWeaponStat.GetStat(WeaponStat.Velocity));
                CoolTime = 0;
            }
        }
    }
}