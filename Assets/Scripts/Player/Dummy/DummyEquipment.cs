using System;

// 임시 코드: 장비 데이터(ItemData)에 보너스 % 필드가 추가되면 PlayerInventory 연동으로 교체 후 삭제
[Serializable]
public class DummyEquipment
{
    public string name;
    public EquipSlotType slot;
    public int atk;
    public int atkBonusPercent;
    public int hp;
    public int hpBonusPercent;
    public WeaponType weaponType; // slot이 Weapon일 때만 사용
}