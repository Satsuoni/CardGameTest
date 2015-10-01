using UnityEngine;
using System.Collections;

public class BezierPlacer : MonoBehaviour {

	//public RectTransform P0;
	public bool direction;
	public Vector2 d0;
	//public RectTransform P1;
	public Vector2 d1;
	public float aspect=0.7f;
	public int numSpawn=7;
	public RectTransform spawn;
	RectTransform self=null;
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
	float eps=0.000001f;
	void cuberoot(ref float a,ref float b)
	{
		float an=Mathf.Atan2(b,a)/3.0f;
		a=Mathf.Cos(an);
		b=Mathf.Sin(an);
	}
	float findNextZero(Vector4 curve, float curp)
	{
		if(curp>1) return -1;
		if(curp<0) return -1;
		float cval=CCurve(curve,curp);
		if(Mathf.Abs(cval)<eps) return curp+eps;
		float dp=0.001f;
		float np=curp+dp;
		while(CCurve(curve,np)*cval>0&&np<=1) np+=dp;

		if(np>1) return -1;
	//	Debug.LogFormat("np: {0} ",np);
		float cp=curp;
		while(np-cp>eps)
		{

			cval=CCurve(curve,cp);
			float nval=CCurve(curve,np);
			float mp=(cp+np)/2;
			float mval=CCurve(curve,mp);
			if(Mathf.Abs(mval)<eps) return mp;
			if(nval*mval>=0)
			{
				np=mp;
			}
			else
			{
				cp=mp;
			}
		}
//		Debug.LogFormat("pcp: {0} del: {1}",cp,curp);
		return cp;
	}
	/*float [] getZeros(Vector4 curve)
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

		Debug.Log(ca);
		Debug.Log(cb);
		float mll=Mathf.Pow(ca*ca+cb*cb,1.0f/6.0f);
		float len=Mathf.Sqrt(ca*ca+cb*cb);
		float sq3=Mathf.Sqrt(3.0f)/2;
		if(len>eps)
		{
		ca/=len;
		cb/=len;
		}
		else
		{
			Debug.Log("zeros");
			Debug.LogFormat("{0} {1} {2} av {3}",p,q,dd,dx);
		}

    bool sgn=(ca<0);
    float bkk=ca*mll;
		cuberoot(ref ca,ref cb);
    Debug.Log(ca);
    Debug.Log(cb);
		//
		float w0r=ca*mll;
		float w1r=(-ca/2+sq3*cb)*mll;
		float w2r=(-ca/2-sq3*cb)*mll;

		float invml=0;
		if(mll>eps)
		 invml=mul/(mll*mll);
//		Debug.Log(invml);
		if(Mathf.Abs(invml-1.0f)<eps) //all real?
		{
			if(Mathf.Abs(cb)<eps) //2 roots
			{
				ret=new float[2];
				ret[0]=improve(w0r*(1+invml)+dx,curve);
				ret[1]=improve(w1r*(1+invml)+dx,curve);
			}
			else //3 roots
			{
				ret=new float[3];
			//	Debug.Log("dx");
			//	Debug.Log(dx);
				ret[0]=improve(w0r*(1+invml)+dx,curve);
				ret[1]=improve(w1r*(1+invml)+dx,curve);
				ret[2]=improve(w2r*(1+invml)+dx,curve);
			}
		}
		else //should be 1 root XD
		{
			ret=new float[1];
      if(!sgn)
				ret[0]=improve(w0r*(1+invml)+dx,curve);
			else ret[0]=improve(bkk*(1+invml)+dx,curve);

		}
		return ret;


	}*/

