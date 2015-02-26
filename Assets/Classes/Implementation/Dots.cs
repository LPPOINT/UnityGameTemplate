using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Classes.Core;
using Assets.Classes.Foundation.Classes;
using Assets.Classes.Foundation.Extensions;
using DG.Tweening;
using UnityEngine;

namespace Assets.Classes.Implementation
{
    public class Dots : Obstacle
    {

        [Serializable]
        public class DotMotion
        {
            public float Time;
            public Ease Ease;
            public float StartScale;

            public float RotationSpeed = 3f;

            private const string UniqueIdForBuildSeed = "DotMotion";

            private string CreateUniqueIdForBuild(DotBuild build)
            {
                return UniqueIdForBuildSeed + build.GetHashCode();
            }
            public DotBuild TargetBuild { get; private set; }


            public void Prepare(DotBuild build)
            {
                TargetBuild = build;
                build.View.transform.SetScale(StartScale);
            }

            public void Play(Action onCompleteAction = null)
            {
                var go = TargetBuild.View;


                var t = go.transform.DOScale(
                    new Vector3(TargetBuild.Scale, TargetBuild.Scale, go.transform.localScale.z), Time)
                    .SetEase(Ease)
                    .SetId(CreateUniqueIdForBuild(TargetBuild))
                    .OnComplete(() =>
                                {
                                    if (onCompleteAction != null) onCompleteAction();
                                });

                if (RotationSpeed != 0)
                {
                    go.transform.DORotate(
                        new Vector3(go.transform.rotation.eulerAngles.x, go.transform.rotation.eulerAngles.y,
                            go.transform.rotation.eulerAngles.z + 180), RotationSpeed, RotateMode.FastBeyond360)
                        .SetSpeedBased()
                        .SetId(CreateUniqueIdForBuild(TargetBuild))
                        .SetLoops(-1, LoopType.Incremental);
                }


            }

            public void Resume()
            {
                
            }

            public void Pause()
            {
                DOTween.Pause(CreateUniqueIdForBuild(TargetBuild));
            }

            public void Kill()
            {
                DOTween.Kill(CreateUniqueIdForBuild(TargetBuild));
            }

        }

        [Serializable]
        public class DotBuild
        {

            public DotBuild()
            {
                
            }

            public DotBuild(GameObject view, float scale, DotMotion motion, Sprite sprite)
            {
                View = view;
                Scale = scale;
                Motion = motion;
                Sprite = sprite;
            }


            public GameObject View;
            public float Scale;
            public DotMotion Motion;
            public Sprite Sprite;

        }

        [Serializable]
        public class DotSequence
        {

            public List<DotBuild> Builds;
        }

        public List<DotMotion> CurrentsMotions { get; private set; }
        public List<DotBuild> CurrentBuilds { get; private set; } 


        public List<DotSequence> Sequences;

        public DotSequence GetRandomSequence()
        {
            return Sequences[UnityEngine.Random.Range(0, Sequences.Count)];
        }


        public void VisualizeBuild(DotBuild build)
        {
            build.View.gameObject.SetActive(true);
            var modelRenderer = build.View.renderer as SpriteRenderer;

            if (modelRenderer == null)
            {
                ProcessError("DotView should contains SpriteRenderer component");
                return;
            }

            modelRenderer.sprite = build.Sprite;
            build.Motion.Prepare(build);

            CurrentBuilds.Add(build);
            CurrentsMotions.Add(build.Motion);
        }
        public void DisableAllDots()
        {
            foreach (var s in Sequences)
            {
                foreach ( var b in s.Builds)
                {
                    b.View.gameObject.SetActive(false);
                }
            }

            if (CurrentsMotions != null)
            {
                foreach (var m in CurrentsMotions)
                {
                    m.Kill();
                }
                CurrentsMotions.Clear();
            }
            else CurrentsMotions = new List<DotMotion>();

            if (CurrentBuilds != null)
            {
                CurrentBuilds.Clear();
            } 
            else CurrentBuilds = new List<DotBuild>();


        }

        private void OnDotsMotionComplete()
        {
            
        }

        private void StartDotMotion(DotMotion d, int index)
        {
            if(d == null) return;
            DotMotion next = null;
            next = index == CurrentsMotions.Count - 1 ? null : CurrentsMotions[index + 1];

            d.Play(() =>
                   {
                       if (next != null)
                       {
                           StartDotMotion(next, index+1);
                       }
                       else
                       {
                           OnDotsMotionComplete();
                       }
                   });

        }

        public void PlayBuildedDotsMotion()
        {
            StartDotMotion(CurrentsMotions.FirstOrDefault(), 0);
        }


        public override Rect ComputeWorldRect()
        {

            if (CurrentBuilds == null || !CurrentBuilds.Any())
            {
                Log("Trying to compute dots world rect when builds not created");
                return default(Rect);
            }

            CurrentBuilds.Sort((b1, b2) =>
                                                  {
                                                      if (b1.View.transform.position.y > b2.View.transform.position.y) return 1;
                                                      if (b1.View.transform.position.y == b2.View.transform.position.y) return 0;
                                                      if (b1.View.transform.position.y < b2.View.transform.position.y) return -1;

                                                      return 0;

                                                  });

            var min = CurrentBuilds.FirstOrDefault();
            var max = CurrentBuilds.LastOrDefault();

            if (min == null || max == null)
            {
                ProcessError("Compute worldRect: cant calculate critical points");
                return default(Rect);
            }

            var y = min.View.transform.position.y;
            var h = Math.Abs(max.View.transform.position.y - min.View.transform.position.y) + max.View.renderer.bounds.size.y/2 + min.View.renderer.bounds.size.y/2;


            return GenerateVerticalWorldRect(y, h);

        }

        public override void OnObjectSpawned(RunObjectBuild build)
        {

            DisableAllDots();

            var s = GetRandomSequence();

            if (s == null)
            {
                ProcessError("Cant select sequence");
                return;
            }

            foreach (var b in s.Builds)
            {
                VisualizeBuild(b);
            }

            PositionateAtTopCenter();

            Invoke("PlayBuildedDotsMotion", 1 * Run.Instance.Speed);

            base.OnObjectSpawned(build);
        }

        public override void OnObjectDespawned()
        {
            DisableAllDots();
            base.OnObjectDespawned();
        }
    }
}
