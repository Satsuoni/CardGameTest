using UnityEditor;
using UnityEngine;

public class uGUITools : MonoBehaviour {
	[MenuItem("uGUI/Anchors to Corners %[")]
	static void AnchorsToCorners(){
		RectTransform t = Selection.activeTransform as RectTransform;
		RectTransform pt = Selection.activeTransform.parent as RectTransform;
		UnityEngine.UI.AspectRatioFitter af=t.gameObject.GetComponent<UnityEngine.UI.AspectRatioFitter>();
		bool oe=true;
		if(af!=null){ oe=af.enabled;af.enabled=false;}
		if(t == null || pt == null) return;
		
		Vector2 newAnchorsMin = new Vector2(t.anchorMin.x + t.offsetMin.x / pt.rect.width,
		                                    t.anchorMin.y + t.offsetMin.y / pt.rect.height);
		Vector2 newAnchorsMax = new Vector2(t.anchorMax.x + t.offsetMax.x / pt.rect.width,
		                                    t.anchorMax.y + t.offsetMax.y / pt.rect.height);
		
		t.anchorMin = newAnchorsMin;
		t.anchorMax = newAnchorsMax;
		t.offsetMin = t.offsetMax = new Vector2(0, 0);
		if(af!=null) af.enabled=oe;
	}
	
	[MenuItem("uGUI/Corners to Anchors %]")]
	static void CornersToAnchors(){
		RectTransform t = Selection.activeTransform as RectTransform;
		
		if(t == null) return;
		
		t.offsetMin = t.offsetMax = new Vector2(0, 0);
	}
}