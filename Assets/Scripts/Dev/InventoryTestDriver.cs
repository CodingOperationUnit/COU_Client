using UnityEngine;

public class InventoryTestDriver : MonoBehaviour
{
    private void Start()
    {
        // 이제 인벤토리(레벨/장착 상태 포함)가 로컬에 저장되므로,
        // 이미 저장된 데이터가 있으면(= 한 번 이상 플레이했으면) 테스트 아이템을 또 추가하지 않는다.
        if (PlayerInventory.Instance.Items.Count > 0) return;

        // 슬롯 종류(무기/방어구/벨트/장갑/목걸이/신발) 전부, 등급(일반/우수/레어)도 골고루 넣어서
        // 장착 테스트를 전 종류 다 해볼 수 있게 구성.
        PlayerInventory.Instance.AddItem(1001); // 가죽 갑옷 (방어구, 일반)
        PlayerInventory.Instance.AddItem(1002); // 사슬 갑옷 (방어구, 우수)
        PlayerInventory.Instance.AddItem(1003); // 강화 판금 갑옷 (방어구, 레어)

        PlayerInventory.Instance.AddItem(2001); // 가죽 벨트 (벨트, 일반)
        PlayerInventory.Instance.AddItem(2004); // 군용 벨트 (벨트, 일반)
        PlayerInventory.Instance.AddItem(2003); // 미스릴 벨트 (벨트, 레어)

        PlayerInventory.Instance.AddItem(3001); // 가죽 장갑 (장갑, 일반)
        PlayerInventory.Instance.AddItem(3003); // 발톱 장갑 (장갑, 레어)

        PlayerInventory.Instance.AddItem(4002); // 부적 목걸이 (목걸이, 우수)

        PlayerInventory.Instance.AddItem(5001); // 가죽 신발 (신발, 일반)
        PlayerInventory.Instance.AddItem(5003); // 질풍 신발 (신발, 레어)

        PlayerInventory.Instance.AddItem(6001); // 낡은 검 (무기, 일반)
        PlayerInventory.Instance.AddItem(6002); // 연발 권총 (무기, 우수)
        PlayerInventory.Instance.AddItem(6003); // 그림자 쿠나이 (무기, 레어)
    }
}
