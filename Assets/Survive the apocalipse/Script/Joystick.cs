using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
	
	//public
	//point of joystick)
	public Image point; 
	public Image bg;
		
	//output horizontal and vertical direction (-1 to 1)
	public Vector3 output = Vector3.zero;
	
	//private 
	private Vector2 startpos;
	public bool _isDrag = false;

    public float x;
    public float y;

	public void OnDrag(PointerEventData data) {
		Vector2 pos = Vector2.zero;
		
		if(RectTransformUtility.ScreenPointToLocalPointInRectangle(bg.rectTransform, data.position, data.pressEventCamera, out pos)) {
			pos.x = (pos.x/bg.rectTransform.sizeDelta.x);
			pos.y = (pos.y/bg.rectTransform.sizeDelta.y);

			output = new Vector3(pos.x, 0, pos.y);
			output = (output.magnitude > 1) ? output.normalized : output;
			
			point.rectTransform.anchoredPosition = new Vector3(output.x * (bg.rectTransform.sizeDelta.x / 3), output.z * (bg.rectTransform.sizeDelta.y / 3));
		}
	}

	public void OnBeginDrag(PointerEventData data) {
		_isDrag = true;
	}
	
	public void OnEndDrag(PointerEventData data) {
		_isDrag = false;
		output = Vector3.zero;
		point.rectTransform.anchoredPosition = Vector2.zero;
	}
	
	public void OnPointerDown(PointerEventData data) {
		OnDrag(data);
	}
	
	public void OnPointerUp(PointerEventData data) {
		_isDrag = false;
		output = Vector3.zero;
		point.rectTransform.anchoredPosition = Vector2.zero;
	}
}