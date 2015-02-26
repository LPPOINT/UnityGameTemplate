using System;
using System.Collections.Generic;
using Assets.Classes.Core;
using Assets.Classes.Effects;
using Assets.Classes.Foundation.Enums;
using DG.Tweening;
using UnityEngine;

namespace Assets.Classes.Implementation
{
    public class Run : GameState<Run>
    {



        public float Speed { get; set; }
        public int Score { get; set; }
        public List<Obstacle> SpawnedObstacles { get; set; }

        public float NormalSpeed = 1;
        public float BoostedSpeed
        {
            get { return NormalSpeed*SpeedBoostMultiplier; }
        }

        public float SpeedBoostMultiplier;
        public float SpeedBoostTime;


        public bool IsSpeedBoostEnabled { get; private set; }

        public override void OnStateEnter(object model)
        {
            Speed = NormalSpeed;
            SpawnedObstacles = new List<Obstacle>();
            Score = 0;

            if (GameStates.Instance.PreviousState == MainMenu.Instance)
            {
                Rocket.Instance.Takeoff();
            }

            Background.Instance.StartColorTranslationLoop();
        }

        public void BoostSpeed()
        {

            if (IsSpeedBoostEnabled)
            {
                CancelInvoke("CancelBoostSpeed");
            }

            Speed = NormalSpeed*SpeedBoostMultiplier;
            Invoke("CancelBoostSpeed", SpeedBoostTime);
            IsSpeedBoostEnabled = true;
        }

        private void CancelBoostSpeed()
        {
            IsSpeedBoostEnabled = false;
            Speed = NormalSpeed;
        }


        public void ResolveRocketCollisionWithObstacle(Obstacle obstacle)
        {
            Blink.PlayInInstance();
        }

        public void ResolveRocketCollisionWithBorder(HorizontalDirection borderSide)
        {
            
        }

        public void ResolveRocketTurnComplete()
        {
            
        }
        public void ResolveRocketTurnStarted()
        {
            BoostSpeed();
        }

        public void ResolveRocketTurnNotCompleted()
        {
            
        }

        private void Update()
        {

        }

 
    }
}
