using System.Collections;
using System.Collections.Generic;
using ItemEnums;
using UnityEngine;

/// <summary>
/// 게임 내 모든 아이템의 정보를 저장하는 기본 데이터 클래스
/// </summary>
[System.Serializable]
public class Item
{
    public int ID;                                          //아이템의 고유 ID
    public string ItemName;                                 //아이템의 이름
    public string Tier;                                     //아이템의 등급
    public string Description;                              //아이템의 상세 설명
    public string EffectType;                               //아이템의 효과 타입(체력, 이동속도, 공격력 등)
    public float Value;                                     //아이템 효과 수치 값
    public string ValueType;                                //효과 적용 방식 (add, multiply 등)
    public List<string> DropSources = new List<string>();   //아이템 획득 경로 (몬스터, 상점)
    public float DropRateMonster;                           //몬스터에서 드롭될 확률
    public float DropRateShop;                              //상점에서 등장할 확률

    /// <summary>
    /// 아이템 정보를 문자열로 반환
    /// </summary>
    /// <returns>아이템 이름과 등급, 설명이 포함된 문자열</returns>
    public override string ToString()
    {
        return $"{ItemName}({Tier}):{Description}";
    }
}
