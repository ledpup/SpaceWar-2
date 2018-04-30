using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets
{
    public static class Targeting
    {
        public static Quaternion RotateToTarget(Vector3 targetPosition, Vector3 objectPosition)
        {
            Vector2 direction = targetPosition - objectPosition;
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            var rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
            return rotation;
        }
    }
}
