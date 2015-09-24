using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class TickerReceptor : ConditionalUIEntity, IDropHandler {
	SingleGame.Conditional _mpl=null;
	SingleGame.Conditional player1
	{
		get{ if(_mpl==null) _mpl=cardData["|-.Player1"] as SingleGame.Conditional; return _mpl; }
	}

	#region IDropHandler implementation
	void IDropHandler.OnDrop (PointerEventData eventData)
	{
		if(!cardData.hasTag(player1["targetTag"] as string)) return;
		if(eventData.pointerDrag!=null)
		{
			

				CardReceptor rc=eventData.pointerDrag.GetComponent<CardReceptor>();
				
				if(rc!=null&&cardData.hasTag(player1["aimTag"] as string))
				{ // this is ability resolution
					player1["AIMED"]=cardData;
					Debug.Log(rc);
					rc.DropOccurred();
					return;
				}
				
				if(rc!=null&&cardData.hasTag(player1["targetTag"] as string))
				{
					player1["TARGETED"]=cardData;
					Debug.Log(rc);
					rc.DropOccurred();
					return;
				}
				

		}
		
	}
	#endregion

	// Use this for initialization
	Image img;
	public override  void Start () {
		 img=gameObject.GetComponent<Image>();
	}
	
	// Update is called once per frame
	public override void Update () {
		base.Update();

	}
	public override void Highlight(bool light, string type="")
	{
		if(light)
		{
			img.color=new Color(0.5f,1.0f,0.5f);
		}
		else
		{
			string dst=cardData["distance"] as string;
			string cdst=cardData["|-._distance"] as string;
			if(dst!=null&&cdst!=null&&dst==cdst)
				img.color=new Color(0.5f,1.0f,1.0f);
				else
			img.color=new Color(1f,1.0f,1.0f);
		}

	}
}
