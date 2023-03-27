using Types;

/* HandGun : WeaponBase
 * 기본적인 권총을 구현한 클래스
 * 반드시 Initialize에서 Holder를 생성해야 합니다.
 * Level 사용은 아직 정하지 않았음.
 * 추후 weaponpos 변경
 */

namespace Weapon
{
    public class HandGun : WeaponBase
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
                .AddBaseStat(BaseWeaponStat.GetStat(WeaponStat.Velocity));
            CoolTime = 0;
        }
    }
}