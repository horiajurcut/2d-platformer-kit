using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour
{
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;

    public LayerMask collisionMask;
    public CollisionInfo Collisions;

    public float maxClimbAngle = 80f;

    private const float SkinWidth = .015f;

    private float _horizontalRaySpacing;
    private float _verticalRaySpacing;

    private BoxCollider2D _boxCollider2D;
    private RaycastOrigins _raycastOrigins;

    private const float Tolerance = 0.00001f;

    private void Start()
    {
        _boxCollider2D = GetComponent<BoxCollider2D>();
        
        CalculateRaySpacing();
    }

    private void VerticalCollisions(ref Vector3 velocity)
    {
        var directionY = Mathf.Sign(velocity.y);
        var rayLength = Mathf.Abs(velocity.y) + SkinWidth;

        for (var i = 0; i < verticalRayCount; i++)
        {
            var rayOrigin = Math.Abs(directionY - (-1f)) < Tolerance ? _raycastOrigins.BottomLeft : _raycastOrigins.TopLeft;
            rayOrigin += Vector2.right * (_verticalRaySpacing * i + velocity.x);

            var hit = Physics2D.Raycast(rayOrigin, directionY * Vector2.up, rayLength, collisionMask);
            
            Debug.DrawRay(rayOrigin, Vector2.up * (directionY * rayLength), Color.red);

            if (hit)
            {
                velocity.y = (hit.distance - SkinWidth) * directionY;
                rayLength = hit.distance;

                if (Collisions.ClimbingSlope)
                {
                    velocity.x = velocity.y / Mathf.Tan(Collisions.SlopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
                }

                Collisions.Below = Math.Abs(directionY - (-1f)) < Tolerance;
                Collisions.Above = Math.Abs(directionY - 1f) < Tolerance;
            }
        }
    }
    
    private void HorizontalCollisions(ref Vector3 velocity)
    {
        var directionX = Mathf.Sign(velocity.x);
        var rayLength = Mathf.Abs(velocity.x) + SkinWidth;

        for (var i = 0; i < horizontalRayCount; i++)
        {
            var rayOrigin = Math.Abs(directionX - (-1f)) < Tolerance ? _raycastOrigins.BottomLeft : _raycastOrigins.BottomRight;
            rayOrigin += Vector2.up * (_horizontalRaySpacing * i);

            var hit = Physics2D.Raycast(rayOrigin, directionX * Vector2.right, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * (directionX * rayLength), Color.red);
            
            if (hit)
            {
                var slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                if (i == 0 && slopeAngle <= maxClimbAngle)
                {
                    var distanceToSlopeStart = 0f;

                    if (Math.Abs(slopeAngle - Collisions.PrevSlopeAngle) > Tolerance)
                    {
                        distanceToSlopeStart = hit.distance - SkinWidth;
                        velocity.x -= distanceToSlopeStart * directionX;
                    }
                    
                    ClimbSlope(ref velocity, slopeAngle);
                    velocity.x += distanceToSlopeStart * directionX;
                }

                if (!Collisions.ClimbingSlope || slopeAngle > maxClimbAngle)
                {
                    velocity.x = (hit.distance - SkinWidth) * directionX;
                    rayLength = hit.distance;

                    if (Collisions.ClimbingSlope)
                    {
                        velocity.y = Mathf.Tan(Collisions.SlopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                    }
                
                    Collisions.Left = Math.Abs(directionX - (-1f)) < Tolerance;
                    Collisions.Right = Math.Abs(directionX - 1f) < Tolerance;
                }
            }
        }
    }

    private void ClimbSlope(ref Vector3 velocity, float slopeAngle)
    {
        var moveDistance = Mathf.Abs(velocity.x);
        
        var climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
        var climbVelocityX = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);

        if (velocity.y <= climbVelocityY)
        {
            velocity.y = climbVelocityY;
            velocity.x = climbVelocityX;

            Collisions.Below = true;
            Collisions.ClimbingSlope = true;
            Collisions.SlopeAngle = slopeAngle;
        }
    }

    private void UpdateRaycastOrigins()
    {
        var bounds = _boxCollider2D.bounds;
        bounds.Expand(SkinWidth * -2);

        _raycastOrigins.BottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        _raycastOrigins.BottomRight = new Vector2(bounds.max.x, bounds.min.y);
        _raycastOrigins.TopLeft = new Vector2(bounds.min.x, bounds.max.y);
        _raycastOrigins.TopRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    private void CalculateRaySpacing()
    {
        var bounds = _boxCollider2D.bounds;
        bounds.Expand(SkinWidth * -2);

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        _horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        _verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    private struct RaycastOrigins
    {
        public Vector2 TopLeft, TopRight;
        public Vector2 BottomLeft, BottomRight;
    }

    public void Move(Vector3 velocity)
    {
        UpdateRaycastOrigins();
        Collisions.Reset();

        if (velocity.x != 0f)
        {
            HorizontalCollisions(ref velocity);
        }

        if (velocity.y != 0f)
        {
            VerticalCollisions(ref velocity);
        }

        transform.Translate(velocity);
    }

    public struct CollisionInfo
    {
        public bool Above, Below;
        public bool Left, Right;

        public bool ClimbingSlope;

        public float SlopeAngle, PrevSlopeAngle;

        public void Reset()
        {
            Above = Below = false;
            Left = Right = false;
            ClimbingSlope = false;

            PrevSlopeAngle = SlopeAngle;
            SlopeAngle = 0f;
        }
    }
}