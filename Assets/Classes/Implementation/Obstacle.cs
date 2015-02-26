using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Classes.Core;
using Assets.Classes.Foundation.Enums;
using Assets.Classes.Foundation.Extensions;
using DG.Tweening;
using UnityEngine;

namespace Assets.Classes.Implementation
{
    public class Obstacle : RunObject
    {
        [Serializable]
        public class ObstacleAnimation
        {
            public float Time;
            public Ease Ease;
            public Color Color;
        }

        [Serializable]
        public class ObstacleIn : ObstacleAnimation
        {

            public float StartProtrusion;
            public float EndProtrusion;

        }

         [Serializable]
        public class ObstacleOut : ObstacleAnimation
        {

            public Vector3 Offset;
        }


        public HorizontalDirection Alignment;
        public BoxCollider2D BoundsCollider;

        public List<BoxCollider2D> CollisionColliders;

        public bool IsCollisionCollider(BoxCollider2D c)
        {
            if (CollisionColliders == null || !CollisionColliders.Any()) return false;
            return CollisionColliders.Any(d => d.GetInstanceID() == c.GetInstanceID());
        }

        public override Rect ComputeWorldRect()
        {
            return BoundsCollider.GetWorldRect();
        }

        public override void OnObjectSpawned(RunObjectBuild build)
        {
            //PositionateAtTopCenter();
            base.OnObjectSpawned(build);
        }
        public override void OnObjectDespawned()
        {
            base.OnObjectDespawned();
        }
    }
}