	float getClosestParameter(Vector4 curve,float t0,float d)
	{
		float x0=CCurve(curve,t0);
//		float minc=10;
		Vector4 dv=new Vector4(0,0,0,1);
		float solp=findNextZero(curve-dv*(x0+d),t0);
		float soln=findNextZero(curve-dv*(x0-d),t0);
		return mnpos(solp,soln);
	//	solp=getZeros(curve-dv*(x0-d));
  
		//	return -1;
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
		varsFromPosWithSize(szx);
		float cp=0;
  //  Debug.Log(szx);
   // Debug.Log(numSpawn);
		float dp=0;;
		for(int i=0;i<numSpawn;i++)
		{
			//center

			//Debug.Log(dp);

			if(i!=numSpawn-1)
			{
				float psx=getClosestParameter(xvars,cp,szx);
				float psy=getClosestParameter(yvars,cp,szy);
			//	Debug.Log(i);
			//	Debug.LogFormat("Psx {0} {1} {2}",psx,psy,CCurve(yvars,psy)-CCurve(yvars,0));
             dp=mnpos(psx,psy);
			}
		//	dp=Mathf.Min(getClosestParameter(yvars,cp,szy/2),100);
			cp=dp;
//			Debug.Log(cp);
//			Debug.LogFormat("cyy: {0}",CCurve(yvars,cp));
			if(cp>1+eps) return -1;
			if(cp<0) return -1;
		}
		return 1.0f-cp;
	}
	void varsFromPosWithSize(float size)
	{
		float xsz=size;
		float ysz=xsz/aspect;
		Vector4 zpos0=pos0;
		Vector4 zpos1=pos1;
		if(direction)
		{
		zpos0.x+=xsz/2;
		zpos0.y+=ysz/2;
		zpos1.x-=xsz/2;
		zpos1.y-=ysz/2;
		}
		else
		{
			zpos0.x+=xsz/2;
			zpos0.y-=ysz/2;
			zpos1.x-=xsz/2;
			zpos1.y+=ysz/2;
		}
		xvars.w=zpos0.x;
		xvars.z=zpos0.z;
		float ab=zpos1.x-zpos0.x-zpos0.z;
		float a3b2=zpos1.z-zpos0.z;
		xvars.x=a3b2-2*ab;
		xvars.y=ab-xvars.x;
		
		yvars.w=zpos0.y;
		yvars.z=zpos0.w;
		ab=zpos1.y-zpos0.y-zpos0.w;
		a3b2=zpos1.w-zpos0.w;
		yvars.x=a3b2-2*ab;
		yvars.y=ab-yvars.x;
	}
	void Rerender()
	{
		//float multipl=1.0f;
		if(self==null) self=transform as RectTransform;
		Rect sr=self.RootCanvasRect();
		//Rect r0=P0.rect;
		//Rect r1=P1.rect;
		if(direction)
		{
			pos0=new Vector4(sr.xMin,sr.yMin,d0.x*sr.width,d0.y*sr.height);
			pos1=new Vector4(sr.xMax,sr.yMax,d1.x*sr.width,d1.y*sr.height);
		}
		else
		{
			pos0=new Vector4(sr.xMin,sr.yMax,d0.x*sr.width,d0.y*sr.height);
			pos1=new Vector4(sr.xMax,sr.yMin,d1.x*sr.width,d1.y*sr.height);
			
		}

		varsFromPosWithSize(0);

		//Debug.Log(pos0);
        //Debug.Log(pos1);
		//Debug.LogFormat("{0} {1} {2} {3} ",CCurve(xvars,0),CCurve(yvars,0),CCurve(xvars,1),CCurve(yvars,1));
		float bdx=maxval(xvars)-minval(xvars);
		float bdy=maxval(yvars)-minval(yvars);
		float minsz=10;//Mathf.Min(bdx,bdy/aspect)/numSpawn;
		//minsz/=2;
		float maxsz=2*(bdx+aspect*bdy)/numSpawn;
		int ilim=20;

		while(maxsz-minsz>0.1f)
		{
			float mp=(maxsz+minsz)/2;
			ilim--;
			if(ilim<0) break;
//			Debug.Log(maxsz-minsz);
		float ma=remgap(minsz,minsz/aspect);
		//float mb=remgap(maxsz,maxsz/aspect);
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
		varsFromPosWithSize(minsz);
		//RectTransform self=transform as RectTransform;

		Debug.LogFormat("{0} {1} {2} {3} ",minval(xvars),minval(yvars),bdx,bdy);
		//self.assignRectAnchors((self.parent as RectTransform).getAnchorsFromCanvasRect(new Rect(minval(xvars),minval(yvars),bdx,bdy)));
		float cp=0;
		float dp=0;
		for(int i=0;i<numSpawn;i++)
		{
			//center
			float cx=CCurve(xvars,cp),cy=CCurve(yvars,cp);

			//if(i!=0)
			 //dp=mnpos(getClosestParameter(xvars,cp,minsz/2),getClosestParameter(yvars,cp,minsz/(2*aspect)));
			//float dp=Mathf.Min(getClosestParameter(yvars,cp,minsz/(2*aspect)),100);//getClosestParameter(yvars,cp,minsz/(2*aspect)));
			cp=dp;

			//Debug.LogFormat("dx: {0} dy: {1} ",CCurve(xvars,cp)-cx,CCurve(yvars,cp)-cy);
			cx=CCurve(xvars,cp);
			cy=CCurve(yvars,cp);
			//Debug.LogFormat("cx :{0} cy: {1}, cp: {2}",cx,cy,cp);
			Rect crect=new Rect(cx-minsz/2,cy-minsz/(2*aspect),minsz,minsz/aspect);
			Vector4 lbt=self.getAnchorsFromCanvasRect(crect);
//			Debug.LogFormat("Anchors: {0} {1:0.000} {2:0.000} {3}",lbt,lbt.x,lbt.z,crect);
			spawned[i].assignRectAnchors(lbt);
			//out

            dp=mnpos(getClosestParameter(xvars,cp,minsz),getClosestParameter(yvars,cp,minsz/(aspect)));
			//dp=Mathf.Min(getClosestParameter(yvars,cp,minsz/(2*aspect)),100);
		//	Debug.LogFormat("2: dx: {0} ( {1} ) dy: {2} ({3} )",CCurve(xvars,dp)-CCurve(xvars,cp),minsz,CCurve(yvars,dp)-CCurve(yvars,cp),minsz/(aspect));
			cp=dp;

		}
	}
	void Start () {
		self=transform as RectTransform;
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
		//Vector4 nv=new Vector4(-2.9f, 1687.5f, -3366.4f, -1.4f);
//		Vector4 nv=new Vector4(-623.0f, 1958.0f, -2047.0f, -311.5f);



/*    float [] sols=getZeros(nv);
		Debug.Log("solutions");

		foreach(float s in sols)
    {Debug.LogFormat("{0} : {1}",s,CCurve(nv,s));
    }*/
	}

	// Update is called once per frame
	void Update () {
		Rect sr=self.rect;
		//Rect r0=P0.rect;
		//Rect r1=P1.rect;
		Vector4 npos0;
		Vector4 npos1;
		if(direction)
		{
			npos0=new Vector4(sr.xMin,sr.yMin,d0.x*sr.width,d0.y*sr.height);
			npos1=new Vector4(sr.xMax,sr.yMax,d1.x*sr.width,d1.y*sr.height);
		}
		else
		{
			npos0=new Vector4(sr.xMin,sr.yMax,d0.x*sr.width,d0.y*sr.height);
			npos1=new Vector4(sr.xMax,sr.yMin,d1.x*sr.width,d1.y*sr.height);

		}
		if((npos0-pos0).sqrMagnitude+(npos1-pos1).sqrMagnitude>0.05f)
		{
			//Rerender();
		}
	}
}
