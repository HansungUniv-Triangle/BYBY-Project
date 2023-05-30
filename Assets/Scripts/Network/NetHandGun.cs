using DG.Tweening;
using Types;

namespace Network
{
    public class NetHandGun : NetworkProjectileHolder
    {
        protected override void Attack()
        {
            if (CanAttack())
            {
                var speedValue = GetCharStat(CharStat.Speed).Total;
                var specialValue = GetWeaponStat(WeaponStat.Special).Total / 50;
                var calcValue = speedValue * specialValue;

                DOTween.Sequence()
                    .OnStart(() => UpdateBullet(-1))
                    .AppendCallback(() => SpawnProjectile(ShootPointTransform.position))
                    .AppendCallback(() => AddCharAdditionStat(CharStat.Speed, +calcValue))
                    .AppendInterval(0.1f)
                    .AppendCallback(() => AddCharAdditionStat(CharStat.Speed, -calcValue));
            }
        }
    }
}