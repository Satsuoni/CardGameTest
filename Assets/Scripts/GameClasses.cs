#define THING
#define WHATHAPPENS
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
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
//helper tags
	public const string EXECUTE_PREFIX="EXECUTE_PREFIX";
	public const string EXECUTE_POSTFIX="EXECUTE_POSTFIX";
	public const string TAG_ABORT="ABORT";
	public const string TAG_STACKED="STACKED";
	public const string TAG_ACTIVATED="ACTIVATED";
	public const string TAG_RETURN="RETURN";
	public const string TAG_CONTINUE="CONTINUE";
	public const string TAG_SUPPRESSED="SUPPRESSED";
// "variable" names
	public const string ownID="ownerID";
	public const string _count="_count";
	public const string _delay="_delay"; //timeline delay for Event
	public const string _target="_target";
	public const string _targetList="_targetList";
	public const string _value="_value";
	public const string _args="_arguments";
	public const string _effects="_effects";
	public const string _effect="_effect";
	public const string _parent="_parent";
	public const string _condition="_condition";
	public const string _commands="_commands";
	public const string _currentCommand="_currentCommand";
	public const string _returnValue="_returnValue";
	public const string _Game="_GAME";
	public const string _Source="_Source";
	public static readonly string[] __stackValues={_Game};
	public const string _template="_template";
	public const string _cardName="_cardName";
	public const string _cardText="_cardText";
//helper dictionaries
	public static Dictionary<string,List<object>> acceptedValues=new Dictionary<string, List<object>>();
	public static Dictionary<string,System.Type> acceptedTypes=new Dictionary<string, System.Type>();
//hooks
	delegate void uiHook(params object[] parameters);
