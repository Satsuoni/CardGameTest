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
	public const string SCOPE_HAND="SCOPE_HAND";
//elements
	public const string ELEMENT_PHYSICAL="ELEMENT_PHYSICAL";
	public const string ELEMENT_DARK="ELEMENT_DARK";
	public const string ELEMENT_LIGHT="ELEMENT_LIGHT";
	public const string ELEMENT_LIFE="ELEMENT_LIFE";
	public const string ELEMENT_DEATH="ELEMENT_DEATH";

// "variable" names
	public const string ownID="ownerID";
	public const string _count="_count";
	public const string _delay="_delay"; //timeline delay for Event
//helper dictionaries
	public static Dictionary<string,List<object>> acceptedValues=new Dictionary<string, List<object>>();
	public static Dictionary<string,System.Type> acceptedTypes=new Dictionary<string, System.Type>();
//classes

	public class Conditional
	{
		protected Dictionary<string,object> _values;
		protected HashSet<string> _tags;
		public virtual Conditional duplicate()
		{
			Conditional ret=new Conditional();
			foreach(string tag in _tags)
				ret.setTag(tag);
			foreach(KeyValuePair<string,object> keyp in _values)
			{
				string key=keyp.Key;
				object obj=keyp.Value;
				if(obj is Conditional)
				{
					ret._values[key]=(obj as Conditional).duplicate();
					continue;
				}
				if(obj is List<Conditional>)
				{
					List<Conditional> cnds=obj as List<Conditional>;
					List<Conditional> nw=new List<Conditional>();
					foreach(Conditional cn in cnds)
					{
						nw.Add(cn.duplicate());
					}
					ret._values[key]=nw;
					continue;
				}
				if(obj is System.ICloneable)
				{
					System.ICloneable cl=obj as System.ICloneable;
					ret._values[key]=cl.Clone();
					continue;
				}
				ret._values[key]=obj;

			}
			return ret;
		}
		public bool hasVariable(string var)
		{
			return _values.ContainsKey(var);
		}
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
			COMPOUND_COUNT,
			///For and/or
			MULTI_AND,
			MULTI_OR,
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
		public object [] values;//value(s) to compare to
		public Condition()
		{
			inverse=false;
		}
		public Condition(Type t, string var, params object [] val)
		{
			inverse=false;
			type=t;
			variable=var;
			values=new object [val.Length];
			for(int i=0;i<val.Length;i++)
			{
				values[i]=val[i];
			}

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
				if(values.Length<1) return false;
				string val2=cnd[variable] as string;
				if(val2==null) return false;
				return (val2==(values[0] as string));
			}
			if(type==Type.COMPOUND_ALL||type==Type.COMPOUND_ANY||type==Type.COMPOUND_COUNT) //compounds! EW
			{
				if(values.Length<1) return false;
				if(values.Length<2&&type==Type.COMPOUND_COUNT) return false;
				Condition compcond=values[0] as Condition;
				if(compcond==null) {
					#if THING
					Debug.Log(string.Format("Invalid value for compound condition : {0}",values[0]));
					#endif
					return false;
				}
				object cval=cnd[variable];
				if(!(cval is IList)) return false;
				List<Conditional> ccnds=cval as List<Conditional>;
				if(ccnds==null) return false;
				int cnt=0;
				foreach(Conditional cn in ccnds)
				{
					bool res=compcond.isFulfilled(cn);
					if(type==Type.COMPOUND_ALL&&!res) return false;
					if(type==Type.COMPOUND_ANY&&res) return true;
					if(type==Type.COMPOUND_COUNT&&res) cnt=cnt+1;
				}
				if(type==Type.COMPOUND_ALL) return true;
				if(type==Type.COMPOUND_ANY)
					return false;
				if(type==Type.COMPOUND_COUNT)
				{
					Condition cnd2=values[1] as Condition;
					if(cnd2==null) return false;
					Conditional temp=new Conditional();
					temp[_count]=cnt;
					return cnd2.isFulfilled(temp);
				}

			}
			if(type==Type.MULTI_AND)
			{
				bool ok=true;
				foreach(object obj in values)
				{
					Condition tcnd=obj as Condition;
					if(tcnd==null)
					{
						#if THING
						Debug.Log(string.Format("Invalid value for multi_and condition : {0}",obj));
						#endif
						return false;
					}
					if(!tcnd.isFulfilled(cnd)){ok=false;break;}
				}
				return ok;
			}
			if(type==Type.MULTI_OR)
			{
				bool ok=false;
				foreach(object obj in values)
				{
					Condition tcnd=obj as Condition;
					if(tcnd==null)
					{
						#if THING
						Debug.Log(string.Format("Invalid value for multi_or condition : {0}",obj));
						#endif
						return false;
					}
					if(tcnd.isFulfilled(cnd)){ok=true;break;}
				}
				return ok;
			}

			// no good way to compare values, I guess...
			if(values.Length<1) return false;
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
				System.Int64 cndval=System.Convert.ToInt64(values[0]);
				System.Int64 cmpval=System.Convert.ToInt64(val);
				cmp=cmpval.CompareTo(cndval);
			};break;
			default:{
				System.Double cndval=System.Convert.ToDouble(values[0]);
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


	public class Effect : Conditional
	{
		//public int ownerID;//player
		public bool applied; //for applying iteration
		public bool active;//may be suppressed by other effects
		Condition conditions;
		public Effect():base()
		{
			acceptedTypes[ownID]=typeof(int);
			this.setTag(SCOPE_EFFECT);// all effects are this scope, effects that would affect other effects need to check for this tag
		}

	}

	public class Event:Conditional
	{
	public Event(float delay=0):base()
		{
			acceptedTypes[_delay]=typeof(float);
			_values[_delay]=delay;
		}
    public float advanceTime(float shift)
		{
			float cdelay=(float)this[_delay];
			if(cdelay<shift) 
			{
				#if THING
				Debug.Log(string.Format("Invalid time shifting at Event {0} of time {1}",this,shift));
				#endif
				shift=cdelay;
			}
			cdelay-=shift;
			this[_delay]=cdelay;
			return shift;
		}
	}

	public class EffectList
	{

	}
}


