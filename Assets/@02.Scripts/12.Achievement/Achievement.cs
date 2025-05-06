using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Achievement : MonoBehaviour
{
    public string Id;                   //업적 고유 ID
    public string Title;                //업적 제목
    public string Info;                 //해금 전 안내 문구
    public string Description;          //조건 달성 시 안내 팝업 문구
    public string IllustrationComment;  //해금 후 업적 도감 문구

    [System.NonSerialized] public bool isUnlocked;
    [System.NonSerialized] public float Progress;

    public override string ToString()
    {
        return $"{Title} ({Id}) : {(isUnlocked ? "해금됨" : "미해금")}";
    }
}
