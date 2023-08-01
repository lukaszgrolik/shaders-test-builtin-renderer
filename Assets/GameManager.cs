using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private new Camera camera;
    [SerializeField] private SurfaceProjector gridProjector;

    private Ray camToMouseRay;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        camToMouseRay = camera.ScreenPointToRay(Input.mousePosition);

        // if (Physics.Raycast(camToMouseRay, out var groundHitInfo, 100, gameLayerMasksProvider.GroundLayerMask))
        if (Physics.Raycast(camToMouseRay, out var groundHitInfo, 100))
        {
            gridProjector.gameObject.SetActive(true);

            var point = groundHitInfo.point;
            var currentPos = gridProjector.transform.position;
            gridProjector.transform.position = new Vector3(point.x, currentPos.y, point.z);
        }
        else
        {
            gridProjector.gameObject.SetActive(false);
        }
    }
}
