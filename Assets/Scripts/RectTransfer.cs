using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RectTransfer : MonoBehaviour, IAnimInterface {

	#region IAnimInterface implementation

	public void Run ()
	{
		throw new System.NotImplementedException ();
	}

	public bool isDone {
		get {
			throw new System.NotImplementedException ();
		}
	}

	#endregion

    
	public RectTransform to;
	public float duration=1.0f;
	RectTransform from;
	RectTransform us;
	Rect start;
	Rect reference;
	Vector4 tovec;
	Vector4 stvec;
	// Use this for initialization
	void Start () {
		us=gameObject.GetComponent<RectTransform>();
		from=us.parent as RectTransform;
		us.SetParent(us.RootCanvasTransform());
		us.SetInternalAnchors (new Vector4 (0, 0, 1, 1));
		start=us.rect;
		reference=from.rect;
		Rect rr = to.RootCanvasRect ();
		tovec = (to.parent as RectTransform).getAnchorsFromCanvasRect (rr);
		stvec = new Vector4 (us.anchorMin.x,us.anchorMin.y,us.anchorMax.x,us.anchorMax.y);
		//us.anchorMax=new Vector2(1,1);
	//	AspectRatioFitter a;
		//a.aspectMode=AspectRatioFitter.AspectMode.FitInParent;
	}
	
	// Update is called once per frame
	float ctime=0;
	void Update () {
		ctime += Time.deltaTime;
		if (ctime <= duration) 
		{
			Vector4 vc = Vector4.Lerp (stvec, tovec, ctime / duration);
			us.anchorMin = new Vector2 (vc.x, vc.y);
			us.anchorMax = new Vector2 (vc.z, vc.w);
		} else
		{
			us.anchorMin = new Vector2 (tovec.x, tovec.y);
			us.anchorMax = new Vector2 (tovec.z, tovec.w);

		}
		//Vector2 dd=10*Time.deltaTime*new Vector2(1.0f/reference.width,1.0f/reference.height);
		//us.anchorMax+=dd;
		//us.anchorMin+=dd;
		//us.localScale=new Vector3(1.03f,1.03f,1.01f);
		//us.NormalizeScale();
	}
}
