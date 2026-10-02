using UnityEngine;

// 움직이지도 공격하지도 않는 상자. 부수면 가중치에 따라 아이템 1개를 드롭한다.
public class Box : Enemy
{
    private static readonly DropItemType[] dropTypes =
    {
        DropItemType.Gold1, DropItemType.Gold2, DropItemType.Gold3, DropItemType.Gold4,
        DropItemType.Bomb, DropItemType.Potion, DropItemType.Magnet
    };
    private static readonly int[] dropWeights = { 31, 5, 3, 1, 20, 20, 20 };   // 합계 100

    protected override void Move() { }

    protected override void Attack() { }

    // 킬 수 집계(OnDied)는 하지 않는다
    protected override void GiveReward()
    {
        GameManager.DropItem.Spawn(RollDrop(), transform.position);
    }

    private DropItemType RollDrop()
    {
        int roll = Random.Range(0, 100);
        for (int i = 0; i < dropWeights.Length; i++)
        {
            roll -= dropWeights[i];
            if (roll < 0) { return dropTypes[i]; }
        }
        return dropTypes[0];
    }
}
