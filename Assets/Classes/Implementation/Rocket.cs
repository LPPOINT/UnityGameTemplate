using System;
using Assets.Classes.Core;
using Assets.Classes.Foundation.Classes;
using Assets.Classes.Foundation.Enums;
using Assets.Classes.Foundation.Extensions;
using DG.Tweening;
using Soomla;
using UnityEngine;

namespace Assets.Classes.Implementation
{
    public class Rocket : SingletonEntity<Rocket>
    {


        #region Variables
        public enum RocketState
        {
            WaifForTakeoff,
            Takeoff,
            WaitForStartInput,
            Idle,
            FallingPhase1,
            FallingPhase2,
            FallingPhase3,
        }


        public GameObject Body;
        public GameObject Fire;


        public HorizontalDirection Direction { get; private set; }
        public RocketState State { get; private set; }
    

        public Color IdleColor = Color.white;
        public Color DeathColor = Color.black;



        public HorizontalDirection InitialDirection;


        #endregion


        #region ACTIONS
        public void Takeoff()
        {
            if (!ValidateState(RocketState.WaifForTakeoff)) return;
            DoTakeoff();
        }
        public void Play()
        {
            if(!ValidateState(RocketState.WaitForStartInput)) return;
            DoFirstTurn();
        }
        public void Turn()
        {
            if(!ValidateState(RocketState.Idle) || IsTurnInputLocked) return;
            DoTurn();
        }
        public void Fall()
        {
            
        }

        private bool ValidateState(RocketState expectedState)
        {
            if (State != expectedState)
            {
                ProcessError("Cant execute rocket action: Current state: '" + State + "', expected state: '" + expectedState + "'");
                return false;
            }
            return true;
        }

        #endregion

        #region INPUT

        public void ResolveInputWasReceived()
        {
            switch (State)
            {
                    
                case RocketState.Idle:
                    Turn();
                    break;
                case RocketState.WaitForStartInput:
                    Play();
                    break;
            }
        }

        #endregion


        #region Takeoff

        public const string TakeoffCompleteEventName = "TakeoffComplete";
        public const string TakeoffPositionMotionTweenId = "TakeoffPositionMotion";
        public string TakeoffAnimationName = "rocket_takeoff";


        private bool isTakeoffPositionMotionComplete;
        private bool isTakeoffAnimationMotionComplete;
        private bool isTakeoffAnimationMotionStarted;
        private bool isTakeoffPositionMotionStarted;

        private void OnTakeoffPositionMotionComplete()
        {
            OnTakeoffComplete();
        }
        private void OnTakeoffAnimationMotionComplete()
        {

        }

        public float TakeoffPositionMotionYOffset = 0.3f;
        public float TakeoffPositionMotionTime = 0.3f;
        public Ease TakeoffPositionMotionEase = Ease.Linear;

        private void StartTakeoffPositionMotion()
        {
            ActivateFire();

            var endPosition = new Vector3(transform.position.x, transform.position.y + TakeoffPositionMotionYOffset, transform.position.z);

            transform.DOMove(endPosition, TakeoffPositionMotionTime)
                .SetId(MakeUniqueId(TakeoffPositionMotionTweenId))
                .SetEase(TakeoffPositionMotionEase)
                .OnStart(() => isTakeoffPositionMotionStarted = true)
                .OnComplete(() =>
                            {
                                isTakeoffPositionMotionStarted = false;
                                OnTakeoffPositionMotionComplete();
                            });

            Ground.Instance.Hide();

        }


        private void OnTakeoffComplete()
        {
            if (State == RocketState.Takeoff)
            {
                ProcessError("Multiply OnTakeoffComplete() call detected!");
                return;
            }

            State = RocketState.WaitForStartInput;
            StartScaleFire();
            StartShake();
            GameMessenger.Broadcast(TakeoffCompleteEventName);
        }

        public void DoTakeoff()
        {
            animation.Play(TakeoffAnimationName);
            isTakeoffAnimationMotionStarted = true;
        }

        private void CheckTakeoffAnimationIfNeeded()
        {
            if(State != RocketState.Takeoff || !isTakeoffAnimationMotionStarted) return;

            if (!animation.IsPlaying(TakeoffAnimationName))
            {
                Log("Takeoff Animation Completed");
                isTakeoffAnimationMotionStarted = false;
                OnTakeoffAnimationMotionComplete();
            }

        }

