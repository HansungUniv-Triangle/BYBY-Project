using System.Collections.Generic;
using UnityEngine;
using Status;

[ CreateAssetMenu(fileName = "WeaponData", menuName = "SO/WeaponData" )]
public class WeaponData : ScriptableObject
{
    public Sprite weaponSprite;
    public string weaponName;
    public List<RatioStat> statList = new List<RatioStat>();
}