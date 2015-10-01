using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Sprites;

[ExecuteInEditMode]
public class DeformeshImage : Graphic {
	public Sprite sprite;
	int numdef=5;
	// Use this for initialization

	// Update is called once per frame

	public override Texture mainTexture
	{
		get { return sprite == null ? s_WhiteTexture : sprite.texture; }
	}
	
	protected override void OnPopulateMesh(Mesh m)
	{
		Vector2 corner1 = new Vector2(0f, 0f);
		Vector2 corner2 = new Vector2(1f, 1f);
		
		corner1.x -= rectTransform.pivot.x;
		corner1.y -= rectTransform.pivot.y;
		corner2.x -= rectTransform.pivot.x;
		corner2.y -= rectTransform.pivot.y;
		
		corner1.x *= rectTransform.rect.width;
		corner1.y *= rectTransform.rect.height;
		corner2.x *= rectTransform.rect.width;
		corner2.y *= rectTransform.rect.height;

		Vector2 dc=corner2-corner1;
		Vector4 uv = sprite == null ? Vector4.zero : DataUtility.GetOuterUV(sprite);
		Vector2 uvz=new Vector2(uv.x, uv.y);
		Vector2 duv=new Vector2(uv.z, uv.w)-uvz;
		using (var vh = new VertexHelper())
		{
			float dx=1.0f/(float)(numdef);
			int numer=0;
			for(int yy=0;yy<numdef;yy++)
			for(int xx=0;xx<numdef;xx++)
			{
				Vector2 c1 = corner1+new Vector2(xx*dc.x*dx, yy*dc.y*dx);
				Vector2 c2 = corner1+new Vector2((xx+1)*dc.x,(yy+1)*dc.y)*dx;

				float rx1=dx*xx;
				float rx2=dx*(xx+1);
				float ry1=dx*yy;
				float ry2=dx*(yy+1);
				rx1*=rx1;
				rx2*=rx2;
				ry1=Mathf.Sqrt(ry1);
				ry2=Mathf.Sqrt(ry2);
				Vector2 uv0 =uvz+new Vector2(rx1*duv.x,ry1*duv.y);
				Vector2 uv1 =uvz+new Vector2(rx2*duv.x,ry2*duv.y);
			vh.AddVert(new Vector3(c1.x, c1.y), color, uv0);
			vh.AddVert(new Vector3(c1.x, c2.y), color, new Vector2(uv0.x, uv1.y));
			vh.AddVert(new Vector3(c2.x, c2.y), color, uv1);
			vh.AddVert(new Vector3(c2.x, c1.y), color, new Vector2(uv1.x, uv0.y));
				vh.AddTriangle(0+numer*4, 1+numer*4, 2+numer*4);
				vh.AddTriangle(2+numer*4, 3+numer*4, 0+numer*4);
				numer++;
			}
			

			
			vh.FillMesh(m);
		}
	}
}
