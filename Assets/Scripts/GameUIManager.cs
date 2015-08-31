using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class GameUIManager : MonoBehaviour {

	public CardControl cardPrefab;
	SingleGame.GameManager _game;
	//SingleGame
	public RectTransform [] handSlots=new RectTransform[7];
	public RectTransform deck;
	public RectTransform flipper;
	public GameObject card;
	List<string> hooks=new List<string>();

	//Dictionary<string
	// Use this for initialization
	void Start () {
		_game=new SingleGame.GameManager();
		_game.Start();
	}
	IEnumerator draw()
	{
		RectTransform rt=deck.RootCanvasTransform();
		GameObject crd=Instantiate(card) as GameObject;
		RectTransform crt=crd.GetComponent<RectTransform>();
		crt.SetParent(rt,false);
		crt.SetAsLastSibling();
		Rect rr = deck.RootCanvasRect ();
		Vector4 tovec = rt.getAnchorsFromCanvasRect (rr);
		//crt.SetInternalAnchors (new Vector4 (0, 0, 1, 1));
		crt.anchorMin=new Vector2(tovec.x,tovec.y);
		crt.anchorMax=new Vector2(tovec.z,tovec.w);
		crt.offsetMax=Vector2.zero;
		crt.offsetMin=Vector2.zero;
		SpawnCardAnim can=crd.GetComponent<SpawnCardAnim>();
		can.middle=flipper;
		IList lst=_game._GameData["HAND"] as IList;
		int cnt=lst.Count;
		if (cnt>=7) cnt=6;
		can.hand=handSlots[cnt];
		while(!can.done) yield return null;
		SingleGame.GameManager.endHook();
		hooks.Remove("draw");
		yield break;
	}
	void ExecuteHookWithNameAndData(string name,SingleGame.Conditional data )
	{
		hooks.Add(name);
		if(name=="draw")
		{
			StartCoroutine(draw ());
		}
	}
	// Update is called once per frame
	void Update () {
	if(_game.hookInProgress&&!hooks.Contains(_game.hookName))
		{
			ExecuteHookWithNameAndData(_game.hookName,_game.hookData);
		}
	}
}
