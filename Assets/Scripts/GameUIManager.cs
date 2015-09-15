﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
public class GameUIManager : MonoBehaviour {
	public enum UIArea
	{
		HAND,
		DECK,
		DISCARD,
		BODY
	}
	public CardControl cardPrefab;
	SingleGame.GameManager _game;
	//SingleGame
	public RectTransform [] handSlots=new RectTransform[7];
	public RectTransform deck;
	public RectTransform flipper;
	public GameObject card;
	List<string> hooks=new List<string>();
  public CardReceptor headRec;
  public CardReceptor torsoRec;
  public CardReceptor lhandRec;
  public CardReceptor rhandRec;
  public CardReceptor llegRec;
  public CardReceptor rlegRec;
  public Text energy;
  SingleGame.Conditional player1;
  bool initDone=false;
	//Dictionary<string
	// Use this for initialization
	void Start () {
		_game=new SingleGame.GameManager();
		_game.Start();
    StartCoroutine(init());
    StartCoroutine("timeProgression");
	}
	bool collectTime=false;
  float timeSinceLastUpdate=0;
  IEnumerator timeProgression()
  {
    while(true)
    {
     // lock(SingleGame.gameLock)
      //{
			if(collectTime)
    timeSinceLastUpdate+=Time.deltaTime;
    if(!_game._GameData.hasTag("UPDATE_TIME"))
    {
				lock(SingleGame.gameLock)
				{
      _game._GameData["deltaTime"]=timeSinceLastUpdate;
      //  Debug.Log(timeSinceLastUpdate);
      timeSinceLastUpdate=0;
      _game._GameData.setTag("UPDATE_TIME");
				}
    }
      //}
    yield return null;
    }
  }
  IEnumerator init()
  {
    while(!_game._GameData.hasTag("ENTITIES_DONE"))
    {
      yield return null;
    }
    headRec.refDataFromListByString("Player1.DEFAULT_BODY","slot","SLOT_HEAD");
    torsoRec.refDataFromListByString("Player1.DEFAULT_BODY","slot","SLOT_TORSO");
    lhandRec.refDataFromListByString("Player1.DEFAULT_BODY","slot","SLOT_LHAND");

    rhandRec.refDataFromListByString("Player1.DEFAULT_BODY","slot","SLOT_RHAND");

   llegRec.refDataFromListByString("Player1.DEFAULT_BODY","slot","SLOT_LLEG");

   rlegRec.refDataFromListByString("Player1.DEFAULT_BODY","slot","SLOT_RLEG");
    player1=_game._GameData["Player1"] as SingleGame.Conditional;
     initDone=true;
		collectTime=true;
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
		collectTime=false;
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

		int cnt=lst.Count-1;
		if (cnt>=7) cnt=6;
		can.hand=handSlots[cnt];
		while(!can.done) yield return null;
		SingleGame.GameManager.endHook();
		hooks.Remove("draw");
		collectTime=true;
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
 IEnumerator trans()
	{
		Debug.Log("trans");
		hooks.Remove("transformation");
		SingleGame.GameManager.endHook();
		yield break;
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
		if(name=="transformation")
		{
			StartCoroutine(trans());
			return;
       }
    Debug.Log(string.Format("Undefined Hook called: {0}",name));
		hooks.Remove(name);
		SingleGame.GameManager.endHook();
	}
	// Update is called once per frame
	void Update () {
		if(!initDone) return;
	if(_game.hookInProgress&&!hooks.Contains(_game.hookName))
		{
			ExecuteHookWithNameAndData(_game.hookName,_game.hookData);
		}
    if(_game.choiceInProgress)
    {
      ExecuteChoice(_game.choiceName,_game.choiceList);
    }
    if(player1!=null&&energy!=null)
    {
      energy.text=player1["Energy"].ToString();
    }
	}
}
