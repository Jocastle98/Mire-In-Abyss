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
        base.OnInspectorGUI();
        
        BattleAreaManager battleAreaManager = (BattleAreaManager)target;
        
        EditorGUILayout.Space();

        
        if (GUILayout.Button("Create Battle Area Manager Init"))
        {
            if (!IsInit)
            {
                battleAreaManager.BattleAreaManagerInit(new GameObject("Player"), 1);
                IsInit = true;
            }
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Create Battle Area Create"))
        {
            if (IsInit && !IsCreate)
            {
                battleAreaManager.BattleAreaCreate();
                IsCreate = true;
                Debug.Log("Create Button Clicked");
            }
        }
        
        EditorGUILayout.Space();

        if (GUILayout.Button("Create Battle Area Clear"))
        {
            if (IsInit && IsCreate)
            {
                battleAreaManager.TestBattleAreaClear();
                clearCount++;
                IsCreate = false;
                
                Debug.Log("Clear Button Clicked");
            }
            
            if(clearCount == battleAreaManager.battleAreaClearLimit) IsInit = false;
        }
        
    }
}
