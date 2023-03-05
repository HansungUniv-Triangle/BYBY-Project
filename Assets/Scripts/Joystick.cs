using UnityEngine;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField]
    private RectTransform lever;
    private RectTransform joystickPanel;

    [SerializeField, Range(10f, 150f)]
    private float leverRange;
    public Vector2 inputVector;
    private bool isInput;

    public Move controller;

    void Start()
    {
        joystickPanel = GetComponent<RectTransform>();
        leverRange = 10.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (isInput)
        {
            InputControlVector();
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        ControlJoystickLever(eventData);
        isInput = true;
    }

    // ������Ʈ�� Ŭ���ؼ� �巡�� �ϴ� ���߿� ������ �̺�Ʈ    // ������ Ŭ���� ������ ���·� ���콺�� ���߸� �̺�Ʈ�� ������ ����    
    public void OnDrag(PointerEventData eventData)
    {
        ControlJoystickLever(eventData);  // �߰�
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        lever.anchoredPosition = Vector2.zero;
        isInput = false;
        controller.CharacterMove(Vector2.zero);
    }

    public void ControlJoystickLever(PointerEventData eventData)
    {
        var inputDir = eventData.position - joystickPanel.anchoredPosition;
        var clampedDir = inputDir.magnitude < leverRange ? inputDir : inputDir.normalized * leverRange;

        lever.anchoredPosition = clampedDir;
        inputVector = clampedDir / leverRange;
    }

    private void InputControlVector()
    {
        controller.CharacterMove(inputVector);
    }
}
