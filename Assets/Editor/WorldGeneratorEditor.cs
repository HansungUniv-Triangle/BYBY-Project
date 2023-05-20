using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (WorldManager)), CanEditMultipleObjects]
public class WorldGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        WorldManager worldGenerator = (WorldManager)target;

        if(DrawDefaultInspector())  // editor의 값이 변경되어서 다시 그려질 때
        {
            if(worldGenerator.AutoUpdate)
            {
                worldGenerator.GeneratorMap(1);
            }
        }

        if (GUILayout.Button ("Generate"))
        {
            worldGenerator.GeneratorMap(1);
        }
    }
}
