using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EventHandlerTest2 : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IDragHandler
{
    [SerializeField] GameObject counterPart;

    public void OnDrag(PointerEventData eventData)
    {
        eventData.dragging = true;
        if (Camera.main.ScreenToWorldPoint(Input.mousePosition).y <= 0.0f)
        {
            counterPart.GetComponent<EventHandlerTest1>().OnDrag(eventData);
            counterPart.SetActive(true);
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("From object2: " + Camera.main.ScreenToWorldPoint(Input.mousePosition));
            transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clicked");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Pointer down");
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
