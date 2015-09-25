using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBar : MonoBehaviour {

	public Image img;
	Text txt;
	 float _maxHP=0;
	 float _HP=0;
	public float maxHP
	{
		get{return _maxHP;}
		set{_maxHP=value; hpAdjust();}
	}
	public float HP
	{
		get{return _HP;}
		set{_HP=value; hpAdjust();}
	}
	void hpAdjust()
	{
		if(_maxHP>0)
		{
			img.fillAmount=_HP/_maxHP;
			if(txt!=null)
				txt.text=string.Format("{0} / {1}",(int)_HP,(int)_maxHP);
		}
	}
	// Use this for initialization
	void Start () {
		//img=gameObject.GetComponentInChildren<Image>();
		img.type=Image.Type.Filled;
		img.fillMethod=Image.FillMethod.Horizontal;
		img.fillOrigin=0;
		txt=gameObject.GetComponentInChildren<Text>();

	}
	
	// Update is called once per frame
	//void Update () {
	
	//}
}
