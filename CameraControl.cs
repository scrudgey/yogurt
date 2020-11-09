using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {
    public enum ControlState { playerInBounds, lerpToCenter, lerpToPoint };
    public bool showDebugIndicators;
    public ControlState state;
    public Vector3 pointTarget = Vector3.zero;
    private GameObject _focus;
    public GameObject focus {
        get { return _focus; }
        set {
            _focus = value;
            focusBody = value.GetComponent<Rigidbody2D>();
            focusControl = value.GetComponent<Controllable>();
        }
    }
    public Rigidbody2D focusBody;
    public Controllable focusControl;
    Vector3 focusVelocity = Vector3.zero;
    private Vector3 smoothVelocity = Vector3.zero;
    private Vector3 shakeVector = Vector3.zero;
    public float maxSize;
    public Vector2 maxXY;
    public Vector2 minXY;
    private Camera mainCamera;
    public AudioSource audioSource;
    public Vector3 offset;
    public Rect boundingBox = new Rect(0.4f, 0.4f, 0.2f, 0.2f);
    public float intensity;
    public float smoothing = 0.5f;
    Vector3 previousPosition;
    public float centerRadius = 0.12f;
    public float pointLerpRadius = 0.02f;
    public float velocityFactor = 0.1f;
    public float boundsOffsetFactor = 0.2f;
    public GameObject targetIndicator;
    public GameObject positionIndicator;
    public GameObject lowerLeftIndicator;
    public GameObject upperLeftIndicator;
    public GameObject lowerRightIndicator;
    public GameObject upperRightIndicator;
    public GameObject centerRadiusIndicator;
    public GameObject pointRadiusIndicator;
    public Vector3 outOfBoundsOffset;
    void Start() {
        mainCamera = GetComponent<Camera>();
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        if (showDebugIndicators) {
            targetIndicator = CreateIndicator(Color.red);
            positionIndicator = CreateIndicator(Color.green);

            lowerLeftIndicator = CreateIndicator(Color.white);
            lowerRightIndicator = CreateIndicator(Color.white);

            upperLeftIndicator = CreateIndicator(Color.white);
            upperRightIndicator = CreateIndicator(Color.white);

            centerRadiusIndicator = CreateIndicator(new Color(200f, 200f, 200f, 0.5f));
            pointRadiusIndicator = CreateIndicator(new Color(200f, 200f, 200f, 0.5f));
        }
    }
    public void Shake(float intensity) {
        if (intensity < 0.001f)
            return;
        this.intensity = intensity;
    }
    GameObject CreateIndicator(Color color) {
        GameObject obj = GameObject.Instantiate(Resources.Load("UI/indicatorObj")) as GameObject;
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        sr.color = color;
        return obj;
    }
    void FixedUpdate() {
        if (focus == null)
            return;

        if (focusBody != null) {
            focusVelocity = focusBody.velocity;
        } else {
            focusVelocity = (focus.transform.position - previousPosition) / Time.deltaTime;
        }
        focusVelocity *= velocityFactor;

        mainCamera.orthographicSize = maxSize;

        // update camera world coordinates.
        Vector3 lowerLeftWorld = mainCamera.ScreenToWorldPoint(Vector2.zero);
        Vector3 upperRightWorld = mainCamera.ScreenToWorldPoint(new Vector2(mainCamera.pixelWidth, mainCamera.pixelHeight));
        float screenWidthWorld = upperRightWorld.x - lowerLeftWorld.x;
        float screenHeightWorld = upperRightWorld.y - lowerLeftWorld.y;
        Rect worldRect = new Rect(lowerLeftWorld.x, lowerLeftWorld.y, screenWidthWorld, screenHeightWorld);


        Rect boundsScreenSpace = new Rect(
            boundingBox.x * mainCamera.pixelWidth,
            boundingBox.y * mainCamera.pixelHeight,
            boundingBox.width * mainCamera.pixelWidth,
            boundingBox.height * mainCamera.pixelHeight);

        Vector3 tempVector = transform.position;
        Vector3 targetPosition = focus.transform.position + offset + focusVelocity + outOfBoundsOffset; // +offset

        if (showDebugIndicators)
            SetIndicatorPositions(targetPosition, boundsScreenSpace);

        if (state == ControlState.lerpToPoint) {
            Vector3 distance = transform.position - pointTarget;
            distance.z = 0;
            if (distance.magnitude <= pointLerpRadius) {
                state = ControlState.playerInBounds;
            }
        }

        if (boundsScreenSpace.Contains(mainCamera.WorldToScreenPoint(focus.transform.position + offset + outOfBoundsOffset))) {  // +offset
            // player inside bounding box
            if (state == ControlState.lerpToCenter) {
                Vector3 distance = transform.position - targetPosition;
                distance.z = 0;
                if (distance.magnitude <= centerRadius) {
                    state = ControlState.lerpToPoint;
                    pointTarget = focus.transform.position + offset + outOfBoundsOffset;
                }
            }
        } else {
            Vector3 r = (mainCamera.WorldToScreenPoint(focus.transform.position + offset + outOfBoundsOffset));
            if (r.x > boundsScreenSpace.x + boundsScreenSpace.width) {
                outOfBoundsOffset = Vector2.right * boundsOffsetFactor;
            } else if (r.x < boundsScreenSpace.x) {
                outOfBoundsOffset = Vector2.left * boundsOffsetFactor;
            }
            // player outside bounding box
            if (state != ControlState.lerpToCenter) {
                state = ControlState.lerpToCenter;
            }
        }


        float smoothConstant = smoothing;
        if (focusControl.running) {
            smoothConstant /= 2f;
        }
        if (state == ControlState.lerpToCenter) {
            tempVector = Vector3.SmoothDamp(tempVector, targetPosition, ref smoothVelocity, smoothConstant);
            // tempVector = Vector3.Lerp(tempVector, targetPosition, smoothing);
        } else if (state == ControlState.lerpToPoint) {
            tempVector = Vector3.SmoothDamp(tempVector, pointTarget, ref smoothVelocity, smoothConstant);
            // tempVector = Vector3.Lerp(tempVector, pointTarget, smoothing);
        }


        //check for edge of level
        if (screenWidthWorld > maxXY.x - minXY.x) {
            tempVector.x = (maxXY.x + minXY.x) / 2f;
        } else {
            if (tempVector.x - screenWidthWorld / 2 < minXY.x) {
                tempVector.x = minXY.x + screenWidthWorld / 2;
            }
            if (tempVector.x + screenWidthWorld / 2 > maxXY.x) {
                tempVector.x = maxXY.x - screenWidthWorld / 2;
            }
        }

        if (screenHeightWorld > maxXY.y - minXY.y) {
            tempVector.y = (maxXY.y + minXY.y) / 2f;
        } else {
            if (tempVector.y - screenHeightWorld / 2 < minXY.y) {
                tempVector.y = minXY.y + screenHeightWorld / 2;
            }
            if (tempVector.y + screenHeightWorld / 2 > maxXY.y) {
                tempVector.y = maxXY.y - screenHeightWorld / 2;
            }
        }

        tempVector = tempVector + shakeVector;
        // update camera position
        tempVector.z = -1f;
        transform.position = tempVector;

        if (intensity > 0.01) {
            shakeVector = Random.insideUnitCircle * intensity;
            intensity = intensity * 0.95f;
        } else shakeVector = Vector3.zero;

        previousPosition = focus.transform.position;
    }

    void SetIndicatorPositions(Vector3 targetPosition, Rect boundsScreenSpace) {
        targetIndicator.transform.position = targetPosition;
        positionIndicator.transform.position = pointTarget;
        // if (state == ControlState.lerpToCenter) {
        //     targetIndicator.SetActive(true);
        //     positionIndicator.SetActive(false);
        // } else if (state == ControlState.lerpToPoint) {
        //     targetIndicator.SetActive(false);
        //     positionIndicator.SetActive(true);
        // }

        Vector3 temp = mainCamera.ScreenToWorldPoint(new Vector2(boundsScreenSpace.x, boundsScreenSpace.y));
        temp.z = 0;
        lowerLeftIndicator.transform.position = temp;

        temp = mainCamera.ScreenToWorldPoint(new Vector2(boundsScreenSpace.x + boundsScreenSpace.width, boundsScreenSpace.y));
        temp.z = 0;
        lowerRightIndicator.transform.position = temp;

        temp = mainCamera.ScreenToWorldPoint(new Vector2(boundsScreenSpace.x, boundsScreenSpace.y + boundsScreenSpace.height));
        temp.z = 0;
        upperLeftIndicator.transform.position = temp;

        temp = mainCamera.ScreenToWorldPoint(new Vector2(boundsScreenSpace.x + boundsScreenSpace.width, boundsScreenSpace.y + boundsScreenSpace.height));
        temp.z = 0;
        upperRightIndicator.transform.position = temp;

        temp = transform.position;
        temp.z = 0;
        centerRadiusIndicator.transform.position = temp;
        pointRadiusIndicator.transform.position = temp;

        // r = 0.087 world units per scale unit
        centerRadiusIndicator.transform.localScale = Vector3.one * (centerRadius / 0.087f);
        pointRadiusIndicator.transform.localScale = Vector3.one * (pointLerpRadius / 0.087f);
    }
}