        #endregion

        #region First turn

        public float FirstTurnTime
        {
            get { return TurnTime/2; }
        }

        public Ease FirstTurnEase;

        public const string FirstTurnTweenId = "FirstTurn";

        private void OnFirstTurnCompleted()
        {
            State = RocketState.Idle;
            Direction = InitialDirection;
        }

        public void DoFirstTurn()
        {
            var rot = InitialDirection == HorizontalDirection.Left ? LeftRotation : RightRotation;
            StartTurnMotionAndPlayTurnSound(rot, FirstTurnTime, FirstTurnEase, FirstTurnTweenId, OnFirstTurnCompleted, null);
        }

        #endregion

        #region Playing

        public float LeftVelocity;
        public float RightVelocity
        {
            get { return -LeftVelocity; }
        }

        public float LeftRotation;
        public float RightRotation
        {
            get { return -LeftRotation; }
        }

        public float TurnTime;
        public Ease TurnEase;
        public AudioClip TurnSound;

        public float CollisionWithBordersLockTime = 0.2f;

        public const string TurnTweenId = "RocketTurn";

        public void OnTurnCompleted()
        {
            Run.Instance.ResolveRocketTurnComplete();
        }

        public void OnTurnStarted()
        {
            Run.Instance.ResolveRocketTurnStarted();
        }

        public void OnTurnNotCompleted()
        {
            Run.Instance.ResolveRocketTurnNotCompleted();
        }

        public void DoTurn()
        {

            if (DOTween.IsTweening(MakeUniqueId(TurnTweenId)))
            {
                DOTween.Kill(MakeUniqueId(TurnTweenId));
                OnTurnNotCompleted();
            }

            var targetDirection = DirectionHelper.SwapHorizontalDirection(Direction);

            var rot = targetDirection == HorizontalDirection.Left ? LeftRotation : RightRotation;
            Direction = DirectionHelper.SwapHorizontalDirection(Direction);
            StartTurnMotionAndPlayTurnSound(rot, TurnTime, TurnEase, TurnTweenId, OnTurnCompleted, OnTurnStarted);

        }

        public void ResolveCollisionWithObstacle(Obstacle obstacle)
        {

        }
        public void ResolveCollisionWithBorder(HorizontalDirection boderSide)
        {
            if(IsTurnInputLocked)
                return;
            if (Direction == HorizontalDirection.Left && boderSide == HorizontalDirection.Left
                || Direction == HorizontalDirection.Right && boderSide == HorizontalDirection.Right)
            {
                Turn();
                LockTurnInput(CollisionWithBordersLockTime);
            }
            else
            {
                //  ¯\_(ツ)_/¯
            }

        }

        public Vector3 GetMovementVector()
        {

            var velocity = Direction == HorizontalDirection.Left ? LeftVelocity : RightVelocity;
            var movement = velocity * Run.Instance.Speed * Time.deltaTime;

            return new Vector3(movement, 0);

        }

        private void UpdateMovementIfNeeded()
        {
            if(State != RocketState.Idle) return;

            var vec = GetMovementVector();
            transform.Translate(vec, UnityEngine.Space.World);
        }



        private void CheckBordersCollisionIfNeeded()
        {
            if(State != RocketState.Idle || IsTurnInputLocked) return;

            var collision = CameraSupervisor.Main.DetectBordersCollisionWith(boxCollider2D);


            if (collision == CameraSupervisor.BorderCollisionType.NoCollision) return;

            var side = collision == CameraSupervisor.BorderCollisionType.CollisionWithLeftBorder
                ? HorizontalDirection.Left
                : HorizontalDirection.Right;


            ResolveCollisionWithBorder(side);
        }

        public bool IsTurning { get; private set; }

        public void StartTurnMotionAndPlayTurnSound(float endRotation, float time, Ease ease, string id, Action onComplete, Action onStarted)
        {

            if(TurnSound != null)
                AudioSource.PlayClipAtPoint(TurnSound, transform.position);

            var endRotationVec = new Vector3(transform.rotation.x, transform.rotation.y, endRotation);
            IsTurning = true;
            transform.DORotate(endRotationVec, time)
                .SetId(MakeUniqueId(id))
                .SetEase(ease)
                .OnComplete(() =>
                            {
                                IsTurning = false;
                                if (onComplete != null) onComplete();
                            })
                .OnStart(() =>
                            {
                             if (onStarted != null) onStarted();
                            });
        }

