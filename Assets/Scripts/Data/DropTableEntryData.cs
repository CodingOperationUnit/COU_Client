using System;

// 드롭 테이블 한 행. DropTable.json의 "dropItemType" 문자열을 enum으로 바꿔서 쓴다.
[Serializable]
public class DropTableEntryData
{
    public int dropTableId;
    public int group;             // 같은 테이블 안에서 그룹마다 한 번씩 추첨
    public string dropItemType;   // DropItemType 이름 또는 "None"
    public int weight;            // 그룹 안의 가중치
    public int count;             // 뽑혔을 때 생성할 개수

    [NonSerialized] private DropItemType parsedType;
    public DropItemType Type => parsedType;
    public bool IsNone => dropItemType == "None";

    // JsonDataManager에서 JSON을 읽은 직후 한 번 호출. 알 수 없는 종류면 false
    public bool OnLoaded() => IsNone || Enum.TryParse(dropItemType, out parsedType);
}
