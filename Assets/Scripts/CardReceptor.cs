using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CardReceptor : MonoBehaviour, IDropHandler {
	SingleGame.Conditional data;

	#region IDropHandler implementation
	void IDropHandler.OnDrop (PointerEventData eventData)
	{
		if(eventData.pointerDrag!=null)
		{
			Debug.Log(eventData.pointerDrag);
			CardControl card=eventData.pointerDrag.GetComponent<CardControl>();
			if(card!=null)
			{
				Debug.Log(card);
				RectTransform cardrt=card.gameObject.GetComponent<RectTransform>();
				cardrt.SetParent(gameObject.transform,false);
				cardrt.anchorMin=Vector2.zero;
				cardrt.anchorMax=Vector2.one;
				cardrt.offsetMax=Vector2.zero;
				cardrt.offsetMin=Vector2.zero;
				cardrt.localScale=new Vector3(1,1,1);
				card.registerDropSuccess(true);
			}
		}
	}
	#endregion


	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
