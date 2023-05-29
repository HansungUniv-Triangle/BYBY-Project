using DG.Tweening;
using Types;
using UnityEngine;

namespace Network
{
    public class NetCannon : NetworkProjectileHolder
    {
        protected override void Attack()
        {
            if (CanAttack())
            {
                Debug.Log("shoot");
                
                var speed = GetCharStat(CharStat.Speed).Total * 0.8f;

                DOTween.Sequence()
                    .OnStart(() =>
                    {
                        IsDoneShootAction = false;
                        SpawnProjectile(ShootPointTransform.position);
                        RemainBullet--;
                        var why = RemainBullet;
                    })
                    .AppendCallback(() => AddCharAdditionStat(CharStat.Speed, -speed))
                    .AppendInterval(2.0f)
                    .AppendCallback(() => AddCharAdditionStat(CharStat.Speed, +speed))
                    .AppendCallback(() => IsDoneShootAction = true);
            }
        }
    }
}