using Assets.Classes.Core;
using Assets.Classes.Foundation.Enums;
using Assets.Classes.Foundation.Extensions;
using DG.Tweening;
using UnityEngine;

namespace Assets.Classes.Implementation
{
    public class HorizontalGate : Obstacle
    {
        public class HorizontalGateBehaviour
        {

            public virtual float GetStartXPosition()
            {
                return CameraSupervisor.Main.CameraViewRect.xMin + CameraSupervisor.Main.CameraViewRect.width / 2;
            }
            public virtual float GetStartSize()
            {
                return -1;
            }



        }
        public class HorizontaGateNoMotion : HorizontalGateBehaviour
        {
            public HorizontaGateNoMotion(float staticXPosition, float staticSize)
            {
                StaticXPosition = staticXPosition;
                StaticSize = staticSize;
            }

            public float StaticXPosition { get; private set; }
            public float StaticSize { get; private set; }

            public override float GetStartXPosition()
            {
                return StaticXPosition;
            }

            public override float GetStartSize()
            {
                return StaticSize;
            }

            public static HorizontaGateNoMotion Random(float minX, float maxX, float minSize, float maxSize)
            {
                return new HorizontaGateNoMotion(UnityEngine.Random.Range(minX, maxX), UnityEngine.Random.Range(minSize, maxSize));
            }
            public static HorizontaGateNoMotion Random()
            {
                return Random(CameraSupervisor.Main.CameraViewRect.xMin + CameraSupervisor.Main.CameraViewRect.width/2 - 1.5f, 
                    CameraSupervisor.Main.CameraViewRect.xMin + CameraSupervisor.Main.CameraViewRect.width/2 + 1.5f, 
                    4.5f,
                    5f);
                return Random(CameraSupervisor.Main.CameraViewRect.xMin, CameraSupervisor.Main.CameraViewRect.xMax, 1,
                    2.5f);
            }
        }
        public class HorizontalGateMotion : HorizontalGateBehaviour
        {
            public HorizontalGateMotion(float time, Ease ease, LoopType loopType)
            {
                Time = time;
                Ease = ease;
                LoopType = loopType;
  
            }

            public float Time { get; private set; }
            public Ease Ease { get; private set; }
            public LoopType LoopType { get; private set; }


            public string GetIdForGate(string origin, HorizontalGate gate)
            {
                return gate.MakeUniqueId(origin).ToString();
            }

            public virtual string MotionTweenId
            {
                get { return GetType().Name; }
            }

            public virtual void PlayMotion(HorizontalGate target)
            {
                
            }

            public virtual void PauseMotion(HorizontalGate target)
            {
                if (MotionTweenId != string.Empty)
                {
                    DOTween.Pause(GetIdForGate(MotionTweenId, target));
                }
            }

            public virtual void ResumeMotion(HorizontalGate target)
            {
                if (MotionTweenId != string.Empty)
                {
                    DOTween.Play(GetIdForGate(MotionTweenId, target));
                }
            }

            public virtual void KillMotion(HorizontalGate target)
            {
                if (MotionTweenId != string.Empty)
                {
                    DOTween.Kill(GetIdForGate(MotionTweenId, target));
                }
            }
        }
        public class HorizontalGateMove : HorizontalGateMotion
        {
            public HorizontalGateMove(float time, Ease ease, LoopType loopType, float xFrom, float xTo, float size) : base(time, ease, loopType)
            {
                Size = size;
                XFrom = xFrom;
                XTo = xTo;
            }

            private HorizontalGateMove(float time, Ease ease, LoopType loopType) : base(time, ease, loopType)
            {
                
            }

            public float XFrom { get; private set; }
            public float XTo { get; private set; }
            public float Size { get; private set; }

            public override float GetStartXPosition()
            {
                return XFrom;
            }
            public override float GetStartSize()
            {
                return Size;
            }

            public override void PlayMotion(HorizontalGate target)
            {
                target.transform.position = new Vector3(XFrom, target.transform.position.y, target.transform.position.z);
                target.transform.DOMoveX(XTo, Time)
                    .SetId(GetIdForGate(MotionTweenId, target))
                    .SetLoops(LoopType == LoopType.Restart ? 0 : -1, LoopType)
                    .SetEase(Ease);
            }

