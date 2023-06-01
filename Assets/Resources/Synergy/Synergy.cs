using System;
using System.Collections.Generic;
using System.Linq;
using GameStatus;
using UnityEngine;
using Types;

[ CreateAssetMenu(fileName = "Synergy", menuName = "SO/Synergy" )]
public class Synergy : ScriptableObject
{
    public Rarity rarity;
    public Sprite sprite;
    public string synergyName;
    [TextArea]
    public string synergyExplain;
    public List<Stat<CharStat>> charStatList = new List<Stat<CharStat>>();
    public List<Stat<WeaponStat>> weaponStatList = new List<Stat<WeaponStat>>();

    private void OnEnable()
    {
        Awake();
    }

    private void Awake()
    {
        var checkChar = charStatList
            .GroupBy(x => x.Type)
            .Count(g => g.Count() > 1);
        
        var checkWeapon = weaponStatList
            .GroupBy(x => x.Type)
            .Count(g => g.Count() > 1);
        
        if ((checkChar > 0) || (checkWeapon > 0))
        {
            Debug.LogWarning($"{synergyName}에 중복된 stat이 존재합니다.");
        }

        if (charStatList.Count == 0 && weaponStatList.Count == 0)
        {
            Debug.LogWarning($"{synergyName}에 시너지 정보가 없어요.");
        }

        string explainText = "";
        
        foreach (var stat in charStatList)
        {
            explainText += ConvertTypeToString(stat.Type);
            
            explainText += stat.Amount switch
            {
                > 0 => $"<color=green> +{stat.Amount}</color>",
                < 0 => $"<color=red> {stat.Amount}</color>",
                _ => ""
            };
            
            explainText += stat.Ratio switch
            {
                > 0 => $"<color=green> +{stat.Ratio * 100}%</color>",
                < 0 => $"<color=red> {stat.Ratio * 100}%</color>",
                _ => ""
            };

            explainText += "\n";
        }

        foreach (var stat in weaponStatList)
        {
            explainText += ConvertTypeToString(stat.Type);

            explainText += stat.Amount switch
            {
                > 0 => $"<color=green> +{stat.Amount}</color>",
                < 0 => $"<color=red> {stat.Amount}</color>",
                _ => ""
            };
            
            explainText += stat.Ratio switch
            {
                > 0 => $"<color=green> +{stat.Ratio * 100}%</color>",
                < 0 => $"<color=red> {stat.Ratio * 100}%</color>",
                _ => ""
            };
            
            explainText += "\n";
        }

        string n = "\n";
        int findN = explainText.LastIndexOf(n, StringComparison.Ordinal);
        string result = explainText.Remove(findN, n.Length);

        synergyExplain = result;
    }

    private string ConvertTypeToString(CharStat charStat)
    {
        return charStat switch
        {
            CharStat.Health => "체력",
            CharStat.Speed => "속도",
            CharStat.Dodge => "회피",
            CharStat.Armor => "방어",
            CharStat.Calm => "차분",
            _ => throw new ArgumentOutOfRangeException(nameof(charStat), charStat, null)
        };
    }

    private string ConvertTypeToString(WeaponStat weaponStat)
    {
        return weaponStat switch
        {
            WeaponStat.Interval => "연사",
            WeaponStat.Special => "특화",
            WeaponStat.Attack => "공격력",
            WeaponStat.Range => "사거리",
            WeaponStat.Reload => "장전",
            WeaponStat.Bullet => "탄환",
            WeaponStat.Velocity => "탄속",
            _ => throw new ArgumentOutOfRangeException(nameof(weaponStat), weaponStat, null)
        };
    }
}