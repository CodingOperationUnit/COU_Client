// 지정한 횟수(pierceCount)만큼 적을 때리며 관통하다가, 다 때리면 사라지는 직선 탄환
// 관통, 연쇄 처리는 LinearProjectile로 옮겼음. 이 클래스를 쓰는 프리팹(안경, 모니터) 연결을 유지하려고 남겨 둠
public sealed class PiercingProjectile : LinearProjectile
{
}
