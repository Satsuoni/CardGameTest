#define THING
#define WHATHAPPENS
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SingleGame
{
//Scope tags
	public const string SCOPE_EFFECT="SCOPE_EFFECT";
	public const string SCOPE_TIMELINE="SCOPE_TIMELINE";
//elements
	public const string ELEMENT_PHYSICAL="ELEMENT_PHYSICAL";
	public const string ELEMENT_DARK="ELEMENT_DARK";
	public const string ELEMENT_LIGHT="ELEMENT_LIGHT";
	public const string ELEMENT_LIFE="ELEMENT_LIFE";
	public const string ELEMENT_DEATH="ELEMENT_DEATH";

// "variable" names
	public const string ownID="ownerID";
//helper dictionaries
	public static Dictionary<string,List<object>> acceptedValues=new Dictionary<string, List<object>>();
	public static Dictionary<string,System.Type> acceptedTypes=new Dictionary<string, System.Type>();
//classes

	public class Conditional
	{
		protected Dictionary<string,object> _values;
		protected HashSet<string> _tags;
		public object this[string name]
		{
			get{
				object ret=null;
				if(_values.TryGetValue(name,out ret)) return ret;
				else return null;
			   }
			set
			{
				if(acceptedValues!=null&&acceptedValues.ContainsKey(name))
				{
					List<object> acc=acceptedValues[name];
					if(!acc.Contains(value))
					{
                        #if THING
						Debug.Log(string.Format("Invalid value: {0} for name: {1}",value,name));
                        #endif
						return;
					}
				}
				if(acceptedTypes!=null&&acceptedTypes.ContainsKey(name))
				{
					System.Type acc=acceptedTypes[name];
					System.Type ct=value.GetType();
					if(ct!=acc&&!ct.IsSubclassOf(acc))//subclasses are accepted
					{
						#if THING
						Debug.Log(string.Format("Invalid type: {0} for name: {1}",ct,name));
						#endif
						return;
					}
				}
				_values[name]=value;
			}
		}
		public Conditional()
		{
			_values=new Dictionary<string, object>();
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
	public class Condition //a single condition
	{
		public enum Type
		{
			TAG,
			STRING,
			COMPOUND_ANY,//for Lists
			COMPOUND_ALL,
			/// for numerical
			EQUAL, //val==cnd.val
			LESS, //val<cnd.val
			GREATER,
			LE,
			GE
		}
		public bool inverse;
		public Type type;
		//public Type type_compound;//type to use for compound conditions
		public string variable; //variable name to compare OR tag
		//public string variable_compound;
		public object value;//value to compare to
		public Condition()
		{
			inverse=false;
		}
		public Condition(Type t, string var, object val)
		{
			inverse=false;
			type=t;
			variable=var;
			value=val;

		}
		public bool isFulfilled(Conditional cnd)
		{
			bool res=__isFulfilled(cnd);
			if(inverse) return !res;
			return res;
		}
		protected bool __isFulfilled(Conditional cnd)
		{
			if(type==Type.TAG)
				return cnd.hasTag(variable);
			if(type==Type.STRING)
			{
				string val2=cnd[variable] as string;
				if(val2==null) return false;
				return (val2==(value as string));
			}
			if(type==Type.COMPOUND_ALL||type==Type.COMPOUND_ANY) //compounds! EW
			{
				Condition compcond=value as Condition;
				if(compcond==null) {
					#if THING
					Debug.Log(string.Format("Invalid value for compound condition : {0}",value));
					#endif
					return false;
				}
				object cval=cnd[variable];
				if(!(cval is IList)) return false;
				List<Conditional> ccnds=cval as List<Conditional>;
				if(ccnds==null) return false;
				foreach(Conditional cn in ccnds)
				{
					bool res=compcond.isFulfilled(cn);
					if(type==Type.COMPOUND_ALL&&!res) return false;
					if(type==Type.COMPOUND_ANY&&res) return true;
				}
				if(type==Type.COMPOUND_ALL) return true;
				else
					return false;

			}
			// no good way to compare values, I guess...

			object val=cnd[variable];
			int cmp=0;
			if(val==null) return false;
			System.Type vtp_stupid=val.GetType();
			System.TypeCode vtp=System.Type.GetTypeCode(vtp_stupid);
			switch(vtp)
			{
			case System.TypeCode.Int16:
			case System.TypeCode.Int32:
			case System.TypeCode.Int64:
			{
				System.Int64 cndval=System.Convert.ToInt64(value);
				System.Int64 cmpval=System.Convert.ToInt64(val);
				cmp=cmpval.CompareTo(cndval);
			};break;
			default:{
				System.Double cndval=System.Convert.ToDouble(value);
				System.Double cmpval=System.Convert.ToDouble(val);
				cmp=cmpval.CompareTo(cndval);

			};break;
			}
			switch(type)
			{
			case Type.GREATER:{return (cmp==1);}
			case Type.LESS:{return (cmp==-1);}
			case Type.EQUAL:{return (cmp==0);}
			case Type.GE:{return (cmp==1||cmp==0);}
			case Type.LE:{return (cmp==-1||cmp==0);}
			default:
				return false;
			}
			//return true;
			//int cmp=val.com
		}
	}

	public class ConditionList
	{
		protected List<List<Condition>> _conds;
		public ConditionList()
		{
			_conds=new List<List<Condition>>();
		}
		public void addAndCond(Condition cnd, bool not)
		 {
			if(_conds.Count==0)
			{
				_conds.Add(new List<Condition>());
			}
			List<Condition> last=_conds[_conds.Count-1];
			cnd.inverse=not;
			last.Add(cnd);
		 }
		public void addOrCond(Condition cnd, bool not)
		{
			_conds.Add(new List<Condition>());
			List<Condition> last=_conds[_conds.Count-1];
			cnd.inverse=not;
			last.Add(cnd);
		}
		public bool isFulfilled(Conditional vars)
		{
			foreach(List<Condition> cl in _conds)
			{
				bool check=true;
				foreach(Condition cnd in cl)
				{
					if(!cnd.isFulfilled(vars)){check=false; break;}
				}
				if(check) return true;
			}
			return false;
		}
	}

	public class Effect : Conditional
	{
		//public int ownerID;//player
		public bool applied; //for applying iteration
		public bool active;//may be suppressed by other effects
		ConditionList conditions;
		public Effect():base()
		{
			acceptedTypes[ownID]=typeof(int);
			this.setTag(SCOPE_EFFECT);// all effects are this scope
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
