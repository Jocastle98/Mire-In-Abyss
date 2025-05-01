using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonController : BattleArea
{
    private int mCellSize;
    private int mDivideLineWidth;
    private int mMinDungeonSize;
    
    public int mMinRoomCount = 6;
    public int eventRoomChance = 10;
    public SODungeonList dungeonListSO;

    private GameObject mPlayer;
    private int mLevelDesign;

    private DungeonGenerator mDungeonGenerator;

    public override void BattleAreaInit(GameObject player, int levelDesign)
    {
        mPlayer = player;
        mLevelDesign = levelDesign;
    }

    public override void BattleAreaClear()
    {
        OnClearBattleArea.Invoke();
    }

    //컨셉과 레벨디자인에 따라 던전최대크기(벡터)를 정하도록하고 해당 함수는 삭제
    public void BattleDungeonInit(GameObject player, int levelDesign, int minDungeonSize, int divideLineWidth,
        int minRoomCount)
    {
        mPlayer = player;
        mLevelDesign = levelDesign;
        mMinDungeonSize = minDungeonSize;
        mDivideLineWidth = divideLineWidth;
        mMinRoomCount = minRoomCount;

        mDungeonGenerator =
            new DungeonGenerator(mCellSize, mMinDungeonSize, mDivideLineWidth, mMinRoomCount, mLevelDesign,
                eventRoomChance, dungeonListSO, mPlayer);

        BattleAreaInit(player, levelDesign);
    }
}
