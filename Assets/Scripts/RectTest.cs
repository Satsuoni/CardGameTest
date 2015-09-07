using UnityEngine;
using System.Collections;

public class RectTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
		RectTransform tr=gameObject.GetComponent<RectTransform>();
		if(tr==null) return;
		/*string st="condition countHand {  and{ condition { true } condition { less _count 7 } condition { greater _count 7 }  }}";
		int pos=0;
		bool res=false;*/
		//SingleGame.Condition cnd=SingleGame.Parser.readCondition(st,ref pos,out res);
    TextAsset ass=Resources.Load("draw") as TextAsset;
    Debug.Log(ass.text);
    //Debug.Log(SingleGame.Conditional.loadFromString(ass.text));
   // string ch="return foreach _targetList { set crap 7.5 add crap 1 }";
//    SingleGame.Operation op=SingleGame.Parser.readOperation(ch,ref pos,out res);
    //Debug.Log(op.);
		//Debug.Log(SingleGame.getRandString(20,0.1f));
		Debug.Log(string.Format("Rect: {0}",tr.rect));
		Debug.Log(string.Format("AncPos: {0}",tr.anchoredPosition));
		Debug.Log(string.Format("Rootrec: {0}",tr.RootCanvasRect()));
		Debug.Log(string.Format("Internal: {0}",tr.InternalAnchors()));
		//tr.SetInternalAnchors(new Vector4(0,0,1,1));
	}
	
	// Update is called once per frame
	void Update () {
		//RectTransform tr=gameObject.GetComponent<RectTransform>();
	//	Debug.Log (tr.offsetMax);

	}
	void OnApplicationQuit() {
		SingleGame.GameManager.STOP();
	}
}
