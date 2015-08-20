using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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
