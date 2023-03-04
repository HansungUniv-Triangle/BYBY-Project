using Type;
using UnityEngine;
using GameStatus;

namespace Weapon
{
    public abstract class Weapon : MonoBehaviour
    {
        protected BaseStat<WeaponStat> BaseStat;
        protected float CoolTime;
        protected int Level;

        private void Awake()
        {
            BaseStat = new BaseStat<WeaponStat>();
            Level = 0;
            CoolTime = 0;
            Initialize();
        }
        
        private void FixedUpdate()
        {
            CoolTime += Time.deltaTime;
        }

        public void IncreaseLevel()
        {
            if (BaseStat.GetStat(WeaponStat.MaxLevel).Total > Level) Level++;
        }
        
        public void DecreaseLevel()
        {
            if (Level > 1) Level--;
        }

        public int GetLevel()
        {
            return Level;
        }

        protected virtual bool CheckCoolTime()
        {
            var interval = BaseStat.GetStat(WeaponStat.Interval).Total;
            return CoolTime > interval;
        }

        protected abstract void Initialize();
        public abstract void Attack();
    }
}