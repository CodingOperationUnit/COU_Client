using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 플레이어 최종 스탯 계산기 (플레이어 오브젝트 없이도 사용 가능)
// 전투(PlayerStats)와 장비창이 같은 계산을 쓰도록 한곳에 둔다
// 최종 = (기본 + 진화 + 장비 합계) × (100 + 보너스%) / 100, 버림
public static class PlayerStatCalculator
{
    public struct Result
    {
        public int FinalAttack;
        public int FinalHp;

        public int BaseAttack;
        public int BaseHp;
        public int EvolutionAttack;
        public int EvolutionHp;
        public int EquipmentAttack;
        public int EquipmentHp;

        public bool HasEvolutionData;
    }


    public static Result Calculate(PlayerBaseStatData baseStat, EvolutionData evolution,
                                   PlayerStatData playerStat, IReadOnlyCollection<OwnedItem> equipped)
    {
        var result = new Result
        {
            BaseAttack = baseStat.playerBaseAttack,
            BaseHp = baseStat.playerBaseHp,
            EvolutionAttack = EvolutionRules.GetBonus(evolution, playerStat, PlayerStatType.Attack),
            EvolutionHp = EvolutionRules.GetBonus(evolution, playerStat, PlayerStatType.Hp),
            EquipmentAttack = equipped.Sum(item => item.ScaledAttack),
            EquipmentHp = equipped.Sum(item => item.ScaledHp),
            HasEvolutionData = evolution != null
        };

        result.FinalAttack = ApplyBonus(result.BaseAttack + result.EvolutionAttack + result.EquipmentAttack, 0);
        result.FinalHp = ApplyBonus(result.BaseHp + result.EvolutionHp + result.EquipmentHp, 0);

        return result;
    }

    public static Result CalculateCurrent()
    {
        var data = GameManager.PlayerData.currentData;
        return Calculate(GameManager.JsonData.PlayerBaseStatData, EvolutionRules.Data,
                         data.playerStat, GetEquipped(data));
    }

    public static List<OwnedItem> GetEquipped(PlayerSaveData data)
    {
        var result = new List<OwnedItem>();

        if (PlayerInventory.Instance != null)
        {
            foreach (EquipSlotType slot in Enum.GetValues(typeof(EquipSlotType)))
            {
                var item = PlayerInventory.Instance.GetEquipped(slot);
                if (item != null)
                    result.Add(item);
            }

            return result;
        }

        InventoryRestorer.Restore(data.inventoryList, out var equipped);
        result.AddRange(equipped.Values);
        return result;
    }

    private static int ApplyBonus(int sum, int bonusPercent)
        => sum * (100 + bonusPercent) / 100;
}
