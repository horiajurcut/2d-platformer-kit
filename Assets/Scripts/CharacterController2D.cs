using UnityEngine;

public class CharacterController2D : RaycastController
{
    public CollisionInfo Collisions;

    public float maxAscentAngle = 80f;
    public float maxDescentAngle = 75f;

    private void VerticalCollisions(ref Vector3 velocity)
    {
        var directionY = Mathf.Sign(velocity.y);
        var rayLength = Mathf.Abs(velocity.y) + SkinWidth;

        for (var i = 0; i < verticalRayCount; i++)
        {
            var rayOrigin = Mathf.Approximately(directionY, -1f)
                ? RayOrigins.BottomLeft
                : RayOrigins.TopLeft;
            rayOrigin += Vector2.right * (VerticalRaySpacing * i + velocity.x);

            var hit = Physics2D.Raycast(rayOrigin, directionY * Vector2.up, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * (directionY * rayLength), Color.red);

            if (hit)
            {
                velocity.y = (hit.distance - SkinWidth) * directionY;
                rayLength = hit.distance;

                if (Collisions.AscendingSlope)
                {
                    velocity.x = velocity.y / Mathf.Tan(Collisions.SlopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
                }
                
                Collisions.Below = Mathf.Approximately(directionY, -1f);
                Collisions.Above = Mathf.Approximately(directionY, 1f);
            }
        }

        if (Collisions.AscendingSlope)
        {
            var directionX = Mathf.Sign(velocity.x);
            rayLength = Mathf.Abs(velocity.x) + SkinWidth;

            var rayOrigin = (Mathf.Approximately(directionX, -1f)
                ? RayOrigins.BottomLeft
                : RayOrigins.BottomRight) + Vector2.up * velocity.y;

            var hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (hit)
            {
                var slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                if (!Mathf.Approximately(slopeAngle, Collisions.SlopeAngle))
                {
                    velocity.x = (hit.distance - SkinWidth) * directionX;
                    Collisions.SlopeAngle = slopeAngle;
                }
            }
        }
    }

    private void HorizontalCollisions(ref Vector3 velocity)
    {
        var directionX = Mathf.Sign(velocity.x);
        var rayLength = Mathf.Abs(velocity.x) + SkinWidth;

        for (var i = 0; i < horizontalRayCount; i++)
        {
            var rayOrigin = Mathf.Approximately(directionX, -1f)
                ? RayOrigins.BottomLeft
                : RayOrigins.BottomRight;
            rayOrigin += Vector2.up * (HorizontalRaySpacing * i);

            var hit = Physics2D.Raycast(rayOrigin, directionX * Vector2.right, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * (directionX * rayLength), Color.red);

            if (hit)
            {
                var slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                if (i == 0 && slopeAngle <= maxAscentAngle)
                {
                    if (Collisions.DescendingSlope)
                    {
                        Collisions.DescendingSlope = false;
                        velocity = Collisions.PrevVelocity;
                    }
                    
                    var distanceToSlopeStart = 0f;

                    if (!Mathf.Approximately(slopeAngle, Collisions.PrevSlopeAngle))
                    {
                        distanceToSlopeStart = hit.distance - SkinWidth;
                        velocity.x -= distanceToSlopeStart * directionX;
                    }

                    AscendSlope(ref velocity, slopeAngle);
                    velocity.x += distanceToSlopeStart * directionX;
                }

                if (!Collisions.AscendingSlope || slopeAngle > maxAscentAngle)
                {
                    velocity.x = (hit.distance - SkinWidth) * directionX;
                    rayLength = hit.distance;

                    if (Collisions.AscendingSlope)
                    {
                        velocity.y = Mathf.Tan(Collisions.SlopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                    }

                    Collisions.Left = Mathf.Approximately(directionX, -1f);
                    Collisions.Right = Mathf.Approximately(directionX, 1f);
                }
            }
        }
    }

    private void AscendSlope(ref Vector3 velocity, float slopeAngle)
    {
        var moveDistance = Mathf.Abs(velocity.x);

        var ascendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
        var ascendVelocityX = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);

        if (velocity.y <= ascendVelocityY)
        {
            velocity.y = ascendVelocityY;
            velocity.x = ascendVelocityX;

            Collisions.Below = true;
            Collisions.AscendingSlope = true;
            Collisions.SlopeAngle = slopeAngle;
        }
    }

    private void DescendSlope(ref Vector3 velocity)
    {
        var directionX = Mathf.Sign(velocity.x);
        var rayOrigin = Mathf.Approximately(directionX, -1f)
            ? RayOrigins.BottomRight
            : RayOrigins.BottomLeft;

        var hit = Physics2D.Raycast(rayOrigin, Vector2.down, Mathf.Infinity, collisionMask);

        if (hit)
        {
            var slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

            if (slopeAngle != 0f && slopeAngle <= maxDescentAngle)
            {
                if (Mathf.Approximately(Mathf.Sign(hit.normal.x), directionX))
                {
                    if (hit.distance - SkinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                    {
                        var moveDistance = Mathf.Abs(velocity.x);
                        var descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                        var descendVelocityX = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);

                        velocity.x = descendVelocityX;
                        velocity.y -= descendVelocityY;

                        Collisions.SlopeAngle = slopeAngle;
                        Collisions.DescendingSlope = true;
                        Collisions.Below = true;
                    }
                }
            }
        }
    }

    public void Move(Vector3 velocity)
    {
        UpdateRaycastOrigins();
        Collisions.Reset();
        Collisions.PrevVelocity = velocity;

        if (velocity.y < 0f)
        {
            DescendSlope(ref velocity);
        }

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

        public bool AscendingSlope;
        public bool DescendingSlope;

        public float SlopeAngle, PrevSlopeAngle;

        public Vector3 PrevVelocity;

        public void Reset()
        {
            Above = Below = false;
            Left = Right = false;
            
            AscendingSlope = false;
            DescendingSlope = false;

            PrevSlopeAngle = SlopeAngle;
            SlopeAngle = 0f;
        }
    }
}