using System.Collections;
using System.Collections.Generic;
using Types;
using UnityEngine;

public class SynergyPage
{
    public bool isRerolled = false;
    public int rerollCount = 1;
    public Rarity synergyRarity;
    public Synergy[] synergies = new Synergy[3];
    public Weapon[] weapons = new Weapon[3];
    public GameObject synergyObj = null;
    public List<Synergy> IsNumInSynergyList = new List<Synergy>();
    public List<Weapon> IsNumInWeaponList = new List<Weapon>();
    public int pageNumber;
    public Synergy selectedSynergy;
    public Weapon selectedWeapon;
    public int[] synergyRecommendationPercentage = new int[3];

    public bool AddSynergy(Synergy synergy)
    {
        if (synergies[0] == null)
        {
            synergies[0] = synergy;
            selectedSynergy = synergy;
            return true;
        }
        else if (synergies[1] == null)
        {
            synergies[1] = synergy;
            return true;
        }
        else if (synergies[2] == null)
        {
            synergies[2] = synergy;
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public bool AddWeapon(Weapon weapon)
    {
        if (weapons[0] == null)
        {
            weapons[0] = weapon;
            selectedWeapon = weapon;
            return true;
        }
        else if (weapons[1] == null)
        {
            weapons[1] = weapon;
            return true;
        }
        else if (weapons[2] == null)
        {
            weapons[2] = weapon;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Clear()
    {
        synergies[0] = null;
        synergies[1] = null;
        synergies[2] = null;
        weapons[0] = null;
        weapons[1] = null;
        weapons[2] = null;
        IsNumInSynergyList.Clear();
        IsNumInWeaponList.Clear();
    }

    public void RerollCountClear()
    {
        if(isRerolled == false)
        {
            rerollCount = 1;
        }
    }

    public void FindSelectedSynergyInSynergies(string synergyExplain)
    {
        for (int i = 0; i < synergies.Length; i++)
        {
            if (synergyExplain == synergies[i].synergyExplain)
            {
                selectedSynergy = synergies[i];
            }
        }
    }
    
    public void FindSelectedWeaponInSynergies(string weaponExplain)
    {
        for (var i = 0; i < weapons.Length; i++)
        {
            if (weaponExplain == weapons[i].weaponExplain)
            {
                selectedWeapon = weapons[i];
            }
        }
    }
    
    public bool CheckIsNumInSynergyList(Synergy randomSynergy)
    {
        if (IsNumInSynergyList.Contains(randomSynergy))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public bool CheckIsNumInWeaponList(Weapon randomWeapon)
    {
        if (IsNumInWeaponList.Contains(randomWeapon))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
