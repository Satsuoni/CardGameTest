using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class DragTest : MonoBehaviour, IBeginDragHandler, IPointerEnterHandler, IPointerDownHandler, IDragHandler, IEndDragHandler {
	public GameObject arr;
	#region IEndDragHandler implementation

	public void OnEndDrag (PointerEventData eventData)
	{
		if(pnt!=null)
		{
		Destroy(pnt.gameObject);
		pnt=null;
		}
	}

	#endregion

	#region IDragHandler implementation

	ArrowPointer pnt;
	RectTransform cnv;
	public void OnDrag (PointerEventData eventData)
	{
		//Debug.Log("Drugging right along");
		Vector2 otherpos;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(cnv, eventData.position,eventData.pressEventCamera,out otherpos);
		(pnt.gameObject.transform as RectTransform).SetAsLastSibling();
		pnt.drawArrow(initpos,otherpos);
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

	Vector2 initpos;

	#region IBeginDragHandler implementation
	public void OnBeginDrag (PointerEventData eventData)
	{
		GameObject go=Instantiate(arr) as GameObject;
		pnt=go.GetComponent<ArrowPointer>();
		RectTransform rt=go.GetComponent<RectTransform>();
		RectTransform self=transform as RectTransform;
		rt.SetParent(self.RootCanvasTransform(),false);
		rt.SetAsLastSibling();

		cnv=rt.RootCanvasTransform();
		RectTransformUtility.ScreenPointToLocalPointInRectangle(cnv, eventData.position,eventData.pressEventCamera,out initpos);

	}
	#endregion

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
