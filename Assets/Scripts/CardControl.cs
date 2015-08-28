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
		Debug.Log(rct);
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
}
public class CardControl : MonoBehaviour {

	public UnityEngine.UI.Text text;
	CardFlip flip;
	// Use this for initialization
	SingleGame.Conditional cardData;

	void Start () {
		flip=gameObject.GetComponent<CardFlip>();


	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
