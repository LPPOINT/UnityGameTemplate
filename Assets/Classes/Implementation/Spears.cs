using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Assets.Classes.Core;
using Assets.Classes.Foundation.Enums;
using Assets.Classes.Foundation.Extensions;
using DG.Tweening;
using UnityEngine;

namespace Assets.Classes.Implementation
{
    public class Spears : Obstacle
    {
        public HorizontalAlignment Side { get; set; }
        public float MotionTime { get; set; }
        public Ease MotionEase { get; set; }
        public int SpearsNumber { get; set; }
        public float SpearsRotation { get; set; }

        public SpearLenghtResolver SizeResolver { get; set; }
        public float FirstSpearSize { get; set; }

        #region Spears objects

        public List<Transform> SpearsObjects;

        public IEnumerable<Transform> GetActiveSpearsObjects()
        {
            return SpearsObjects.Where(transform1 => transform1.gameObject.activeSelf);
        }

        public void DeactivateAllSpears()
        {
            foreach (var spearsObject in SpearsObjects)
            {
                spearsObject.gameObject.SetActive(false);
            }
        }

        public void ActivateSpear(Transform s)
        {
            s.gameObject.SetActive(true);
        }
        public void ActivateSpear(int spearNumber)
        {

            var s = GetSpearByNumber(spearNumber);
            if (s == null)
            {
                ProcessError("ActivateSpear(): spear to activate not found!");
            }
            ActivateSpear(s);
        }

        public Transform GetHightestSpear()
        {
            return SpearsObjects.LastOrDefault();
        }
        public Transform GetCurrentHightestSpear()
        {
            return GetActiveSpearsObjects().LastOrDefault();
        }

        public Transform GetLowestSpear()
        {
            return SpearsObjects.FirstOrDefault();
        }
        public Transform GetCurrentLowestSpear()
        {
            return GetActiveSpearsObjects().FirstOrDefault();
        }


        public Transform GetSpearByNumber(int spearNumber)
        {
            if (spearNumber < 0 || spearNumber > SpearsObjects.Count - 1)
            {
                ProcessError("GetSpearByNumber(): unexpected spear number");
                return null;
            }

            var s = SpearsObjects[spearNumber];
            return s;
        }

        public Tweener MoveSpear(Transform s, float by, float time, Ease ease, Action onComplete)
        {


            return s.DOMoveX(by, time)
                .SetEase(ease)
                .OnComplete(() =>
                            {
                                if (onComplete != null) onComplete();
                            });
        }


        public const string SpearsMotionCompleteEventName = "SpearsMotionComplete";

        public IEnumerator MoveSpears()
        {
            for (var i = 0; i < SpearsNumber; i++)
            {
                var s = GetSpearByNumber(i);
                if (s == null)
                {
                    ProcessError("unexpected end of spears list");
                    yield return null;
                }

                var tweener = MoveSpear(s, SizeResolver(i, FirstSpearSize), MotionTime, MotionEase, null);

                yield return tweener.WaitForCompletion();

            }

            GameMessenger.Broadcast(SpearsMotionCompleteEventName, this);
        }

        #endregion

        #region Spears lenght calculation

        public delegate float SpearLenghtResolver(int spearNumber, float firstSpearLenght);

        public float LinearSpearLenghtResolver(int spearNumber, float firstSpearLenght)
        {
            return firstSpearLenght;
        }
        
        #endregion

        #region Positionation

        public HorizontalDirection InitialSide;

        private void ResolvePosition()
        {

        }



        #endregion

        public override Rect ComputeWorldRect()
        {
            var lowest = GetCurrentLowestSpear();
            var hightest = GetCurrentHightestSpear();

            if (lowest == null || hightest == null)
            {
                ProcessError("Cant calculate rect of spears: to critical points found!");
                return default(Rect);
            }

            var min = lowest.renderer.bounds.ToRect().yMin;
            var max = hightest.renderer.bounds.ToRect().yMax;

            var h = Math.Abs(min - max);

            return new Rect(CameraSupervisor.Main.CameraViewRect.xMin + 0.01f, transform.position.y, CameraSupervisor.Main.CameraViewRect.width - 0.02f, h);
        }


        public override void OnObjectSpawned(RunObjectBuild build)
        {
            DeactivateAllSpears();
            ResolvePosition();
            StartCoroutine(MoveSpears());
            base.OnObjectSpawned(build);
        }

        public override void OnObjectDespawned()
        {
            DeactivateAllSpears();
            base.OnObjectDespawned();
        }
    }
}
