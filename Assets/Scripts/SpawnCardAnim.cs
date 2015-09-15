using UnityEngine;
using System.Collections;

public class SpawnCardAnim : MonoBehaviour {
  public float flydur=0.2f;
	public RectTransform middle;
	public RectTransform hand;
	public bool done=false;
	// Use this for initialization
	void Start () {
		StartCoroutine(anim ());
	}
	IEnumerator anim()
	{
		RectTransform me=gameObject.GetComponent<RectTransform>();
		me.SetAsLastSibling();
		IAnimInterface a1=RectTransfer.Apply(gameObject,middle,flydur);
		a1.Run();
		while(!a1.isDone) yield return null;
		Destroy(a1 as Component);
		a1=gameObject.GetComponent<CardFlip>();
		a1.Run();
		while(!a1.isDone) yield return null;
		//Destroy(a1 as Component);
		a1=RectTransfer.Apply(gameObject,hand,flydur);
		a1.Run();
		while(!a1.isDone) yield return null;
		Destroy(a1 as Component);
		done=true;
	}
	// Update is called once per frame
	void Update () {
	
	}
}
