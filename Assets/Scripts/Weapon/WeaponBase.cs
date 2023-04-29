using System.Collections.Generic;
using Types;
using UnityEngine;
using GameStatus;

namespace Weapon
{
    public abstract class WeaponBase : MonoBehaviour
    {
        protected BaseStat<WeaponStat> BaseWeaponStat;
        protected Transform WeaponPos;
        protected float CoolTime;
        protected int Level;

        private void Awake()
        {
            BaseWeaponStat = new BaseStat<WeaponStat>(1 ,1);
            CoolTime = 0;
            Level = 0;
            WeaponPos = GameObject.Find("Gun").transform;
            Initialize();
        }
        
        private void FixedUpdate()
        {
            CoolTime += Time.deltaTime;
        }

        public void IncreaseLevel()
        {
            if (4 > Level) Level++;
        }
        
        public void DecreaseLevel()
        {
            if (Level > 1) Level--;
        }

        public int GetLevel()
        {
            return Level;
        }

        public void AddWeaponStat(Stat<WeaponStat> stat)
        {
            BaseWeaponStat.AddStat(stat);
        }
        
        public void AddWeaponStatList(List<Stat<WeaponStat>> statList)
        {
            BaseWeaponStat.AddStatList(statList);
        }
        
        public Stat<WeaponStat> GetWeaponStat(WeaponStat stat)
        {
            return BaseWeaponStat.GetStat(stat);
        }

        public void ClearWeaponStat()
        {
            BaseWeaponStat.ClearStatList();
        }

        protected virtual bool CheckCoolTime()
        {
            var interval = BaseWeaponStat.GetStat(WeaponStat.Interval).Total;
            return CoolTime > interval;
        }

        protected abstract void Initialize();
        public abstract void Attack();
    }
}