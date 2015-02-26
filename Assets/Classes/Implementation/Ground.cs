using Assets.Classes.Core;
using CUDLR;
using DG.Tweening;
using UnityEngine;

namespace Assets.Classes.Implementation
{
    public class Ground : SingletonEntity<Ground>
    {



        public const string GroundShowedEvent = "GroundShowed";
        public const string GroundHidedEvent = "GroundShowed";
        public const string GroundMotionTweenId = "GroundMotion";

        public enum GroundState
        {
            Showing,
            Hiding,
            Showed,
            Hided
        }

        public GroundState State { get; private set; }


        public Transform HidedSpot;
        public Transform ShowedSpot;

        public float InTime;
        public Ease InEase;

        public float OutTime;
        public Ease OutEase;

        public GroundState InitialState;

        private void Motion(Transform start, Transform end, float time, Ease ease, string eventToBroadcastAfterComplete, GroundState stateToSetAfterComplete)
        {
            transform.position = start.position;
            transform.DOMove(end.position, time)
                .SetEase(ease)
                .SetId(MakeUniqueId(GroundMotionTweenId))
                .OnComplete(() =>
                            {
                                GameMessenger.Broadcast(eventToBroadcastAfterComplete);
                                State = stateToSetAfterComplete;
                            });
        }

        [CUDLRCommand("ground_show")]
        public void Show()
        {
            State = GroundState.Showing;
            Motion(HidedSpot, ShowedSpot, InTime, InEase, GroundShowedEvent, GroundState.Showed);
        }

        [CUDLRCommand("ground_hide")]
        public void Hide()
        {
            State = GroundState.Hiding;
            Motion(ShowedSpot, HidedSpot, OutTime, OutEase, GroundHidedEvent, GroundState.Hided);
        }

        [CUDLRCommand("ground_show_nomotion")]
        public void ShowWithoutMotion()
        {
            State = GroundState.Showed;
            transform.position = ShowedSpot.transform.position;
        }

        [CUDLRCommand("ground_hide_nomotion")]
        public void HideWithoutMotion()
        {
            State = GroundState.Hided;
            transform.position = HidedSpot.transform.position;
        }

        private void Start()
        {
            if (HidedSpot == null || ShowedSpot == null)
            {
                ProcessError("Ground: Hided and/or showed spot not initialized");
                return;
            }
            switch (InitialState)
            {
                case GroundState.Showed:
                    ShowWithoutMotion();
                    break;
                case GroundState.Hided:
                    HideWithoutMotion();
                    break;
                case GroundState.Showing:
                    Show();
                    break;
                case GroundState.Hiding:
                    Hide();
                    break;

            }
        }
    }
}
