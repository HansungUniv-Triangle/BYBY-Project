using GameStatus;
using Type;
using UnityEngine;

/* HandGun : WeaponBase
 * 핸드건이랑 같은데 총알이 유도탄 추가
 */

namespace Weapon
{
    public class GuidedGun : WeaponBase
    {
        private ProjectileHolder<BasicBullet> _projectileHolder;

        protected override void Initialize()
        {
            _projectileHolder = new ProjectileHolder<BasicBullet>("Bullet");
        }

        public override void Attack()
        {
            if (!CheckCoolTime()) return;
            
            _projectileHolder.GetProjectile(WeaponPos)
                .AddBaseStat(BaseWeaponStat.GetStat(WeaponStat.Damage))
                .AddBaseStat(BaseWeaponStat.GetStat(WeaponStat.Range))
                .AddBaseStat(BaseWeaponStat.GetStat(WeaponStat.Velocity))
                .AddBaseStat(BaseWeaponStat.GetStat(WeaponStat.Guided));
            CoolTime = 0;
        }
    }
}