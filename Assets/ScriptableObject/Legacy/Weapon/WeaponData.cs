using System.Collections.Generic;
using GameStatus;
using UnityEngine;
using Type;

[ CreateAssetMenu(fileName = "WeaponData", menuName = "SO/WeaponData" )]
public class WeaponData : ScriptableObject
{
    public Sprite weaponSprite;
    public string weaponName;
    public List<Stat<WeaponStat>> statList = new List<Stat<WeaponStat>>();
}