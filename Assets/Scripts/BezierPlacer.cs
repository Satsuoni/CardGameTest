using UnityEngine;
using System.Collections;

public class BezierPlacer : MonoBehaviour {

	public RectTransform P0;
	public Vector2 d0;
	public RectTransform P1;
	public Vector2 d1;
	public float aspect=0.7f;
	public int numSpawn=7;
	public RectTransform spawn;

	// Use this for initialization
	Vector4 xvars;
	Vector4 yvars;

	Vector4 pos0;
	Vector4 pos1;
	RectTransform [] spawned;
	public RectTransform [] slots {get {return spawned; }}


	float CCurve(Vector4 curve,float t)
	{
		return curve.x*t*t*t+curve.y*t*t+curve.z*t+curve.w;
	}
	float eps=0.0001f;
	void cuberoot(ref float a,ref float b)
	{
		float an=Mathf.Atan2(b,a)/3.0f;
		a=Mathf.Cos(an);
		b=Mathf.Sin(an);
	}

	float [] getZeros(Vector4 curve)
	{
		float [] ret=null;
		if(Mathf.Abs(curve.x)<eps)
		{
			if(Mathf.Abs(curve.y)<eps)
			{
				ret=new float[1];
				ret[0]=-curve.w/(curve.z);
				return ret;
			}
			float d=curve.z*curve.z-4*curve.y*curve.w;
			if(d<0) return null;
			if(Mathf.Abs (d)<eps)
			{
				ret=new float[1];
				ret[0]=-curve.z/(2*curve.y);
				return ret;
			}
			ret=new float[2];
			d=Mathf.Sqrt(d);
			ret[0]=-(curve.z-d)/(2*curve.y);
			ret[1]=-(curve.z-d)/(2*curve.y);
			return ret;
		}

		float p=(3*curve.x*curve.z-curve.y*curve.y)/(3*curve.x*curve.x);
		float q=(2*curve.y*curve.y*curve.y-9*curve.x*curve.y*curve.z+27*curve.x*curve.x*curve.w)/(27*curve.x*curve.x*curve.x);
		float dx=-curve.y/(3*curve.x);

		float dd=q*q+(p*p*p*4.0f/27.0f);
		float mul=-p/3.0f;
		float ca,cb;

		if(dd<0) //complex...
		{
			ca=-q/2;
			cb=Mathf.Sqrt(Mathf.Abs(dd))/2;
		}
		else
		{
			ca=-q/2+Mathf.Sqrt(Mathf.Abs(dd))/2;
			cb=0;
		}
//		Debug.Log(ca);
	//	Debug.Log(cb);
		float mll=Mathf.Pow(ca*ca+cb*cb,1.0f/6.0f);
		float len=Mathf.Sqrt(ca*ca+cb*cb);
		float sq3=Mathf.Sqrt(3.0f)/2;
		ca/=len;
		cb/=len;
    bool sgn=(ca<0);
    float bkk=ca*mll;
		cuberoot(ref ca,ref cb);
  //  Debug.Log(ca);
 //   Debug.Log(cb);

		float w0r=ca*mll;
		float w1r=(-ca/2+sq3*cb)*mll;
		float w2r=(-ca/2-sq3*cb)*mll;

		float invml=0;
		if(mll>eps)
		 invml=mul/(mll*mll);
	
		if(Mathf.Abs(invml-1.0f)<eps) //all real?
		{
			if(Mathf.Abs(cb)<eps) //2 roots
			{
				ret=new float[2];
				ret[0]=w0r*(1+invml)+dx;
				ret[1]=w1r*(1+invml)+dx;
			}
			else //3 roots
			{
				ret=new float[3];
			//	Debug.Log("dx");
			//	Debug.Log(dx);
				ret[0]=w0r*(1+invml)+dx;
				ret[1]=w1r*(1+invml)+dx;
				ret[2]=w2r*(1+invml)+dx;
			}
		}
		else //should be 1 root XD
		{
			ret=new float[1];
      if(!sgn)
			ret[0]=w0r*(1+invml)+dx;
      else ret[0]=bkk*(1+invml)+dx;

		}
		return ret;


	}
	
