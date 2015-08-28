using UnityEngine;
using System.Collections;

public class RectTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
		RectTransform tr=gameObject.GetComponent<RectTransform>();
		if(tr==null) return;
		Debug.Log(SingleGame.getRandString(20,0.1f));
		Debug.Log(string.Format("Rect: {0}",tr.rect));
		Debug.Log(string.Format("AncPos: {0}",tr.anchoredPosition));
		Debug.Log(string.Format("Rootrec: {0}",tr.RootCanvasRect()));
		tr.NormalizeScale();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
