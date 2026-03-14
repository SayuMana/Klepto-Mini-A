using UnityEngine;

/// <summary>
/// Attach script ini ke GameObject child dari enemy.
/// Script ini akan membuat mesh vision cone yang terlihat di Game View.
/// Warna bisa diubah dari Inspector atau via kode: SetColor(color)
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class VisionCone : MonoBehaviour
{
    [Header("Cone Settings")]
    public float viewDistance = 8f;
    [Range(1f, 360f)]
    public float viewAngle = 90f;
    public int   resolution = 30; // Semakin tinggi semakin halus

    [Header("Color")]
    public Color coneColor = new Color(1f, 1f, 0f, 0.25f); // Kuning transparan default

    private Mesh         mesh;
    private MeshFilter   meshFilter;
    private MeshRenderer meshRenderer;
    private Material     material;

    void Awake()
    {
        meshFilter   = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        // Buat material transparan
        material = new Material(Shader.Find("Transparent/Diffuse"));
        if (material.shader == null || !material.shader.isSupported)
        {
            // Fallback shader jika Transparent/Diffuse tidak ada
            material = new Material(Shader.Find("Standard"));
            material.SetFloat("_Mode", 3); // Transparent mode
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
        }

        material.color   = coneColor;
        meshRenderer.material = material;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows    = false;

        mesh             = new Mesh();
        mesh.name        = "VisionConeMesh";
        meshFilter.mesh  = mesh;

        // Pastikan tidak ada transform offset
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    void LateUpdate()
    {
        DrawCone();

        // Update warna material jika berubah di Inspector
        if (material != null && material.color != coneColor)
            material.color = coneColor;
    }

    // ─── Buat Mesh Vision Cone ────────────────────────────────────────────
    void DrawCone()
    {
        int   vertCount = resolution + 2; // titik pusat + titik-titik arc
        var   vertices  = new Vector3[vertCount];
        var   triangles = new int[resolution * 3];

        // Titik pusat (origin enemy)
        vertices[0] = Vector3.zero;

        float angleStep  = viewAngle / resolution;
        float startAngle = -viewAngle / 2f;

        for (int i = 0; i <= resolution; i++)
        {
            float angle = startAngle + angleStep * i;
            float rad   = angle * Mathf.Deg2Rad;

            // Posisi titik di ujung cone (space lokal)
            vertices[i + 1] = new Vector3(
                Mathf.Sin(rad)  * viewDistance,
                0f,
                Mathf.Cos(rad)  * viewDistance
            );
        }

        // Buat triangle dari pusat ke setiap pasang titik arc
        for (int i = 0; i < resolution; i++)
        {
            triangles[i * 3]     = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.Clear();
        mesh.vertices  = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    // ─── API Publik ───────────────────────────────────────────────────────
    /// <summary>Ubah warna cone dari script lain</summary>
    public void SetColor(Color color)
    {
        coneColor      = color;
        if (material != null)
            material.color = color;
    }

    /// <summary>Update jarak dan sudut cone dari script lain</summary>
    public void SetVisionParams(float distance, float angle)
    {
        viewDistance = distance;
        viewAngle    = angle;
    }
}