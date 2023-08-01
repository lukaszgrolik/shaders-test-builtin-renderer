using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// rename to grid projector
public class SurfaceProjector : MonoBehaviour
{
    [SerializeField] private Projector projector;
    // [SerializeField] private Material material;

    // private MeshRenderer meshRend;

    [SerializeField] private float radius = 10f;
    [SerializeField] private float borderBlendSize = 5f;

    void Awake()
    {
        // meshRend = GetComponent<MeshRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // projector = GetComponent<Projector>();
        // projector.material = material;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnValidate()
    {
        // var props = new MaterialPropertyBlock();
        // props.SetFloat("_Radius", pos);
        // props.SetFloat("_BorderBlendSize", pos);

        var newMat = new Material(projector.material);
        // newMat.SetFloatArray("_BaseColor", new List<float>() { color.r, color.g, color.b, color.a });
        newMat.SetFloat("_Radius", radius);
        newMat.SetFloat("_BorderBlendSize", borderBlendSize);

        projector.material = newMat;

        // meshRend.SetPropertyBlock(props);
    }

    // public void SetPosition(Vector3 position)
    // {
    //     var props = new MaterialPropertyBlock();
    //     var pos = new Vector4(x: position.x, y: position.y, z: position.z, w: 0);
    //     props.SetVector("_Position", pos);

    //     meshRend.SetPropertyBlock(props);
    // }
}
