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

        if(DrawDefaultInspector())  // editor�� ���� ����Ǿ �ٽ� �׷��� ��
        {
            if(worldGenerator.AutoUpdate)
            {
                worldGenerator.GeneratorMap();
            }
        }

        if (GUILayout.Button ("Generate"))
        {
            worldGenerator.GeneratorMap();
        }
    }
}
