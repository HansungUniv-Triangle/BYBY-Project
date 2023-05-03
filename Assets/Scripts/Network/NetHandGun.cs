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
                DOTween.Sequence()
                    .OnStart(() => RemainBullet--)
                    .AppendCallback(() => SpawnProjectile(WeaponTransform))
                    .AppendCallback(() => AddCharAdditionStat(CharStat.Speed, 10))
                    .AppendInterval(0.1f)
                    .AppendCallback(() => AddCharAdditionStat(CharStat.Speed, -10));
            }
        }
    }
}