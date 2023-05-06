using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SynergyPanelCreate : MonoBehaviour
{
    public GameObject prefabPanel;
    private GameObject synergyPanel;
    private RectTransform _rectTransform;

    // Start is called before the first frame update
    void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (synergyPanel == null)
            {
                synergyPanel = Instantiate(prefabPanel, transform);
                synergyPanel.GetComponent<SynergySelectPanel>().MakeSynergyPage();
            }
            else
            {
                synergyPanel.SetActive(true);
                synergyPanel.GetComponent<SynergySelectPanel>().MakeSynergyPage();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            synergyPanel.SetActive(false);
        }
    }
}
