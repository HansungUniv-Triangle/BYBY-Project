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
    public string synergyExplain;
    public List<Stat<CharStat>> charStatList = new List<Stat<CharStat>>();
    public List<Stat<WeaponStat>> weaponStatList = new List<Stat<WeaponStat>>();
    
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

        if (charStatList.Count == 0 || weaponStatList.Count == 0)
        {
            Debug.LogWarning($"{synergyName}에 시너지 정보가 없어요.");
        }
    }
}