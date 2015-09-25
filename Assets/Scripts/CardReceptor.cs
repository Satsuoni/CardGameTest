using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

abstract public class ConditionalUIEntity: MonoBehaviour
{
	protected SingleGame.Conditional data=null;
	public SingleGame.Conditional cardData {get{return data;} set {data=value;}}
	public void refDataFromString(string dat)
	{
		if(SingleGame.GameManager.self!=null)
		{
			SingleGame.Conditional gd=SingleGame.GameManager.self._GameData;
			if(gd!=null)
				data=gd[dat] as SingleGame.Conditional;
		}
	}
	public void refDataFromList(string dat,int num)
	{
		if(SingleGame.GameManager.self!=null)
		{
			SingleGame.Conditional gd=SingleGame.GameManager.self._GameData;
			if(gd!=null)
			{
				IList lsr=gd[dat] as IList;
				if(num>=0&&num<lsr.Count)
				{
					data=lsr[num] as SingleGame.Conditional;
				}
			}
		}
	}
  public void refDataFromListByTag(string dat,string tag)
  {
    if(SingleGame.GameManager.self!=null)
    {
      SingleGame.Conditional gd=SingleGame.GameManager.self._GameData;
      if(gd!=null)
      {
        IList lsr=gd[dat] as IList;
        foreach(object o in lsr)
        {
          SingleGame.Conditional cc=o as SingleGame.Conditional;
          if(cc!=null&&cc.hasTag(tag))
          {
            data=cc;

            break;
          }
        }

      }
    }
  }
  public void refDataFromListByString(string dat,string valname,string val)
  {
    if(SingleGame.GameManager.self!=null)
    {
      SingleGame.Conditional gd=SingleGame.GameManager.self._GameData;
      if(gd!=null)
      {
        IList lsr=gd[dat] as IList;
        foreach(object o in lsr)
        {
          SingleGame.Conditional cc=o as SingleGame.Conditional;
          if(cc!=null)
          {
            string vv=cc[valname] as string;
            if(vv==val)
            {
              data=cc;
            break;
            }
          }
        }
        
      }
    }
  }
	public virtual  void Highlight(bool light,string type=""){}

	public virtual void Start ()
	{
	}
	public virtual void Update ()
	{
		if(data!=null&&(data.hasTag("main_ACTIVE")||data.hasTag("HIGHLIGHTED")||data.hasTag("main_TARGETABLE")||data.hasTag("main_AIMABLE")))
		   {
			Highlight(true);
		   }
		else
		{

      if(data!=null&&(data.hasTag("main_DISMISSABLE")))
        Highlight(true,"dismissal");
        else
			Highlight(false);
		}
	}
}

public class CardReceptor : ConditionalUIEntity, IDropHandler,IBeginDragHandler,IDragHandler,IEndDragHandler {

	SingleGame.Conditional _mpl=null;
	SingleGame.Conditional player1
	{
		get{ if(_mpl==null) _mpl=cardData["|-.Player1"] as SingleGame.Conditional; return _mpl; }
	}

	ArrowPointer to;
	ArrowPointer from;
	#region IBeginDragHandler implementation
	Vector2 frompoint;
  public GameObject redGlow;
	public void OnBeginDrag (PointerEventData eventData)
	{
		if(!cardData.hasTag(player1["activeTag"] as string)&&
       !cardData.hasTag(player1["dismissableTag"] as string)) return;

		GameObject go=Instantiate(arrow) as GameObject;
		ArrowPointer pnt=go.GetComponent<ArrowPointer>();
		RectTransform rt=go.GetComponent<RectTransform>();
		RectTransform self=transform as RectTransform;
		rt.SetParent(self.RootCanvasTransform(),false);
		rt.SetAsLastSibling();
		from=pnt;
		frompoint=self.getPivotInCanvas();
		Debug.Log(player1["TARGETED"]);
		if(player1["TARGETED"]==cardData)
		{
			//Debug.Log("highlight");
		player1.setTag("EMITTER_HIGHLIGHTED");
		}
		else
		{
			lock(SingleGame.gameLock)
			{
				SingleGame.GameManager.self._GameData["Player1.SELECTED"]=data;//card.cardData;
			//	Debug.Log(player1["SELECTED"]);
			}
		}
	}
  
  #endregion
  
  #region IDragHandler implementation

	public void OnDrag (PointerEventData eventData)
	{
		Vector2 otherpos;
		RectTransform self=transform as RectTransform;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(self.RootCanvasTransform(), eventData.position,eventData.pressEventCamera,out otherpos);
		if(from!=null)
		{
			from.drawArrow(frompoint,otherpos);
		}
	}

	#endregion

	#region IEndDragHandler implementation
	bool waitingForDrop=false;
	bool droppedOnTarget=false;
	public void DropOccurred()
	{
		droppedOnTarget=true;
	}
	void IEndDragHandler.OnEndDrag (PointerEventData eventData)
	{
	 if(from!=null)
		{
			Destroy(from.gameObject);
			from=null;
		}
		player1.removeTag("EMITTER_HIGHLIGHTED");
		if(to!=null)
		{
			Destroy(to.gameObject);
			to=null;
		}

		waitingForDrop=true;

	}

	#endregion

