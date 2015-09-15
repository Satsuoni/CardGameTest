using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

abstract public class ConditionalUIEntity: MonoBehaviour
{
	protected SingleGame.Conditional data=null;
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
	public virtual  void Highlight(bool light){}

	public virtual void Start ()
	{
	}
	public virtual void Update ()
	{
		if(data!=null&&(data.hasTag("ACTIVE")||data.hasTag("HIGHLIGHTED")||data.hasTag("TARGETABLE")))
		   {
			Highlight(true);
		   }
		else
		{
			Highlight(false);
		}
	}
}

public class CardReceptor : ConditionalUIEntity, IDropHandler {

  public RectTransform glow;
	#region IDropHandler implementation
	void IDropHandler.OnDrop (PointerEventData eventData)
	{
		if(eventData.pointerDrag!=null)
		{
			
			CardControl card=eventData.pointerDrag.GetComponent<CardControl>();
			if(card!=null)
			{
        if(data!=null&&data.hasTag("TARGETABLE"))
        {
          lock(SingleGame.gameLock)
          {
            SingleGame.GameManager.self._GameData["TARGETED"]=data;//card.cardData;

          }
				//Debug.Log(card);
				/*RectTransform cardrt=card.gameObject.GetComponent<RectTransform>();
				cardrt.SetParent(gameObject.transform,false);
				cardrt.anchorMin=Vector2.zero;
				cardrt.anchorMax=Vector2.one;
				cardrt.offsetMax=Vector2.zero;
				cardrt.offsetMin=Vector2.zero;
				cardrt.localScale=new Vector3(1,1,1);*/
				card.registerDropSuccess(false);
        }
        else
         card.registerDropSuccess(false);
			}
		}

	}
	#endregion


	// Use this for initialization
	public override void Start () {

	}
  public override void Highlight(bool light)
  {
    if(glow!=null)
    {
      if(light)
      {
        glow.gameObject.SetActive(true);
        glow.SetAsLastSibling();
      }
      else
      {
        glow.gameObject.SetActive(false);
      }
    }
  }
	
	// Update is called once per frame
	public override void Update () {
		base.Update();
	
	}
}
