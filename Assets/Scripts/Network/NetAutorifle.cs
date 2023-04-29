using DG.Tweening;
using Types;

namespace Network
{
    public class NetAutorifle : NetworkProjectileHolder
    {
        protected override void Attack()
        {
            if (CanAttack())
            {
                var sequence = DOTween.Sequence()
                    .SetAutoKill(false)
                    .OnStart(() => IsDoneShootAction = false)
                    .OnComplete(() => IsDoneShootAction = true);
                    
                for (int i = 0; i < GetWeaponStatTotal(WeaponStat.Special); i++)
                {
                    sequence
                        .AppendCallback(() =>
                        {
                            SpawnProjectile(WeaponTransform);
                            RemainBullet--;
                        })
                        .AppendInterval(0.05f);
                }
                
                sequence.Play();
            }
        }
    }
}