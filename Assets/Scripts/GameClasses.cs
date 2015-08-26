#define THING
#define WHATHAPPENS
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
public class SingleGame
{
//Scope tags
	public const string SCOPE_EFFECT="SCOPE_EFFECT";
	public const string SCOPE_TIMELINE="SCOPE_TIMELINE";
	public const string SCOPE_HAND="SCOPE_HAND";
	public const string SCOPE_COMMAND="SCOPE_COMMAND";
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
	public const string _target="_target";
	public const string _value="_value";
	public const string _args="_arguments";
	public const string _effects="_effects";
	public const string _effect="_effect";
	public const string _condition="_condition";
	public const string _commands="_commands";
	public const string _returnValue="_returnValue";
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
	public class Operation:Conditional
	{
		public enum Commands
		{
        TAG_SET,
		TAG_SWITCH,
		VALUE_SET,
			ADD,
			SUBTRACT,
			MULTIPLY,
			DIVIDE,
			ABORT,
			CONTINUE,
			RETURN
		}
		Commands _command;
		public Operation(Commands com)
		{
			_command=com;
		}
		public Conditional createStack(Conditional oldstack=null)
		{
			return null;//TODO
		}
		void __pureExecute(Conditional stack)
		{
			if(stack.hasTag("ABORT")) return;
			Conditional target=stack[_target] as Conditional;
			IList args=this[_args] as IList;
			switch(_command)
			{
			case Commands.TAG_SET:{target.setTag(args[0] as string);}break;
			case Commands.TAG_SWITCH:{
				 if(target.hasTag(args[0] as string))
				 {
					target.removeTag(args[0] as string);
					target.setTag(args[1] as string);
				 }
			      }break;
			case Commands.VALUE_SET:{target[args[0] as string]=args[1];}break;
			case Commands.ADD:{
				object a1=target[args[0] as string];
				object a2=args[1];
				target[args[0] as string]=System.Convert.ChangeType(System.Convert.ToDouble(a1)+System.Convert.ToDouble(a2),a1.GetType());}break;
			case Commands.SUBTRACT:{
				object a1=target[args[0] as string];
				object a2=args[1];
				target[args[0] as string]=System.Convert.ChangeType(System.Convert.ToDouble(a1)-System.Convert.ToDouble(a2),a1.GetType());}break;
			case Commands.MULTIPLY:{
				object a1=target[args[0] as string];
				object a2=args[1];
				target[args[0] as string]=System.Convert.ChangeType(System.Convert.ToDouble(a1)*System.Convert.ToDouble(a2),a1.GetType());}break;
			case Commands.DIVIDE:{
				object a1=target[args[0] as string];
				object a2=args[1];
				target[args[0] as string]=System.Convert.ChangeType(System.Convert.ToDouble(a1)/System.Convert.ToDouble(a2),a1.GetType());}break;
			case Commands.ABORT:
			{
				stack.setTag("ABORT");
			}break;
			case Commands.CONTINUE:
			{
				stack.setTag("CONTINUE");
			}break;
			case Commands.RETURN:
			{
				stack.setTag("RETURN");
				stack[_returnValue]=args[0];
			}break;
			};
		}
		Operation executeList(object lst, Conditional stack)
		{
			if(!(lst is IList))
			{
				#if THING
				Debug.Log("Invalid command list");
				#endif
				return null;
			}
		}
		void iterateOverEffects(IList efs,Conditional stack,string tag)
		{
			if(stack.hasTag("ABORT")) return;
			foreach(object o in efs)
			{
				Conditional effect=o as Conditional;
				if(effect!=null&&effect.hasTag(tag)&&!effect.hasTag("ACTIVATED"))
				{
					effect=effect[_effect] as Conditional;
					Condition cn=effect[_condition] as Condition;
					if(cn==null)
					{
						Debug.Log(string.Format("Invalid effect condition {0}",effect[_condition]));
					}
					else
					{
						if(cn.isFulfilled(stack)) //execute effect
						{
							Operation ret= executeList(effect[_commands],stack);
							if(ret!=null)
							 ret.__pureExecute(stack);
							if(stack.hasTag("ABORT")) return;
						}
					}
					
				}
				else
				{
					Debug.Log(string.Format("Invalid effect {0}",o));
				}
			}
		}
		void _execute(Conditional stack)
		{
			IList efs=stack[_effects] as IList;
			if(efs==null)
			{
         #if THING
				Debug.Log("Possibly invalid setting of _effects in stack");
         #endif
			}
			else
			{
				stack.setTag("EXECUTE_PREFIX");
				iterateOverEffects(efs,stack,"EXECUTE_PREFIX");
				stack.removeTag("EXECUTE_PREFIX");
			}
				__pureExecute(stack);
			if(efs!=null)
			{
				stack.setTag("EXECUTE_POSTFIX");
				iterateOverEffects(efs,stack,"EXECUTE_POSTFIX");
				stack.removeTag("EXECUTE_POSTFIX");
			}

			
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
		public string [] variables; //variable name to compare OR tag
		//public string variable_compound;
		public object [] values;//value(s) to compare to
		public Condition()
		{
			inverse=false;
		}
		public Condition(Type t, string  [] var, params object [] val)
		{
			inverse=false;
			type=t;
			variables= var;
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
			string variable="";
			if(variables.Length==0) return false;
			if(variables.Length>1)
			{
				string [] deeper=new string[variables.Length-1];
				for(int i=1;i<variables.Length;i++)
					deeper[i-1]=variables[i];
				string cvar=variables[0];
				Conditional cv=cnd[cvar] as Conditional;
				if(cv==null)
					return false;
				Condition deepcond=new Condition(type,deeper,values);
				return deepcond.__isFulfilled(cv);
			}
			else
				variable=variables[0];

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
	public class Parser
	{
		static string _text;
		static int rpos=0;
		enum types
		{
			CONDITION,
			CONDITIONAL,
			FUNCTION,
			INT,
			FLOAT, 
			STRING
		}
		public static readonly string[] BaseTypes = { "condition", "conditional", "function", "int","float","string" };
		public static Dictionary<string,object> _context=new Dictionary<string, object>();
		public static void ParseIntoContext(string txt)
		{
			_text=txt;
			rpos=0;
		}
		public static string readString(string _txt,ref int pos,out bool res)
		{
			//skip whitespace
			while(pos<_txt.Length&&char.IsWhiteSpace(_txt[pos])) pos++;
			res=true;
			if(pos==_txt.Length) 
			{
                #if THING
				Debug.LogWarning("no string encountered until end of file");
				#endif
				res=false;
				return null;
			}
			StringBuilder ret=new StringBuilder();
			if(_txt[pos]=='{')
			{
				res=true;
				pos++;
				return "{";//opening brace
			}
			if(_txt[pos]=='"') //opening quote
			{
				pos++;
				while(pos<_txt.Length&&_txt[pos]!='"')
				{
					if(_txt[pos]!='\\')
					ret.Append(_txt[pos]);
					else
					{
						pos++;
						if(pos<_txt.Length) ret.Append(_txt[pos]);
					}
					pos++;
				}
				if(pos==_txt.Length)
				{
					#if THING
					Debug.LogWarning("no end quote encountered");
					#endif
					res=false;
					return null;
				}
				pos++;
				return ret.ToString();
			} // end quote
			while(pos<_txt.Length&&!char.IsWhiteSpace(_txt[pos])&&_txt[pos]!='('&&_txt[pos]!='{') 
			{
				ret.Append(_txt[pos]);
				pos++;
			}
			return ret.ToString();
		}

		public static string readParameter(string _txt,ref int pos,out bool res)
		{
			//skip whitespace
			while(pos<_txt.Length&&char.IsWhiteSpace(_txt[pos])) pos++;
			res=true;
			if(pos==_txt.Length) 
			{
				#if THING
				Debug.LogWarning("no string encountered until end of file in Parameter read");
				#endif
				res=false;
				return null;
			}
			StringBuilder ret=new StringBuilder();
			if(_txt[pos]==')')
			 {
				#if THING
				Debug.Log("Parameter expected, but ) encountered - might be OK");
				#endif
				res=false;
				return null;
			 }
			if(_txt[pos]==',')
			{
				#if THING
				Debug.Log("Parameter expected, but , encountered - parameter assumed empty");
				#endif
				res=true;
				return "";
			}
			if(_txt[pos]=='"') //opening quote
			{
				pos++;
				while(pos<_txt.Length&&_txt[pos]!='"')
				{
					if(_txt[pos]!='\\')
						ret.Append(_txt[pos]);
					else
					{
						pos++;
						if(pos<_txt.Length) ret.Append(_txt[pos]);
					}
					pos++;
				}
				if(pos==_txt.Length)
				{
					#if THING
					Debug.LogWarning("no end quote encountered");
					#endif
					res=false;
					return null;
				}
				pos++;

			}// end quote
			else
			{
				while(pos<_txt.Length&&!char.IsWhiteSpace(_txt[pos])&&_txt[pos]!=','&&_txt[pos]!=')') 
				{
					ret.Append(_txt[pos]);
					pos++;
				}
			}


			while(pos<_txt.Length&&char.IsWhiteSpace(_txt[pos])) pos++;
			if(_txt[pos]!=','&&_txt[pos]!=')')
			{
				#if THING
				Debug.LogWarning(string.Format("Invalid syntax in string at pos: {0} - should be ) or , but is {1}",pos,_txt[pos]));
				#endif
				
				res=false;
				return ret.ToString();
			}
			if(_txt[pos]==',')
				pos++;
			return ret.ToString();


		}

		public static T readTypeCast<T>(string _txt,ref int pos,out bool res)
		{
			bool strd=false;
			string rstr=readString(_txt,ref pos,out strd);
			if(!strd)
			{
				res=false;
				return default(T);
			}
			T ret=default(T);
			try
			{
				ret=(T)System.ComponentModel.TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(rstr);
				res=true;
			}
			catch
			{
				res=false;
			}
			
			return (T)ret;

		}
		/*public static object getPrototypeFromString(string tp)
		{
			if(tp=="condition") return new Condition();
			if(tp
		}*/
		public class Parametric //hmm...
		{
			public object Resolve(List<object> pars)
			{
			}
		}
		public static object readDefinition(string _txt,ref int pos,out bool res)
		{
			bool result=false;
			object ret=null;
			string type=readString(_txt,ref pos,out result);
			if(!result)
			{
				#if THING
				Debug.LogWarning(string.Format("Cannot read definition in string at pos: {0} - type missing",pos));
				#endif
				res=false;
				return null;
			}
			if(System.Array.IndexOf(BaseTypes,type)==-1&&!_context.ContainsKey(type))
			{
				#if THING
				Debug.LogWarning(string.Format("Cannot read definition in string at pos: {0} - invalid type {1}",pos,type));
				#endif
				res=false;
				return null;
			}
			string name=readString(_txt,ref pos,out result);
			if(!result)
			{
				#if THING
				Debug.LogWarning(string.Format("Cannot read definition in string at  end pos: {0} - invalid name",pos));
				#endif
				res=false;
				return null;
			}
			string def=readString(_txt,ref pos,out result);
			bool parametricDefinition=false;
			List<string> pars=new List<string>();
			if(def[0]=='(')//parameters
			{
				pos=pos-def.Length+1;
				bool parread=false;

				string par=readParameter(_txt,ref pos,out parread);
				while(parread&&_txt[pos]!=')')
				{
					pars.Add(par);
					par=readParameter(_txt,ref pos,out parread);
				}
				if(_txt[pos]!=')')
				{
					#if THING
					Debug.LogWarning(string.Format("Cannot read definition in string at  end pos: {0} - invalid parameter definition",pos));
					#endif
					res=false;
					return null;
				}
				parametricDefinition=true;
				def=readString(_txt,ref pos,out result);
			}
			if(def[0]!='{') //alias
			{
				if(!_context.ContainsKey(def))
				{
					#if THING
					Debug.LogWarning(string.Format("Cannot read definition in string at  end pos: {0} - unknown alias {1}",pos,def));
					#endif
					res=false;
					return null;
				}
				object alias=_context[def];
				if(alias is Parametric)
				{
					def=readString(_txt,ref pos,out result);
					if(!result||def[0]!='(')
					{
						#if THING
						Debug.LogWarning(string.Format("Cannot read definition in string at  end pos: {0} - parametric alias without parameters",pos));
						#endif
						res=false;
						return null;
					}
					pos=pos-def.Length+1;

				}
				else
				{
					ret=alias;
				}
				def=readString(_txt,ref pos,out result);
			}
			if(System.Array.IndexOf(BaseTypes,type)!=-1)
			{
				//it is one of the base types. Switch.

			}
			return null;//TODO
		}

	}
}


