using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterManager : MonoBehaviour
{
    private MeshFilter _meshFilter;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshFilter.mesh.RecalculateBounds();
        _meshFilter.mesh.RecalculateNormals();
    }
    
    // private void Start()
    private void Update()
    {
        var scale = transform.localScale;
        var vertices = _meshFilter.mesh.vertices;
        for (var i = 0; i < vertices.Length; i++)
        {
            vertices[i].y = Waves.instance.GetWaveHeight((transform.position.x + vertices[i].x));
            //Debug.Log((transform.position.x + vertices[i].x) + " " +
                      // ((transform.position.x + vertices[i].x) * scale.x));
        }

        _meshFilter.mesh.vertices = vertices;
        _meshFilter.mesh.RecalculateNormals();
    }
}