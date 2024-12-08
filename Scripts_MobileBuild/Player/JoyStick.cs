using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JoyStick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    private Image bgImage;
    private Image handleImage;
    private Vector2 inputVector;

    private void Start()
    {
        bgImage = GetComponent<Image>();
        handleImage = transform.GetChild(4).GetComponent<Image>(); // 첫 번째 자식 이미지가 조이스틱 핸들
    }

    /// <summary>
    /// 온드래그함수
    /// </summary>
    /// <param name="ped"></param>
    public virtual void OnDrag(PointerEventData ped)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(bgImage.rectTransform, ped.position, ped.pressEventCamera, out pos))
        {
            pos.x = (pos.x / bgImage.rectTransform.sizeDelta.x);
            pos.y = (pos.y / bgImage.rectTransform.sizeDelta.y);

            // inputVector 값 조정 (-1에서 1 사이로 제한)
            inputVector = new Vector2(pos.x * 2, pos.y * 2);
            inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

            // 조이스틱 핸들 위치 설정
            handleImage.rectTransform.anchoredPosition = new Vector2(inputVector.x * (bgImage.rectTransform.sizeDelta.x / 2), inputVector.y * (bgImage.rectTransform.sizeDelta.y / 2));
        }
    }

    public virtual void OnPointerDown(PointerEventData ped)
    {
        OnDrag(ped);
    }

    public virtual void OnPointerUp(PointerEventData ped)
    {
        inputVector = Vector2.zero;
        handleImage.rectTransform.anchoredPosition = Vector2.zero; // 중앙으로 복귀
    }

    public bool isMoving()
    {
        return handleImage.rectTransform.anchoredPosition != Vector2.zero; // 중앙으로 복귀

    }

    public float Horizontal()
    {
        return inputVector.x;
    }

    public float Vertical()
    {
        return inputVector.y;
    }
}
