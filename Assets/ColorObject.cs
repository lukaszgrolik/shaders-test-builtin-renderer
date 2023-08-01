using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// [ExecuteInEditMode]
public class ColorObject : MonoBehaviour
{
    private Color color;

    // Start is called before the first frame update
    void Start()
    {
        var meshRend = GetComponent<MeshRenderer>();

        color = Random.ColorHSV(0, 1, .5f, .5f, .5f, .5f);
        // SetColor_ExistingMaterial(meshRend, color);
        // SetColor_NewMaterial(meshRend, color);
        SetColor_PropertyBlock(meshRend, color);

        Debug.Log($"new mat color: {color}");
    }

    void SetColor_ExistingMaterial(MeshRenderer meshRend, Color color)
    {
        meshRend.sharedMaterial.SetVector("_BaseColor", new Vector4(x: color.r, y: color.g, z: color.b, w: color.a));
    }

    void SetColor_NewMaterial(MeshRenderer meshRend, Color color)
    {
        var newMat = new Material(meshRend.sharedMaterial);
        // newMat.SetFloatArray("_BaseColor", new List<float>() { color.r, color.g, color.b, color.a });
        newMat.SetVector("_BaseColor", new Vector4(x: color.r, y: color.g, z: color.b, w: color.a));

        meshRend.sharedMaterial = newMat;
    }

    void SetColor_PropertyBlock(MeshRenderer meshRend, Color color)
    {
        // var newMat = new Material(meshRend.sharedMaterial);
        // meshRend.sharedMaterial = newMat;
        var props = new MaterialPropertyBlock();
        props.SetVector("_BaseColor", new Vector4(x: color.r, y: color.g, z: color.b, w: color.a));

        meshRend.SetPropertyBlock(props);

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDrawGizmos()
    {
        Handles.Label(transform.position, color.ToString());
    }
}
