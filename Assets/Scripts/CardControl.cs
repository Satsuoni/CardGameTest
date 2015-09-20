using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public static class rtExt
{
	public static RectTransform RootCanvasTransform(this RectTransform go)
	{
		Transform cnt=go as Transform;
		while(cnt!=null)
		{
			Canvas tst=cnt.gameObject.GetComponent<Canvas>();
			if(tst!=null) return cnt as RectTransform;
			cnt=cnt.parent as Transform;
		}
		return null;
	}
	public static Rect RootCanvasRect(this RectTransform go) //doesn't work on rotations
	{
		//Transform cnt=go as Transform;
		List<RectTransform> tree=new List<RectTransform>();
		while(go!=null)
		{
			tree.Insert(0,go);
			Canvas tst=go.gameObject.GetComponent<Canvas>();
			if(tst!=null) break;
			go=go.parent as RectTransform;
			//RectTransformUtility.
		}
		Rect rct=tree[0].rect;
//		Debug.Log(rct);
		for(int i=1;i<tree.Count;i++)
		{
			RectTransform ct=tree[i];
			Vector2 urAnch=new Vector2(ct.anchorMax.x*rct.size.x,ct.anchorMax.y*rct.size.y);
			Vector2 blAnch=new Vector2(ct.anchorMin.x*rct.size.x,ct.anchorMin.y*rct.size.y);
			urAnch+=ct.offsetMax;
			blAnch+=ct.offsetMin;
			Vector2 pivot=new Vector2(blAnch.x*(1-ct.pivot.x)+urAnch.x*ct.pivot.x,blAnch.y*(1-ct.pivot.y)+urAnch.y*ct.pivot.y);
			//Debug.Log (blAnch-pivot);
			rct=new Rect(rct.position+pivot+Vector2.Scale(blAnch-pivot,ct.localScale),Vector2.Scale(urAnch-blAnch,ct.localScale));
		}
	
		return rct;
	}
	public static void NormalizeScale(this RectTransform go) //doesn't work on rotations, i guess beware aspect fitters
	{
		if(Vector3.Dot(go.localScale-new Vector3(1,1,1),go.localScale-new Vector3(1,1,1))<0.001f) return;
		Rect idrect=go.rect;
	
		Vector2 pivot=new Vector2(idrect.width*(go.pivot.x)+idrect.position.x,idrect.height*(go.pivot.y)+idrect.position.y);
		Vector2 offsdf=Vector2.Scale(idrect.position-pivot,go.localScale-Vector3.one);
		go.offsetMin=go.offsetMin+offsdf;
		go.offsetMax=go.offsetMax-offsdf;
		go.localScale=Vector3.one;
	}
	public static Vector4 InternalAnchors(this RectTransform go)
	{
		Rect rct=go.rect;
		Vector4 ret=new Vector4(-go.offsetMin.x/rct.width,-go.offsetMin.y/rct.height,(rct.width-go.offsetMax.x)/rct.width,(rct.height-go.offsetMax.y)/rct.height);
		return ret;
	}
	public static void SetInternalAnchors(this RectTransform go,Vector4 intern) //works strangely with layouts
	{
		Rect rct=go.rect;
		Vector2 newOffsetMin=new Vector2(-intern.x*rct.width,-intern.y*rct.height);
		Vector2 newOffsetMax=new Vector2(rct.width-intern.z*rct.width,rct.height-intern.w*rct.height);

		RectTransform par=go.parent as RectTransform;
		Rect ext=par.rect;
		Vector2 urAnch=new Vector2(go.anchorMax.x*ext.size.x,go.anchorMax.y*ext.size.y);
		Vector2 blAnch=new Vector2(go.anchorMin.x*ext.size.x,go.anchorMin.y*ext.size.y);
		urAnch+=go.offsetMax-newOffsetMax;
		blAnch+=go.offsetMin-newOffsetMin;
		go.anchorMax=new Vector2(urAnch.x/ext.width,urAnch.y/ext.height);
		go.anchorMin=new Vector2(blAnch.x/ext.width,blAnch.y/ext.height);
		go.offsetMax=newOffsetMax;
		go.offsetMin=newOffsetMin;

    }
	public static Vector4 getAnchorsFromCanvasRect(this RectTransform go,Rect rct)
	{
		Rect crct = go.RootCanvasRect ();
		Vector2 dmin = rct.min - crct.min;
		Vector2 dmax = rct.max - crct.min;
		return new Vector4(dmin.x/crct.width,dmin.y/crct.height,dmax.x/crct.width,dmax.y/crct.height);
	}
	public static void assignRectAnchors(this RectTransform go,Vector4 rct)
	{
		go.anchorMin=new Vector2(rct.x,rct.y);
		go.anchorMax=new Vector2(rct.z,rct.w);
		go.offsetMax=Vector2.zero;
		go.offsetMin=Vector2.zero;
	}
	public static Vector2 getPivotInCanvas(this RectTransform go)
	{
		Rect crct = go.RootCanvasRect ();
	
		Vector2 ret=new Vector2(crct.max.x*go.pivot.x+(1-go.pivot.x)*crct.min.x,crct.max.y*go.pivot.y+(1-go.pivot.y)*crct.min.y);
		return ret;
	}

}
public class CardControl : ConditionalUIEntity, UnityEngine.EventSystems.IBeginDragHandler,  UnityEngine.EventSystems.IDragHandler,UnityEngine.EventSystems.IEndDragHandler, UnityEngine.ICanvasRaycastFilter
{
	public string playerID{get;set;}
	public int slotPos {get;set;}

