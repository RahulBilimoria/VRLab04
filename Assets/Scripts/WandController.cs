using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
[RequireComponent(typeof(SteamVR_TrackedObject))]
public class WandController : MonoBehaviour {
    //Made Public so that it made be accessed from the Interactables
    public SteamVR_Controller.Device input;
    //Only one held item for now.
    public Interactable heldItem;
    public Text infoText;

    private string selectedObject = "";
    private string positionSO = "";
    private string rotationSO = "";
    private string scaleSO = "";

    public GameObject selected;
    private GameObject hover;
    private LineRenderer laser;
    private Material hMaterial;
    private Material sMaterial;
    private Material dMaterial;

    private void Start() {

        //Get Input obj
        var trackedObj = this.GetComponent<SteamVR_TrackedObject>();
        input = SteamVR_Controller.Input((int)trackedObj.index);

        updateText();
        // To make sure we only collide with the actual handles and knobs, We'll use the collisionMatrix.
        // The controller's layers are set to "Controllers" and the Interactables' are set to "Interactables"
        // We can then restrict collisions in Edit->ProjectSettings->Physics 

        hover = null;
        selected = null;
        hMaterial = Resources.Load<Material>("HoverHighlight");
        sMaterial = Resources.Load<Material>("ItemPickupOutline");
        dMaterial = Resources.Load<Material>("Default");
        input = SteamVR_Controller.Input((int)trackedObj.index);
        laser = GetComponent<LineRenderer>();
        laser.SetPosition(0, transform.position);
        laser.SetPosition(1, transform.position);

        if (gameObject.layer != LayerMask.NameToLayer("Controllers")) Debug.LogError("Controllers should be in 'Controllers' Collision Layer");
    }

    private void Update() {

        setSelectedText();
        updateText();
        //Let go of item. We don't clear heldItem here, as it's take care of in OnItemDetach.
        bool triggerReleased = input.GetPressUp(EVRButtonId.k_EButton_SteamVR_Trigger);
        if (heldItem && triggerReleased) {
            heldItem.DetachController();
        }

        selectObject();
    }

    public void OnItemDetach(Interactable item) {
        heldItem = null;
    }



    //Send HoverEnter message to Interactable
    private void OnTriggerEnter(Collider other) {
        // The collider we collided with may not be the root object of the interactable.
        // We assume that the rigidbody is.
        if (other.attachedRigidbody == null) return;
        var interactable = other.attachedRigidbody.GetComponent<Interactable>();
        if (interactable == null) return;
        if (heldItem == interactable) return;
        interactable.OnHoverEnter(this);
    }

    private void OnTriggerStay(Collider other) {
        if (other.attachedRigidbody == null) return;
        var interactable = other.attachedRigidbody.GetComponent<Interactable>();
        if (interactable == null) return;
        if (heldItem == interactable) return;
        // Start Interacting 
        if (input.GetPressDown(EVRButtonId.k_EButton_SteamVR_Trigger)) {
            bool didAttach = interactable.TryAttach(this);
            if (didAttach) heldItem = interactable;
        }
        else interactable.OnHoverStay(this);
    }
    //Send HoverExit message to Interactable
    private void OnTriggerExit(Collider other) {
        if (other.attachedRigidbody == null) return;
        var interactable = other.attachedRigidbody.GetComponent<Interactable>();
        if (interactable == null) return;
        if (heldItem == interactable) return;
        interactable.OnHoverExit(this);
    }

    private void selectObject() {
        if (hover != null && hover != selected) {
            hover.GetComponent<MeshRenderer>().material = dMaterial;
            hover = null;
        }
        if (input.GetTouch(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad)) {
            laser.SetPosition(0, transform.position);
            laser.SetPosition(1, transform.position + transform.forward * 10f);
            laser.material.mainTextureOffset = new Vector2(-Time.time, 0);
            RaycastHit collide;
            bool hit = Physics.Raycast(gameObject.transform.position, gameObject.transform.forward, out collide, 1000);
            if (hit) {
                if (collide.collider.tag.Equals("selectable")) {
                    hover = collide.collider.gameObject;
                    if (hover != selected) {
                        hover.GetComponent<MeshRenderer>().material = hMaterial;
                    }
                }
            }
        }
        else {
            laser.SetPosition(0, transform.position);
            laser.SetPosition(1, transform.position);
        }
        if (input.GetPressDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad)) {
            if (hover != null) {
                if (selected != null) {
                    selected.GetComponent<MeshRenderer>().material = dMaterial;
                }
                selected = hover;
                hover = null;
                selected.GetComponent<MeshRenderer>().material = sMaterial;
            }
            else {
                if (selected != null) {
                    selected.GetComponent<MeshRenderer>().material = dMaterial;
                }
                selected = null;
                removeSelectedText();
            }
        }
    }

    private void setSelectedText() {
        if (selected != null) {
            selectedObject = selected.name;
            positionSO = selected.transform.position.ToString();
            rotationSO = selected.transform.rotation.ToString();
            scaleSO = selected.transform.localScale.ToString();
        }
    }

    private void removeSelectedText() {
        selectedObject = "";
        positionSO = "";
        rotationSO = "";
        scaleSO = "";
    }

    private void updateText() {
        infoText.text = "Selected Object: " + selectedObject + "\nPosition Vector: " + positionSO + "\nRotation Vector: " + rotationSO + "\nScale Vector: " + scaleSO;
    }
}