using UnityEngine;
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

	public CardReceptor opponent_headRec;
	public CardReceptor opponent_torsoRec;
	public CardReceptor opponent_lhandRec;
	public CardReceptor opponent_rhandRec;
	public CardReceptor opponent_llegRec;
	public CardReceptor opponent_rlegRec;

  public CardReceptor dismissArea;
	List<CardReceptor> mpBody=new List<CardReceptor>();
  public Text energy;
	public Text opponent_energy;
  SingleGame.Conditional player1;
  bool initDone=false;
	public float shiftdur=0.2f;
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
		headRec.validTag="SLOT_HEAD";
		torsoRec.validTag="SLOT_TORSO";
		lhandRec.validTag="SLOT_LHAND";
		rhandRec.validTag="SLOT_RHAND";
		llegRec.validTag="SLOT_LLEG";
		rlegRec.validTag="SLOT_RLEG";
		mpBody.Add(headRec);mpBody.Add(torsoRec);mpBody.Add(lhandRec);mpBody.Add(rhandRec);
		mpBody.Add(llegRec);mpBody.Add(rlegRec);
    headRec.refDataFromListByString("Player1.DEFAULT_BODY","slot","SLOT_HEAD");
    torsoRec.refDataFromListByString("Player1.DEFAULT_BODY","slot","SLOT_TORSO");
    lhandRec.refDataFromListByString("Player1.DEFAULT_BODY","slot","SLOT_LHAND");

    rhandRec.refDataFromListByString("Player1.DEFAULT_BODY","slot","SLOT_RHAND");

   llegRec.refDataFromListByString("Player1.DEFAULT_BODY","slot","SLOT_LLEG");

   rlegRec.refDataFromListByString("Player1.DEFAULT_BODY","slot","SLOT_RLEG");


		opponent_headRec.refDataFromListByString("Player2.DEFAULT_BODY","slot","SLOT_HEAD");
		opponent_torsoRec.refDataFromListByString("Player2.DEFAULT_BODY","slot","SLOT_TORSO");
		opponent_lhandRec.refDataFromListByString("Player2.DEFAULT_BODY","slot","SLOT_LHAND");
	
		opponent_rhandRec.refDataFromListByString("Player2.DEFAULT_BODY","slot","SLOT_RHAND");
		
		opponent_llegRec.refDataFromListByString("Player2.DEFAULT_BODY","slot","SLOT_LLEG");
		
		opponent_rlegRec.refDataFromListByString("Player2.DEFAULT_BODY","slot","SLOT_RLEG");
    dismissArea.refDataFromString("dismiss_area");
    Debug.Log("dimissaAREA");
    Debug.Log(dismissArea.cardData.hasTag("DISMISS_AREA"));
    player1=_game._GameData["Player1"] as SingleGame.Conditional;
     initDone=true;
		collectTime=true;
  }
	List<CardControl> handCards=new List<CardControl>();
	IEnumerator draw()
	{
		/*IAnimInterface a1=RectTransfer.Apply(gameObject,middle,flydur);
		a1.Run();
		while(!a1.isDone) yield return null;*/
//		Debug.Log(_game.hookData["_Owner"]);
		if(_game.hookData["_Owner"]!=_game._GameData["Player1"])
		{
			SingleGame.GameManager.endHook();
			hooks.Remove("draw");
			yield break;
		}
		/*if(postshuf)
		{
			SingleGame.Conditional dt= _game.hookData;
			dt.logtags();
		}*/
		collectTime=false;
		IList handlst=(_game._GameData["Player1"] as SingleGame.Conditional)["HAND"] as IList;
		List<CardControl> trem=new List<CardControl>();
		List<IAnimInterface> anims=new List<IAnimInterface>();
		for(int handc=0;handc<handCards.Count;handc++)
		{
			int newp=-1;
			if(_game.hookData!=handCards[handc].cardData)
			for(int pos=0;pos<handlst.Count;pos++)
			{
				if(handlst[pos]==handCards[handc].cardData)
				{
					newp=pos;
					break;
				}
			}
			if(newp==-1)
			{
				trem.Add(handCards[handc]);
			}
			else
			{
				if(newp!=handCards[handc].slotPos)
				{
					int dif=Mathf.Abs(newp-handCards[handc].slotPos);
					anims.Add (RectTransfer.Apply(handCards[handc].gameObject,handSlots[newp],shiftdur*dif));
					handCards[handc].slotPos=newp;
				}
			}
		}
		foreach(CardControl td in trem)
		{
			handCards.Remove(td);
			Destroy(td.gameObject);
		}
		trem=null;
      foreach(IAnimInterface anim in anims)
		{
			anim.Run();
		}
		bool done=false;
		while(!done)
		{
			done=true;
			foreach(IAnimInterface anim in anims) done=(done & anim.isDone);
        yield return null;
		}
		RectTransform rt=deck.RootCanvasTransform();
		GameObject crd=Instantiate(card) as GameObject;
		CardControl crc=crd.GetComponent<CardControl>();
		crc.playerID=player1["playerID"] as string;
		crc.cardData=_game.hookData;
		crc.slotPos=0;
		RectTransform crt=crd.GetComponent<RectTransform>();
		crt.SetParent(rt,false);
		crt.SetAsLastSibling();
		Rect rr = deck.RootCanvasRect ();
		Vector4 tovec = rt.getAnchorsFromCanvasRect (rr);
		//crt.SetInternalAnchors (new Vector4 (0, 0, 1, 1));
		crt.assignRectAnchors(tovec);
		/*crt.anchorMin=new Vector2(tovec.x,tovec.y);
		crt.anchorMax=new Vector2(tovec.z,tovec.w);
		crt.offsetMax=Vector2.zero;
		crt.offsetMin=Vector2.zero;*/
		SpawnCardAnim can=crd.GetComponent<SpawnCardAnim>();
		can.middle=flipper;
		handCards.Add(crc);
		int cnt=0;
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
//	bool postshuf=false;
 IEnumerator trans()
	{
		Debug.Log("trans");
		SingleGame.Conditional castCard=_game.hookData["castSource"] as SingleGame.Conditional;
		//SingleGame.Conditional targetCard=_game.hookData["castTarget"] as SingleGame.Conditional;
	    if(castCard!=null)
		{
			int handnum=-1;
			for(int i=0;i<handCards.Count;i++)
			{
				if(castCard==handCards[i].cardData){handnum=i;break;}
			}
			if(handnum==-1)
			{
				Debug.Log("Some error");
			}
			else
			{
				CardControl toMove=handCards[handnum];
				RectTransform original=toMove.GetComponent<RectTransform>();
				RectTransform canv=original.RootCanvasTransform();
				Vector4 posvec=canv.getAnchorsFromCanvasRect(original.RootCanvasRect());
				handCards.Remove(toMove);
				List<IAnimInterface> anims=new List<IAnimInterface>();
				for(int j=0;j<mpBody.Count;j++)
				{
					CardReceptor cr=mpBody[j];
					if(castCard.hasTag(cr.validTag))
					{
						GameObject crd=Instantiate(card) as GameObject;
						CardControl crc=crd.GetComponent<CardControl>();
						crc.cardData=castCard;
						crc.playerID=castCard["_Owner.playerID"] as string;
						SpawnCardAnim todel=crd.GetComponent<SpawnCardAnim>();
						Destroy(todel as Component);
						CardFlip flp=crd.GetComponent<CardFlip>();
						flp.Flip();
						RectTransform ctr=crd.GetComponent<RectTransform>();
						ctr.SetParent(canv,false);
						ctr.SetAsLastSibling();
						ctr.assignRectAnchors(posvec);
						RectTransform targpos=cr.gameObject.GetComponent<RectTransform>();
						IAnimInterface an=RectTransfer.Apply(crd,targpos,shiftdur);
						an.Run();
						anims.Add(an);
						cr.cardData=castCard;
					}
				}
				Destroy(toMove.gameObject);
				
				bool done=false;
				while(!done)
				{
					done=true;
					foreach(IAnimInterface anim in anims) done=(done & anim.isDone);
					yield return null;
				}
				foreach(IAnimInterface anim in anims) 
				{
					RectTransfer rt=anim as RectTransfer;
					Destroy(rt.gameObject);
				}

			}
		}
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
    if(name=="discard")
    {
      Debug.Log("discarded");
      foreach(CardReceptor cr in mpBody)
      {
        cr.refDataFromListByString("Player1.BODY","slot",cr.validTag);
        if(cr.cardData==null)
          cr.refDataFromListByTag("Player1.BODY",cr.validTag);
      }


      hooks.Remove(name);
      SingleGame.GameManager.endHook();
      return;
    }
		if(name=="test")
		{
			Debug.Log("testlog");
      Debug.Log(data["slot"]);
      Debug.Log(data["_Owner.BODY"]);
			//Debug.Log(data.hasTag(player1["activeTag"] as string));
			//IList dl=data["_used"] as IList;
		//	Debug.Log(dl.Count);
			hooks.Remove("test");
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

		if(name=="shuffle")
		{
			Debug.Log("shuffle");
			IList dl=player1["DISCARD"] as IList;
			Debug.Log(dl.Count);
			dl=player1["DECK"] as IList;
			Debug.Log(dl.Count);
			Debug.Log(player1==data);
			hooks.Remove(name);
			SingleGame.GameManager.endHook();
//			postshuf=true;
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
      energy.text=Mathf.FloorToInt((float)player1["Energy"]).ToString();
    }
		if(opponent_energy!=null)
		{
			opponent_energy.text=Mathf.FloorToInt((float)_game._GameData["Player2.Energy"]).ToString();
		}
  }
}
