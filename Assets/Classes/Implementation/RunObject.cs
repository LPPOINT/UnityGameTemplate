using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Classes.Core;
using Assets.Classes.Foundation.Enums;
using Assets.Classes.Foundation.Extensions;

using PathologicalGames;
using UnityEngine;

namespace Assets.Classes.Implementation
{
    public class RunObject : RoseEntity
    {
        public RunObjectBuild Build { get; private set; }

        public virtual float Height
        {
            get
            {
                return WorldRect.height;
            }
        }
        public virtual bool ShouldDespawnAfterBelowScreen
        {
            get { return true; }
        }


        #region Positionation
        public class PositionationEvent
        {
            public PositionationEvent(EventPositionationContext context, float y, Action<PositionationEvent> handler)
            {
                Context = context;
                Y = y;
                Handler = handler;
            }

            public enum EventPositionationContext
            {
                Above,
                Below
            }

            public EventPositionationContext Context { get; private set; }
            public float Y { get; private set; }
            public Action<PositionationEvent> Handler { get; private set; }
            public bool IsAlreadyInvoked { get; private set; }

            public void Invoke()
            {
                if(IsAlreadyInvoked) return;
                IsAlreadyInvoked = true;
                Handler(this);
            }

            public bool InvokeIfNeeded(Rect rect)
            {
                var min = rect.yMin;
                var max = rect.yMax;

                if ((Context == EventPositionationContext.Above && min > Y) || (Context == EventPositionationContext.Below && max < Y))
                {
                    Invoke();
                    return true;
                }
                return false;
            }

        }

        public List<PositionationEvent> PositionationEvents { get; private set; }

        private void CheckPositionationEvents()
        {
            if (PositionationEvents == null || !PositionationEvents.Any()) return;
            var wr = WorldRect;
            for (var i = 0; i < PositionationEvents.Count; i++)
            {
                var e = PositionationEvents[i];
                Log("Check");
                if (e.InvokeIfNeeded(wr))
                {
                    Log("Remove");
                    PositionationEvents.Remove(e);
                    break;
                }
            }
        }

        public void AddPositionationEvent(float y, PositionationEvent.EventPositionationContext type, Action<PositionationEvent> handler)
        {
            if(PositionationEvents == null) PositionationEvents = new List<PositionationEvent>();
            PositionationEvents.Add(new PositionationEvent(type, y, handler));
        }

        public virtual bool IsCompletelyOnScreen
        {
            get
            {
                var wrMin = WorldRect.yMin;
                var wrh = WorldRect.height;
                var cmMin = CameraSupervisor.Main.CameraViewRect.yMin;
                var cmh = CameraSupervisor.Main.CameraViewRect.height;

                return wrMin + wrh < cmMin && (wrMin - wrh) > (cmMin - cmh);

            }
        }
        public virtual bool IsIntersectsWithScreen
        {
            get
            {
                var r1 = CameraSupervisor.Main.CameraViewRect;
                var r2 = WorldRect;

                return r1.Intersects(r2);
            }
        }
        public virtual bool IsCompletelyOutOfScreen
        {
            get
            {
                var r1 = WorldRect;
                var r2 = CameraSupervisor.Main.CameraViewRect;
                return !r2.ContainsOrIntersects(r1);
            }
        }
        public virtual bool IsAboveThan(float y)
        {
            return WorldRect.yMin > y;
        }

        public enum PositionationType
        {
            OnScreen,
            BelowScreen,
            AboveScreen
        }
        public PositionationType CalculatePositionationType()
        {
            var wrMin = WorldRect.yMin;
            var wrMax = WorldRect.yMax;
            var wrh = WorldRect.height;
            var cmMin = CameraSupervisor.Main.CameraViewRect.yMin;
            var cmMax = CameraSupervisor.Main.CameraViewRect.yMax;
            var cmh = CameraSupervisor.Main.CameraViewRect.height;

            if(wrMin < cmMin && (wrMin - wrh) > (cmMin - cmh)) return PositionationType.OnScreen;
            if (wrMin + wrh < (cmMin - cmh)) return PositionationType.BelowScreen;
            return PositionationType.AboveScreen;
        }

        protected void PositionateAtTopCenter()
        {
            transform.position =
                new Vector3(
                    CameraSupervisor.Main.CameraViewRect.xMin + CameraSupervisor.Main.CameraViewRect.width / 2,
                    RunObjects.Instance.GetHightestPoint(), transform.position.z);
        }

        #endregion

        #region WorldRect

        private Rect worldRect;
        public Rect WorldRect
        {
            get
            {
                if (worldRect == default(Rect))
                {
                    Log("World rect probably not initialized. Trying to initialize");
                    InvalidateWorldRect();
                    if (worldRect == default(Rect))
                    {
                        ProcessError("ComputeWorldRect() gives unexpected result. Fix ComputeWorldRect() for class '" + GetType().Name + "'");
                    }
                }
                return worldRect;
            }
            set { worldRect = value; }
        }

        protected Rect GenerateVerticalWorldRect(float y, float h)
        {
            return new Rect(CameraSupervisor.Main.CameraViewRect.xMin + 0.01f, y, CameraSupervisor.Main.CameraViewRect.width - 0.02f, h);
        }

        public virtual Rect ComputeWorldRect()
        {
            if (boxCollider2D == null) 
                return new Rect();
            return boxCollider2D.GetWorldRect();
        }
        public virtual bool ShouldInvalidateWorldRect { get { return true; } }

        public void InvalidateWorldRect()
        {
            WorldRect = ComputeWorldRect();
        }
        public void InvalidateWorldRectIfNeeded()
        {
            if (ShouldInvalidateWorldRect) InvalidateWorldRect();
        }


        #endregion

        public bool IsSpawned { get; private set; }
        public int SpawnCount { get; private set; }

        public bool IsFirstSpawn
        {
            get { return SpawnCount == 0; }
        }

        public virtual void OnObjectSpawned(RunObjectBuild build)
        {
            if(PositionationEvents == null) PositionationEvents = new List<PositionationEvent>();
            IsSpawned = true;
            Build = build;
            SpawnCount++;
            InvalidateWorldRect();
        }
        public virtual void OnObjectDespawned()
        {
            IsSpawned = false;
            PositionationEvents = new List<PositionationEvent>();
        }

        protected virtual void Update()
        {
            CheckPositionationEvents();
            InvalidateWorldRectIfNeeded();
        }
    }
}
