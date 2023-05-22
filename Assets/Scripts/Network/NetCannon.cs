using DG.Tweening;
using Types;

namespace Network
{
    public class NetCannon : NetworkProjectileHolder
    {
        protected override void Attack()
        {
            if (CanAttack())
            {
                var speed = GetCharStat(CharStat.Speed).Total * 0.8f;

                GameManager.Instance.shootCount += 1;
                
                DOTween.Sequence()
                    .OnStart(() =>
                    {
                        IsDoneShootAction = false;
                        RemainBullet--;
                    })
                    .OnComplete(() =>
                    {
                        IsDoneShootAction = true;
                    })
                    .AppendCallback(() => SpawnProjectile(ShootPointTransform))
                    .AppendCallback(() => AddCharAdditionStat(CharStat.Speed, -speed))
                    .AppendInterval(2.0f)
                    .AppendCallback(() => AddCharAdditionStat(CharStat.Speed, +speed))
                    .AppendInterval(2.0f);
            }
        }
    }
}