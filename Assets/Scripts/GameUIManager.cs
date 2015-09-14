﻿using UnityEngine;
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
//		Debug.Log(_game.hookData["_Owner"]);
		if(_game.hookData["_Owner"]!=_game._GameData["Player1"])
		{
			SingleGame.GameManager.endHook();
			hooks.Remove("draw");
			yield break;
		}
		RectTransform rt=deck.RootCanvasTransform();
		GameObject crd=Instantiate(card) as GameObject;
		CardControl crc=crd.GetComponent<CardControl>();
		crc.cardData=_game.hookData;
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
		IList plst=_game._GameData["_Players"] as IList;
		IList lst=(plst[0] as SingleGame.Conditional)["HAND"] as IList;
		int cnt=lst.Count;
		if (cnt>=7) cnt=6;
		can.hand=handSlots[cnt];
		while(!can.done) yield return null;
		SingleGame.GameManager.endHook();
		hooks.Remove("draw");
		yield break;
	}
  void ExecuteChoice(string name,IList lst )
  {
    if(name=="getMainPlayer")
    {
      SingleGame.GameManager.endChoice(lst[0] as SingleGame.Conditional);
      return;
    }
    SingleGame.GameManager.endChoice(null);
  }
 
	void ExecuteHookWithNameAndData(string name,SingleGame.Conditional data )
	{
		hooks.Add(name);
		if(name=="draw")
		{
			StartCoroutine(draw ());
			return;
		}
		if(name=="highlight")
		{
			Debug.Log("highlight");
			Debug.Log(data);
			SingleGame.GameManager.endHook();
			hooks.Remove("highlight");
			return;
		}
		if(name=="log")
		{
      Debug.Log("hooklog");
			Debug.Log(data["text"]);
      hooks.Remove("log");
			SingleGame.GameManager.endHook();
			return;
		}
    if(name=="id")
    {
      Debug.Log("idlog");
      data["playerID"]=SingleGame.getRandString(12);
      Debug.Log(data["playerID"]);
      hooks.Remove(name);
      SingleGame.GameManager.endHook();
      return;
    }
    Debug.Log(string.Format("Undefined Hook called: {0}",name));
		hooks.Remove(name);
		SingleGame.GameManager.endHook();
	}
	// Update is called once per frame
	void Update () {
  
	if(_game.hookInProgress&&!hooks.Contains(_game.hookName))
		{
			ExecuteHookWithNameAndData(_game.hookName,_game.hookData);
		}
    if(_game.choiceInProgress)
    {
      ExecuteChoice(_game.choiceName,_game.choiceList);
    }

	}
}
