using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRotateBasedOnCamera : MonoBehaviour
{
    public List<RectTransform> uiList;

    // Update is called once per frame
    void Update()
    {
        foreach(var ui in uiList)
            ui.rotation = Quaternion.Euler(0, 0, -transform.eulerAngles.z);
    }
}
