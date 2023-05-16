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
                
                DOTween.Sequence()
                    .OnStart(() => RemainBullet--)
                    .AppendCallback(() => SpawnProjectile(WeaponTransform))
                    .AppendCallback(() => AddCharAdditionStat(CharStat.Speed, +speedValue))
                    .AppendInterval(0.1f)
                    .AppendCallback(() => AddCharAdditionStat(CharStat.Speed, -speedValue));
            }
        }
    }
}