	float getClosestParameter(Vector4 curve,float t0,float d)
	{
		float x0=CCurve(curve,t0);
		float minc=10;
		Vector4 dv=new Vector4(0,0,0,1);
		float [] solp=getZeros(curve-dv*(x0+d));
//    Debug.Log("plus");
		for(int i=0;i<solp.Length;i++)
		{
			if(solp[i]>t0&&solp[i]-t0<minc) minc=solp[i]-t0;
     /* if(Mathf.Abs(CCurve(curve-dv*(x0+d),solp[i]))>eps)
        {
        Debug.Log(curve-dv*(x0+d));
      Debug.LogFormat("sol: {0} {1}",solp[i],CCurve(curve-dv*(x0+d),solp[i]));
      }*/
		}
		solp=getZeros(curve-dv*(x0-d));
  //  Debug.Log("minus");
		for(int i=0;i<solp.Length;i++)
		{
			if(solp[i]>t0&&solp[i]-t0<minc) minc=solp[i]-t0;
     /* if(Mathf.Abs(CCurve(curve-dv*(x0-d),solp[i]))>eps)
      {
        Debug.Log(curve-dv*(x0-d));
        Debug.Log ((x0-d));
        Debug.LogFormat("sol: {0} {1}",solp[i],CCurve(curve-dv*(x0-d),solp[i]));
      }*/
     
		}
		if(minc<9)
			return t0+minc;
		else
			return -1;
	}
	float minval(Vector4 curve)
	{
		float mx=Mathf.Min(CCurve(curve,0),CCurve(curve,1));
		if(Mathf.Abs (curve.x)<eps)
		{
			if(Mathf.Abs (curve.y)<eps)
				return mx;
			return Mathf.Min(CCurve(curve,-curve.z/(2.0f*curve.y)),mx);
		}
		float d=4*curve.y*curve.y-4*3*curve.x*curve.z;
		if(d<0)
		 return Mathf.Min(CCurve(curve,0),CCurve(curve,1));
		d=Mathf.Sqrt(d);
		float s1=(-2*curve.y-d)/(2*3*curve.x);
		float s2=(-2*curve.y+d)/(2*3*curve.x);
		if(s1>0&&s1<1)
		mx=Mathf.Min(CCurve(curve,s1),mx);
		if(s2>0&&s2<1)
		mx=Mathf.Min(CCurve(curve,s2),mx);
		return mx;
	}
	float maxval(Vector4 curve)
	{
		float mx=Mathf.Max(CCurve(curve,0),CCurve(curve,1));
		if(Mathf.Abs (curve.x)<eps)
		{
			if(Mathf.Abs (curve.y)<eps)
				return mx;
			return Mathf.Max(CCurve(curve,-curve.z/(2.0f*curve.y)),mx);
		}
		float d=4*curve.y*curve.y-4*3*curve.x*curve.z;
		if(d<0)
			return mx;
		d=Mathf.Sqrt(d);
		float s1=(-2*curve.y-d)/(2*3*curve.x);
		float s2=(-2*curve.y+d)/(2*3*curve.x);
		if(s1>0&&s1<1)
			mx=Mathf.Max(CCurve(curve,s1),mx);
		if(s2>0&&s2<1)
			mx=Mathf.Max(CCurve(curve,s2),mx);
		return mx;
	}
  float mnpos(float f1,float f2)
  {
    if(Mathf.Min(f1,f2)<0)
      return Mathf.Max(f1,f2);
    return Mathf.Min(f1,f2);
  }
	float remgap(float szx,float szy)
	{
		float cp=0;
    Debug.Log(szx);
    Debug.Log(numSpawn);
		for(int i=0;i<numSpawn;i++)
		{
			//center
      float dp=mnpos(getClosestParameter(xvars,cp,szx/2),getClosestParameter(yvars,cp,szy/2));
//			float dp=Mathf.Min(getClosestParameter(yvars,cp,szy/2),100);
			cp=dp;
			if(cp>1) return -1;
			if(cp<0) return -1;
			//out
			Debug.Log(i);
			Debug.Log(dp);
		//	Debug.LogFormat("cyy: {0}",CCurve(yvars,cp));
      dp=mnpos(getClosestParameter(xvars,cp,szx/2),getClosestParameter(yvars,cp,szy/2));
		//	dp=Mathf.Min(getClosestParameter(yvars,cp,szy/2),100);
			cp=dp;
			;
			if(cp>1) return -1;
			if(cp<0) return -1;
		}
		return 1.0f-cp;
	}
	void Rerender()
	{
		Rect r0=P0.RootCanvasRect();
		Rect r1=P1.RootCanvasRect();
		pos0=new Vector4(r0.center.x,r0.center.y,d0.x,d0.y);
		pos1=new Vector4(r1.center.x,r1.center.y,d1.x,d1.y);
		xvars.w=pos0.x;
		xvars.z=pos0.z;
		float ab=pos1.x-pos0.x-pos0.z;
		float a3b2=pos1.z-pos0.z;
		xvars.x=a3b2-2*ab;
		xvars.y=ab-xvars.x;

		yvars.w=pos0.y;
		yvars.z=pos0.w;
		ab=pos1.y-pos0.y-pos0.w;
		a3b2=pos1.w-pos0.w;
		yvars.x=a3b2-2*ab;
		yvars.y=ab-yvars.x;
		//Debug.Log(pos0);
     //		Debug.Log(pos1);
		//Debug.LogFormat("{0} {1} {2} {3} ",CCurve(xvars,0),CCurve(yvars,0),CCurve(xvars,1),CCurve(yvars,1));
		float bdx=maxval(xvars)-minval(xvars);
		float bdy=maxval(yvars)-minval(yvars);
		float minsz=1;//Mathf.Min(bdx,bdy/aspect)/numSpawn;
		//minsz/=2;
		float maxsz=2*(bdx+aspect*bdy)/numSpawn;
		while(maxsz-minsz>0.1f)
		{
			float mp=(maxsz+minsz)/2;
		float ma=remgap(minsz,minsz/aspect);
		float mb=remgap(maxsz,maxsz/aspect);
		float mm=remgap(mp,mp/aspect);
		//	Debug.LogFormat("minsz: {0} ma : {1}",minsz,ma);
		if(ma<0)
		{
			Debug.LogError("Doesn't fit for some reason");
			return;
		}
		if(mm<0)
		{
				maxsz=mp;
		}
		else
			{
				if(mm<ma) minsz=mp;
				else
					maxsz=mp;
			}
		}
		Debug.Log(minsz);
		Debug.Log(remgap(minsz,minsz/aspect));
		RectTransform self=transform as RectTransform;

		Debug.LogFormat("{0} {1} {2} {3} ",minval(xvars),minval(yvars),bdx,bdy);
		self.assignRectAnchors((self.parent as RectTransform).getAnchorsFromCanvasRect(new Rect(minval(xvars),minval(yvars),bdx,bdy)));
		float cp=0;
		for(int i=0;i<numSpawn;i++)
		{
			//center
			float cx=CCurve(xvars,cp),cy=CCurve(yvars,cp);
      float dp=mnpos(getClosestParameter(xvars,cp,minsz/2),getClosestParameter(yvars,cp,minsz/(2*aspect)));
			//float dp=Mathf.Min(getClosestParameter(yvars,cp,minsz/(2*aspect)),100);//getClosestParameter(yvars,cp,minsz/(2*aspect)));
			cp=dp;

		//	Debug.LogFormat("dx: {0} dy: {1} ",CCurve(xvars,cp)-cx,CCurve(yvars,cp)-cy);
			cx=CCurve(xvars,cp);
			cy=CCurve(yvars,cp);
			//Debug.LogFormat("cx :{0} cy: {1}, cp: {2}",cx,cy,cp);
			spawned[i].assignRectAnchors(self.getAnchorsFromCanvasRect(new Rect(cx-minsz/2,cy-minsz/(2*aspect),minsz,minsz/aspect)));
			//out
      dp=mnpos(getClosestParameter(xvars,cp,minsz/2),getClosestParameter(yvars,cp,minsz/(2*aspect)));
			//dp=Mathf.Min(getClosestParameter(yvars,cp,minsz/(2*aspect)),100);
			cp=dp;
			//Debug.LogFormat("2: dx: {0} dy: {1} ",CCurve(xvars,cp)-cx,CCurve(yvars,cp)-cy);
		}
	}
	void Start () {
		Canvas.ForceUpdateCanvases();
		spawned=new RectTransform[numSpawn];
		for(int i=0;i<numSpawn;i++)
		{
			GameObject go=Instantiate(spawn.gameObject) as GameObject;
			RectTransform tr=go.transform as RectTransform;
			tr.SetParent(transform,false);
			spawned[i]=tr;
		}
		Rerender();
    Vector4 nv=new Vector4(2221.0f, -2831.5f, 0.0f, 1109.4f);
    float [] sols=getZeros(nv);
		Debug.Log("solutions");

		foreach(float s in sols)
    {Debug.LogFormat("{0} : {1}",s,CCurve(nv,s));
    }
	}

	// Update is called once per frame
	void Update () {
		Rect r0=P0.rect;
		Rect r1=P1.rect;
		Vector4 npos0=new Vector4(r0.center.x,r0.center.y,d0.x,d0.y);
		Vector4 npos1=new Vector4(r1.center.x,r1.center.y,d1.x,d1.y);
		if((npos0-pos0).sqrMagnitude+(npos1-pos1).sqrMagnitude>0.05f)
		{
			//Rerender();
		}
	}
}
