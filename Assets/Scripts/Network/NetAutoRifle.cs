using System;
using DG.Tweening;
using Types;

namespace Network
{
    public class NetAutoRifle : NetworkProjectileHolder
    {
        protected override void Attack()
        {
            if (CanAttack())
            {
                var sequence = DOTween.Sequence()
                    .OnStart(() => IsDoneShootAction = false)
                    .OnComplete(() => IsDoneShootAction = true);
                
                var shoot = Math.Min(GetWeaponStatTotal(WeaponStat.Special), GetBullet());

                for (int i = 0; i < shoot; i++)
                {
                    sequence
                        .AppendCallback(() =>
                        {
                            SpawnProjectile(ShootPointTransform.position);
                            UpdateBullet(-1);
                        })
                        .AppendInterval(0.1f);
                }
            }
        }
    }
}