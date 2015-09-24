using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EventProgressBar : ConditionalUIEntity {
	public bool mark{get;set;}
	Image img;
	// Use this for initialization
	public override void Start () {
		img=gameObject.GetComponent<Image>();
		img.type=Image.Type.Filled;
		img.fillMethod=Image.FillMethod.Radial360;
		img.fillAmount=0;
		//
		//event.Delay <=_ability.delay
		//set event.orDelay <=event.Delay
	}
	
	// Update is called once per frame
	public override  void Update () {
	if(cardData!=null)
		{
			float fl_all=System.Convert.ToSingle( cardData["orDelay"]);
			float fl_now=System.Convert.ToSingle( cardData["Delay"]);
			if(fl_now<=0) return;
			float fl=(fl_all-fl_now)/fl_all;
			img.fillAmount=fl;
		}
	}
}
