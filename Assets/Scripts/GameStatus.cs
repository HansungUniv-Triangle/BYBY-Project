using System;
using System.Collections.Generic;
using UnityEngine;
using Types;

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
        
        [field: SerializeReference]
        public float Addition { get; private set; }
        
        public float Total
        {
            get
            {
                var calcValue = Amount * Ratio + Addition;
                return calcValue > 1 ? calcValue : 1;
            }
        }

        public Stat(T type, float amount, float ratio)
        {
            Type = type;
            Amount = amount;
            Ratio = ratio;
            Addition = 0;
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
        
        public Stat<T> SetAddition(float add)
        {
            Addition = add;
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
        
        public Stat<T> AddAddition(float add)
        {
            Addition += add;
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
        private float _defaultAmount;
        private float _defaultRatio;
        
        public BaseStat(int amount, int ratio)
        {
            _defaultAmount = amount;
            _defaultRatio = ratio;
            
            _statList = new List<Stat<T>>();
            foreach (T stat in Enum.GetValues(typeof(T)))
            {
                _statList.Add(new Stat<T>(stat, amount, ratio));
            }
        }
        
        public void ClearStatList()
        {
            foreach (var stat in _statList)
            {
                stat.SetAmount(_defaultAmount);
                stat.SetRatio(_defaultRatio);
                stat.SetAddition(0);
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
