using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RectTransfer : MonoBehaviour, IAnimInterface {

	#region IAnimInterface implementation
	bool _isDone=true;
	public void Run ()
	{
		if(_isDone)
		{
			_isDone=false;
			StartCoroutine(running());
		}
		else
			Debug.Log("Still running");
	}

	public bool isDone {
		get{return _isDone;}
	}
	public static IAnimInterface Apply(GameObject obj,RectTransform to,float dur)
	{
		RectTransfer flp=obj.AddComponent(typeof(RectTransfer)) as RectTransfer;
		flp.to=to;
		flp.duration=dur;
		flp.Init();
		return flp;
	}
	#endregion

	IEnumerator running()
	{
		float ctime=0;
		_isDone=false;

		while  (ctime <= duration) 
		{
			Vector4 vc = Vector4.Lerp (stvec, tovec, ctime / duration);
			us.anchorMin = new Vector2 (vc.x, vc.y);
			us.anchorMax = new Vector2 (vc.z, vc.w);
			yield return null;
			ctime += Time.deltaTime;
		} 

			us.anchorMin = new Vector2 (tovec.x, tovec.y);
			us.anchorMax = new Vector2 (tovec.z, tovec.w);
			
		_isDone=true;
	}
    
	public RectTransform to;
	public float duration=1.0f;
	RectTransform from;
	RectTransform us;
	Rect start;
	Rect reference;
	Vector4 tovec;
	Vector4 stvec;
	// Use this for initialization
	void Init()
	{
		Debug.Log("apply");
		us=gameObject.GetComponent<RectTransform>();
		from=us.parent as RectTransform;
		us.SetParent(us.RootCanvasTransform());
		us.SetInternalAnchors (new Vector4 (0, 0, 1, 1));
		start=us.rect;
		reference=from.rect;
		Rect rr = to.RootCanvasRect ();
		Debug.Log(rr);
		tovec = (us.parent as RectTransform).getAnchorsFromCanvasRect (rr);
		Debug.Log(tovec);
		stvec = new Vector4 (us.anchorMin.x,us.anchorMin.y,us.anchorMax.x,us.anchorMax.y);
	}
	void Start () {

		//us.anchorMax=new Vector2(1,1);
	//	AspectRatioFitter a;
		//a.aspectMode=AspectRatioFitter.AspectMode.FitInParent;
	}
	
	// Update is called once per frame

	void Update () {

		//Vector2 dd=10*Time.deltaTime*new Vector2(1.0f/reference.width,1.0f/reference.height);
		//us.anchorMax+=dd;
		//us.anchorMin+=dd;
		//us.localScale=new Vector3(1.03f,1.03f,1.01f);
		//us.NormalizeScale();
	}
}
