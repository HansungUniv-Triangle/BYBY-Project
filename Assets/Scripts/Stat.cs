using System;
using System.Collections.Generic;
using Type;

namespace Status
{
    [Serializable]
    public class RatioStat
    {
        public Stat statType;
        public float amount = 0;
        public float ratio = 0;

        public RatioStat(Stat statType)
        {
            this.statType = statType;
            amount = 1;
            ratio = 1;
        }

        public float CalcValue()
        {
            return amount * ratio;
        }
    }

    [Serializable]
    public class BaseStat
    {
        public List<RatioStat> statList = new List<RatioStat>();

        public float Speed => statList[(int)Stat.Speed].CalcValue();
        public float FireRate => statList[(int)Stat.FireRate].CalcValue();
        public int ShotAtOnce => (int)statList[(int)Stat.ShotAtOnce].CalcValue();
        public float Special => statList[(int)Stat.Special].CalcValue();
        public float Reload => statList[(int)Stat.Reload].CalcValue();
        public int Bullet => (int)statList[(int)Stat.Bullet].CalcValue();
        public float Range => statList[(int)Stat.Range].CalcValue();
        public float Shield => statList[(int)Stat.ShieldBreak].CalcValue();
        public float Damage => statList[(int)Stat.Damage].CalcValue();
        public float Size => statList[(int)Stat.BulletSize].CalcValue();
        public float Velocity => statList[(int)Stat.Velocity].CalcValue();
        
        public BaseStat()
        {
            foreach (Stat stat in Enum.GetValues(typeof(Stat)))
            {
                statList.Add(new RatioStat(stat));
            }
        }

        public void ClearStatList()
        {
            foreach (var ratioStat in statList)
            {
                ratioStat.amount = 1;
                ratioStat.ratio = 1;
            }
        }

        public void AddRatioStat(List<RatioStat> ratioStatList)
        {
            foreach (var ratioStat in ratioStatList)
            {
                var stat = GetRatioStat(ratioStat.statType);
                stat.amount += ratioStat.amount;
                stat.ratio += ratioStat.ratio;
            }
        }

        private RatioStat GetRatioStat(Stat stat) => statList.Find(e => e.statType.Equals(stat));
    }
}
