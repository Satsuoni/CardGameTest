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
	public float duration;
	RectTransform from;
	RectTransform us;
	Rect start;
	Rect reference;
	// Use this for initialization
	void Start () {
		us=gameObject.GetComponent<RectTransform>();
		from=us.parent as RectTransform;
		us.SetParent(us.RootCanvasTransform());
		start=us.rect;
		reference=from.rect;
		//us.anchorMax=new Vector2(1,1);
	//	AspectRatioFitter a;
		//a.aspectMode=AspectRatioFitter.AspectMode.FitInParent;
	}
	
	// Update is called once per frame
	void Update () {
		Vector2 dd=10*Time.deltaTime*new Vector2(1.0f/reference.width,1.0f/reference.height);
		us.anchorMax+=dd;
		us.anchorMin+=dd;
		//us.localScale=new Vector3(1.03f,1.03f,1.01f);
		//us.NormalizeScale();
	}
}
