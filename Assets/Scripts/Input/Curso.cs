using UnityEngine;

public class Curso : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;

    private void LateUpdate()
    {
        rectTransform.position = Vector2.Lerp(rectTransform.position, Input.mousePosition, Time.deltaTime * 10f);
    }
}