//classes
	public static System.Random RNG=new System.Random();
	static object rnglock=new object();
	public static int rngRange(int from, int to)
	{
		int ret;
		lock (rnglock) {
			ret=RNG.Next(from,to);
		}
		#if WHATHAPPENS
		//Debug.Log(string.Format("rnd range: {0} - {1} : {2}",from,to,ret));
#endif
		return ret;
	}
	public static string getRandString(int length, float spaceprob=0)
	{
		var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
		var stringChars = new char[length];

		for (int i = 0; i < stringChars.Length; i++)
		{
			if(Random.value>spaceprob)
			{
				stringChars[i] = chars[rngRange(0,chars.Length)];
			}
			else
				stringChars[i]=' ';
		}
		
		return new string(stringChars);
	}
	public static Conditional generateRandomCardTemplate()
	{
		Conditional ret=new Conditional();
		ret[_cardName]=getRandString(5,0);
		ret[_cardText]=getRandString(25,0.1f);
		return ret;
	}
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
		public object this[string name] //let us add the dot notation...
		{
			get{
				string [] ln=name.Split('.');
				if(ln.Length==1)
				 {
				object ret=null;
				if(_values.TryGetValue(ln[0],out ret)) return ret;
				else return null;
				 }
				else
				{
					object ret=null;
					if(_values.TryGetValue(ln[0],out ret)) 
					{
						Conditional nxt=ret as Conditional;
						return nxt[name.Substring(ln[0].Length+1)];//hopefully...
					}
					else return null;
				}
			   }
			set
			{
				string [] ln=name.Split('.');
				if(ln.Length==1)
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
				else
				{
					object ret=null;
					if(_values.TryGetValue(ln[0],out ret)) 
					{
						Conditional nxt=ret as Conditional;
						nxt[name.Substring(ln[0].Length+1)]=value;//hopefully...
					}
				}
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
	public class GameManager
	{

		public Conditional _GameData;
		object locket=new object();
		Thread gameThread;
		EventWaitHandle _waitHandle ;
		static GameManager self=null;

		bool _choiceInProgress=false;

		public bool choiceInProgress
		{
			get {
				return _choiceInProgress;
			}
		}

		bool _hookInProgress=false;

		public bool isWaiting
		{
			get {
				return _choiceInProgress||_hookInProgress;
			}
		}

		public bool hookInProgress
		{
			get{return _hookInProgress;}
		}

		Conditional _hookData;
		string _hookName=null;
		public string hookName{ get{return _hookName;}
		}

		public Conditional hookData
		{
			get{return _hookData;}
		}

		Conditional chosen=null;
		IList _choiceList;
		string _choiceName;
		public string choiceName{ get{return _choiceName;}
		}
		public IList choiceList
		{
			get{return _choiceList;}
		}

		void mainGameThread()
		{
			IList rulesAndEffects=_GameData[_effects] as IList;
		   Conditional stack=new Conditional();
			stack[_Game]=_GameData;
			List<Conditional> wrappedEffects=new List<Conditional>();
			foreach(object obj in rulesAndEffects)
			{
				Conditional eff=obj as Conditional;
				if(eff==null)
				{
					#if THING
					Debug.Log(string.Format("Invalid effect description: {0}",obj));
					#endif
				}
				else
				{
					Conditional wrap=new Conditional();
					wrap[_effect]=eff;
					wrappedEffects.Add(wrap);
				}
			}
			stack[_effects]=wrappedEffects;
			stack[_target]=null;


			while(true)
			{
				foreach(object obj in rulesAndEffects)
				{
					Conditional eff=obj as Conditional;
					if(!eff.hasTag(EXECUTE_PREFIX)&&!eff.hasTag(EXECUTE_POSTFIX))
					{
						Condition cnd=eff[_condition] as Condition;

						if(cnd.isFulfilled(stack))
						{
							Operation op=new Operation(Operation.Commands.NEW);
							Conditional nstack=op.createStack(stack,eff);
							op.executeList(eff[_commands],nstack);
							if(nstack.hasTag(TAG_ABORT)) 
							{
								Debug.Log("GameObject Overlapped");
								return;//gameover? I guess
							}
						}
					}
				}
			}
		}

		public static Conditional startChoice(string chname,IList objects)
		{
			if(self==null||Thread.CurrentThread!=self.gameThread)
			{
				#if THING
				Debug.Log(string.Format("Invalid thread calling choice: {0}",Thread.CurrentThread));
				#endif
				return null;
			}
			if(self._choiceInProgress)
			{
				#if THING
				Debug.Log(string.Format("Choice called for the second time -wtf?: {0}",Thread.CurrentThread));
				#endif
				return null;
			}
        lock(self.locket)
			{
				self._choiceInProgress=true;
				self._choiceList=objects;
				self._choiceName=chname;
			}
			self._waitHandle.WaitOne();
			Conditional ret=null;
			lock(self.locket)
			{
				self._choiceInProgress=false;
				self._choiceList=null;
				ret=self.chosen;
			}
			return ret;
		}
		public static void endChoice(Conditional retValue)
		{
			if(self==null||Thread.CurrentThread==self.gameThread)
			{
				#if THING
				Debug.Log(string.Format("Invalid thread calling choice end: {0}",Thread.CurrentThread));
				#endif
				return;
			}
			if(!self._choiceInProgress)
			{
				#if THING
				Debug.Log(string.Format("Choice called when it was not asked for"));
				#endif
				return;
			}
			lock(self.locket)
			{
				self._choiceInProgress=false;
				self._choiceName=null;
				self.chosen=retValue;
			}
			self._waitHandle.Set();
		}
		public static void startHook(string hName,Conditional hookData)
		{
			if(self==null||Thread.CurrentThread!=self.gameThread)
			{
				#if THING
				Debug.Log(string.Format("Invalid thread calling hook start: {0}",Thread.CurrentThread));
				#endif
				return;
			}
			if(self._hookInProgress)
			{
				#if THING
				Debug.Log(string.Format("Hook called for the second time -wtf?: {0}",Thread.CurrentThread));
				#endif
				return ;
			}
			lock(self.locket)
			{
				self._hookInProgress=true;
				self._hookData=hookData;
				self._hookName=hName;
			}
			self._waitHandle.WaitOne();

			lock(self.locket)
			{
				self._hookInProgress=false;
				self._hookData=null;

			}
		}
		public static void endHook()
		{
			if(self==null||Thread.CurrentThread==self.gameThread)
			{
				#if THING
				Debug.Log(string.Format("Invalid thread calling hook end: {0}",Thread.CurrentThread));
				#endif
				return;
			}
			if(!self._hookInProgress)
			{
				#if THING
				Debug.Log(string.Format("Hook end called when it was not asked for"));
				#endif
				return;
			}
			self._waitHandle.Set();
		}
		public void fillGameData()
		 {
			_GameData=new Conditional(); //a test, for now..
			List<Conditional> deck=new List<Conditional>();
			for(int i=0;i<30;i++)
				deck.Add(generateRandomCardTemplate());
			_GameData["DECK"]=deck;
			_GameData["HAND"]=new List<Conditional>();
			List<Conditional> effs=new List<Conditional>();
			Conditional drawRule=new Conditional();
			Condition compareto7=new Condition(Condition.Type.LESS,_count,7);
			Condition count=new Condition(Condition.Type.COMPOUND_COUNT,_Game+".HAND",new Condition(Condition.Type.TRUE,""),compareto7);

			drawRule[_condition]=count;
			List<Operation> seq=new List<Operation>();
			Operation a1=new Operation(Operation.Commands.ANY);
			List<object> arg1=new List<object>();
			arg1.Add(_Game+".DECK");
			arg1.Add("_tmp");
			a1[_args]=arg1;
			seq.Add(a1);

			a1=new Operation(Operation.Commands.REMOVE);
			arg1=new List<object>();
			arg1.Add(_Game+".DECK");
			arg1.Add("_tmp");
			a1[_args]=arg1;
			seq.Add(a1);

			a1=new Operation(Operation.Commands.HOOK);
			arg1=new List<object>();
			arg1.Add("draw");
			arg1.Add("_tmp");
			a1[_args]=arg1;
			seq.Add(a1);

			a1=new Operation(Operation.Commands.PUSH);
			arg1=new List<object>();
			arg1.Add(_Game+".HAND");
			arg1.Add("_tmp");
			a1[_args]=arg1;
			seq.Add(a1);

			drawRule[_commands]=seq;
			effs.Add(drawRule);
			_GameData[_effects]=effs;
		 }
		public void Start()
		{
			if(self!=null)
			{
				#if THING
				Debug.Log(string.Format("GameManager not cleared properly...."));
				#endif
			}
			fillGameData();
			self=this;
			gameThread=new Thread(mainGameThread);
			_waitHandle =new AutoResetEvent(false);
			gameThread.Start();
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
			RETURN,
			FOREACH, //looks like foreach and target would have to be separate
			TARGET,
			ACCUMULATE, //like target, but for arbitrary list
			CLEAR, //sets argument to null
			HOOK, //calls hook on the main thread
			CHOICE, //calls for choice
			NEW, //makes new conditional of name on stack
			///list operations
			POP, //get fist and delete
			PUSH, //put first
			APPEND, //put last
			SHIFT,//get last and delete
			REMOVE,
			ANY, //get any in list without deleting
		}
		Commands _command;
		public Operation(Commands com)
		{
			_command=com;
		}
		public Conditional createStack(Conditional oldstack,Conditional exeffect)
		{
			Conditional ret=new Conditional();
			List <Conditional> efs=new List<Conditional>();
			if(oldstack[_effects]==null)
				ret[_effects]=null;
			else
			{
				IList elist=oldstack[_effects] as IList;
				foreach(object ef in elist)
				{
					Conditional eff=ef as Conditional;
					Conditional efContain=new Conditional();
					if(eff.hasTag(TAG_STACKED))
						efContain.setTag(TAG_STACKED);
					if(eff==exeffect)
					{
						Debug.Log(string.Format("Stacking: {0}",ef));
						efContain.setTag(TAG_STACKED);
					}
					efContain[_effect]=eff[_effect];
				}
				ret[_effects]=efs;
			}
			ret[_target]=this;//current command becomes a target?
			ret[_currentCommand]=null;
			foreach(string nm in __stackValues)
			  ret[nm]=oldstack[nm];
			ret[_Source]=exeffect[_effect];
			return ret;//TOFIX
		}
		static public object deRef(object a, Conditional stack)
		{
			if(a is string &&(a as string).StartsWith("<="))
			{
				return stack[(a as string).Substring(2)];
			}
			return a;
		}
		void  __pureExecute(Conditional stack)
		{
			if(stack.hasTag(TAG_ABORT)) return;
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
			case Commands.VALUE_SET:{if(args[1]is string &&(args[1] as string).StartsWith("<="))
				{
					target[args[0] as string]=stack[(args[1] as string).Substring(2)];
				}
				else
				target[args[0] as string]=args[1];}break;
			case Commands.ADD:{
				object a1=target[args[0] as string];
				object a2=deRef( args[1],stack);
				target[args[0] as string]=System.Convert.ChangeType(System.Convert.ToDouble(a1)+System.Convert.ToDouble(a2),a1.GetType());}break;
			case Commands.SUBTRACT:{
				object a1=target[args[0] as string];
				object a2=deRef( args[1],stack);
				target[args[0] as string]=System.Convert.ChangeType(System.Convert.ToDouble(a1)-System.Convert.ToDouble(a2),a1.GetType());}break;
			case Commands.MULTIPLY:{
				object a1=target[args[0] as string];
				object a2=deRef( args[1],stack);
				target[args[0] as string]=System.Convert.ChangeType(System.Convert.ToDouble(a1)*System.Convert.ToDouble(a2),a1.GetType());}break;
			case Commands.DIVIDE:{
				object a1=target[args[0] as string];
				object a2=deRef( args[1],stack);
				target[args[0] as string]=System.Convert.ChangeType(System.Convert.ToDouble(a1)/System.Convert.ToDouble(a2),a1.GetType());}break;
			case Commands.ABORT:
			{
				stack.setTag(TAG_ABORT);
			}break;
			case Commands.CONTINUE:
			{
				stack.setTag(TAG_CONTINUE);
			}break;
			case Commands.NEW:
			{
				string nm=args[0] as string;
				if(stack[nm] as Conditional==null)
					stack[nm]=new Conditional();
			}break;
			case Commands.RETURN: //argument :  returned command/operation
			{
				stack.setTag(TAG_RETURN);
				stack[_returnValue]=args[0] as Operation;
			}break;
			case Commands.FOREACH:// arguments: ...none? XD I guess list name would work. arg[0]-> list name in stack args[1]->list of commands
			{
				string lname=args[0] as string;
				if(stack[lname]!=null)
				{
					IList lst=stack[lname] as IList;
					if(lst!=null)
					{
						foreach(object pcn in lst)
						{
							Conditional newtarget=pcn as Conditional;
							if(newtarget==null)
							{
								#if THING
								Debug.Log("Invalid target in foreach list");
								#endif
							}
							else
							{
								stack[_target]=newtarget;
								executeList(args[1],stack);
								if(stack.hasTag(TAG_ABORT)) return;
								stack.removeTag(TAG_CONTINUE);
							}
						}
					}
					else
					{
						#if THING
						Debug.Log(string.Format("Invalid foreach argument: {0}",lname));
						#endif
					}
				}
			}break;
				case Commands.TARGET://will assign _targetList? value in stack arg0 - condition, arg1 - list, equivalent of accumulate for specific list
				{
					List<Conditional> targs=new List<Conditional>();
					Condition baseCond=args[0] as Condition;
					if(baseCond==null)
					{
						#if THING
						Debug.Log(string.Format("Invalid target condition : {0}",args[0]));
						#endif
						return;
					}
					IList vars=stack[args[1] as string] as IList;
					if(vars==null)
					{
						#if THING
						Debug.Log(string.Format("Invalid target list : {0}",args[1]));
						#endif
						return;
					}
					foreach(object potCn in vars)
					{
						Conditional check=potCn as Conditional;
						if(baseCond.isFulfilled(check)) targs.Add(check);
					}
					stack[_targetList]=targs;
				};break;
			case Commands.ACCUMULATE://will assign arg2 value in stack arg0 - condition, arg1 - list. appends to list if it exists
			{
				List<Conditional> targs=new List<Conditional>();
				string listname=args[2] as string;
				IList oldList=stack[listname] as IList;
				if(oldList!=null)
				{
					foreach(object l in oldList) targs.Add(l as Conditional);
				}
				Condition baseCond=args[0] as Condition;
				if(baseCond==null)
				{
					#if THING
					Debug.Log(string.Format("Invalid target condition : {0}",args[0]));
					#endif
					return;
				}
				IList vars=stack[args[1] as string] as IList;
				if(vars==null)
				{
					#if THING
					Debug.Log(string.Format("Invalid target list : {0}",args[1]));
					#endif
					return;
				}
				foreach(object potCn in vars)
				{
					Conditional check=potCn as Conditional;
					if(baseCond.isFulfilled(check)) targs.Add(check);
				}
				stack[listname]=targs;
			};break;
			case Commands.CLEAR:
			{
				string nm=args[0] as string;
				if(nm!=null)
					stack[nm]=null;
			};break;
			case Commands.POP: //arg0 - list name, arg1- stackvar
			{
				string nm=args[0] as string;
				IList lst=stack[nm] as IList;
				string retname=args[1] as string;
				if(lst!=null&&retname!=null)
				{
					if(lst.Count>0)
					{
						stack[retname]=lst[0];
						lst.RemoveAt(0);
					}
				}
			};break;
			case Commands.PUSH: //arg0 - list name, arg1- stackvar
			{
				string nm=args[0] as string;
				IList lst=stack[nm] as IList;
				string retname=args[1] as string;
				if(lst==null) {lst=new List<object>(); stack[nm]=lst;} //should be avoided...
				if(lst!=null&&retname!=null&&stack[retname]!=null)
				{
					lst.Insert(0,stack[retname]);
				}
			};break;
			case Commands.SHIFT: //arg0 - list name, arg1- stackvar
			{
				string nm=args[0] as string;
				IList lst=stack[nm] as IList;
				string retname=args[1] as string;
				if(lst!=null&&retname!=null)
				{
					if(lst.Count>0)
					{
						stack[retname]=lst[lst.Count-1];
						lst.RemoveAt(lst.Count-1);
					}
				}
			};break;
			case Commands.APPEND: //arg0 - list name, arg1- stackvar
			{
				string nm=args[0] as string;
				IList lst=stack[nm] as IList;
				string retname=args[1] as string;
				if(lst==null) {lst=new List<object>(); stack[nm]=lst;} //should be avoided...
				if(lst!=null&&retname!=null&&stack[retname]!=null)
				{
					lst.Add(stack[retname]);
				}
			};break;
			case Commands.REMOVE: // removes object from listarg0 - list name, arg1- stackvar
			{
				string nm=args[0] as string;
				IList lst=stack[nm] as IList;
				string retname=args[1] as string;
				if(lst==null) {lst=new List<object>(); stack[nm]=lst;} //should be avoided...
				if(lst!=null&&retname!=null&&stack[retname]!=null)
				{
					lst.Remove(stack[retname]);
				}
			};break;
			case Commands.ANY: //get any (random) in list without deleting arg0 - list name, arg1- stackvar
			{
				string nm=args[0] as string;
				IList lst=stack[nm] as IList;
				string retname=args[1] as string;
				if(lst!=null&&retname!=null)
				{
					if(lst.Count>0)
					{

						stack[retname]=lst[rngRange(0,lst.Count)];

					}
				}
			};break;
			case Commands.HOOK: //call hook of name with data
			{
				string nm=args[0] as string;
				string nmData=args[1] as string;
				GameManager.startHook(nm,stack[nmData] as Conditional);
			};break;
			case Commands.CHOICE: //call for player choice, store result  in arg2: 
			{
				string chname=args[0] as string;
				string nm=args[1] as string;
				IList lst=stack[nm] as IList;
				if(lst==null)
				{
					#if THING
					Debug.Log(string.Format("Invalid choice list : {0}",nm));
					#endif
					return;
				}
				string nmRet=args[2] as string;
				Conditional ret=GameManager.startChoice(chname,lst);
				stack[nmRet]=ret;
			};break;

			};

		}
		public Operation executeList(object lst, Conditional stack) //has *new* stack as parameter
		{
			if(!(lst is IList))
			{
				#if THING
				Debug.Log("Invalid command list");
				#endif
				return null;
			}
			IList list=lst as IList;
			foreach(object obj in list)
			{
				Operation op = obj as Operation;
				if(op==null)
				{
					#if THING
					Debug.Log(string.Format("Invalid command: {0}",obj));
					#endif
					return null;
				}
				op._execute(stack);
				if(stack.hasTag(TAG_RETURN))
					return stack[_returnValue] as Operation;
				if(stack.hasTag(TAG_ABORT))
					return null;
				//newstack[_command]
			}
			return null;
		}
		void iterateOverEffects(IList efs,Conditional stack,string tag)
		{
			if(stack.hasTag(TAG_ABORT)) return;
			bool didActivate=true;
				foreach(object o in efs)
				{
					Conditional preffect=o as Conditional;
					if(preffect!=null)
						preffect.removeTag(TAG_ACTIVATED);
				}
			while(didActivate)
				{
					didActivate=false;
			foreach(object o in efs)
			{
				Conditional preffect=o as Conditional;
				Conditional effect=null;
				if(preffect!=null&&!preffect.hasTag(TAG_ACTIVATED)&&!preffect.hasTag(TAG_STACKED))
				{
					effect=preffect[_effect] as Conditional;
					if(effect.hasTag(tag))
					{
					Condition cn=effect[_condition] as Condition;
					if(cn==null)
					{
						Debug.Log(string.Format("Invalid effect condition {0}",effect[_condition]));
					}
					else
					{
						if(cn.isFulfilled(stack)) //execute effect
						{
							preffect.setTag(TAG_ACTIVATED);
										didActivate=true;
								Operation ret= executeList(effect[_commands],createStack(stack,effect));
							if(ret!=null)
							 ret.__pureExecute(stack);
							if(stack.hasTag(TAG_ABORT)) return;
						}
					}

					}

					
				}
				else
				{
					Debug.Log(string.Format("Invalid effect {0}",o));
				}
			}
				}
		}
		public void _execute(Conditional stack)
		{
			stack[_currentCommand]=this;
			IList efs=stack[_effects] as IList;
			if(efs==null)
			{
         #if THING
				Debug.Log("Possibly invalid setting of _effects in stack");
         #endif
			}
			else
			{
				stack.setTag(EXECUTE_PREFIX);
				iterateOverEffects(efs,stack,EXECUTE_PREFIX);
				stack.removeTag(EXECUTE_PREFIX);
				if(stack.hasTag(TAG_ABORT)) return;
			}
				__pureExecute(stack);
			if(stack.hasTag(TAG_ABORT)) return;
			if(efs!=null)
			{
								stack.setTag(EXECUTE_POSTFIX);
				iterateOverEffects(efs,stack,EXECUTE_POSTFIX);
				stack.removeTag(EXECUTE_POSTFIX);
				if(stack.hasTag(TAG_ABORT)) return;
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
			GE,
		    TRUE
		}
		public bool inverse;
		public Type type;
		//public Type type_compound;//type to use for compound conditions
		public string  variables; //variable name to compare OR tag
		//public string variable_compound;
		public object [] values;//value(s) to compare to
		public Condition()
		{
			inverse=false;
		}
		public Condition(Type t, string   var, params object [] val)
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
					if(type==Type.TRUE) return true;
			string variable=variables;
			/*if(variables.Length==0) return false;
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
				variable=variables[0];*/

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
					temp[_parent]=cnd;
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
			object ncmpval=values[0];
			if(values[0] is string)// try loading it as cnd name?
			{
				string st=values[0] as string;
				if(st!=null&&st.StartsWith("<="))
				{
					string vname=st.Substring(2);
					ncmpval=cnd[vname];
				}
			}
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
				System.Int64 cndval=System.Convert.ToInt64(ncmpval);
				System.Int64 cmpval=System.Convert.ToInt64(val);
				cmp=cmpval.CompareTo(cndval);
			};break;
			default:{
				System.Double cndval=System.Convert.ToDouble(ncmpval);
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
			if(_txt[pos]=='}')
			{
				res=true;
				pos++;
				return "}";//closing brace
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
		// Let's avoid parametrics for now...
		/*public class Parametric //hmm...
		{
			public object Resolve(List<object> pars)
			{
				return null;
			}
		}*/
		public static Condition readCondition(string _txt,ref int pos,out bool res) //after condition keyword, though not always?
		{
			bool result;
			string vname=null;
			string first=readString(_txt,ref pos,out result);
			if(!result)
			{
				#if THING
				Debug.LogWarning(string.Format("Cannot read condition in string at pos: {0} ",pos));
				#endif
				res=false;
				return null;
			}
			if(first=="condition")
			{
				first=readString(_txt,ref pos,out result);
				if(!result)
				{
				#if THING
				Debug.LogWarning(string.Format("Cannot read condition in string at pos: {0} ",pos));
				#endif
				res=false;
				return null;
				}
			}
			if(first=="condition")
			{
				#if THING
				Debug.LogWarning(string.Format("Invalid condition name"));
				#endif
				res=false;
				return null;
			}
			if(first!="{")
			{
				vname=first;
				first=readString(_txt,ref pos,out result);
				if(!result)
				{
				#if THING
				Debug.LogWarning(string.Format("Cannot read condition in string at pos: {0} ",pos));
				#endif
				res=false;
				return null;
				}
			}
			Condition ret=null;
			if(first!="{") //alias
			{
				if(_context.ContainsKey(first))
				{
				ret=_context[first] as Condition;
				}
				else
				{
					#if THING
					Debug.LogWarning(string.Format("Invalid condition alias:  {0} ",first));
					#endif
					ret=null;
				}

			}
			else
			{
				string ltype;
				bool invert=false;
				bool tpRead=false;
				while(!tpRead)
				{
				ltype=readString(_txt,ref pos,out result);
				if(!result)
				{
					#if THING
					Debug.LogWarning(string.Format("Cannot read condition type at pos: {0} ",pos));
					#endif
					res=false;
					return null;
				}
				if(ltype=="inverse")
					invert=true;
                else
					{
						switch(ltype)
						{
						case "tag":{
							string tagname=readString(_txt,ref pos,out result);
							if(!result)
							{
								#if THING
								Debug.LogWarning(string.Format("Cannot read condition type at pos: {0} ",pos));
								#endif
								res=false;
								return null;
							}
							ret=new Condition(Condition.Type.TAG,tagname);
						 }break;
						case "true":{
							ret=new Condition(Condition.Type.TRUE,"");
						}break;
						case "strcomp":{
							string varname=readString(_txt,ref pos,out result);
							if(!result)
							{
								#if THING
								Debug.LogWarning(string.Format("Cannot read condition var name at pos: {0} ",pos));
								#endif
								res=false;
								return null;
							}
							string compval=readString(_txt,ref pos,out result);
							if(!result)
							{
								#if THING
								Debug.LogWarning(string.Format("Cannot read condition comp value at pos: {0} ",pos));
								#endif
								res=false;
								return null;
							}
							ret=new Condition(Condition.Type.TAG,varname,compval);
						}break;

						case "any":{
							string listname=readString(_txt,ref pos,out result);
							if(!result)
							{
								#if THING
								Debug.LogWarning(string.Format("Cannot read list name at pos: {0} ",pos));
								#endif
								res=false;
								return null;
							}

							string nxtname=readString(_txt,ref pos,out result);
							if(!result)
							{
								#if THING
								Debug.LogWarning(string.Format("Cannot read condition  at pos: {0} ",pos));
								#endif
								res=false;
								return null;
							}
							Condition tr;
							if(nxtname!="condition")
							{
								if(!_context.ContainsKey(nxtname))
								{
									#if THING
									Debug.LogWarning(string.Format("Invalid condition name:  {0} ",nxtname));
									#endif
									res=false;
									return null;
								}
								tr=_context[nxtname] as Condition;
							}
							else
							{
								tr=readCondition(_txt,ref pos,out result);
								if(!result)
								{
									#if THING
									Debug.LogWarning(string.Format("Invalid condition around:  {0} ",pos));
									#endif
									res=false;
									return null;
								}
							}
							ret=new Condition(Condition.Type.COMPOUND_ANY,listname,tr);
						}break;
									
						case "all":{
							string listname=readString(_txt,ref pos,out result);
							if(!result)
							{
								#if THING
								Debug.LogWarning(string.Format("Cannot read list name at pos: {0} ",pos));
								#endif
								res=false;
								return null;
							}
							
							string nxtname=readString(_txt,ref pos,out result);
							if(!result)
							{
								#if THING
								Debug.LogWarning(string.Format("Cannot read condition  at pos: {0} ",pos));
								#endif
								res=false;
								return null;
							}
							Condition tr;
							if(nxtname!="condition")
							{
								if(!_context.ContainsKey(nxtname))
								{
									#if THING
									Debug.LogWarning(string.Format("Invalid condition name:  {0} ",nxtname));
									#endif
									res=false;
									return null;
								}
								tr=_context[nxtname] as Condition;
							}
							else
							{
								tr=readCondition(_txt,ref pos,out result);
								if(!result)
								{
									#if THING
									Debug.LogWarning(string.Format("Invalid condition around:  {0} ",pos));
									#endif
									res=false;
									return null;
								}
							}
							ret=new Condition(Condition.Type.COMPOUND_ALL,listname,tr);
						}break;
						case "count":{
							string listname=readString(_txt,ref pos,out result);
							if(!result)
							{
								#if THING
								Debug.LogWarning(string.Format("Cannot read list name at pos: {0} ",pos));
								#endif
								res=false;
								return null;
							}
							
							string nxtname=readString(_txt,ref pos,out result);
							if(!result)
							{
								#if THING
								Debug.LogWarning(string.Format("Cannot read condition  at pos: {0} ",pos));
								#endif
								res=false;
								return null;
							}
							Condition tr;
							if(nxtname!="condition")
							{
								if(!_context.ContainsKey(nxtname))
								{
									#if THING
									Debug.LogWarning(string.Format("Invalid condition name:  {0} ",nxtname));
									#endif
									res=false;
									return null;
								}
								tr=_context[nxtname] as Condition;
							}
							else
							{
								tr=readCondition(_txt,ref pos,out result);
								if(!result)
								{
									#if THING
									Debug.LogWarning(string.Format("Invalid condition around:  {0} ",pos));
									#endif
									res=false;
									return null;
								}
							}

							string ccname=readString(_txt,ref pos,out result);
							if(!result)
							{
								#if THING
								Debug.LogWarning(string.Format("Cannot read condition  at pos: {0} ",pos));
								#endif
								res=false;
								return null;
							}
							Condition tr2;
							if(ccname!="condition")
							{
								if(!_context.ContainsKey(ccname))
								{
									#if THING
									Debug.LogWarning(string.Format("Invalid condition name:  {0} ",ccname));
									#endif
									res=false;
									return null;
								}
								tr2=_context[ccname] as Condition;
							}
							else
							{
								tr2=readCondition(_txt,ref pos,out result);
								if(!result)
								{
									#if THING
									Debug.LogWarning(string.Format("Invalid condition around:  {0} ",pos));
									#endif
									res=false;
									return null;
								}
							}
							ret=new Condition(Condition.Type.COMPOUND_COUNT,listname,tr,tr2);
						}break;
						case "less":{
							string varname=readString(_txt,ref pos,out result);
							if(!result)
							{
								#if THING
								Debug.LogWarning(string.Format("Cannot read var type at pos: {0} ",pos));
								#endif
								res=false;
								return null;
							}
							string cmpval=readString(_txt,ref pos,out result);
							if(!result)
							{
								#if THING
								Debug.LogWarning(string.Format("Cannot read compare value  at pos: {0} ",pos));
								#endif
								res=false;
								return null;
							}
							ret=new Condition(Condition.Type.LESS,varname,cmpval);

						}break;

						case "equal":{
							string varname=readString(_txt,ref pos,out result);
							if(!result)
							{
								#if THING
								Debug.LogWarning(string.Format("Cannot read var type at pos: {0} ",pos));
								#endif
								res=false;
								return null;
							}
							string cmpval=readString(_txt,ref pos,out result);
							if(!result)
							{
								#if THING
								Debug.LogWarning(string.Format("Cannot read compare value  at pos: {0} ",pos));
								#endif
								res=false;
								return null;
							}
							ret=new Condition(Condition.Type.EQUAL,varname,cmpval);
							
						}break;
						case "greater":{
							string varname=readString(_txt,ref pos,out result);
							if(!result)
							{
								#if THING
								Debug.LogWarning(string.Format("Cannot read var type at pos: {0} ",pos));
								#endif
								res=false;
								return null;
							}
							string cmpval=readString(_txt,ref pos,out result);
							if(!result)
							{
								#if THING
								Debug.LogWarning(string.Format("Cannot read compare value  at pos: {0} ",pos));
								#endif
								res=false;
								return null;
							}
							ret=new Condition(Condition.Type.GREATER,varname,cmpval);
							
						}break;
						case "ge":{
							string varname=readString(_txt,ref pos,out result);
							if(!result)
							{
								#if THING
								Debug.LogWarning(string.Format("Cannot read var type at pos: {0} ",pos));
								#endif
								res=false;
								return null;
							}
							string cmpval=readString(_txt,ref pos,out result);
							if(!result)
							{
								#if THING
								Debug.LogWarning(string.Format("Cannot read compare value  at pos: {0} ",pos));
								#endif
								res=false;
								return null;
							}
							ret=new Condition(Condition.Type.GE,varname,cmpval);
							
						}break;
						case "le":{
							string varname=readString(_txt,ref pos,out result);
							if(!result)
							{
								#if THING
								Debug.LogWarning(string.Format("Cannot read var type at pos: {0} ",pos));
								#endif
								res=false;
								return null;
							}
							string cmpval=readString(_txt,ref pos,out result);
							if(!result)
							{
								#if THING
								Debug.LogWarning(string.Format("Cannot read compare value  at pos: {0} ",pos));
								#endif
								res=false;
								return null;
							}
							ret=new Condition(Condition.Type.LE,varname,cmpval);
							
						}break;

						case "and":{
							string bracket=readString(_txt,ref pos,out result);
							if(!result||bracket!="{")
							{
								#if THING
								Debug.LogWarning(string.Format("Cannot find bracket { at pos {0} ",pos));
								#endif
								res=false;
								return null;
							}
							List<Condition> lst=new List<Condition>();
							string nxtpos=readString(_txt,ref pos,out result);
							while(nxtpos!="}")
							{
								Condition tr;
								if(nxtpos!="condition")
								{
									if(!_context.ContainsKey(nxtpos))
									{
										#if THING
										Debug.LogWarning(string.Format("Invalid condition name:  {0} ",nxtpos));
										#endif
										res=false;
										return null;
									}
									tr=_context[nxtpos] as Condition;
								}
								else
								{
									tr=readCondition(_txt,ref pos,out result);
									if(!result)
									{
										#if THING
										Debug.LogWarning(string.Format("Invalid condition around:  {0} ",pos));
										#endif
										res=false;
										return null;
									}
								}
								lst.Add(tr);
								nxtpos=readString(_txt,ref pos,out result);
								if(!result)
								{
									#if THING
									Debug.LogWarning(string.Format("Cannot find bracket { at pos {0} ",pos));
									#endif
									res=false;
									return null;
								}
							}
							object [] pars=new object[lst.Count];
							for(int i=0;i<lst.Count;i++)
							{
								pars[i]=lst[i];
							}
							ret=new Condition(Condition.Type.MULTI_AND,"_noname", pars);
							
						}break;
						case "or":{
							string bracket=readString(_txt,ref pos,out result);
							if(!result||bracket!="{")
							{
								#if THING
								Debug.LogWarning(string.Format("Cannot find bracket { at pos {0} ",pos));
								#endif
								res=false;
								return null;
							}
							List<Condition> lst=new List<Condition>();
							string nxtpos=readString(_txt,ref pos,out result);
							while(nxtpos!="}")
							{
								Condition tr;
								if(nxtpos!="condition")
								{
									if(!_context.ContainsKey(nxtpos))
									{
										#if THING
										Debug.LogWarning(string.Format("Invalid condition name:  {0} ",nxtpos));
										#endif
										res=false;
										return null;
									}
									tr=_context[nxtpos] as Condition;
								}
								else
								{
									tr=readCondition(_txt,ref pos,out result);
									if(!result)
									{
										#if THING
										Debug.LogWarning(string.Format("Invalid condition around:  {0} ",pos));
										#endif
										res=false;
										return null;
									}
								}
								lst.Add(tr);
								nxtpos=readString(_txt,ref pos,out result);
								if(!result)
								{
									#if THING
									Debug.LogWarning(string.Format("Cannot find bracket { at pos {0} ",pos));
									#endif
									res=false;
									return null;
								}
							}
							object [] pars=new object[lst.Count];
							for(int i=0;i<lst.Count;i++)
							{
								pars[i]=lst[i];
							}
							ret=new Condition(Condition.Type.MULTI_OR,"_noname", pars);
							
						}break;
							/*

and {<condition 1>  <condition 2> <condition ...> }
or {<condition 1>  <condition 2> <condition ...> }

*/
						default:
						{
							#if THING
							Debug.LogWarning(string.Format("Invalid condition type: {0} ",ltype));
							#endif
							res=false;
							return null;
						}

						}
						tpRead=true;

					}
				}
				ret.inverse=invert;
				while(first!="}")
				{
					first=readString(_txt,ref pos,out result);
					if(!result)
					{
						#if THING
						Debug.LogWarning(string.Format("Condition definition not closed with {} properly",pos));
						#endif
						res=false;
						return null;
					}
				}
			}
			if(vname!=null)
			{
				if(_context.ContainsKey(vname))
				{
					#if THING
					Debug.LogWarning(string.Format("Duplicate condition name:  {0} ",vname));
					#endif
				}
				_context[vname]=ret;
			}
			if(ret!=null) {res=true;}
			else res=false;
			return ret;
		}
		public static object readDefinition(string _txt,ref int pos,out bool res)
		{
			bool result=false;
			object ret=null;
			res=false;
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
				/*if(alias is Parametric)
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
				else*/
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


