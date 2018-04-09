using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingLever : Interactable {

    public float rotateLimit = 15;

    [Header("Direction")]
    //[Range(0, 1)]
    public Vector3 direction;
    [Header("Configuration")]
    [Range(0.01f, 1)]                   // This attribute makes the field underneath into a slider in the editor.
    public float pullDelay = .05f;      // The responsiveness with which the lever matches the controller's position.
    public float Accuracy = 0.01f;
    public float BreakDistance = .4f;   // Distance in worldspace units at which the user is forced to let go of the lever.
    [Header("Spring")]
    public bool isSpring = false;      // If set, the handle returns to a resting position
    public float SpringDelay = .05f;    // The responsiveness with which the lever returns to its restingValue.
    public float RestingXValue = 0f;
    [Header("Haptics")]
    public float dragHaptics = .5f;     // Multiplier for the haptic feedback when dragging. 
    public float hoverHaptics = 1f;     // Multiplier for the haptic feedback when hovering. 

    private Transform handleBar;
    private Transform handle;

    private float Value;

    private void OnValidate() {
        
    }

    // Use this for initialization
    void Start () {
        handleBar = transform.Find("Handle");
        handle = transform.Find("Handle").Find("Ball");
        Value = handle.localPosition.z;
        direction = direction.normalized;
    }
	
	// Update is called once per frame
	void Update () {
		if (attachedController != null) {
            Vector3 controllerPos = attachedController.transform.position;

            float distanceToHandle = Vector3.Distance(controllerPos, handle.position);
            if (distanceToHandle > BreakDistance) {
                attachedController.input.TriggerHapticPulse(2999);
                DetachController();
                return;
            }

            Vector3 localPos = this.transform.InverseTransformPoint(controllerPos);

            float xAngle = Mathf.Atan2(localPos.y, localPos.z) * Mathf.Rad2Deg - 90;
            float xTransformAngle = Mathf.Clamp(-xAngle, -rotateLimit, rotateLimit);
            handleBar.localEulerAngles = new Vector3(xTransformAngle, handleBar.localEulerAngles.y, handleBar.localEulerAngles.z);
                if (Mathf.Abs(xAngle) > Accuracy) {
                    float hapticStrength = 2999 * (Mathf.Abs(xAngle) + .1f) * dragHaptics;
                    hapticStrength = Mathf.Clamp(hapticStrength, 0, 2999);
                    attachedController.input.TriggerHapticPulse((ushort)hapticStrength);
            
                }
            if (attachedController.selected != null) {
                attachedController.selected.transform.Translate(direction * (xTransformAngle / 10) * Time.deltaTime);
            }
        }
        else if (isSpring) {
            if (handleBar.localEulerAngles.x <= 60) {
                float newValue = Mathf.Lerp(handleBar.localEulerAngles.x, 0, (1 / SpringDelay) * Time.deltaTime);
                handleBar.localEulerAngles = new Vector3(newValue, handleBar.localEulerAngles.y, handleBar.localEulerAngles.z);
            }
            else if (handleBar.localEulerAngles.x > 300) {
                float newValue = Mathf.Lerp(handleBar.localEulerAngles.x, 360, (1 / SpringDelay) * Time.deltaTime);
                handleBar.localEulerAngles = new Vector3(newValue, handleBar.localEulerAngles.y, handleBar.localEulerAngles.z);
            }
        }
	}

    public override void OnHoverEnter(WandController ctrl) {
        ctrl.input.TriggerHapticPulse((ushort)(1500 * 1f));
        handle.GetComponent<MeshRenderer>();
    }
}
