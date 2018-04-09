using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleButton : Interactable {

    private bool pressed;

    public float scaleX, scaleY, scaleZ;

	// Use this for initialization
	void Start () {
        pressed = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (!pressed && attachedController != null) {
            pressed = true;
            float hapticStrength = 2999;
            hapticStrength = Mathf.Clamp(hapticStrength, 0, 2999);
            attachedController.input.TriggerHapticPulse((ushort)hapticStrength);
            Press();
        }

        if (pressed && attachedController == null) {
            pressed = false;
        }
	}

    private void Press() {
        if (attachedController.selected != null) {
            attachedController.selected.transform.localScale = Vector3.Scale(attachedController.selected.transform.localScale, new Vector3(scaleX, scaleY, scaleZ));
        }
    }

    public override void OnHoverEnter(WandController ctrl) {
        ctrl.input.TriggerHapticPulse((ushort)(500 * 1f));
    }
}
