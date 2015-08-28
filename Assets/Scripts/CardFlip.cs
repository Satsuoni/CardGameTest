using UnityEngine;
using System.Collections;
public interface IAnimInterface
{
	bool isDone{get;}
	void Run();
}
public class CardFlip : MonoBehaviour, IAnimInterface {

	public GameObject back;
	public GameObject front;
	public float flipTime=1.0f;
	bool _isDone=true;
	public bool isDone{get{return _isDone;}}
	//public Vector3 axis=new Vector3(0,1,0);
	// Use this for initialization
	void Start () {

		//gameObject.RotateBy(new Vector3(0,0.5f,0),5,1,EaseType.linear);
		//iTween.MoveUpdate

	}
	public void Run()
	{
		StartCoroutine(flipCoRoutine());
	}
	IEnumerator flipCoRoutine()
	{
		_isDone=false;
		if(back.activeSelf&&front.activeSelf)
		{
			front.SetActive(false);
		}
		GameObject flipFrom;
		GameObject flipTo;
		if(back.activeSelf)
		{
			flipFrom=back;
			flipTo=front;
		}
		else
		{
			flipFrom=front;
			flipTo=back;
		}
		flipTo.SetActive(false);

		float ctime=0;
		Quaternion rot=Quaternion.identity;
		rot.eulerAngles=new Vector3(0,90,0);
		flipTo.transform.localRotation=rot;
		flipFrom.transform.localRotation=Quaternion.identity;

		while(ctime<flipTime*0.5f)
		{
			rot.eulerAngles=new Vector3(0,180*ctime/flipTime,0);
			flipFrom.transform.localRotation=rot;
			yield return null;
			ctime+=Time.deltaTime;
		}

		rot.eulerAngles=new Vector3(0,90,0);
		flipFrom.transform.localRotation=rot;
		yield return null;
		flipFrom.SetActive(false);
		flipTo.SetActive(true);
		yield return null;
		ctime=0;
		while(ctime<flipTime*0.5f)
		{
			rot.eulerAngles=new Vector3(0,90-180*ctime/flipTime,0);
			flipTo.transform.localRotation=rot;
			yield return null;
			ctime+=Time.deltaTime;
		}

		rot.eulerAngles=new Vector3(0,0,0);
		flipTo.transform.localRotation=rot;
		_isDone=true;
	}
	// Update is called once per frame
	void Update () {
	
	}
}
