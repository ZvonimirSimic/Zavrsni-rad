using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace BNG
{

    public class Button2 : MonoBehaviour
    {
        public bool pressed = false;
        public GameObject gam;

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

        void Start()
        {
            joint = GetComponent<SpringJoint>();
            rigid = GetComponent<Rigidbody>();

            // Set to kinematic so we are not affected by outside forces
            if (!AllowPhysicsForces)
            {
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


        void Update()
        {

            buttonDownPosition = GetButtonDownPosition();
            buttonUpPosition = GetButtonUpPosition();
            bool grabberInButton = false;
            bool UITriggerInButton = uiTriggers != null && uiTriggers.Count > 0;

            // Find a valid grabber to push down
            for (int x = 0; x < grabbers.Count; x++)
            {
                if (!grabbers[x].HoldingItem)
                {
                    grabberInButton = true;
                    break;
                }
            }
            // push button down
            if (grabberInButton || UITriggerInButton)
            {
                float speed = ButtonSpeed;
                transform.localPosition = Vector3.Lerp(transform.localPosition, buttonDownPosition, speed * Time.deltaTime);

                if (joint)
                {
                    joint.spring = 0;
                }
            }
            else
            {
                // Let the spring push the button up if physics forces are enabled
                if (AllowPhysicsForces)
                {
                    if (joint)
                    {
                        joint.spring = SpringForce;
                    }
                }
                // Need to lerp back into position if spring won't do it for us
                else
                {
                    float speed = ButtonSpeed;
                    transform.localPosition = Vector3.Lerp(transform.localPosition, buttonUpPosition, speed * Time.deltaTime);
                    if (joint)
                    {
                        joint.spring = 0;
                    }
                }
            }

            // Cap values
            if (transform.localPosition.y < MinLocalY)
            {
                transform.localPosition = buttonDownPosition;
            }
            else if (transform.localPosition.y > MaxLocalY)
            {
                transform.localPosition = buttonUpPosition;
            }

            // Click Down?
            float buttonDownDistance = transform.localPosition.y - buttonDownPosition.y;
            if (buttonDownDistance <= ClickTolerance && !clickingDown)
            {
                clickingDown = true;
                OnButtonDown();
            }
            // Click Up?
            float buttonUpDistance = buttonUpPosition.y - transform.localPosition.y;
            if (buttonUpDistance <= ClickTolerance && clickingDown)
            {
                clickingDown = false;
                OnButtonUp();
            }
        }

        public virtual Vector3 GetButtonUpPosition()
        {
            return new Vector3(transform.localPosition.x, MaxLocalY, transform.localPosition.z);
        }

        public virtual Vector3 GetButtonDownPosition()
        {
            return new Vector3(transform.localPosition.x, MinLocalY, transform.localPosition.z);
        }

        // Callback for ButtonDown
        public virtual void OnButtonDown()
        {

            // Play sound
            if (audioSource && ButtonClick)
            {
                audioSource.clip = ButtonClick;
                audioSource.Play();
            }

            // Call event
            if (onButtonDown != null)
            {
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
                    gam.transform.position = newPos;
                    Transform[] children = gam.GetComponentsInChildren<Transform>(true);
                    List<GameObject> childrenList = new List<GameObject>();
                    List<GameObject> textFields = new List<GameObject>();
                    List<GameObject> gamObjs = new List<GameObject>();
                    for (int i = 0; i < children.Length; ++i)
                    {
                        GameObject g = new GameObject();
                        GameObject t = new GameObject();
                        textFields.Add(t);
                        gamObjs.Add(g);
                        Transform child = children[i];
                        childrenList.Add(child.gameObject);
                    }
                    for (int i = 1; i < childrenList.Count; i++)
                    {
                        if (childrenList[i].GetComponent<Grabbable>() == null)
                        {
                            childrenList[i].AddComponent<Grabbable>();
                            childrenList[i].GetComponent<Grabbable>().SecondaryGrabBehavior = OtherGrabBehavior.DualGrab;
                            childrenList[i].GetComponent<Grabbable>().GrabMechanic = GrabType.Precise;
                            childrenList[i].GetComponent<Grabbable>().GrabButton = GrabButton.Grip;
                            childrenList[i].GetComponent<Grabbable>().GrabPhysics = GrabPhysics.Kinematic;
                            childrenList[i].GetComponent<Grabbable>().Grabtype = HoldType.HoldDown;
                            childrenList[i].GetComponent<Grabbable>().HideHandGraphics = true;
                            childrenList[i].GetComponent<Grabbable>().ParentToHands = true;
                            childrenList[i].GetComponent<Grabbable>().ParentHandModel = false;
                            childrenList[i].GetComponent<Grabbable>().SnapHandModel = true;
                        }
                        if (childrenList[i].GetComponent<Rigidbody>() == null)
                        {
                            childrenList[i].AddComponent<Rigidbody>();
                            childrenList[i].GetComponent<Rigidbody>().isKinematic = true;
                            childrenList[i].GetComponent<Rigidbody>().useGravity = true;
                        }
                        if (childrenList[i].GetComponent<MeshCollider>() == null)
                        {
                            //childrenList[i].AddComponent<BoxCollider>();
                            childrenList[i].AddComponent<MeshCollider>();
                        }
                        if (textFields[i].GetComponent<TextMeshPro>() == null)
                        {
                            textFields[i].AddComponent<TextMeshPro>();
                        }
                        Mesh m = childrenList[i].GetComponent<MeshFilter>().sharedMesh;
                        gamObjs[i] = Instantiate(childrenList[i], newPos + childrenList[i].transform.localPosition / 50, childrenList[i].transform.rotation);
                        gamObjs[i].transform.localScale = new Vector3(gamObjs[i].transform.localScale.x / 50, gamObjs[i].transform.localScale.y / 50, gamObjs[i].transform.localScale.z / 50);
                        textFields[i].transform.parent = gamObjs[i].transform;
                        float pixelRatio = (Camera.main.orthographicSize * 2.0f) / Camera.main.pixelHeight;
                        textFields[i].transform.localScale /= 30;
                        textFields[i].GetComponent<TextMeshPro>().transform.position = new Vector3 ( gamObjs[i].transform.position.x, gamObjs[i].transform.position.y - gamObjs[i].transform.localPosition.y, gamObjs[i].transform.position.z);
                    }
                    gam.SetActive(false);
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
        public virtual void OnButtonUp()
        {
            // Play sound
            if (audioSource && ButtonClickUp)
            {
                audioSource.clip = ButtonClickUp;
                audioSource.Play();
            }

            // Call event
            if (onButtonUp != null)
            {
                onButtonUp.Invoke();
            }
        }

        void OnTriggerEnter(Collider other)
        {
            // Check Grabber
            Grabber grab = other.GetComponent<Grabber>();
            if (grab != null)
            {
                if (grabbers == null)
                {
                    grabbers = new List<Grabber>();
                }

                if (!grabbers.Contains(grab))
                {
                    grabbers.Add(grab);
                }
            }

            // Check UITrigger, which is another type of activator
            UITrigger trigger = other.GetComponent<UITrigger>();
            if (trigger != null)
            {
                if (uiTriggers == null)
                {
                    uiTriggers = new List<UITrigger>();
                }

                if (!uiTriggers.Contains(trigger))
                {
                    uiTriggers.Add(trigger);
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            Grabber grab = other.GetComponent<Grabber>();
            if (grab != null)
            {
                if (grabbers.Contains(grab))
                {
                    grabbers.Remove(grab);
                }
            }

            UITrigger trigger = other.GetComponent<UITrigger>();
            if (trigger != null)
            {
                if (uiTriggers.Contains(trigger))
                {
                    uiTriggers.Remove(trigger);
                }
            }
        }

        void OnDrawGizmosSelected()
        {
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