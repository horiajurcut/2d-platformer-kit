using System.Collections.Generic;
using UnityEngine;

public class PlatformController : RaycastController
{
    public LayerMask passengerMask;
    public Vector3 move;
    
    protected override void Start()
    {
        base.Start();
    }

    private void Update()
    {
        UpdateRaycastOrigins();
        
        var velocity = move * Time.deltaTime;
        MovePassengers(velocity);
        
        transform.Translate(velocity);
    }

    private void MovePassengers(Vector3 velocity)
    {
        var movedPassengers = new HashSet<Transform>();
        
        var directionX = Mathf.Sign(velocity.x);
        var directionY = Mathf.Sign(velocity.y);
        
        // Vertically moving platform
        if (velocity.y != 0f)
        {
            var rayLength = Mathf.Abs(velocity.y) + SkinWidth;
            
            for (var i = 0; i < verticalRayCount; i++)
            {
                var rayOrigin = Mathf.Approximately(directionY, -1f)
                    ? RayOrigins.BottomLeft
                    : RayOrigins.TopLeft;
                rayOrigin += Vector2.right * (VerticalRaySpacing * i);

                var hit = Physics2D.Raycast(rayOrigin, directionY * Vector2.up, rayLength, passengerMask);
                
                if (!hit) continue;

                // Each passenger is moved only once per frame
                if (movedPassengers.Contains(hit.transform)) continue;
                
                movedPassengers.Add(hit.transform);
                        
                var pushX = Mathf.Approximately(directionY, 1f)
                    ? velocity.x
                    : 0f;
                var pushY = velocity.y - (hit.distance - SkinWidth) * directionY;
                    
                hit.transform.Translate(new Vector3(pushX, pushY, 0f));
            }
            
            // Horizontally moving platform
            if (velocity.x != 0f)
            {
                
            }
        }
    }
}
