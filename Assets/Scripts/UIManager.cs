using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public void OpenCloseMenu(GameObject menu) 
    {
        menu.SetActive(!menu.activeSelf);
    }
}