	public UnityEngine.UI.RawImage cardImg;
	public UnityEngine.UI.RawImage glow;
	public UnityEngine.UI.Text text;
	SingleGame.Conditional game;
	SingleGame.Conditional player1;
	RectTransform rtransform;
	RectTransform canv;

	const string sel="SELECTED";
	const string tar="TARGETED";
	#region ICanvasRaycastFilter implementation


	public bool IsRaycastLocationValid (Vector2 sp, Camera eventCamera)
	{
		return !isDragging;
	}


	#endregion

	#region IEndDragHandler implementation
	bool didFinishDragging=false;
	bool dropSuccess=false;
	public void registerDropSuccess(bool ds)
	{
		dropSuccess=ds;
	}
	public void OnEndDrag (UnityEngine.EventSystems.PointerEventData eventData)
	{

		/*if(player1[sel]==cardData&&player1[tar]==null)
    {
      cardData.setTag("DESELECT");
			Debug.Log("DESELECT");
    }*/
		if(cardData.hasTag("main_ACTIVE"))
		{
			isDragging=false;
		}
		if(wasDragged)
		{
		//rtransform.localScale=new Vector3(1f,1f,1f);
		//rtransform.anchoredPosition=oPos;
			didFinishDragging=true;
		}
	}

	#endregion




	#region IDragHandler implementation

	public void OnDrag (UnityEngine.EventSystems.PointerEventData eventData)
	{
		//Debug.Log(string.Format("Dragging {0}",gameObject));

		if(cardData.hasTag("main_ACTIVE"))
		{
			//Debug.Log(string.Format("Pos {0}", eventData.position));
			wasDragged=true;
			Vector2 lp;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(canv, eventData.position,eventData.pressEventCamera,out lp);
			rtransform.anchoredPosition=oPos+ lp-ePos;
			rtransform.SetAsLastSibling();
		}
	}

	#endregion

	#region IBeginDragHandler implementation
	Vector4 originPos;
	Vector2 oPos;
	Vector2 ePos;
	bool wasDragged=false;
	bool isDragging=false;
	public Vector2 getOPos()
	{
		return oPos;
	}
	public void OnBeginDrag (UnityEngine.EventSystems.PointerEventData eventData)
	{
		//Debug.Log(string.Format("Drag {0}",gameObject));
		if(cardData.hasTag("main_ACTIVE"))
		{
			isDragging=true;
			///allow dragging
			//eventData.pos
			rtransform.localScale=new Vector3(1.3f,1.3f,1.3f);
			oPos=rtransform.anchoredPosition;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(canv, eventData.position,eventData.pressEventCamera,out ePos);
			player1[sel]=cardData;
		}
	}
	#endregion


//	CardFlip flip;
	// Use this for initialization
	//public SingleGame.Conditional cardData;

	public override void Start () {

		rtransform=gameObject.GetComponent<RectTransform>();
				canv=rtransform.RootCanvasTransform();
		text.text=cardData[SingleGame._cardText] as string;
		string imgname=cardData["_cardImage"] as string;
		if(imgname !=null)
		{
			Texture2D txt=Resources.Load(imgname) as Texture2D;
			if(txt!=null)
				cardImg.texture=txt;
		}
		game=cardData[SingleGame._rootl] as SingleGame.Conditional;
		player1=game["Player1"] as SingleGame.Conditional;
	}
	public override  void Highlight(bool light, string type=""){
		glow.gameObject.SetActive(light);
	}
	// Update is called once per frame
	public override void Update () {
		base.Update();
		//glow.gameObject.SetActive(cardData.hasTag("main_ACTIVE"));
		if(player1[sel]==cardData&&player1[tar]==null&&!isDragging)
		{
			cardData.setTag("DESELECT");
//			Debug.Log("DESELECT");
		}
		if(player1[sel]==cardData&&player1[tar]!=null&&!isDragging)
		{
			cardData.removeTag(player1["activeTag"] as string);
		}
		if(didFinishDragging)
		{
			if(!dropSuccess)
			{
			rtransform.localScale=new Vector3(1f,1f,1f);
			rtransform.anchoredPosition=oPos;
			}
			didFinishDragging=false;
		}

	}
}