            public static HorizontalGateMove Random()
            {
                var m = new HorizontalGateMove(
                    UnityEngine.Random.Range(0.78f, 1.2f),
                    Ease.Linear,
                    LoopType.Yoyo,
                    CameraSupervisor.Main.CameraViewRect.xMin + CameraSupervisor.Main.CameraViewRect.width / 2 - UnityEngine.Random.Range(1f, 3f),
                    CameraSupervisor.Main.CameraViewRect.xMin + CameraSupervisor.Main.CameraViewRect.width / 2 + UnityEngine.Random.Range(1f, 3f),
                    UnityEngine.Random.Range(4.5f, 5f)
               );
                return m;
            }

        }
        public class HorizontalGateOpening : HorizontalGateMotion
        {
            public HorizontalGateOpening(float time, Ease ease, LoopType loopType, float startSize, float endSize, float x) : base(time, ease, loopType)
            {
                StartSize = startSize;
                EndSize = endSize;
                X = x;
            }

            public float StartSize { get; private set; }
            public float EndSize { get; private set; }
            public float X { get; private set; }

            public override float GetStartXPosition()
            {
                return X;
            }

            public override float GetStartSize()
            {
                return StartSize;
            }

            public override void PlayMotion(HorizontalGate target)
            {
                DOTween.To(() => target.GateSize, value => target.GateSize = value, EndSize, Time)
                    .SetId(GetIdForGate(MotionTweenId, target))
                    .SetLoops(LoopType == LoopType.Restart ? 0 : -1, LoopType)
                    .SetEase(Ease);
            }

            public static HorizontalGateOpening Random()
            {
                return new HorizontalGateOpening(1, Ease.OutBack, LoopType.Restart, 5.8f, 4.5f, CameraSupervisor.Main.CameraViewRect.xMin + CameraSupervisor.Main.CameraViewRect.width / 2);
            }
        }


        public HorizontalGateBehaviour Behaviour;

        public float LeftSideOffset;
        public float RightSideOffset;

        public Renderer LeftSide;
        public Renderer RightSide;

        public Renderer GetSideRenderer(HorizontalDirection side)
        {
            if (side == HorizontalDirection.Left) return LeftSide;
            return RightSide;
        }
        public float GetGatePartXPositionForSize(HorizontalDirection side, float size)
        {
            if (side == HorizontalDirection.Left) return -size;
            return size;
        }

        private float gateSize;
        public float GateSize
        {
            get { return gateSize; }
             set
            {
                gateSize = value;
                LeftSide.transform.localPosition = new Vector3(GetGatePartXPositionForSize(HorizontalDirection.Left, value), 0, LeftSide.transform.localPosition.z);
                RightSide.transform.localPosition = new Vector3(GetGatePartXPositionForSize(HorizontalDirection.Right, value), 0, RightSide.transform.localPosition.z);
            }
        }


        public override void OnObjectSpawned(RunObjectBuild build)
        {

            if (Behaviour == null)
            {
                ProcessError("HorizontalGate.OnObjectSpawned(): Behaviour == null");
                return;
            }
            if (LeftSide == null || RightSide == null)
            {
                ProcessError("HorizontalGate.OnObjectSpawned(): LeftSide == null || RightSide == null");
                return;
            }


            var startX = Behaviour.GetStartXPosition();
            var startSize = Behaviour.GetStartSize();

            transform.position = new Vector3(startX, RunObjects.Instance.GetHightestPoint() + BoundsCollider.GetWorldRect().height/2, transform.position.z);

            LeftSide.transform.localPosition = new Vector3(LeftSideOffset, 0, LeftSide.transform.localPosition.z);
            RightSide.transform.localPosition = new Vector3(RightSideOffset, 0, RightSide.transform.localPosition.z);


            //LeftSideOffset = LeftSide.transform.position.x;
            //RightSideOffset = RightSide.transform.position.x;

            GateSize = startSize;

            if (Behaviour is HorizontalGateMotion)
            {
                var m = Behaviour as HorizontalGateMotion;
                m.PlayMotion(this);
            }

            base.OnObjectSpawned(build);
        }
        public override void OnObjectDespawned()
        {
            GateSize = 0;
            if (Behaviour is HorizontalGateMotion)
            {
                var m = Behaviour as HorizontalGateMotion;
                m.KillMotion(this);
            }

            LeftSide.transform.localPosition = new Vector3(LeftSideOffset, 0, LeftSide.transform.localPosition.z);
            RightSide.transform.localPosition = new Vector3(RightSideOffset, 0, RightSide.transform.localPosition.z);

            base.OnObjectDespawned();
        }
    }
}
