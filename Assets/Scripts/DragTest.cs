using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class DragTest : MonoBehaviour, IBeginDragHandler, IPointerEnterHandler, IPointerDownHandler, IDragHandler {
	#region IDragHandler implementation

	public void OnDrag (PointerEventData eventData)
	{
		Debug.Log("Drugging right along");
	}

	#endregion

	#region IPointerDownHandler implementation

	public void OnPointerDown (PointerEventData eventData)
	{
	//	Debug.Log("Pointer down");
	}

	#endregion

	#region IPointerEnterHandler implementation

	public void OnPointerEnter (PointerEventData eventData)
	{
	//	Debug.Log("POinter enter");
	}

	#endregion

	#region IBeginDragHandler implementation
	public void OnBeginDrag (PointerEventData eventData)
	{
		Debug.Log("Dragging right along");
	}
	#endregion

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
