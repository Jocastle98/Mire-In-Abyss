using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 내 퀘스트의 정보를 저장하는 데이터 클래스
/// </summary>
[System.Serializable]
public class Quest
{
    public string Id;                   //퀘스트 고유 식별자 Q001~Q015
    public string Title;                //퀘스트 제목
    public string RequestInformation;   //퀘스트 요청 정보 (~해주세요)
    public string Goal;                 //퀘스트 목표 설명 (~명 처치하세요)
    public string Objective;            //퀘스트 목표 유형 (처치, 수집 등)
    public int TargetAmount;            //퀘스트 목표 달성에 필요한 수량 
    public int RewardSoul;              //퀘스트 완료시 영혼석 보상 개수
    public string Description;          //퀘스트 수락 후 플레이어 고정 UI에 표시되는 퀘스트내용

    [NonSerialized] public int CurrentAmount;   //현재 달성한 수량
    [NonSerialized] public bool isCompleted;    //퀘스트 완료 여부

    public float Progress => (float)CurrentAmount / TargetAmount;   //퀘스트 진행 상황

    /// <summary>
    /// 퀘스트 정보를 문자열로 반환
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{Title} ({Id}): {Description}";
    }
}
