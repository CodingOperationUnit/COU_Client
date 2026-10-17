using UnityEngine;
using UnityEngine.UI;

// 로비 배경 패턴: Repeat 텍스처를 깐 RawImage의 uvRect를 밀어 무늬가 대각선으로 끝없이 흐르게 한다
[RequireComponent(typeof(RawImage))]
public class ScrollingPatternBackground : MonoBehaviour
{
    [SerializeField] private float tileSize = 400f;                     // 타일 한 장의 캔버스 크기
    [SerializeField] private Vector2 speed = new Vector2(0.05f, 0.05f); // 초당 이동량 (타일 비율, +면 오른쪽 위)

    private RawImage image;
    private RectTransform rect;

    private void Awake()
    {
        image = GetComponent<RawImage>();
        rect = (RectTransform)transform;
    }

    private void Update()
    {
        Rect uv = image.uvRect;
        uv.size = rect.rect.size / tileSize;
        uv.position = new Vector2(
            Mathf.Repeat(uv.x - speed.x * Time.deltaTime, 1f),
            Mathf.Repeat(uv.y - speed.y * Time.deltaTime, 1f));
        image.uvRect = uv;
    }
}
