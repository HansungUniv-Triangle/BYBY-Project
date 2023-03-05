/* BasicBullet
 * 기본 총알
 */

using System;
using UnityEngine;
using Util;

namespace Weapon
{
    public class BasicBullet : ProjectileBase<BasicBullet>
    {
        protected override void MoveProjectile()
        {
            gameObject.transform.Translate(Direction * TotalVelocity);
        }
        
        protected override bool CheckDestroy()
        {
            return Distance > MaxRange;
        }
        
        protected override void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Block"))
            {
                if (collision.gameObject.TryGetComponent<DamagedBlock>(out var damagedBlockScript))
                {
                    damagedBlockScript.DecreaseHP(TotalDamage);
                }
                else
                {
                    throw new Exception(Message.CantFindBlockTagInDamagedBlock);
                }
            }
        }
    }
}