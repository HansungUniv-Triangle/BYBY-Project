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
            
            BaseWeaponStat.GetStat(WeaponStat.BulletSize).AddRatio(-0.5f);
            BaseWeaponStat.GetStat(WeaponStat.Range).AddRatio(360.0f);
            BaseWeaponStat.GetStat(WeaponStat.Velocity).AddRatio(360.0f);
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