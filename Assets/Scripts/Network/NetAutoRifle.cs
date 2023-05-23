﻿using System;
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
                
                var shoot = Math.Min(GetWeaponStatTotal(WeaponStat.Special), RemainBullet);

                for (int i = 0; i < shoot; i++)
                {
                    sequence
                        .AppendCallback(() =>
                        {
                            SpawnProjectile(ShootPointTransform);
                            RemainBullet--;
                        })
                        .AppendInterval(0.05f);
                }
            }
        }
    }
}