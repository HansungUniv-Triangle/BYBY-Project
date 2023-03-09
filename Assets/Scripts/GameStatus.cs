using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameStatus
{
    [Serializable]
    public class Stat<T> where T : Enum
    {
        [field: SerializeReference]
        public T Type { get; private set; }

        [field: SerializeReference]
        public float Amount { get; private set; }

        [field: SerializeReference]
        public float Ratio { get; private set; }
        
        public float Total => Amount * Ratio;
        
        public Stat(T type, float amount)
        {
            Type = type;
            Amount = amount;
            Ratio = 1f;
        }
        
        public Stat<T> SetAmount(float amount)
        {
            Amount = amount;
            return this;
        }
        
        public Stat<T> SetRatio(float ratio)
        {
            Ratio = ratio;
            return this;
        }
        
        public Stat<T> AddAmount(float amount)
        {
            Amount += amount;
            return this;
        }
        
        public Stat<T> AddRatio(float ratio)
        {
            Ratio += ratio;
            return this;
        }

        public Stat<T> AddStat(Stat<T> stat)
        {
            if (!Type.Equals(stat.Type)) throw new InvalidCastException();
            
            Amount += stat.Amount;
            Ratio += stat.Ratio;
            return this;
        }
    }

    [Serializable]
    public class BaseStat<T> where T : Enum
    {
        private readonly List<Stat<T>> _statList;
        
        public BaseStat()
        {
            _statList = new List<Stat<T>>();
            foreach (T stat in Enum.GetValues(typeof(T)))
            {
                _statList.Add(new Stat<T>(stat, 1));
            }
        }
        
        public void ClearStatList()
        {
            foreach (var stat in _statList)
            {
                stat.SetAmount(1);
                stat.SetRatio(1);
            }
        }

        public void AddStat(Stat<T> stat)
        {
            GetStat(stat.Type).AddStat(stat);
        }
        
        public void AddStatList(List<Stat<T>> statList)
        {
            foreach (var stat in statList)
            {
                AddStat(stat);
            }
        }

        public Stat<T> GetStat(T type)
        {
            return _statList.Find(e => e.Type.Equals(type));
        }
    }
}
