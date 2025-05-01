using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;

[CustomEditor(typeof(BattleAreaManager))]
public class BattleAreaManagerEditor : Editor
{
    bool IsInit = false;
    bool IsCreate = false;
    int clearCount = 0;
    public override void OnInspectorGUI()
    {
        
        
    }
}
