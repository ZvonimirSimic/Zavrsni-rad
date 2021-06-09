using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace BNG {
    /// <summary>
    /// Physical button helper with 
    /// </summary>
    public class Button : MonoBehaviour {
        public bool pressed = false;
        public GameObject game;

        [Tooltip("The Local Y position of the button when it is pushed all the way down. Local Y position will never be less than this.")]
        public float MinLocalY = 0.25f;

        [Tooltip("The Local Y position of the button when it is not being pushed. Local Y position will never be greater than this.")]
        public float MaxLocalY = 0.55f;

        [Tooltip("How far away from MinLocalY / MaxLocalY to be considered a click")]
        public float ClickTolerance = 0.01f;

        [Tooltip("If true the button can be pressed by physical object by utiizing a Spring Joint. Set to false if you don't need / want physics interactions, or are using this on a moving platform.")]
        public bool AllowPhysicsForces = true;

        List<Grabber> grabbers = new List<Grabber>(); // Grabbers in our trigger
        List<UITrigger> uiTriggers = new List<UITrigger>(); // UITriggers in our trigger
        SpringJoint joint;

        bool clickingDown = false;
        public AudioClip ButtonClick;
        public AudioClip ButtonClickUp;

        public UnityEvent onButtonDown;
        public UnityEvent onButtonUp;

        AudioSource audioSource;
        Rigidbody rigid;

        void Start() {
            joint = GetComponent<SpringJoint>();
            rigid = GetComponent<Rigidbody>();

            // Set to kinematic so we are not affected by outside forces
            if(!AllowPhysicsForces) {
                rigid.isKinematic = true;
            }

            // Start with button up top / popped up
            transform.localPosition = new Vector3(transform.localPosition.x, MaxLocalY, transform.localPosition.z);

            audioSource = GetComponent<AudioSource>();
        }

        // These have been hard coded for hand speed
        float ButtonSpeed = 15f;
        float SpringForce = 1500f;
        Vector3 buttonDownPosition;
        Vector3 buttonUpPosition;


        void Update() {

            buttonDownPosition = GetButtonDownPosition();
            buttonUpPosition = GetButtonUpPosition();
            bool grabberInButton = false;
            bool UITriggerInButton = uiTriggers != null && uiTriggers.Count > 0;

            // Find a valid grabber to push down
            for (int x = 0; x < grabbers.Count; x++) {
                if (!grabbers[x].HoldingItem) {
                    grabberInButton = true;
                    break;
                }
            }
            // push button down
            if (grabberInButton || UITriggerInButton) {
                float speed = ButtonSpeed; 
                transform.localPosition = Vector3.Lerp(transform.localPosition, buttonDownPosition, speed * Time.deltaTime);

                if(joint) {
                    joint.spring = 0;
                }
            }
            else {
                // Let the spring push the button up if physics forces are enabled
                if (AllowPhysicsForces) {
                    if(joint) {
                        joint.spring = SpringForce;
                    }
                }
                // Need to lerp back into position if spring won't do it for us
                else {
                    float speed = ButtonSpeed;
                    transform.localPosition = Vector3.Lerp(transform.localPosition, buttonUpPosition, speed * Time.deltaTime);
                    if(joint) {
                        joint.spring = 0;
                    }
                }
            }

            // Cap values
            if (transform.localPosition.y < MinLocalY) {
                transform.localPosition = buttonDownPosition;
            }
            else if (transform.localPosition.y > MaxLocalY) {
                transform.localPosition = buttonUpPosition;
            }

            // Click Down?
            float buttonDownDistance = transform.localPosition.y - buttonDownPosition.y;
            if (buttonDownDistance <= ClickTolerance && !clickingDown) {
                clickingDown = true;
                OnButtonDown();
            }
            // Click Up?
            float buttonUpDistance = buttonUpPosition.y - transform.localPosition.y;
            if (buttonUpDistance <= ClickTolerance && clickingDown) {
                clickingDown = false;
                OnButtonUp();
            }
        }

        public virtual Vector3 GetButtonUpPosition() {
            return new Vector3(transform.localPosition.x, MaxLocalY, transform.localPosition.z);
        }

        public virtual Vector3 GetButtonDownPosition() {
            return new Vector3(transform.localPosition.x, MinLocalY, transform.localPosition.z);
        }

        // Callback for ButtonDown
        public virtual void OnButtonDown() {

            // Play sound
            if (audioSource && ButtonClick) {
                audioSource.clip = ButtonClick;
                audioSource.Play();
            }

            // Call event
            if (onButtonDown != null) {
                if (pressed == false)
                {
                    GameObject desk = GameObject.FindGameObjectWithTag("Desk");
                    Vector3 newPos = new Vector3(desk.transform.position.x, desk.transform.position.y + 0.5F, desk.transform.position.z);
                    Vector3 scal = new Vector3(0.05F, 0.05F, 0.05F);
                    //gam = Instantiate(gameObj, newPos, new Quaternion(0, 0, 0, 0));
                    //newPos = new Vector3(0.05F, 0.05F, 0.05F);
                    //gam.transform.localScale = newPos;

                    /*Grabbable grab = gam.AddComponent<Grabbable>();
                    grab.SecondaryGrabBehavior = OtherGrabBehavior.DualGrab;
                    grab.GrabMechanic = GrabType.Precise;
                    grab.GrabButton = GrabButton.Grip;
                    grab.GrabPhysics = GrabPhysics.Kinematic;
                    grab.Grabtype = HoldType.HoldDown;
                    grab.HideHandGraphics = true;
                    grab.ParentToHands = true;
                    grab.ParentHandModel = false;
                    grab.SnapHandModel = true;
                    //grab.RemoteGrabbable = true;
                    Rigidbody rigid = gam.AddComponent<Rigidbody>();
                    rigid.isKinematic = true;
                    rigid.useGravity = true;
                    CapsuleCollider capsuleCollider = gam.AddComponent<CapsuleCollider>();
                    capsuleCollider.center = transform.localPosition;
                    capsuleCollider.height = transform.lossyScale.y;
                    capsuleCollider.radius = 1;*/

                    GameObject gamObj = new GameObject();
                    gamObj = game;

                    if (gamObj.GetComponent<Grabbable>() == null)
                    {
                        gamObj.AddComponent<Grabbable>();
                        gamObj.GetComponent<Grabbable>().SecondaryGrabBehavior = OtherGrabBehavior.DualGrab;
                        gamObj.GetComponent<Grabbable>().GrabMechanic = GrabType.Precise;
                        gamObj.GetComponent<Grabbable>().GrabButton = GrabButton.Grip;
                        gamObj.GetComponent<Grabbable>().GrabPhysics = GrabPhysics.Kinematic;
                        gamObj.GetComponent<Grabbable>().Grabtype = HoldType.HoldDown;
                        gamObj.GetComponent<Grabbable>().HideHandGraphics = true;
                        gamObj.GetComponent<Grabbable>().ParentToHands = true;
                        gamObj.GetComponent<Grabbable>().ParentHandModel = false;
                        gamObj.GetComponent<Grabbable>().SnapHandModel = true;
                    }
                    if (gamObj.GetComponent<Rigidbody>() == null)
                    {
                        gamObj.AddComponent<Rigidbody>();
                        gamObj.GetComponent<Rigidbody>().isKinematic = true;
                        gamObj.GetComponent<Rigidbody>().useGravity = true;
                    }
                    if (gamObj.GetComponent<BoxCollider>() == null)
                    {
                        //game.AddComponent<BoxCollider>();
                        gamObj.AddComponent<BoxCollider>();
                    }
                    gamObj.GetComponent<BoxCollider>().size = new Vector3(3, 3, 3);
                    GameObject go = new GameObject();
                    go.AddComponent<TextMeshPro>();
                    GameObject newObject = new GameObject();
                    newObject = Instantiate(gamObj, newPos, gamObj.transform.rotation);
                    newObject.transform.localScale = new Vector3(newObject.transform.localScale.x / 50, newObject.transform.localScale.y / 50, newObject.transform.localScale.z / 50);
                    go.transform.SetParent(newObject.transform);
                    go.transform.position = new Vector3(newObject.transform.position.x, newObject.transform.position.y - newObject.transform.localScale.y*7, newObject.transform.position.z);
                    go.transform.localScale /= 30;
                    //game.SetActive(true);
                    //gamObj.SetActive(true);
                    newObject.SetActive(true);
                    onButtonDown.Invoke();
                    pressed = true;
                }
                /* MeshCollider meshCollider = gam.AddComponent<MeshCollider>();
                 meshCollider.convex = true;
                 meshCollider.enabled = true;
                 MeshRenderer meshRenderer = gam.AddComponent<MeshRenderer>();
                 meshRenderer.enabled = true;
                 MeshFilter meshFilter = gam.AddComponent<MeshFilter>();
                 meshCollider.transform.position = gam.transform.position;
                 meshCollider.transform.localPosition = gam.transform.localPosition;*/

                /*foreach (var child in gam.transform)
                    child.position += transform.position;
                transform.position = Vector3.zero;
                transform.rotation = Quaternion.identity;*/

                //MeshFilter meshFilt = gam.GetComponentInChildren<MeshFilter>();
                /*for (int i = 0; i < gam.transform.childCount; i++)
                {
                    meshFilter.mesh = gam.transform.GetChild(i).GetComponent<MeshFilter>().mesh;
                }*/
                /*CapsuleCollider capsuleCollider = gam.AddComponent<CapsuleCollider>();
                capsuleCollider.center = transform.localPosition;
                capsuleCollider.height = transform.lossyScale.y;
                capsuleCollider.radius = 1;*/
                /*GrabbableRingHelper ringHelper = gam.AddComponent<GrabbableRingHelper>();
                ringHelper.RingHelperScale = 1;*/
            }
        }

        // Callback for ButtonDown
        public virtual void OnButtonUp() {
            // Play sound
            if (audioSource && ButtonClickUp) {
                audioSource.clip = ButtonClickUp;
                audioSource.Play();
            }

            // Call event
            if (onButtonUp != null) {
                onButtonUp.Invoke();
            }
        }

        void OnTriggerEnter(Collider other) {
            // Check Grabber
            Grabber grab = other.GetComponent<Grabber>();
            if (grab != null) {
                if (grabbers == null) {
                    grabbers = new List<Grabber>();
                }

                if (!grabbers.Contains(grab)) {
                    grabbers.Add(grab);
                }
            }

            // Check UITrigger, which is another type of activator
            UITrigger trigger = other.GetComponent<UITrigger>();
            if (trigger != null) {
                if (uiTriggers == null) {
                    uiTriggers = new List<UITrigger>();
                }

                if (!uiTriggers.Contains(trigger)) {
                    uiTriggers.Add(trigger);
                }
            }
        }

        void OnTriggerExit(Collider other) {
            Grabber grab = other.GetComponent<Grabber>();
            if (grab != null) {
                if (grabbers.Contains(grab)) {
                    grabbers.Remove(grab);
                }
            }

            UITrigger trigger = other.GetComponent<UITrigger>();
            if (trigger != null) {
                if (uiTriggers.Contains(trigger)) {
                    uiTriggers.Remove(trigger);
                }
            }
        }

        void OnDrawGizmosSelected() {
            // Show Grip Point
            Gizmos.color = Color.blue;

            Vector3 upPosition = transform.TransformPoint(new Vector3(transform.localPosition.x, MaxLocalY, transform.localPosition.z));
            Vector3 downPosition = transform.TransformPoint(new Vector3(transform.localPosition.x, MinLocalY, transform.localPosition.z));

            Vector3 size = new Vector3(0.005f, 0.005f, 0.005f);

            Gizmos.DrawCube(upPosition, size);

            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(downPosition, size);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(upPosition, downPosition);
        }
    }
}
