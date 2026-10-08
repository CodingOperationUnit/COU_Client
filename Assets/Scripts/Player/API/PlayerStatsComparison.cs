using System.Text;
using UnityEngine;

// 서버 계산과 클라 계산 비교 로그
// 서버에 장비 레벨/등급 배율이 생기기 전까지는 비교/검증 용도로만 사용
public static class PlayerStatsComparison
{
    public static void Log(FinalStatsResponse server, PlayerStats client)
    {
        var sb = new StringBuilder("[PlayerStatsComparison] 서버 vs 클라이언트\n");

        Line(sb, "최종 공격력", server.finalAttack , client.FinalAtk);
        Line(sb, "최종 체력", server.finalHp , client.FinalHp );
        Line(sb, "치명타 피해", server.criticalDamage , client.CriticalDamage );
        Line(sb, "치명타 확률", server.criticalChance , client.CriticalChance );
        Line(sb, "스킬 피해", server.skillDamage , client.SkillDamage );
        Line(sb, "이동 속도", server.moveSpeed , client.Speed );
        Line(sb, "이동 속도 상한", server.maxMoveSpeed , client.MaxSpeed );
        Line(sb, "루팅 범위", server.lootRadius , client.LootRadius );

        if (server.breakdown != null)
        {
            var b = server.breakdown;

            sb.AppendLine("서버 세부 : 기본 " + Parts(b.baseStats) + " / 장비 " + Parts(b.equipment) + " / 진화 " + Parts(b.evolution));
        }

        sb.AppendLine("서버 전용(클라이언트 미구현): 방어력 " + server.finalDefense + ", 포션 회복량 " + server.finalPotionRecovery);
        sb.Append("클라이언트 장비 출처: " + client.EquipmentSource);

        Debug.Log(sb.ToString());
    }


    public static void Line(StringBuilder sb, string label, float server, float client)
    {
        bool same = Mathf.Approximately(server, client);
        sb.AppendLine((same ? "  = " : "  ≠ ") + label + ": 서버 " + server + " / 클라이언트 " + client);
    }

    private static string Parts(StatParts p)
        => p == null ? "(없음)" : "공 " + p.attack + ", 체 " + p.hp + ", 방 " + p.defense + ", 포션 " + p.potionRecovery;
}
