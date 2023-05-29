using Types;
using UnityEngine;

namespace Network
{
    public class BuffMachine : NetworkProjectileHolder
    {
        private bool IsAlreadyBuff = false;
        
        protected override void Attack()
        {
            if (CanAttack())
            {
                NetworkPlayer player = GameManager.Instance.NetworkManager.PlayerCharacter;
                if (player.GetNowHp() < (player.GetMaxHp() / 3))
                {
                    var armor = player.GetCharStat(CharStat.Armor);
                    armor.AddAddition(armor.Total / 2);
                    
                    var speed = player.GetCharStat(CharStat.Speed);
                    speed.AddAddition(speed.Total / 2);

                    var weapons = player.GetProjectileHolderList();
                    foreach (var networkProjectileHolder in weapons)
                    {
                        var damage = networkProjectileHolder.GetWeaponStat(WeaponStat.Attack);
                        damage.AddAddition(damage.Total / 2);
                    }
                    IsAlreadyBuff = true;
                }
            }
        }

        protected override bool CanAttack()
        {
            return !IsAlreadyBuff;
        }
    }
}