using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ValveControl : Interactable {

    public float rotateScale;

    [Header("Configuration")]
    [Range(0.01f, 1)]
    public float pullDelay = .05f;
    public float Accuracy = 0.01f;
    public float BreakDistance = .4f;

    const float raduis = 0.4f;

    private Transform handle;
    private Vector3 handlePos;

    // Use this for initialization
    void Start () {
        handle = transform.Find("Handle");
    }

    // Update is called once per frame
    void Update() {
        if (attachedController != null) {
            Vector3 localPos = this.transform.InverseTransformPoint(attachedController.transform.position);
            handlePos = handle.position;

            float dist = Vector3.Distance(attachedController.transform.position, handlePos);

            if (dist > BreakDistance) {
                // Send a click to the controller if we disconnect.
                attachedController.input.TriggerHapticPulse(2999);
                DetachController();
                return;
            }

            float yAngle = Mathf.Atan2(localPos.x, localPos.z) * Mathf.Rad2Deg - transform.rotation.y;
            if (Math.Abs(yAngle) > Accuracy) {
                float hapticStrength = 2999 * (Math.Abs(yAngle) + .1f) * 0.5f;
                hapticStrength = Mathf.Clamp(hapticStrength, 0, 2999);
                attachedController.input.TriggerHapticPulse((ushort)hapticStrength);
            }


            transform.Rotate(new Vector3(0, yAngle, 0));
            if (attachedController.selected != null) {
                attachedController.selected.transform.Rotate(new Vector3(0, yAngle, 0));
            }
        }
    }

    public override void OnHoverEnter(WandController ctrl) {
        ctrl.input.TriggerHapticPulse((ushort)(1000 * 1f));
        handle.GetComponent<MeshRenderer>();
    }
}
