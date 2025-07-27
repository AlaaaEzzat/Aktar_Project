using UnityEngine;
using UnityEngine.UI;

public class UIPlayerMover : MonoBehaviour
{


	// Call from UI event trigger → PointerUp
	public void OnUp() { PlayerController2D.Instance.MoveUpByButtonClick(); }
	public void OnDown() { PlayerController2D.Instance.MoveDownByButtonClick(); }
	public void OnLeft() { PlayerController2D.Instance.MoveLeftByButtonClick(); }
	public void OnRight() { PlayerController2D.Instance.MoveRightByButtonClick(); }
}