	static GameObject _arrow=null;
	static GameObject arrow
	{
		get { if(_arrow==null) _arrow=Resources.Load("arrowPrefab") as GameObject; return _arrow;}
	}
  public RectTransform glow;
	public string validTag;
	#region IDropHandler implementation
	void IDropHandler.OnDrop (PointerEventData eventData)
	{
		if(eventData.pointerDrag!=null)
		{

			CardControl card=eventData.pointerDrag.GetComponent<CardControl>();
			if(card!=null)
			{
				if(data!=null&&data.hasTag(data["|-.Player1.targetTag"] as string))
        {
          lock(SingleGame.gameLock)
          {
            SingleGame.GameManager.self._GameData["Player1.TARGETED"]=data;//card.cardData;
						//Debug.Log(player1["TARGETED"]);
          }
				//Debug.Log(card);
				/*RectTransform cardrt=card.gameObject.GetComponent<RectTransform>();
				cardrt.SetParent(gameObject.transform,false);
				cardrt.anchorMin=Vector2.zero;
				cardrt.anchorMax=Vector2.one;
				cardrt.offsetMax=Vector2.zero;
				cardrt.offsetMin=Vector2.zero;
				cardrt.localScale=new Vector3(1,1,1);*/
				if(!card.cardData.hasTag("ABILITY"))
				 card.registerDropSuccess(false);
					else
					{
						Debug.Log("ssss");
						Vector2 ops= card.getOPos();
						RectTransform ctp=card.gameObject.GetComponent<RectTransform>();
						Vector2 sv=ctp.anchoredPosition;
						ctp.anchoredPosition=ops;
						ops=ctp.getPivotInCanvas();
						ctp.anchoredPosition=sv;
						Vector2 dest=(transform as RectTransform).getPivotInCanvas();
						GameObject go=Instantiate(arrow) as GameObject;
						ArrowPointer pnt=go.GetComponent<ArrowPointer>();
						RectTransform rt=go.GetComponent<RectTransform>();
						RectTransform self=transform as RectTransform;
						rt.SetParent(self.RootCanvasTransform(),false);
						rt.SetAsLastSibling();
						Debug.Log(ops);
						pnt.drawArrow(ops,dest);
						to=pnt;
						card.registerDropSuccess(false);
					}
        }
        else
         card.registerDropSuccess(false);
			}
			else
			{
				CardReceptor rc=eventData.pointerDrag.GetComponent<CardReceptor>();
        if(cardData.hasTag("DISMISS_AREA")&&rc!=null&&
           rc.cardData.hasTag(player1["dismissableTag"] as string))
        {
          rc.cardData.setTag("DESELECT");
          rc.cardData.setTag("DISMISS_MARK");
          return;
        }
				if(rc!=null&&cardData.hasTag(player1["aimTag"] as string))
				{ // this is ability resolution
					player1["AIMED"]=cardData;
					Debug.Log(rc);
					rc.DropOccurred();
					return;
				}

				if(rc!=null&&cardData.hasTag(player1["targetTag"] as string))
				   {
					player1["TARGETED"]=cardData;
					Debug.Log(rc);
					rc.DropOccurred();
               return;
				}

			}
		}

	}
	#endregion


	// Use this for initialization
	public override void Start () {

	}
  public override void Highlight(bool light, string type="")
  {
    if(glow!=null)
    {
      if(light)
      {
        if(type!="dismissal")
        {
        glow.gameObject.SetActive(true);
        glow.SetAsLastSibling();
          if(redGlow!=null)
          redGlow.SetActive(false);
        }
        else
        {
          if(redGlow!=null)
          {
            redGlow.SetActive(true);
            (redGlow.transform as RectTransform).SetAsLastSibling();
          }
        }
      }
      else
      {
        if(redGlow!=null)
          redGlow.SetActive(false);
        glow.gameObject.SetActive(false);
      }
    }
  }
	UnityEngine.UI.RawImage img=null;
	UnityEngine.UI.Image health=null; 
	// Update is called once per frame
	public override void Update () {
		base.Update();
    if(validTag=="DISMISS_AREA"&&cardData!=null)
    {
    //  Debug.Log(cardData.hasTag("main_TARGETABLE"));
    //  Debug.Log(cardData.hasTag("DISMISS_AREA"));
    } 
	 if(data!=null&&data.hasTag("CARD"))
		{//render a card on this thing...
			if(img==null)
			{
				GameObject go=new GameObject("img");
				go.AddComponent(typeof(UnityEngine.UI.RawImage));
				img=go.GetComponent<UnityEngine.UI.RawImage>();
				RectTransform tt=go.GetComponent<RectTransform>();
				tt.SetParent(transform,false);
				tt.assignRectAnchors(new Vector4(0,0.5f,1,1f));
				string imgname=cardData["_cardImage"] as string;
				if(!string.IsNullOrEmpty(imgname))
				{
					Texture2D txt=Resources.Load(imgname) as Texture2D;
					if(txt!=null)
						img.texture=txt;
				}
			}
			if(health==null)
			{
				GameObject go=Instantiate(Resources.Load("healthbar") as GameObject) as GameObject;
				health=go.GetComponent<UnityEngine.UI.Image> ();
				RectTransform rt=go.GetComponent<RectTransform>();
				rt.SetParent(transform,false);
				rt.assignRectAnchors(new Vector4(0.1f,0,0.9f,0.1f));


			}
			if(health!=null)
			{
				if(data["_durability"]!=null&&data["Energy"]!=null)
				{
					float dr=System.Convert.ToSingle(data["_durability"]);
					float cr=System.Convert.ToSingle(data["Energy"]);
					health.fillAmount=cr/dr;
				}
				
			}

		}
    else
    {
      if(img!=null){ Destroy(img.gameObject); img=null;}
			if(health!=null){ Destroy(health.gameObject); health=null;}
    }
		if(waitingForDrop)
		{
			if(!droppedOnTarget)
			{
				if(player1["TARGETED"]==cardData)
				cardData.setTag("DETARGET");
				if(player1["SELECTED"]==cardData)
					cardData.setTag("DESELECT");
				//SingleGame.GameManager.self._GameData["Player1.TARGETED"]=null;
			}
		waitingForDrop=false;
		 droppedOnTarget=false;
		}
	}
}
