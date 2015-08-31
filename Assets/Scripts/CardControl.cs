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
}
public class CardControl : MonoBehaviour {

	public UnityEngine.UI.Text text;
	CardFlip flip;
	// Use this for initialization
	public SingleGame.Conditional cardData;

	void Start () {
		flip=gameObject.GetComponent<CardFlip>();
		text.text=cardData[SingleGame._cardText] as string;

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
