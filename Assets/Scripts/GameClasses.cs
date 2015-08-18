using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class SingleGame
{
//Scope tags
	public const string SCOPE_EFFECT="SCOPE_EFFECT";
	public const string SCOPE_TIMELINE="SCOPE_TIMELINE";
//classes
	public class Condition
	{
	}
	public class Effect
	{
		public int ownerID;//player
		public bool applied; //for applying iteration
		public bool active;//may be suppressed by other effects
		HashSet<string> _tags;
		public Effect()
		{
			_tags=new HashSet<string>();
		}
		public bool hasTag(string tag)
		{
			return _tags.Contains(tag);
		}
		public void setTag(string tag)
		{
			_tags.Add(tag);
		}
		public void removeTag(string tag)
		{
			_tags.Remove(tag);
		}
	}
	public class EffectList
	{

	}
}
public class GameClasses   {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
