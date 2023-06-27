using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GorillaSlingshot : MonoBehaviour
{
    public Transform leftTop;
    public Transform rightTop;
    public Transform grabPoint;
    public LineRenderer lineRenderer;
    public GameObject projectilePrefab;

    private XRGrabInteractable grabInteractable;
    private Vector3 grabOffset;
    private bool isGrabbing = false;

    void Start()
    {
        grabInteractable = grabPoint.GetComponent<XRGrabInteractable>();
        grabInteractable.onActivate.AddListener(OnGrab);
        grabInteractable.onDeactivate.AddListener(OnRelease);
    }

    void OnGrab(XRBaseInteractor interactor)
    {
        isGrabbing = true;
        grabOffset = grabPoint.transform.position - interactor.transform.position;
    }

    void OnRelease(XRBaseInteractor interactor)
    {
        isGrabbing = false;
        var projectile = Instantiate(projectilePrefab, grabPoint.transform.position, Quaternion.identity);
        projectile.GetComponent<Rigidbody>().AddForce(grabPoint.transform.forward * 10f, ForceMode.Impulse);
        grabPoint.transform.position = transform.position;
    }

    void Update()
    {
        if (lineRenderer.positionCount >= 3)
        {
            lineRenderer.SetPosition(0, leftTop.position);
            lineRenderer.SetPosition(1, rightTop.position);
            lineRenderer.SetPosition(2, grabPoint.position);
        }

        if (isGrabbing)
        {
            grabPoint.transform.position = transform.position + grabOffset;
        }
    }
}
