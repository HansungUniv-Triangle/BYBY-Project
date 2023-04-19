using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SynergyPanelCreate : MonoBehaviour
{
    public GameObject prefabPanel;
    public GameObject spawnPoint;
    private GameObject synergyPanel;
    private bool isCreate = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isCreate == false)
            {
                isCreate = true;
                synergyPanel = Instantiate(prefabPanel, spawnPoint.transform.position, Quaternion.identity, GameObject.Find("ItemPanel").transform);
                synergyPanel.GetComponent<SynergySelectPanel>().MakeSynergyPage();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            isCreate = false;
            Destroy(synergyPanel);
        }
    }
}
