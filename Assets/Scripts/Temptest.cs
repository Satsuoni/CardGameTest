using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class base1
{
}
public class check1:base1
{
	public int a;
}
public class Temptest : MonoBehaviour {

	// Use this for initialization
	void Start () {
		TextAsset ta=Resources.Load("utftest") as TextAsset;
		//MemoryStream stream = new MemoryStream(ta.bytes);
		string st=System.Text.Encoding.UTF8.GetString(ta.bytes);

		Debug.Log(char.IsWhiteSpace(st[0]));
		Debug.Log('\n');
		List<check1> ch=new List<check1>();
		ch.Add(new check1());
		Debug.Log(ch is IList);
		foreach(object o in(IList) ch)
			Debug.Log(o);



	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
