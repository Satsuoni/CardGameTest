using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Temptest : MonoBehaviour {

	// Use this for initialization
	void Start () {
		int a=0;
		float b=2;
		System.IComparable aa=a as System.IComparable;
		System.IComparable bb=((int)b ) as System.IComparable;
		List<int> saa=new List<int>();
		Debug.Log(b is IList);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
