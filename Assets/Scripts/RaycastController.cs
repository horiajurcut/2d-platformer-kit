using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour
{
    public LayerMask collisionMask;
    
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;

    protected const float SkinWidth = .01f;
    
    protected float HorizontalRaySpacing;
    protected float VerticalRaySpacing;

    private BoxCollider2D _boxCollider2D;
    protected RaycastOrigins RayOrigins;
    
    protected virtual void Start()
    {
        _boxCollider2D = GetComponent<BoxCollider2D>();

        CalculateRaySpacing();
    }
    
    protected void UpdateRaycastOrigins()
    {
        var bounds = _boxCollider2D.bounds;
        bounds.Expand(SkinWidth * -2);

        RayOrigins.BottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        RayOrigins.BottomRight = new Vector2(bounds.max.x, bounds.min.y);
        RayOrigins.TopLeft = new Vector2(bounds.min.x, bounds.max.y);
        RayOrigins.TopRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    private void CalculateRaySpacing()
    {
        var bounds = _boxCollider2D.bounds;
        bounds.Expand(SkinWidth * -2);

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        HorizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        VerticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    protected struct RaycastOrigins
    {
        public Vector2 TopLeft, TopRight;
        public Vector2 BottomLeft, BottomRight;
    }
}
