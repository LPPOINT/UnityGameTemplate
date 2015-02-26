using System;
using System.Runtime.InteropServices;
using Assets.Classes.Core;
using PathologicalGames;
using UnityEngine;

namespace Assets.Classes.Implementation
{

    public interface ISpace
    {
        float SpaceHeight { get; set; }
    }

    public class SpaceBuild : RunObjectBuild, ISpace
    {

        public float SpaceHeight { get; set; }

        public override Type GetObjectType()
        {
            return typeof(Space);
        }

        public override void Build(RunObject obj)
        {
            var space = CastObjectTo<Space>(obj);
            space.SpaceHeight = SpaceHeight;
        }
    }

    public class Space : RunObject, ISpace
    {
        public float SpaceHeight { get; set; }

        public override float Height
        {
            get { return SpaceHeight; }
        }

        public override Rect ComputeWorldRect()
        {

            return new Rect(CameraSupervisor.Main.CameraViewRect.xMin + 0.01f, transform.position.y , CameraSupervisor.Main.CameraViewRect.width - 0.02f, Height);
        }

        public override void OnObjectSpawned(RunObjectBuild build)
        {
            PositionateAtTopCenter();
            base.OnObjectSpawned(build);
        }

        public bool ShouldDrawGizmos = true;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            var rect = WorldRect;

            Gizmos.DrawLine(new Vector3(rect.xMin, rect.yMax), new Vector3(rect.xMax, rect.yMax));
            Gizmos.DrawLine(new Vector3(rect.xMin, rect.yMax), new Vector3(rect.xMin, rect.yMin));
            Gizmos.DrawLine(new Vector3(rect.xMin, rect.yMin), new Vector3(rect.xMax, rect.yMin));
            Gizmos.DrawLine(new Vector3(rect.xMax, rect.yMax), new Vector3(rect.xMax, rect.yMin));

        }
    }
}
