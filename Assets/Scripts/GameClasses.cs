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
	public const string _Game="_Game";
	public const string _Owner="_Owner";
	public const string _Opponent="_Opponent";
	public const string _Source="_Source";
	public static readonly string[] __stackValues={_Game,_Owner,_Opponent,_rootl};
	public const string _template="_template";
	public const string _cardName="_cardName";
	public const string _cardText="_cardText";
	public const string _dr="<=";
  public const string _upcond="<<";
	public const int _drl=2;
  public const string _parentl="<--|";
	public const string _rootl="|-";
  public const string _conditional_self="_";
  public const string _accessBindings="_accessBindings";
  public const string _id="_id";
  //helper dictionaries
	public static Dictionary<string,List<object>> acceptedValues=new Dictionary<string, List<object>>();
	public static Dictionary<string,System.Type> acceptedTypes=new Dictionary<string, System.Type>();
//hooks
	delegate void uiHook(params object[] parameters);
//classes
	public static System.Random RNG=new System.Random();
	static object rnglock=new object();
  public static object gameLock=new object();
	public static int rngRange(int from, int to)
	{
		int ret;
		lock(rnglock)
		{
			ret=RNG.Next(from, to);
		}
		#if WHATHAPPENS
		//Debug.Log(string.Format("rnd range: {0} - {1} : {2}",from,to,ret));
#endif
		return ret;
	}
	public static string getRandString(int length, float spaceprob=0)
	{
		var chars="ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
		var stringChars=new char[length];

		for(int i = 0; i < stringChars.Length; i++)
		{
			if(Random.value>spaceprob)
			{
				stringChars[i]=chars[rngRange(0, chars.Length)];
			} else
				stringChars[i]=' ';
		}
		
		return new string(stringChars);
	}
	public static Conditional generateRandomCardTemplate()
	{
		Conditional ret=new Conditional();
		ret[_cardName]=getRandString(5, 0);
		ret[_cardText]=getRandString(25, 0.1f);
		return ret;
	}
	public class Conditional
	{
		protected Dictionary<string,object> _values;
		protected HashSet<string> _tags;
		public void logtags()
		{
			foreach(string t in _tags)
				Debug.Log(t);
		}
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
					/*if(key!=_parentl)
					{
					Debug.Log(key);
					ret._values[key]=(obj as Conditional).duplicate();
					}*/
					Conditional knd=obj as Conditional;
					if(key!=_parentl&&knd.hasTag("DEEP_COPY"))
						ret._values[key]=knd.duplicate();
						else
							ret._values[key]=obj;
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
    IList accessBindings(string name)
    {
      object game;
      _values.TryGetValue(_rootl,out game);
      if(game==null) return null;
      Conditional gm=game as Conditional;
      if(gm==null) return null;
      object ex;
      gm._values.TryGetValue(_accessBindings,out ex);
      Conditional dict=ex as Conditional;
      if(dict==null) return null;
      object exlist=null;
      if(! dict._values.TryGetValue(name,out exlist))
         return null;
      return (exlist as IList);
    }
    bool tryGetDouble(object obj,out double ret)
    {
      if(obj==null)
      {ret=0; return false;}
      if(obj is System.IConvertible)
      {
        ret=((System.IConvertible)obj).ToDouble(null);
        return true;
      }
      else
      {
        ret=0;
        return false;
      }
    }
		public object this [string name] { //let us add the dot notation...
			get {
				string [] ln=name.Split('.');
				if(ln.Length==1)
				{
					object ret=null;
          if(ln[0]==_conditional_self)
            return this;
					if(_values.TryGetValue(ln[0], out ret))
          {
            IList ab=accessBindings(name);
            double dret=0;
            if(ab!=null&&ab.Count>0&&tryGetDouble(ret,out dret))
            {
              bool changed=false;
              foreach(object o in ab)
              {
                Conditional accessBound=o as Conditional;
                if(accessBound==null)
                {
                  #if THING
                  Debug.Log(string.Format("Invalid access bound list element {0}",o));
                  #endif
                  return ret;
                }
                if(accessBound.hasTag("IN_BINDING")) continue;
                Condition cnd=accessBound[_condition] as Condition;
                if(cnd==null)
                {
                  #if THING
                  Debug.Log(string.Format("No condition ffound in access bound element {0}",accessBound));
                  #endif
                  continue;
                }
                accessBound.setTag("IN_BINDING");

                if(cnd.isFulfilled(this,this[_upcond] as Conditional))
                {
                  Conditional nstack=new Conditional(false);
                  nstack[_parentl]=this;
                  nstack[_upcond]=this;
                  nstack[_effects]=new List<Conditional>();
                  nstack[_rootl]=_values[_rootl];
                  nstack["_bound"]=dret;
                  Operation e=new Operation(Operation.Commands.NEW);
                  e.executeList(accessBound[_commands],nstack);
                  dret=(double)nstack["_bound"];
                  //Debug.Log("bound");
                  changed=true;
                }
                accessBound.removeTag("IN_BINDING");
              }
              if(changed) return dret;
            }
						return ret;
          }
					else
						return null;
				} else
				{
					object ret=null;
					if(_values.TryGetValue(ln[0], out ret))
					{
						Conditional nxt=ret as Conditional;
						return nxt[name.Substring(ln[0].Length+1)];//hopefully...
					} else
						return null;
				}
			}
			set {
				string [] ln=name.Split('.');
				if(ln.Length==1)
				{
					
					_values[name]=value;
				} else
				{
					object ret=null;
					if(_values.TryGetValue(ln[0], out ret))
					{
						Conditional nxt=ret as Conditional;
						nxt[name.Substring(ln[0].Length+1)]=value;//hopefully...
					}
				}
			}
		}
    static int curId=0;
		public Conditional(bool useid=true)
		{
			_values=new Dictionary<string, object>();
			_tags=new HashSet<string>();
      if(useid)
      {
      _values[_id]=curId;
      curId++;
      }
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
    //////////////Parser area
    ///
    object getFromContext(string id)
    {
      Conditional chk=this;
      while(chk!=null)
      {
        if(chk[id]!=null) return chk[id];
        chk=chk[_parentl] as Conditional;
      }
      return null;
    }
    public static Conditional loadFromString(string _txt)
    {
      int pos=0;
      Conditional ret=new Conditional();
      if(!ret.readInternalList(_txt+"}",ref pos))
      {
        #if THING
        Debug.LogWarning(string.Format("Couldn't load conditional from string :{0} ",_txt));
        #endif
        return null;
      }
      return ret;
    }
    bool readInternalList(string _txt,ref int pos)
    {
      bool result;
      string first;
      while(true)
      {
      first=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Invalid syntax in conditional at pos: {0} ",pos));
          #endif
          return false;
        }
        if(first=="}") break;
        switch(first)
        {
        case "tag":
        {
          string tagname=readString(_txt,ref pos,out result); 
          if(!result) return false;
          this.setTag(tagname);
        }break;
        case "int":
        {
          string nm=readString(_txt,ref pos,out result); 
          if(!result) return false;
          string val=readString(_txt,ref pos,out result); 
          if(!result) return false;
          int pval=0;
          if(!int.TryParse(val,out pval))
          {
            object vl=getFromContext(val);
            if(vl!=null&&vl is int)
            {
              this[nm]=(int)vl;
            }
            else
            {
              #if THING
              Debug.LogWarning(string.Format("Invalid int value: {0} ",val));
              #endif
              return false;
            }
          }
          else
          {
					
            this[nm]=pval;
          }
        }break;
        case "float":
        {
          string nm=readString(_txt,ref pos,out result); 
          if(!result) return false;
          string val=readString(_txt,ref pos,out result); 
          if(!result) return false;
          float pval=0;
          if(!float.TryParse(val,out pval))
          {
            object vl=getFromContext(val);
            if(vl!=null&&vl is float)
            {
              this[nm]=(float)vl;
            }
            else
            {
              #if THING
              Debug.LogWarning(string.Format("Invalid float value: {0} ",val));
              #endif
              return false;
            }
          }
          else
          {
            this[nm]=pval;
          }
        }break;
        case "string":
        {
          string nm=readString(_txt,ref pos,out result); 
          if(!result) return false;
          string val=readString(_txt,ref pos,out result); 
          if(!result) return false;
         
          if(val.StartsWith(_dr))
          {
            object vl=getFromContext(val.Substring(_drl));
            if(vl!=null&&vl is string)
            {
              this[nm]=(string)vl;
            }
            else
            {
              #if THING
              Debug.LogWarning(string.Format("Invalid string ref : {0} ",val));
              #endif
              return false;
            }
          }
          else
          {
            this[nm]=val;
          }
        }break;
        case "conditional":
        {
          readConditional(_txt,ref pos,out result); 
          if(!result) return false;
        }break;
				case "list":
				{
          string nm=readString(_txt,ref pos,out result); 
          if(!result) return false;
          string br=readString(_txt,ref pos,out result); 
          if(!result) return false;
          if(br!="{") return false;
          List<Conditional> lst=new List<Conditional>();
          string crn=readString(_txt,ref pos,out result); 
          if(!result) return false;
          while(crn!="}")
          {
           if(crn=="conditional")
            {
             // _parentl;
					  Conditional listmem=readConditional(_txt,ref pos,out result); 
              if(!result) return false;
              lst.Add (listmem);
            }
            else
            {
              Conditional listmem=getFromContext(crn) as Conditional;
              if(listmem==null) return false;
              lst.Add (listmem);
            }
            crn=readString(_txt,ref pos,out result); 
            if(!result) return false;
          }
					this[nm]=lst;
				}break;
				case "function":
				{
					readFunction(_txt,ref pos,out result); 
					if(!result) return false;
				}break;
        case "condition":
        {
          readCondition(_txt,ref pos,out result); 
          if(!result) return false;
        }break;
        }
      }
      return true;
    }
    public Conditional readConditional (string _txt,ref int pos,out bool res)
    {
      bool result;
      string vname=null;
      string first=readString(_txt,ref pos,out result);
      if(!result)
      {
        #if THING
        Debug.LogWarning(string.Format("Cannot read conditional in string at pos: {0} ",pos));
        #endif
        res=false;
        return null;
      }
      if(first=="conditional")
      {
        first=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Cannot read conditional in string at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
      }
      if(first=="conditional")
      {
        #if THING
        Debug.LogWarning(string.Format("Invalid conditional name"));
        #endif
        res=false;
        return null;
      }
      Conditional ret=null;
      if(first!="{")
      {
        vname=first;
        first=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Cannot read conditional in string at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
      }
      if(first!="{") //alias
      {
        ret=getFromContext(first) as Conditional;
        if(ret==null)
        {
          #if THING
          Debug.LogWarning(string.Format("Cannot read function in string at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        first=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Cannot read conditional in string at pos: {0}, invalid syntax",pos));
          #endif
          res=false;
          return null;
        }
        ret=ret.duplicate();
      }
      if(first!="{") //rollback...
      {
        pos=pos-first.Length;
        if(ret!=null) res=true;
        else res=false;
        return ret;
      }
      else
      {
        if(ret==null)
          ret=new Conditional();
        ret[_parentl]=this;
				if(this[_rootl]==null)
					ret[_rootl]=this;
				else
					ret[_rootl]=this[_rootl];
        if(!ret.readInternalList(_txt,ref pos))
        {
          #if THING
          Debug.LogWarning(string.Format("Cannot read conditional in string at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
      }
      if(vname!=null)
      {
        if(this[vname]!=null)
        {
          #if THING
          Debug.LogWarning(string.Format("Duplicate conditional name:  {0} ",vname));
          #endif
        }
        this[vname]=ret;
        ret["__name"]=vname;
      }
      
      if(ret!=null) res=true;
      else res=false;
      return ret;
    }
    public List<Operation> readFunction(string _txt,ref int pos,out bool res) //after function keyword, though not always?
    {
      bool result;
      string vname=null;
      string first=readString(_txt,ref pos,out result);
      if(!result)
      {
        #if THING
        Debug.LogWarning(string.Format("Cannot read function in string at pos: {0} ",pos));
        #endif
        res=false;
        return null;
      }
      if(first=="function")
      {
        first=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Cannot read function in string at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
      }
      if(first=="function")
      {
        #if THING
        Debug.LogWarning(string.Format("Invalid function name"));
        #endif
        res=false;
        return null;
      }
      List<Operation> ret=null;
      if(first!="{")
      {
        vname=first;
        first=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Cannot read function in string at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
      }
      if(first!="{") //alias
      {
        ret=getFromContext(first) as List<Operation>;
        if(ret==null)
        {
          #if THING
          Debug.LogWarning(string.Format("Cannot read function in string at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
      }
      else
      {
        ret=readOperationList(_txt,ref pos, out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Cannot read function in string at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
      }
      if(vname!=null)
      {
        if(this[vname]!=null)
        {
          #if THING
          Debug.LogWarning(string.Format("Duplicate function name:  {0} ",vname));
          #endif
        }
        this[vname]=ret;
      }

      if(ret!=null) res=true;
      else res=false;
      return ret;
    }

    public  Condition readCondition(string _txt,ref int pos,out bool res) //after condition keyword, though not always?
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
        ret=getFromContext(first) as Condition;
        if(ret==null)
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
            case "self":{
              string varname=readString(_txt,ref pos,out result);
              if(!result)
              {
                #if THING
                Debug.LogWarning(string.Format("Cannot read condition var name at pos: {0} ",pos));
                #endif
                res=false;
                return null;
              }
              ret=new Condition(Condition.Type.SELF,varname);
            };break;
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
              ret=new Condition(Condition.Type.STRING,varname,compval);
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
                tr=getFromContext(nxtname) as Condition;
                if(tr==null)
                {
                  #if THING
                  Debug.LogWarning(string.Format("Invalid condition name:  {0} ",nxtname));
                  #endif
                  res=false;
                  return null;
                }
               
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
                tr=getFromContext(nxtname) as Condition;
                if(tr==null)
                {
                  #if THING
                  Debug.LogWarning(string.Format("Invalid condition name:  {0} ",nxtname));
                  #endif
                  res=false;
                  return null;
                }
               
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
                tr=getFromContext(nxtname) as Condition;
                if(tr==null)
                {
                  #if THING
                  Debug.LogWarning(string.Format("Invalid condition name:  {0} ",nxtname));
                  #endif
                  res=false;
                  return null;
                }
               
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
                tr2=getFromContext(ccname) as Condition;
                if(tr2==null)
                {
                  #if THING
                  Debug.LogWarning(string.Format("Invalid condition name:  {0} ",ccname));
                  #endif
                  res=false;
                  return null;
                }

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
                  tr=getFromContext(nxtpos) as Condition;
                  if(tr==null)
                  {
                    #if THING
                    Debug.LogWarning(string.Format("Invalid condition name:  {0} ",nxtpos));
                    #endif
                    res=false;
                    return null;
                  }
                 
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
                  tr=getFromContext(nxtpos) as Condition;
                  if(tr==null)
                  {
                    #if THING
                    Debug.LogWarning(string.Format("Invalid condition name:  {0} ",nxtpos));
                    #endif
                    res=false;
                    return null;
                  }

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
            case "command_type":{
              string varname=readString(_txt,ref pos,out result);
              if(!result)
              {
                #if THING
                Debug.LogWarning(string.Format("Cannot read var type at pos: {0} ",pos));
                #endif
                res=false;
                return null;
              }
              string cmptype=readString(_txt,ref pos,out result);
              if(!result||Operation.getTypeFromString(cmptype)==Operation.Commands.ERROR)
              {
                #if THING
                Debug.LogWarning(string.Format("Cannot read compare value  at pos: {0} {1}",pos,cmptype));
                #endif
                res=false;
                return null;
              }
              ret=new Condition(Condition.Type.COMMAND_TYPE,varname,cmptype);
              
            }break;
            case "command_arg":{
              string varname=readString(_txt,ref pos,out result);
              if(!result)
              {
                #if THING
                Debug.LogWarning(string.Format("Cannot read var type at pos: {0} ",pos));
                #endif
                res=false;
                return null;
              }
              string cmptype=readString(_txt,ref pos,out result);
              int prr=0;
              if(!result||int.TryParse(cmptype,out prr))
              {
                #if THING
                Debug.LogWarning(string.Format("Cannot read compare value  at pos: {0} {1}",pos,cmptype));
                #endif
                res=false;
                return null;
              }
              Condition tr;
              string nxtpos=readString(_txt,ref pos,out result);
              if(!result)
              {
                #if THING
                Debug.LogWarning(string.Format("Cannot read var condition at pos: {0} ",pos));
                #endif
                res=false;
                return null;
              }
              if(nxtpos!="condition")
              {
                tr=getFromContext(nxtpos) as Condition;
                if(tr==null)
                {
                  #if THING
                  Debug.LogWarning(string.Format("Invalid condition name:  {0} ",nxtpos));
                  #endif
                  res=false;
                  return null;
                }
                
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
              ret=new Condition(Condition.Type.COMMAND_ARG,varname,prr,tr);
              
            }break;
            case "refcomp":{
              string varname=readString(_txt,ref pos,out result);
              if(!result)
              {
                #if THING
                Debug.LogWarning(string.Format("Cannot read var1 in refcomp at pos: {0} ",pos));
                #endif
                res=false;
                return null;
              }
              string var2name=readString(_txt,ref pos,out result);
            
              if(!result)
              {
                #if THING
                Debug.LogWarning(string.Format("Cannot read var2 in refcomp   at pos: {0} ",pos));
                #endif
                res=false;
                return null;
              }
             
              ret=new Condition(Condition.Type.REFCOMP,varname,var2name);
              
            }break;
            case "isset":{
							string varname=readString(_txt,ref pos,out result);
							if(!result)
							{
								#if THING
								Debug.LogWarning(string.Format("Cannot read var name at pos: {0} ",pos));
								#endif
								res=false;
								return null;
							}

							ret=new Condition(Condition.Type.ISSET,varname);
							
						}break;
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
        if(this[vname]!=null)
        {
          #if THING
          Debug.LogWarning(string.Format("Duplicate condition name:  {0} ",vname));
          #endif
        }
       this[vname]=ret;
      }
      if(ret!=null) {res=true;}
      else res=false;
      return ret;
    }

    public  Operation readOperation(string _txt,ref int pos,out bool res)
    {
      bool result;
      string kind=readString(_txt,ref pos,out result);
      if(!result)
      {
        #if THING
        Debug.LogWarning(string.Format("Cannot read operation in string at pos: {0} ",pos));
        #endif
        res=false;
        return null;
      }
      if (kind == "}") //end of operation list
      {
        res=true;
        return null;
      }
      Operation ret=null;
      Operation.Commands cmd=Operation.getTypeFromString(kind);
      if(cmd==Operation.Commands.ERROR)
      {
        #if THING
        Debug.LogWarning(string.Format("Invalid operation code: {0} ",kind));
        #endif
        res=false;
        return null;
      }
      List<object> args=new List<object>();
      switch (cmd)
      {
      case Operation.Commands.TAG_SET:{
				string val=readString(_txt,ref pos,out result);
				
				if(!result)
				{
					#if THING
					Debug.LogWarning(string.Format("Couldn't read tag name at pos: {0} ",pos));
					#endif
					res=false;
					return null;
				}
        args.Add(val);
        string tag=readString(_txt,ref pos,out result);
        
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read tag name at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
     
        args.Add(tag);
        ret=new Operation(Operation.Commands.TAG_SET);
        ret[_args]=args;
        res=true;
        return ret;
      }
			case Operation.Commands.TAG_REMOVE:{
				string val=readString(_txt,ref pos,out result);
				
				if(!result)
				{
					#if THING
					Debug.LogWarning(string.Format("Couldn't read tag name at pos: {0} ",pos));
					#endif
					res=false;
					return null;
				}
				args.Add(val);
				string tag=readString(_txt,ref pos,out result);
				
				if(!result)
				{
					#if THING
					Debug.LogWarning(string.Format("Couldn't read tag name at pos: {0} ",pos));
					#endif
					res=false;
					return null;
				}
				
				args.Add(tag);
				ret=new Operation(Operation.Commands.TAG_REMOVE);
				ret[_args]=args;
        res=true;
        return ret;
      }
      case Operation.Commands.TAG_SWITCH:{
        string val=readString(_txt,ref pos,out result);
        
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read tag name at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        args.Add(val);
        string tag_from=readString(_txt,ref pos,out result);
        
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read tag_from name at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        string tag_to=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read tag_to name at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        args.Add(tag_from);
        args.Add(tag_to);
        ret=new Operation(Operation.Commands.TAG_SWITCH);
        ret[_args]=args;
        res=true;
        return ret;
      }
      case Operation.Commands.VALUE_SET:{

        string settarg=readString(_txt,ref pos,out result);
        
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read tag_from name at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        string tag_to=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read tag_to name at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        args.Add(settarg);
        args.Add(tag_to);
        ret=new Operation(Operation.Commands.VALUE_SET);
        ret[_args]=args;
        res=true;
        return ret;
      }
      case Operation.Commands.ADD:{
        string settarg=readString(_txt,ref pos,out result);
        
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read tag_from name at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        string tag_to=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read tag_to name at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        args.Add(settarg);
        if(tag_to.StartsWith(_dr))
        {
          args.Add(tag_to);
        }
        else
        {
          float np=0;
          if(!float.TryParse(tag_to,out np))
          {
            #if THING
            Debug.LogWarning(string.Format("Invalid float value: {0} ",tag_to));
            #endif
            res=false;
            return null;
          }
          args.Add(np);
        }
        ret=new Operation(Operation.Commands.ADD);
        ret[_args]=args;
        res=true;
        return ret;
      }
      case Operation.Commands.SUBTRACT:{
        string settarg=readString(_txt,ref pos,out result);
        
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read tag_from name at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        string tag_to=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read tag_to name at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        args.Add(settarg);
        if(tag_to.StartsWith(_dr))
        {
          args.Add(tag_to);
        }
        else
        {
          float np=0;
          if(!float.TryParse(tag_to,out np))
          {
            #if THING
            Debug.LogWarning(string.Format("Invalid float value: {0} ",tag_to));
            #endif
            res=false;
            return null;
          }
          args.Add(np);
        }
        ret=new Operation(Operation.Commands.SUBTRACT);
        ret[_args]=args;
        res=true;
        return ret;
      }
      case Operation.Commands.MULTIPLY:{
        string settarg=readString(_txt,ref pos,out result);
        
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read tag_from name at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        string tag_to=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read tag_to name at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        args.Add(settarg);
        if(tag_to.StartsWith(_dr))
        {
          args.Add(tag_to);
        }
        else
        {
          float np=0;
          if(!float.TryParse(tag_to,out np))
          {
            #if THING
            Debug.LogWarning(string.Format("Invalid float value: {0} ",tag_to));
            #endif
            res=false;
            return null;
          }
          args.Add(np);
        }
        ret=new Operation(Operation.Commands.MULTIPLY);
        ret[_args]=args;
        res=true;
        return ret;
      }
      case Operation.Commands.DIVIDE:{
        string settarg=readString(_txt,ref pos,out result);
        
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read tag_from name at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        string tag_to=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read tag_to name at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        args.Add(settarg);
        if(tag_to.StartsWith(_dr))
        {
          args.Add(tag_to);
        }
        else
        {
          float np=0;
          if(!float.TryParse(tag_to,out np))
          {
            #if THING
            Debug.LogWarning(string.Format("Invalid float value: {0} ",tag_to));
            #endif
            res=false;
            return null;
          }
          args.Add(np);
        }
        ret=new Operation(Operation.Commands.DIVIDE);
        ret[_args]=args;
        res=true;
        return ret;
      }
      case Operation.Commands.ABORT:
      {
        ret=new Operation(Operation.Commands.ABORT);
        res=true;
        return ret;
      }
      case Operation.Commands.CONTINUE:
      {
        ret=new Operation(Operation.Commands.CONTINUE);
        res=true;
        return ret;
      }
        
      case Operation.Commands.RETURN:
      {
        Operation retval=readOperation(_txt,ref pos,out result);
        if(!result||retval==null)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read returned command  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        args.Add(retval);
        ret=new Operation(Operation.Commands.RETURN);
        ret[_args]=args;
        res=true;
        return ret;
      }
      case Operation.Commands.INSERT:
      {
        string varval=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read foreach list  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        string listval=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read foreach list  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        string sortval=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read foreach list  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        args.Add(varval);
        args.Add(listval);
        args.Add(sortval);
        ret=new Operation(Operation.Commands.INSERT);
        ret[_args]=args;
        res=true;
        return ret;
      }
      case Operation.Commands.FOREACH:
      {
        string listval=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read foreach list  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        string ob=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read foreach at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        if(ob!="{")
        {
          #if THING
          Debug.LogWarning(string.Format("No opening bracket, this: {0} ",ob));
          #endif
          res=false;
          return null;
        }
        List<Operation> lst=readOperationList(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read foreach operations at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        
        args.Add(listval);
        args.Add(lst);
        ret=new Operation(Operation.Commands.FOREACH);
        ret[_args]=args;
        res=true;
        return ret;
      }

      case Operation.Commands.EXECUTE:
      {
        string valname=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read execute variable  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        args.Add(valname);
        string ob=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read execute function  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        if(ob!="{"&&!ob.StartsWith(_dr))
        {
          #if THING
          Debug.LogWarning(string.Format("This is neither opening bracket nor a reference: {0} ",ob));
          #endif
          res=false;
          return null;
        }
        if(ob!="{")
        {
          args.Add(ob);
        }
        else
        {
        List<Operation> lst=readOperationList(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read foreach operations at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        args.Add(lst);
        }
        ret=new Operation(Operation.Commands.EXECUTE);
        ret[_args]=args;
        res=true;
        return ret;
      }
      case Operation.Commands.IF:
      {
        string listval=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read if variable  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        string condname=readString(_txt,ref pos,out result);
        Condition a1=null;
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read if condition  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        if(condname=="condition")
        {
          a1=readCondition(_txt,ref pos,out result);
          if(!result){
            #if THING
            Debug.LogWarning(string.Format("Couldn't read if cond  at pos: {0} ",pos));
            #endif
            res=false;
            return null;}
          
        }
        else
        {
          a1=getFromContext(condname) as Condition;
          if(a1==null)
          {
            #if THING
            Debug.LogWarning(string.Format("Condition not defined : {0}  ",condname));
            #endif
            res=false;
            return null;
          }
          
        }
        string ob=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read if statement at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        if(ob!="{")
        {
          #if THING
          Debug.LogWarning(string.Format("No opening bracket, this: {0} ",ob));
          #endif
          res=false;
          return null;
        }
        List<Operation> lst=readOperationList(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read if operations at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        
        args.Add(listval);
        args.Add(a1);
        args.Add(lst);
        ret=new Operation(Operation.Commands.IF);
        ret[_args]=args;
        res=true;
        return ret;
      }
			case Operation.Commands.CONDITION_AND:
			{
				string varname=readString(_txt,ref pos,out result);
				if(!result)
        {
					#if THING
					Debug.LogWarning(string.Format("Couldn't read condand   at pos: {0} ",pos));
					#endif
					res=false;
          return null;
        }
				args.Add(varname);
				string condname=readString(_txt,ref pos,out result);
				Condition a1=null;
				if(!result)
				{
					#if THING
					Debug.LogWarning(string.Format("Couldn't read condand condition  at pos: {0} ",pos));
					#endif
					res=false;
					return null;
				}
				if(condname=="condition")
				{
					a1=readCondition(_txt,ref pos,out result);
					if(!result){
						#if THING
						Debug.LogWarning(string.Format("Couldn't read condand cond  at pos: {0} ",pos));
						#endif
						res=false;
						return null;}
					
				}
				else
				{
					a1=getFromContext(condname) as Condition;
					if(a1==null)
					{
						#if THING
						Debug.LogWarning(string.Format("Condition not defined : {0}  ",condname));
						#endif
						res=false;
						return null;
					}
					
				}
				args.Add(a1);

				ret=new Operation(Operation.Commands.CONDITION_AND);
				ret[_args]=args;
				res=true;
				return ret;
      }
			case Operation.Commands.CONDITION_OR:
			{
				string varname=readString(_txt,ref pos,out result);
				if(!result)
				{
					#if THING
					Debug.LogWarning(string.Format("Couldn't read condand   at pos: {0} ",pos));
					#endif
					res=false;
					return null;
				}
				args.Add(varname);
				string condname=readString(_txt,ref pos,out result);
				Condition a1=null;
				if(!result)
				{
					#if THING
					Debug.LogWarning(string.Format("Couldn't read condand condition  at pos: {0} ",pos));
					#endif
					res=false;
					return null;
				}
				if(condname=="condition")
				{
					a1=readCondition(_txt,ref pos,out result);
					if(!result){
						#if THING
						Debug.LogWarning(string.Format("Couldn't read condand cond  at pos: {0} ",pos));
						#endif
						res=false;
						return null;}
					
				}
				else
				{
					a1=getFromContext(condname) as Condition;
					if(a1==null)
					{
						#if THING
						Debug.LogWarning(string.Format("Condition not defined : {0}  ",condname));
						#endif
						res=false;
            return null;
          }
          
        }
        args.Add(a1);
        
        ret=new Operation(Operation.Commands.CONDITION_OR);
        ret[_args]=args;
        res=true;
        return ret;
      }
      case Operation.Commands.WHILE:
      {
        string condname=readString(_txt,ref pos,out result);
        Condition a1=null;
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read while condition  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        if(condname=="condition")
        {
          a1=readCondition(_txt,ref pos,out result);
          if(!result){
            #if THING
            Debug.LogWarning(string.Format("Couldn't read accumulate cond  at pos: {0} ",pos));
            #endif
            res=false;
            return null;}
          
        }
        else
        {
          a1=getFromContext(condname) as Condition;
          if(a1==null)
          {
            #if THING
            Debug.LogWarning(string.Format("Condition not defined : {0}  ",condname));
            #endif
            res=false;
            return null;
          }
          
        }
        args.Add(a1);
        string ob=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read foreach at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        if(ob!="{")
        {
          #if THING
          Debug.LogWarning(string.Format("No opening bracket, this: {0} ",ob));
          #endif
          res=false;
          return null;
        }
        List<Operation> lst=readOperationList(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read foreach operations at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        

        args.Add(lst);
        ret=new Operation(Operation.Commands.WHILE);
        ret[_args]=args;
        res=true;
        return ret;
      }
      case Operation.Commands.TARGET:
      {
        string condname=readString(_txt,ref pos,out result);
        Condition a1=null;
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read target condition  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        if(condname=="condition")
        {
          a1=readCondition(_txt,ref pos,out result);
          if(!result){
            #if THING
            Debug.LogWarning(string.Format("Couldn't read target cond  at pos: {0} ",pos));
            #endif
            res=false;
            return null;}
          
        }
        else
        {
          a1=getFromContext(condname) as Condition;
          if(a1==null)
          {
            #if THING
            Debug.LogWarning(string.Format("Condition not defined : {0}  ",condname));
            #endif
            res=false;
            return null;
          }
      
        }
        args.Add(a1);
        string listname=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read target list at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        args.Add(listname);
        
        ret=new Operation(Operation.Commands.TARGET);
        ret[_args]=args;
        res=true;
        return ret;
      }
      case Operation.Commands.ACCUMULATE:
      {
        string condname=readString(_txt,ref pos,out result);
        Condition a1=null;
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read accumulate condition  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        if(condname=="condition")
        {
          a1=readCondition(_txt,ref pos,out result);
          if(!result){
            #if THING
            Debug.LogWarning(string.Format("Couldn't read accumulate cond  at pos: {0} ",pos));
            #endif
            res=false;
            return null;}
          
        }
        else
        {
          a1=getFromContext(condname) as Condition;
          if(a1==null)
          {
            #if THING
            Debug.LogWarning(string.Format("Condition not defined : {0}  ",condname));
            #endif
            res=false;
            return null;
          }
        
        }
        args.Add(a1);
        string listname=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read accumulate list at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        args.Add(listname);
        string assignname=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read accumulate targ list at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        args.Add(assignname);
        ret=new Operation(Operation.Commands.ACCUMULATE);
        ret[_args]=args;
        res=true;
        return ret;
      }
      case Operation.Commands.CLEAR:
      {
        string valname=readString(_txt,ref pos,out result);
        
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read accumulate condition  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        
        args.Add(valname);
        ret=new Operation(Operation.Commands.CLEAR);
        ret[_args]=args;
        res=true;
        return ret;
      }
      case Operation.Commands.HOOK:
      {
        string hookname=readString(_txt,ref pos,out result);
        
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read accumulate condition  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        
        args.Add(hookname);
        string dataname=readString(_txt,ref pos,out result);
        
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read accumulate condition  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        
        args.Add(dataname);
        ret=new Operation(Operation.Commands.HOOK);
        ret[_args]=args;
        res=true;
        return ret;
      }
        
      case Operation.Commands.CHOICE:
      {
        string choice_name=readString(_txt,ref pos,out result);
        
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read accumulate condition  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        
        args.Add(choice_name);
        string list_name=readString(_txt,ref pos,out result);
        
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read accumulate condition  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        
        args.Add(list_name);
        string ret_name=readString(_txt,ref pos,out result);
        
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read accumulate condition  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        
        args.Add(ret_name);
        ret=new Operation(Operation.Commands.CHOICE);
        ret[_args]=args;
        res=true;
        return ret;
      }
      case Operation.Commands.NEW:
      {
        string var_name=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read new condition  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        
        args.Add(var_name);
        
        ret=new Operation(Operation.Commands.NEW);
        ret[_args]=args;
        res=true;
        return ret;
      }
	  case Operation.Commands.NEWLIST:
      {
        string var_name=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read newlist condition  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        
        args.Add(var_name);
        
        ret=new Operation(Operation.Commands.NEWLIST);
        ret[_args]=args;
        res=true;
        return ret;
      }
      case Operation.Commands.POP:
      {
        string list_name=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read accumulate condition  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        
        args.Add(list_name);
        string vn=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read accumulate condition  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        
        args.Add(vn);
        ret=new Operation(Operation.Commands.POP);
        ret[_args]=args;
        res=true;
        return ret;
      }
      case Operation.Commands.PUSH:
      {
        string list_name=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read accumulate condition  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        
        args.Add(list_name);
        string vn=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read accumulate condition  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        
        args.Add(vn);
        ret=new Operation(Operation.Commands.PUSH);
        ret[_args]=args;
        res=true;
        return ret;
      }
      case Operation.Commands.SHIFT:
      {
        string list_name=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read accumulate condition  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        
        args.Add(list_name);
        string vn=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read accumulate condition  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        
        args.Add(vn);
        ret=new Operation(Operation.Commands.SHIFT);
        ret[_args]=args;
        res=true;
        return ret;
      }
      case Operation.Commands.APPEND:
      {
        string list_name=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read accumulate condition  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        
        args.Add(list_name);
        string vn=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read accumulate condition  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        
        args.Add(vn);
        ret=new Operation(Operation.Commands.APPEND);
        ret[_args]=args;
        res=true;
        return ret;
      }
      case Operation.Commands.REMOVE:
      {
        string list_name=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read accumulate condition  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        
        args.Add(list_name);
        string vn=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read accumulate condition  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        
        args.Add(vn);
        ret=new Operation(Operation.Commands.REMOVE);
        ret[_args]=args;
        res=true;
        return ret;
      }
      case Operation.Commands.ANY:
      {
        string list_name=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read accumulate condition  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        
        args.Add(list_name);
        string vn=readString(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Couldn't read accumulate condition  at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        
        args.Add(vn);
        ret=new Operation(Operation.Commands.ANY);
        ret[_args]=args;
        res=true;
        return ret;
      }
        
      default:
      {
        #if THING
        Debug.LogWarning(string.Format("Invalid command name at pos: {0} ",pos));
        #endif
        res=false;
        return null;
      }
      }
    }
    public  List<Operation> readOperationList(string _txt,ref int pos,out bool res)
    {
      List<Operation> ret=new List<Operation>();
      bool result;
      while(true)
      {
        Operation rd=readOperation(_txt,ref pos,out result);
        if(!result)
        {
          #if THING
          Debug.LogWarning(string.Format("Cannot read operation in string at pos: {0} ",pos));
          #endif
          res=false;
          return null;
        }
        if(rd==null) break;
        ret.Add(rd);
      }
      
      res=true;
      return ret;
    }

    public  string readString(string _txt,ref int pos,out bool res)
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

	}
	public class GameManager
	{

		public Conditional _GameData;
		object locket=new object();
		bool runThread=false;
		Thread gameThread;
		EventWaitHandle _waitHandle ;
		static public GameManager self=null;

		bool _choiceInProgress=false;

		public bool choiceInProgress {
			get {
				return _choiceInProgress;
			}
		}

		bool _hookInProgress=false;

		public bool isWaiting {
			get {
				return _choiceInProgress||_hookInProgress;
			}
		}

		public bool hookInProgress {
			get{ return _hookInProgress;}
		}

		Conditional _hookData;
		string _hookName=null;
		public string hookName{ get { return _hookName; } }

		public Conditional hookData {
			get{ return _hookData;}
		}

		Conditional chosen=null;
		IList _choiceList;
		string _choiceName;
		public string choiceName{ get { return _choiceName; } }
		public IList choiceList {
			get{ return _choiceList;}
		}

    List<Conditional> wrapGameEffects()
    {
      IList rulesAndEffects=_GameData[_effects] as IList;
      List<Conditional> wrappedEffects=new List<Conditional>();
      foreach(object obj in rulesAndEffects)
      {
        
        Conditional eff=obj as Conditional;
        if(eff==null)
        {
          #if THING
          Debug.Log(string.Format("Invalid effect description: {0}", obj));
          #endif
        } else
        {
          Conditional wrap=new Conditional(false);
          wrap[_effect]=eff;
          wrappedEffects.Add(wrap);
        }
      }
      return wrappedEffects;
    }
		void mainGameThread()
		{
			IList rulesAndEffects=_GameData[_effects] as IList;

			Conditional stack=new Conditional();
			stack[_Game]=_GameData;
      List<Conditional> wrappedEffects=wrapGameEffects();
			
			stack[_effects]=wrappedEffects;
			stack[_target]=null;
			stack[_rootl]=_GameData;
			string curName="";
      Debug.Log(string.Format("effects : {0}",wrappedEffects.Count));
			try
			{

			
      while(runThread)
			{
          int ii=0;
				while(ii<rulesAndEffects.Count)
				{
            object obj =rulesAndEffects[ii];
            ii++;
					Conditional eff=obj as Conditional;
						curName=eff["__name"] as string;
					if(!eff.hasTag(EXECUTE_PREFIX)&&!eff.hasTag(EXECUTE_POSTFIX))
					{
						Condition cnd=eff[_condition] as Condition;
						//Debug.Log(cnd);
						//Monitor.Enter(gameLock);
						if(cnd.isFulfilled(stack,stack[_Game] as Conditional))
						{
							//Debug.Log(cnd);
							Operation op=new Operation(Operation.Commands.NEW);
							Conditional nstack=op.createStack(stack, eff);
              //Debug.Log(string.Format("nstack effects : {0}",(nstack[_effects] as IList).Count));
                int oneff=rulesAndEffects.Count;
              op.executeList(eff[_commands], nstack);
                if(rulesAndEffects.Count!=oneff)
                {
                  stack[_effects]=wrapGameEffects();
                  Debug.Log("added trules");
                }
							if(nstack.hasTag(TAG_ABORT))
							{
								Debug.Log("GameObject Overlapped");
								return;//gameover? I guess
							}
						}

						//Monitor.Exit(gameLock);
					}
				}
			}

			}
			catch (System.Exception ex)
			{
				Debug.Log(ex);
				Debug.Log(curName);

				// I WILL LOG THE EXCEPTION object "EX" here ! but ex.StackTrace is truncated!
          }
      
    }

		public static Conditional startChoice(string chname, IList objects)
		{
			if(self==null||Thread.CurrentThread!=self.gameThread)
			{
				#if THING
				Debug.Log(string.Format("Invalid thread calling choice: {0}", Thread.CurrentThread));
				#endif
				return null;
			}
			if(self._choiceInProgress)
			{
				#if THING
				Debug.Log(string.Format("Choice called for the second time -wtf?: {0}", Thread.CurrentThread));
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
				Debug.Log(string.Format("Invalid thread calling choice end: {0}", Thread.CurrentThread));
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
		public static void STOP()
		{
      self.runThread=false;
			self.gameThread.Abort();

		}
		public static void startHook(string hName, Conditional hookData)
		{
			if(self==null||Thread.CurrentThread!=self.gameThread)
			{
				#if THING
				Debug.Log(string.Format("Invalid thread calling hook start: {0}", Thread.CurrentThread));
				#endif
				return;
			}
			if(self._hookInProgress)
			{
				#if THING
				Debug.Log(string.Format("Hook called for the second time -wtf?: {0}", Thread.CurrentThread));
				#endif
				return;
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
				Debug.Log(string.Format("Invalid thread calling hook end: {0}", Thread.CurrentThread));
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

			TextAsset flist=Resources.Load("flist") as TextAsset;
			Conditional read=new Conditional();
			bool result=true;
			int pos=0;
			string file=read.readString(flist.text,ref pos, out result);
			StringBuilder sb=new StringBuilder();
			while(result)
			{
				TextAsset ass=Resources.Load(file) as TextAsset;
				if(ass!=null)
				{
					sb.Append(ass.text);
				}
				file=read.readString(flist.text,ref pos, out result);
			}
			_GameData=Conditional.loadFromString(sb.ToString()); //a test, for now..
      /*IList testop=_GameData["testFunc"] as IList;
      Conditional tstack=new Conditional();
      tstack[_target]=tstack;
      Debug.Log("testop");
      foreach(object o in testop)
      {
        Operation op=o as Operation;
        op._execute(tstack);
      }
      Debug.Log(tstack["Damage"]);
      Debug.Log("endtestop");*/
			List<Conditional> deck=new List<Conditional>();
			for(int i=0; i<30; i++)
				deck.Add(generateRandomCardTemplate());
			//_GameData["DECK"]=deck;
			//_GameData["HAND"]=new List<Conditional>();
			//List<Conditional> effs=new List<Conditional>();
		//	Conditional drawRule=_GameData["drawRule"] as Conditional;
			//Debug.Log(drawRule);
			//Debug.Log(drawRule[_commands]);
			//Debug.Log(drawRule[_condition]);
			// new Conditional();
			
			//effs.Add(drawRule);
			//_GameData[_effects]=effs;
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
      runThread=true;
			gameThread=new Thread(mainGameThread);
			_waitHandle=new AutoResetEvent(false);
			gameThread.Start();
		}
	}
	public class Operation:Conditional
	{
		public enum Commands
		{
			TAG_SET,
			TAG_REMOVE,
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
			NEWLIST,
      WHILE,
			/// Condition manipulation
			CONDITION_AND,
			CONDITION_OR,
      IF,
      INSERT,
			EXECUTE,
			ERROR
		}
		Commands _command;
    public Commands command{get{return _command;}}
    public static Commands getTypeFromString(string str)
    {
      switch(str)
      {
      case "tag_set":return Commands.TAG_SET;
	  case "tag_remove":return Commands.TAG_REMOVE;
      case "tag_switch":return Commands.TAG_SWITCH;
      case "set":return Commands.VALUE_SET;
      case "add":return Commands.ADD;
      case "sub":return Commands.SUBTRACT;
      case "mul":return Commands.MULTIPLY;
      case "div":return Commands.DIVIDE;
      case "abort":return Commands.ABORT;
      case "continue":return Commands.CONTINUE;
      case "return":return Commands.RETURN;
      case "foreach":return Commands.FOREACH;
      case "target":return Commands.TARGET;
      case "accumulate":return Commands.ACCUMULATE;
      case "clear":return Commands.CLEAR;
      case "hook":return Commands.HOOK;
      case "choice":return Commands.CHOICE;
      case "new":return Commands.NEW;
	  case "newlist":return Commands.NEWLIST;
      case "pop":return Commands.POP;
      case "push":return Commands.PUSH;
      case "shift":return Commands.SHIFT;
      case "append":return Commands.APPEND;
      case "remove":return Commands.REMOVE;
      case "any":return Commands.ANY;
      case "while":return Commands.WHILE;
			case "condition_and":return Commands.CONDITION_AND;
			case "condition_or":return Commands.CONDITION_OR;
      case "if":return Commands.IF;
      case "insert":return Commands.INSERT;
			case "execute":return Commands.EXECUTE;
      }
     
      return Commands.ERROR;
    }
    public void uplink()
    {
      IList args=this[_args] as IList;
			if(args!=null)
      for(int i=0;i<args.Count;i++)
      {
        this["arg"+i]=args[i];
      }
    }
		public Operation(Commands com)
		{
			_command=com;
		}
		public Conditional createStack(Conditional oldstack, Conditional exeffect)
		{
			Conditional ret=new Conditional(false);
			List <Conditional> efs=new List<Conditional>();
			if(oldstack[_effects]==null)
				ret[_effects]=null;
			else
			{
				IList elist=oldstack[_effects] as IList;
  
				foreach(object ef in elist)
				{
					Conditional eff=ef as Conditional;
					Conditional efContain=new Conditional(false);
					if(eff.hasTag(TAG_STACKED))
						efContain.setTag(TAG_STACKED);
          if(eff[_effect]==exeffect)
					{
            continue;//maybe just skip it??
//						Debug.Log(string.Format("Stacking: {0}", ef));
				//		efContain.setTag(TAG_STACKED);

					}
					efContain[_effect]=eff[_effect];
          efs.Add(efContain);
				}
				ret[_effects]=efs;
			}
			ret[_upcond]=oldstack;
			ret[_target]=this;//current command becomes a target?
			ret[_currentCommand]=null;
			foreach(string nm in __stackValues)
				ret[nm]=oldstack[nm];
			ret[_Source]=exeffect;
			return ret;//TOFIX
		}
		static public object deRef(object a, Conditional stack)
		{
			if(a is string&&(a as string).StartsWith(_dr))
			{
				return stack[(a as string).Substring(_drl)];
			}
			return a;
		}
		void  __pureExecute(Conditional stack)
		{
	
    
      {
			if(stack.hasTag(TAG_ABORT))
				return;
//			Conditional target=stack[_target] as Conditional;
			//IList args=this[_args] as IList;
			switch(_command)
			{
			case Commands.CONDITION_AND:
			{
				string nm1=this["arg0"] as string;
				Condition cnds=stack[nm1] as Condition;
				if(cnds==null)
				{
#if THING
					Debug.Log("Cond_and : not condition");
#endif
				}
				else
				{
					Condition toAdd=deRef(this["arg1"], stack) as Condition;
					if(toAdd!=null)
					{
						Condition wrp=new Condition(Condition.Type.MULTI_AND,"_wrap",cnds,toAdd);
						stack[nm1]=wrp;
					}
				}
				deRef(this["arg1"], stack);
			}break;
			case Commands.CONDITION_OR:
			{
				string nm1=this["arg0"] as string;
				Condition cnds=stack[nm1] as Condition;
				if(cnds==null)
				{
					#if THING
					Debug.Log("Cond_or : not condition");
					#endif
				}
				else
				{
					Condition toAdd=deRef(this["arg1"], stack) as Condition;
					if(toAdd!=null)
					{
						Condition wrp=new Condition(Condition.Type.MULTI_OR,"_wrap",cnds,toAdd);
            stack[nm1]=wrp;
          }
        }
        deRef(this["arg1"], stack);
      }break;
      case Commands.TAG_SET:
				{
					(stack[this["arg0"] as string] as Conditional).setTag(deRef(this["arg1"],stack) as string);}
				break;
			case Commands.TAG_REMOVE:
			{
					(stack[this["arg0"] as string] as Conditional).removeTag(deRef(this["arg1"],stack) as string);}
				break;
      case Commands.TAG_SWITCH:
				{
        Conditional ct=(stack[this["arg0"]as string] as Conditional);
        if(ct.hasTag(this["arg1"] as string))
					{
						ct.removeTag(deRef(this["arg1"],stack) as string);
						ct.setTag(deRef(this["arg2"],stack) as string);
					}
				}
				break;
			case Commands.VALUE_SET:
				{
       // Conditional ct=(stack[this["arg0"] as string] as Conditional);
         // Debug.Log(this["arg0"] as string);
         // Debug.Log((this["arg1"] as string).Substring(_drl));
          //Debug.Log(stack["|-"]);
        if(this["arg1"]is string&&(this["arg1"] as string).StartsWith(_dr))
					{
				     stack[this["arg0"] as string]=stack[(this["arg1"] as string).Substring(_drl)];
					} else
          stack[this["arg0"] as string]=this["arg1"];}
				break;
        case Commands.INSERT:
        {
          Conditional cndi=stack[this["arg0"] as string] as Conditional;
          IList lst=stack[this["arg1"] as string] as IList;
          string vl=this["arg2"] as string;
          if(lst!=null&&cndi!=null&&cndi[vl]!=null)
          {

							double ins=System.Convert.ToDouble(cndi[vl]);
		
            int ind=0;
          foreach(object o in lst)
            {
              Conditional ccc=o as Conditional;
              if(ccc!=null&&ccc[vl]!=null)
              {
								ccc[vl]=System.Convert.ToDouble(ccc[vl]);
								double cr=System.Convert.ToDouble(ccc[vl]);

                if(ins.CompareTo(cr)<0) {break;};
              }
              ind++;
            }
            lst.Insert(ind,cndi);
           // Debug.Log(ind);
          }
          else
          {
						Debug.Log(string.Format("Invalid list : {0} {1} {2} {3} ",this["arg0"],cndi,this["arg1"],lst));
          }
          if(this["arg1"]is string&&(this["arg1"] as string).StartsWith(_dr))
          {
            stack[this["arg0"] as string]=stack[(this["arg1"] as string).Substring(_drl)];
          } else
            stack[this["arg0"] as string]=this["arg1"];}
          break;
			case Commands.ADD:
				{
        object a1=stack[this["arg0"] as string];
        object a2=deRef(this["arg1"], stack);
        stack[this["arg0"] as string]=System.Convert.ChangeType(System.Convert.ToDouble(a1)+System.Convert.ToDouble(a2), a1.GetType());}
				break;
			case Commands.SUBTRACT:
				{
         // Debug.Log(this["arg0"]);
          //Debug.Log(stack["castPlayer"]);
        object a1=stack[this["arg0"] as string];
         
        object a2=deRef(this["arg1"], stack);
        stack[this["arg0"] as string]=System.Convert.ChangeType(System.Convert.ToDouble(a1)-System.Convert.ToDouble(a2), a1.GetType());}
				break;
			case Commands.MULTIPLY:
				{
					object a1=stack[this["arg0"] as string];
					object a2=deRef(this["arg1"], stack);
					stack[this["arg0"] as string]=System.Convert.ChangeType(System.Convert.ToDouble(a1)*System.Convert.ToDouble(a2), a1.GetType());}
				break;
			case Commands.DIVIDE:
				{
					object a1=stack[this["arg0"] as string];
					object a2=deRef(this["arg1"], stack);
					stack[this["arg0"] as string]=System.Convert.ChangeType(System.Convert.ToDouble(a1)/System.Convert.ToDouble(a2), a1.GetType());}
				break;
			case Commands.ABORT:
				{
					stack.setTag(TAG_ABORT);
				}
				break;
			case Commands.CONTINUE:
				{
					stack.setTag(TAG_CONTINUE);

				}
				break;
			case Commands.NEW:
				{
					string nm=this["arg0"] as string;
					if(stack[nm] as Conditional==null)
					{
						Conditional cn=new Conditional();
						cn[_rootl]=stack[_rootl];

						stack[nm]=cn;
					}
				}
				break;
			case Commands.NEWLIST:
			{
				string nm=this["arg0"] as string;
				if(stack[nm] as IList==null)
					stack[nm]=new List<Conditional>();
			}
				break;
			case Commands.RETURN: //argument :  returned command/operation
				{
					stack.setTag(TAG_RETURN);
					stack[_returnValue]=this["arg0"] as Operation;
				}
				break;
        case Commands.EXECUTE:// arguments: ...none? XD I guess list name would work. arg[0]-> list name in stack this["arg1"]->list of commands
        {
          object oldtarget=stack[_target];
          string cname=this["arg0"] as string;
          Conditional pseudostack=stack[cname] as Conditional;
          if(pseudostack!=null)
          {
            IList lst=deRef(this["arg1"],stack) as IList;
            
            if(lst!=null)
            {
              object st= pseudostack[_effects];
              pseudostack[_effects]=stack[_effects];
              executeList(lst, pseudostack);
              pseudostack[_effects]=st;
             
            } else
            {
              #if THING
              Debug.Log(string.Format("Invalid execute argument: {0}", this["arg1"]));
              #endif
            }
          }
          stack[_target]=oldtarget;
        }
          break;
			case Commands.FOREACH:// arguments: ...none? XD I guess list name would work. arg[0]-> list name in stack this["arg1"]->list of commands
				{
        object oldtarget=stack[_target];
					string lname=this["arg0"] as string;
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
								} else
								{
									stack[_target]=newtarget;
									executeList(this["arg1"], stack);
									if(stack.hasTag(TAG_ABORT))
										return;
									stack.removeTag(TAG_CONTINUE);
								}
							}
						} else
						{
							#if THING
							Debug.Log(string.Format("Invalid foreach argument: {0}", lname));
							#endif
						}
					}
        stack[_target]=oldtarget;
      }
				break;
      case Commands.IF:// arguments: var name, cond, list
      {
 
        string vname=this["arg0"] as string;
        vname=deRef(vname,stack) as string;
        Conditional vr=stack[vname] as Conditional;
        Condition appl=this["arg1"] as Condition;
        if(vr==null||appl==null)
        {
          #if THING
						Debug.Log(string.Format("Invalid variable or condition in the if statement {0} {1} ",this["arg0"],this["arg1"] ));
          #endif
        }
        else
        {
			//Debug.Log(TAG_CONTINUE);
          if(appl.isFulfilled(vr,stack))
           executeList(this["arg2"], stack);
        }
       
      }
        break;
      case Commands.WHILE:// arguments: ...none? XD I guess list name would work. arg[0]-> list name in stack this["arg1"]->list of commands
      {
       // object oldtarget=stack[_target];
        Condition lname=this["arg0"] as Condition;
        if(lname!=null)
        {

            while(lname.isFulfilled(stack,stack[_Game] as Conditional))
            {
                executeList(this["arg1"], stack);
                if(stack.hasTag(TAG_ABORT))
                  return;
                stack.removeTag(TAG_CONTINUE);
              }
            }

        //stack[_target]=oldtarget;
      }
        break;
			case Commands.TARGET://will assign _targetList? value in stack arg0 - condition, arg1 - list, equivalent of accumulate for specific list
				{
					List<Conditional> targs=new List<Conditional>();
					Condition baseCond=this["arg0"] as Condition;
					if(baseCond==null)
					{
						#if THING
						Debug.Log(string.Format("Invalid target condition : {0}", this["arg0"]));
						#endif
						return;
					}
					IList vars=stack[this["arg1"] as string] as IList;
					if(vars==null)
					{
						#if THING
						Debug.Log(string.Format("Invalid target list : {0}", this["arg1"]));
						#endif
						return;
					}
					foreach(object potCn in vars)
					{
						Conditional check=potCn as Conditional;
						if(baseCond.isFulfilled(check,stack))
							targs.Add(check);
					}
					stack[_targetList]=targs;
				}
				;
				break;
			case Commands.ACCUMULATE://will assign arg2 value in stack arg0 - condition, arg1 - list. appends to list if it exists
				{
					List<Conditional> targs=new List<Conditional>();
					string listname=this["arg2"] as string;
					IList oldList=stack[listname] as IList;
					if(oldList!=null)
					{
						foreach(object l in oldList)
							targs.Add(l as Conditional);
					}
					Condition baseCond=this["arg0"] as Condition;
					if(baseCond==null)
					{
						#if THING
						Debug.Log(string.Format("Invalid target condition : {0}", this["arg0"]));
						#endif
						return;
					}
					IList vars=stack[this["arg1"] as string] as IList;
					if(vars==null)
					{
						#if THING
						Debug.Log(string.Format("Invalid target list : {0}", this["arg1"]));
						#endif
						return;
					}
					foreach(object potCn in vars)
					{
						Conditional check=potCn as Conditional;
						if(baseCond.isFulfilled(check,stack))
							targs.Add(check);
					}
					stack[listname]=targs;
				}
				;
				break;
			case Commands.CLEAR:
				{
          string nm=deRef(this["arg0"],stack) as string;
					if(nm!=null)
						stack[nm]=null;
				}
				;
				break;
			case Commands.POP: //arg0 - list name, arg1- stackvar
				{
          string nm=deRef(this["arg0"],stack) as string;
					IList lst=stack[nm] as IList;
					string retname=this["arg1"] as string;
					if(lst!=null&&retname!=null)
					{
						if(lst.Count>0)
						{
							stack[retname]=lst[0];
							lst.RemoveAt(0);
						}
					}
				}
				;
				break;
			case Commands.PUSH: //arg0 - list name, arg1- stackvar
				{

          string nm=deRef(this["arg0"],stack) as string;
					IList lst=stack[nm] as IList;
					string retname=this["arg1"] as string;
					if(lst==null)
					{
						lst=new List<object>();
						stack[nm]=lst;
					} //should be avoided...
					if(lst!=null&&retname!=null&&stack[retname]!=null)
					{
						lst.Insert(0, stack[retname]);
					}
				}
				;
				break;
			case Commands.SHIFT: //arg0 - list name, arg1- stackvar
				{
          string nm=deRef(this["arg0"],stack) as string;
					IList lst=stack[nm] as IList;
					string retname=this["arg1"] as string;
					if(lst!=null&&retname!=null)
					{
						if(lst.Count>0)
						{
							stack[retname]=lst[lst.Count-1];
							lst.RemoveAt(lst.Count-1);
						}
					}
				}
				;
				break;
			case Commands.APPEND: //arg0 - list name, arg1- stackvar
				{
          string nm=deRef(this["arg0"],stack) as string;
					IList lst=stack[nm] as IList;
					string retname=this["arg1"] as string;
					if(lst==null)
					{
						lst=new List<object>();
						stack[nm]=lst;
					} //should be avoided...
					if(lst!=null&&retname!=null&&stack[retname]!=null)
					{
						lst.Add(stack[retname]);
					}
				}
				;
				break;
			case Commands.REMOVE: // removes object from listarg0 - list name, arg1- stackvar
				{
          string nm=deRef(this["arg0"],stack) as string;
					IList lst=stack[nm] as IList;
					string retname=this["arg1"] as string;
					if(lst==null)
					{
						lst=new List<object>();
						stack[nm]=lst;
					} //should be avoided...
					if(lst!=null&&retname!=null&&stack[retname]!=null)
					{
						lst.Remove(stack[retname]);
					}
				}
				;
				break;
			case Commands.ANY: //get any (random) in list without deleting arg0 - list name, arg1- stackvar
				{
          string nm=deRef(this["arg0"],stack) as string;
					IList lst=stack[nm] as IList;
					string retname=this["arg1"] as string;
					if(lst!=null&&retname!=null)
					{
						if(lst.Count>0)
						{

							stack[retname]=lst[rngRange(0, lst.Count)];

						}
					}
				}
				;
				break;
			case Commands.HOOK: //call hook of name with data
				{
					//Monitor.Exit(gameLock);
          string nm=deRef(this["arg0"],stack) as string;
					string nmData=this["arg1"] as string;
					GameManager.startHook(nm, stack[nmData] as Conditional);
					//Monitor.Enter(gameLock);
				}
				;
				break;
			case Commands.CHOICE: //call for player choice, store result  in arg2: 
				{
					//Monitor.Exit(gameLock);
					string chname=deRef(this["arg0"],stack) as string;
					string nm=this["arg1"] as string;
					IList lst=stack[nm] as IList;
					if(lst==null)
					{
						#if THING
						Debug.Log(string.Format("Invalid choice list : {0}", nm));
						#endif
						return;
					}
					string nmRet=this["arg2"] as string;
					Conditional ret=GameManager.startChoice(chname, lst);
					//Monitor.Enter(gameLock);
					stack[nmRet]=ret;
          
				}
				;
				break;

			}
			;
      }
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
				Operation op=obj as Operation;
				if(op==null)
				{
					#if THING
					Debug.Log(string.Format("Invalid command: {0}", obj));
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
		void iterateOverEffects(IList efs, Conditional stack, string tag)
		{
			if(stack.hasTag(TAG_ABORT))
				return;
			bool didActivate=true;
			foreach(object o in efs)
			{
				Conditional preffect=o as Conditional;
				if(preffect!=null)
					preffect.removeTag(TAG_ACTIVATED);
			}
     // Debug.Log(efs.Count);
			while(didActivate)
			{
				didActivate=false;
        int ii=0;
				while(ii<efs.Count)
				{
          object o= efs[ii];
          ii++;
          //Debug.Log("effecting");
					Conditional preffect=o as Conditional;
					Conditional effect=null;
					if(preffect!=null&&!preffect.hasTag(TAG_ACTIVATED)&&!preffect.hasTag(TAG_STACKED))
					{
						effect=preffect[_effect] as Conditional;
           // Debug.Log(effect[_condition]);
						if(effect.hasTag(tag))
						{
							//Debug.Log(tag);
							Condition cn=effect[_condition] as Condition;
							if(cn==null)
							{
								Debug.Log(string.Format("Invalid effect condition {0}", effect[_condition]));
							} else
							{
								if(cn.isFulfilled(stack,stack[_Game] as Conditional)) //execute effect
								{

									preffect.setTag(TAG_ACTIVATED);
									didActivate=true;
									Operation ret=executeList(effect[_commands], createStack(stack, effect));
									if(ret!=null)
                  {
                    ret.uplink();
                   
										ret.__pureExecute(stack);
                  }
									if(stack.hasTag(TAG_ABORT)||stack.hasTag(TAG_CONTINUE))
										return;
								}
							}

						}

					
					} else
					{
            if(preffect==null)
						Debug.Log(string.Format("Invalid effect {0}", o));
					}
				}
			}
		}
		public void _execute(Conditional stack)
		{
      uplink();
			stack[_currentCommand]=this;
			IList efs=stack[_effects] as IList;
			if(efs==null)
			{
				#if THING
				Debug.Log("Possibly invalid setting of _effects in stack");
				#endif
			} else
			{
				stack.setTag(EXECUTE_PREFIX);
				iterateOverEffects(efs, stack, EXECUTE_PREFIX);
				stack.removeTag(EXECUTE_PREFIX);
				if(stack.hasTag(TAG_ABORT))
					return;
			}
			__pureExecute(stack);
			if(stack.hasTag(TAG_ABORT))
				return;
			if(efs!=null)
			{
				stack.setTag(EXECUTE_POSTFIX);
				iterateOverEffects(efs, stack, EXECUTE_POSTFIX);
				stack.removeTag(EXECUTE_POSTFIX);
				if(stack.hasTag(TAG_ABORT))
					return;
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
			TRUE,
      ///for commands/operations
      COMMAND_TYPE,
      COMMAND_ARG,
			ISSET,
      SELF,
      REFCOMP
		}
		public bool inverse;
		public Type type;
		//public Type type_compound;//type to use for compound conditions
		public string  variables; //variable name to compare OR tag
		//public string variable_compound;
		public object[] values;//value(s) to compare to
		public Condition()
		{
			inverse=false;
		}
		public Condition(Type t, string   var, params object[] val)
		{
			inverse=false;
			type=t;
			variables=var;
			values=new object [val.Length];
			for(int i=0; i<val.Length; i++)
			{
				values[i]=val[i];
			}

		}
		public bool isFulfilled(Conditional cnd,Conditional upstage)
		{
      object oldcond=cnd[_upcond];
      cnd[_upcond]=upstage;
			bool res=__isFulfilled(cnd);
      cnd[_upcond]=oldcond;
			if(inverse)
				return !res;
			return res;
		}
		protected bool __isFulfilled(Conditional cnd)
		{
     // Debug.Log(type);
			if(type==Type.TRUE)
				return true;
			string variable=variables;
      if(type==Type.ISSET)
			{
				object var=cnd[variable];
				if(var!=null) return true;
				return false;
			}
      if(type==Type.SELF)
      {
        Condition var=cnd[variable] as Condition;
				//Debug.Log(variable+var.ToString());
				if(var!=null)
        return var.isFulfilled(cnd,cnd[_upcond] as Conditional);
				else
					return false;
      }
      if(type==Type.COMMAND_TYPE)
      {
        if(values.Length<1)
          return false;
        object var=cnd[variable];
        if(var==null||!(var is Operation))
        {
          return false;
        }
        Operation op= var as Operation;

        string cndn=values[0] as string;
        Operation.Commands cmd=Operation.getTypeFromString(cndn);
      
        if(cmd==Operation.Commands.ERROR) return false;
        return (op.command==cmd);
      }
      if(type==Type.COMMAND_ARG)
      {
        if(values.Length<2)
          return false;
        object var=cnd[variable];
        if(var==null||!(var is Operation))
        {
          return false;
        }
        Operation op= var as Operation;
        int nm=(int)values[0];
        Condition scond=values[1] as Condition;
        if(scond==null) return false;
        IList lsta=op[_args] as IList;
        if(lsta.Count<=nm) return false;
        Conditional secval=new Conditional(false);
        secval["_arg"]=lsta[nm];
        return scond.isFulfilled(secval,cnd);
      }
      if(type==Type.REFCOMP) //deref? ;)
      {
        if(variable.StartsWith(_dr))
          variable=cnd[variable.Substring(_drl)] as string;
        if(values.Length<1) return false;
        string valstr=values[0] as string;
        if(valstr==null) return false;
        if(valstr.StartsWith(_dr))
          valstr=cnd[valstr.Substring(_drl)] as string;
        if(valstr==null) return false;
        if(variable==null) return false;
        if(cnd[variable]==cnd[valstr]) return true;
        return false;
      }
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

			if(type==Type.TAG) //also dereferenced
      {
       
        if(variable.StartsWith(_dr))
          variable=cnd[variable.Substring(_drl)] as string;

        return cnd.hasTag(variable);
      }
    

			if(type==Type.STRING)
			{
				if(values.Length<1)
					return false;
				string val2=cnd[variable] as string;
        string cmpr=(values[0] as string);
        if(cmpr.StartsWith(_dr))
          cmpr=cnd[cmpr.Substring(_drl)] as string;
       // Debug.Log(variable);
       // Debug.Log(cmpr);
       // Debug.Log(val2);
				if(val2==null)
					return false;
				return (val2==cmpr);
			}
			if(type==Type.COMPOUND_ALL||type==Type.COMPOUND_ANY||type==Type.COMPOUND_COUNT) //compounds! EW
			{

				if(values.Length<1)
					return false;
       
				if(values.Length<2&&type==Type.COMPOUND_COUNT)
					return false;
      
				Condition compcond=values[0] as Condition;
				if(compcond==null)
				{
					#if THING
					Debug.Log(string.Format("Invalid value for compound condition : {0}", values[0]));
					#endif
					return false;
				}
				object cval=cnd[variable];
                if(cval is Conditional) //a list of one?
				{
					List<Conditional> vl=new List<Conditional>();
					vl.Add (cval as Conditional);
					cval=vl;
				}
				if(!(cval is IList))
					return false;
				IList ccnds=cval as IList;
				if(ccnds==null)
					return false;
				int cnt=0;
				foreach(object ocn in ccnds)
				{
          Conditional cn=ocn as Conditional;
          if(cn==null) return false;
					bool res=compcond.isFulfilled(cn,cnd);
					if(type==Type.COMPOUND_ALL&&!res)
						return false;
					if(type==Type.COMPOUND_ANY&&res)
						return true;
					if(type==Type.COMPOUND_COUNT&&res)
						cnt=cnt+1;
				}
				if(type==Type.COMPOUND_ALL)
					return true;
				if(type==Type.COMPOUND_ANY)
					return false;
				if(type==Type.COMPOUND_COUNT)
				{
       
					Condition cnd2=values[1] as Condition;
					if(cnd2==null)
						return false;
					Conditional temp=new Conditional(false);
         // Debug.Log(string.Format("Count: {0}",cnt));
					temp[_count]=cnt;
					temp[_parent]=cnd;
					return cnd2.isFulfilled(temp,cnd);
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
						Debug.Log(string.Format("Invalid value for multi_and condition : {0}", obj));
						#endif
						return false;
					}
					if(!tcnd.isFulfilled(cnd,cnd[_upcond] as Conditional))
					{
						ok=false;
						break;
					}
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
						Debug.Log(string.Format("Invalid value for multi_or condition : {0}", obj));
						#endif
						return false;
					}
					if(tcnd.isFulfilled(cnd,cnd[_upcond] as Conditional))
					{
						ok=true;
						break;
					}
				}
				return ok;
			}

			// no good way to compare values, I guess...
			if(values.Length<1)
				return false;
			object val=cnd[variable];
			object ncmpval=values[0];
			if(values[0] is string)// try loading it as cnd name?
			{
				string st=values[0] as string;
				if(st!=null&&st.StartsWith(_dr))
				{
					string vname=st.Substring(_drl);
					ncmpval=cnd[vname];
				}
			}
			int cmp=0;
			if(val==null)
				return false;
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
				}
				;
				break;
			default:
				{
					System.Double cndval=System.Convert.ToDouble(ncmpval);
					System.Double cmpval=System.Convert.ToDouble(val);
					cmp=cmpval.CompareTo(cndval);

				}
				;
				break;
			}
			//Debug.Log(string.Format("{0}: {1} ({2}), {3}  {4} in {5} {6}",type,variable,val,values[0],ncmpval,cnd,cnd["slot"]));

			switch(type)
			{
			case Type.GREATER:
				{
					return (cmp==1);}
			case Type.LESS:
				{
					return (cmp==-1);}
			case Type.EQUAL:
				{
					return (cmp==0);}
			case Type.GE:
				{
					return (cmp==1||cmp==0);}
			case Type.LE:
				{
					return (cmp==-1||cmp==0);}
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
				Debug.Log(string.Format("Invalid time shifting at Event {0} of time {1}", this, shift));
				#endif
				shift=cdelay;
			}
			cdelay-=shift;
			this[_delay]=cdelay;
			return shift;
		}
	}


}


