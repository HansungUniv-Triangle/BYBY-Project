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
        public const string Disconnect = "Disconnecting";
        public const string Cat = "Cat";
    }

    public static class GameInfo
    {
        public const float GameStartWait = 10f;
        public const float SynergySelectTime = 40f;
        public const float RoundTime = 120f;
        public const float RoundAnalysis = 10f;
        public const int MaxRound = 5;
        public const int WinRound = MaxRound / 2 + 1;
    }

    public static class StatConverter
    {
        public static float ConversionStatValue(Stat<CharStat> stat)
        {
            var total = stat.Total;
            
            switch (stat.Type)
            {
                case CharStat.Health:
                    return total * 5;
                case CharStat.Speed:
                    return 2 + total * 0.2f;
                case CharStat.Dodge:
                    return 15 + total * 0.5f;
                case CharStat.Armor:
                    return 50 / (50 + total);
                case CharStat.Calm:
                    var value = 200 - (total * 5);
                    return value > 0 ? value : 1;
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
                case WeaponStat.Attack:
                    return total;
                case WeaponStat.Range:
                    return 10 + total;
                case WeaponStat.Reload:
                    return total;
                case WeaponStat.Bullet:
                    return total;
                case WeaponStat.Velocity:
                    return 20 + total;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}