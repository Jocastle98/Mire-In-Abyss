using UnityEditor;
using UnityEngine;
using PlayerEnums;
using Unity.VisualScripting;

[CustomEditor(typeof(PlayerController))]
public class PlayerControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기본 인스펙터를 그리기
        base.OnInspectorGUI();
        
        // 타겟 컴포넌트 참조 가져오기
        PlayerController playerController = (PlayerController)target;
        PlayerStats playerStats = playerController.GetComponent<PlayerStats>();
        
        // 여백 추가
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("캐릭터 상태 디버그 정보", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        // 상태별 색상 지정
        switch (playerController.CurrentPlayerState)
        {
            case PlayerState.None:
                GUI.backgroundColor = new Color(0.0f, 0.0f, 0.0f);
                break;
            case PlayerState.Idle:
                GUI.backgroundColor = new Color(1.0f, 1.0f, 1.0f);
                break;
            case PlayerState.Move:
                GUI.backgroundColor = new Color(1.0f, 0.0f, 0.0f);
                break;
            case PlayerState.Jump:
                GUI.backgroundColor = new Color(1.0f, 0.5f, 0.0f);
                break;
            case PlayerState.Roll:
                GUI.backgroundColor = new Color(1.0f, 1.0f, 0.0f);
                break;
            case PlayerState.Attack:
                GUI.backgroundColor = new Color(0.0f, 1.0f, 0.0f);
                break;
            case PlayerState.Defend:
                GUI.backgroundColor = new Color(0.0f, 0.0f, 1.0f);
                break;
            case PlayerState.Parry:
                GUI.backgroundColor = new Color(0.0f, 0.0f, 0.5f);
                break;
            case PlayerState.Stun:
                GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f);
                break;
            case PlayerState.Freeze:
                GUI.backgroundColor = new Color(0.5f, 1.0f, 1.0f);
                break;
            case PlayerState.Dead:
                GUI.backgroundColor = new Color(0.0f, 0.0f, 0.0f);
                break;
        }
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("현재 상태", playerController.CurrentPlayerState.ToString(), EditorStyles.boldLabel);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndVertical();
        
        // 지면 접촉 상태 체크
        GUI.backgroundColor = Color.white;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("캐릭터 위치 디버그 정보", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.enabled = false;
        EditorGUILayout.Toggle("지면 접촉", playerController.bIsGrounded);
        GUI.enabled = true;
        EditorGUILayout.EndVertical();
        
        // 강제로 상태 변경 버튼
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("캐릭터 상태 강제 변경", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Idle"))
        {
            playerController.SetPlayerState(PlayerState.Idle);
        }
        if (GUILayout.Button("Move"))
        {
            playerController.SetPlayerState(PlayerState.Move);
        }
        if (GUILayout.Button("Jump"))
        {
            playerController.SetPlayerState(PlayerState.Jump);
        }
        if (GUILayout.Button("Roll"))
        {
            playerController.SetPlayerState(PlayerState.Roll);
        }
        if (GUILayout.Button("Attack"))
        {
            playerController.SetPlayerState(PlayerState.Attack);
        }
        if (GUILayout.Button("Defend"))
        {
            playerController.SetPlayerState(PlayerState.Defend);
        }
        if (GUILayout.Button("Parry"))
        {
            playerController.SetPlayerState(PlayerState.Parry);
        }
        if (GUILayout.Button("Stun"))
        {
            playerController.SetStatusEffect(StatusEffect.Stun, 3.0f);
        }
        if (GUILayout.Button("Freeze"))
        {
            playerController.SetStatusEffect(StatusEffect.Freeze, 3.0f);
        }
        if (GUILayout.Button("Dead"))
        {
            playerController.SetPlayerState(PlayerState.Dead);
        }
        if (GUILayout.Button("CooltimeReduction"))
        {
            playerStats.SetCoolDownReduction(0.5f);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    private void OnEnable()
    {
        EditorApplication.update += OnEditorUpdate;
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
    }

    private void OnEditorUpdate()
    {
        if (target != null)
        {
            Repaint();
        }
    }
}