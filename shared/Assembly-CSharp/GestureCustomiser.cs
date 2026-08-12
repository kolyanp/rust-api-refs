using Rust.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class GestureCustomiser : MonoBehaviour
{
	public CustomGestureWidget[] Shapes;

	public float Padding;

	public float Offset;

	public RustText SelectedGestureName;

	public Image SelectedGestureIcon;

	public Phrase EmptySlotName;

	public Sprite EmptySlotIcon;

	public GameObject PickerRoot;

	public GameObjectRef PickerPrefab;

	public Transform PickerContent;

	public RustText WheelHeader;

	public RustButton WheelRightButton;

	public RustButton WheelLeftButton;

	public VideoPlayer PreviewVideo;

	public RawImage VideoImage;

	public GameObject NoValidGesturesText;

	public GameObject EmptySlotInputHelp;

	public GameObject FilledSlotInputHelp;

	public GestureCustomiser()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Padding = 1f;
		EmptySlotName = new Phrase("empty_gesture_slot", "Empty Slot");
		((MonoBehaviour)this)._002Ector();
	}
}