        public bool IsTurnInputLocked { get; private set; }
        public void LockTurnInput(float time)
        {
            CancelInvoke("UnlockTurnInput");
            Invoke("UnlockTurnInput", time);
            IsTurnInputLocked = true;
        }
        public void UnlockTurnInput()
        {
            IsTurnInputLocked = false;
        }

        #endregion

        #region Falling

        #endregion

        #region Shaking
        public bool IsShaking { get; private set; }
        public const string ShakingTweenId = "RocketShaking";

        public Vector3 ShakeStrength = new Vector3(1, 0.2f);
        public int ShakeVibrato = 10;
        public float ShakeRandomness = 90f;
        public float ShakeTime = 1f;



        public void StartShake()
        {
            IsShaking = true;
            Body.transform.DOShakePosition(ShakeTime, ShakeStrength, ShakeVibrato, ShakeRandomness)
                .SetId(MakeUniqueId(ShakingTweenId))
                .SetLoops(-1, LoopType.Yoyo);
        }
        public void StopShake()
        {
            IsShaking = false;
            DOTween.Kill(MakeUniqueId(ShakingTweenId));
        }

        #endregion

        #region Fire 

        public bool IsScalingFire { get; private set; }

        public const string FireScaleTweenId = "RocketFireScale";

        public float FireScaleMultiplier;
        public float FireScaleTime;
        public Ease FireScaleEase;

        private float initialFireScale = -1;

        private void PrepareFireScale()
        {

            var fireScale = Fire.transform.localScale;

            if (initialFireScale == -1) initialFireScale = fireScale.x;
        }

        public void StartScaleFire()
        {

            if(!IsFireActive)
                ActivateFire();

            if(IsScalingFire)
                StopScaleFire();

            Fire.transform.localScale = new Vector3(initialFireScale, initialFireScale, Fire.transform.localScale.z);
            var fireScale = Fire.transform.localScale;


            var endScale = new Vector3(initialFireScale*FireScaleMultiplier, initialFireScale*FireScaleMultiplier,
                fireScale.z);

            Fire.transform.DOScale(endScale, FireScaleTime)
                .SetId(MakeUniqueId(FireScaleTweenId))
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(FireScaleEase);

            IsScalingFire = true;

        }
        public void StopScaleFire(bool alsoDeactivateFireObject = false)
        {
            DOTween.Kill(FireScaleTweenId);
            Fire.transform.localScale = new Vector3(initialFireScale, initialFireScale, Fire.transform.localScale.z);
            if(alsoDeactivateFireObject)
                DeactivateFire();

            IsScalingFire = false;
        }

        public bool IsFireActive
        {
            get { return Fire.activeSelf; }
        }
        public void ActivateFire()
        {
            Fire.SetActive(true);
        }
        public void DeactivateFire()
        {
            Fire.SetActive(false);
        }

        #endregion


        #region Unity callbacks
        private void Awake()
        {
            PrepareFireScale();
            State = RocketState.WaifForTakeoff;
        }
        private void Update()
        {
            UpdateMovementIfNeeded();
            CheckTakeoffAnimationIfNeeded();
        }

        private void FixedUpdate()
        {
            CheckBordersCollisionIfNeeded();
        }
        private void Start()
        {
            DeactivateFire();
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {


            //switch (collision.gameObject.tag)
            //{
            //    case Tags.Obstacle:
            //    {
            //        var obstacle = collision.gameObject.GetComponent<Obstacle>();
            //        ResolveCollisionWithObstacle(obstacle);
            //        Run.Instance.ResolveRocketCollisionWithObstacle(obstacle);
            //        break;
            //    }
            //}

            var go = collision.gameObject;
            var o = go.GetComponent<Obstacle>();
            if (o == null) o = go.GetComponentInParent<Obstacle>();

            if (o != null)
            {
                if (o.IsCollisionCollider(collision as BoxCollider2D))
                {
                    ResolveCollisionWithObstacle(o);
                    Run.Instance.ResolveRocketCollisionWithObstacle(o);
                }
            }

        }

        #endregion
    }
}
