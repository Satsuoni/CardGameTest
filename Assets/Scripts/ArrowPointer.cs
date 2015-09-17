using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
[RequireComponent (typeof (RectTransform))]
public class ArrowPointer : MonoBehaviour {

	public Texture2D tail;
	public Texture2D body;
	public Texture2D head;
	//public Vector2 headPoint;
	//public Vector2 tailPoint;
	public float height=10f;
	public float bodylengaps=0.7f;
	// Use this for initialization
	RectTransform trans;
	RectTransform canv;
//	Rect canvrect;
	float taillen;
	float headlen;
	float bodylen;
	void Init()
	{
		trans= gameObject.GetComponent<RectTransform>();
		if(trans==null)
		{
			Destroy(this);return;
		}
		canv=trans.RootCanvasTransform();
		//canvrect=canv.rect;
		taillen=height*tail.width/tail.height;
		headlen=height*head.width/head.height;
		bodylen=height*body.width/body.height;
	}
	void Start () {

		Init ();
		//drawArrow(new Vector2(-300,0),new Vector2(-200,200));
	}
	RectTransform headItem;
	RectTransform  tailItem;
	List<RectTransform> bodyItems=new List<RectTransform>(); 
	int bid=0;
	public void drawArrow(Vector2 pt0,Vector2 pt1) //in canvas coords?
	{
		if(trans==null) Init ();
		float distance= Vector2.Distance(pt0,pt1);
		if(distance<0.01f) return;
		float bodydist=distance-taillen-headlen;
		Rect rct=new Rect((pt0+pt1)/2-(new Vector2(distance,height)/2),new Vector2(distance,height));
		Vector4 vs=canv.getAnchorsFromCanvasRect(rct);
//		Debug.Log(rct);
		trans.assignRectAnchors(vs);
		trans.pivot=new Vector2(0.5f,0.5f);
		//Debug.Log(trans.rect);
		float tailoff=taillen/distance;
		float headoff=headlen/distance;
		float bodyoff=bodylen/distance;
	
		if(headItem==null)
		{
			GameObject go=new GameObject("head");
			go.AddComponent(typeof(RawImage));
			RawImage img=go.GetComponent<UnityEngine.UI.RawImage>();
			img.texture=head;
			headItem=go.GetComponent<RectTransform>();
			headItem.SetParent(trans,false);
		}

		if(tailItem==null)
		{
			GameObject go=new GameObject("tail");
			go.AddComponent(typeof(RawImage));
			RawImage img=go.GetComponent<UnityEngine.UI.RawImage>();
			img.texture=tail;
			tailItem=go.GetComponent<RectTransform>();
			tailItem.SetParent(trans,false);
		}
		headItem.assignRectAnchors(new Vector4(1.0f-headoff,0,1,1f));
		tailItem.assignRectAnchors(new Vector4(0,0,tailoff,1f));
		float ang=Mathf.Atan2(pt1.y-pt0.y,pt1.x-pt0.x);
		Quaternion rot=Quaternion.identity;
		rot.eulerAngles=new Vector3(0,0,ang*180.0f/Mathf.PI);
		trans.rotation=rot;
		if(bodydist<bodylen) return;
		int maxnecessarybodies=Mathf.CeilToInt((bodydist-(bodylengaps*bodylen))/(bodylen+bodylengaps*bodylen));
		while(maxnecessarybodies>bodyItems.Count)
		{
			RectTransform bi;
			GameObject go=new GameObject("body"+bid);
			go.AddComponent(typeof(RawImage));
			RawImage img=go.GetComponent<UnityEngine.UI.RawImage>();
			img.texture=body;
			bi=go.GetComponent<RectTransform>();
			bi.SetParent(trans,false);
			bid++;
			bodyItems.Add(bi);
		}
		foreach(RectTransform r in bodyItems)
			r.gameObject.SetActive(false);
		float gapsize=(bodydist-(float)maxnecessarybodies*bodylen)/((float)maxnecessarybodies+1.0f);
		float gapoff=gapsize/distance;
		float stp=gapoff+tailoff;
		for(int i=0;i<maxnecessarybodies;i++)
		{
			RectTransform bi=bodyItems[i];
			bi.assignRectAnchors(new Vector4(stp,0,stp+bodyoff,1f));
			bi.gameObject.SetActive(true);
			stp=stp+bodyoff+gapoff;
		}

	}
	// Update is called once per frame
	void Update () {
	
	}
}
