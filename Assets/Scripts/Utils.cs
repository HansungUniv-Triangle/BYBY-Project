using System;
using GameStatus;
using Types;

// ReSharper disable once CheckNamespace
namespace Utils
{
    public static class Path
    {
        public const string Synergy = "Synergy";
        public const string Weapon = "Weapon";
        public const string Loading = "Loading";
        public const string Disconnect = "Disconnect";
    }

    public static class StatConverter
    {
        public static float ConversionStatValue(Stat<CharStat> stat)
        {
            var total = stat.Total;
            
            switch (stat.Type)
            {
                case CharStat.Health:
                    return 10 + total;
                case CharStat.Speed:
                    return 5 + total * 0.02f;
                case CharStat.Rolling:
                    return total;
                case CharStat.Armor:
                    return 100 / (100 + total);
                case CharStat.Calm:
                    return total;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public static float ConversionStatValue(Stat<WeaponStat> stat)
        {
            var total = stat.Total;
            
            switch (stat.Type)
            {
                case WeaponStat.Interval:
                    if (total >= 100)
                    {
                        return (float)Math.Max(0, 0.1 - (total - 100) * 0.001);
                    }
                    else
                    {
                        return (float)(2 - (1.9 * total / 100));
                    }
                case WeaponStat.Special:
                    return total;
                case WeaponStat.Damage:
                    return total;
                case WeaponStat.Range:
                    return 10 + total;
                case WeaponStat.Reload:
                    return total;
                case WeaponStat.Bullet:
                    return total;
                case WeaponStat.Velocity:
                    return 10 + total;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}