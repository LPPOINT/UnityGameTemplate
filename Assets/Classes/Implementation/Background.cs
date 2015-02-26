using System.Collections.Generic;
using Assets.Classes.Core;
using Assets.Classes.Foundation.Classes;
using DG.Tweening;
using UnityEngine;

namespace Assets.Classes.Implementation
{
    public class Background : SingletonEntity<Background>
    {

        #region Color main
        public class ColorTranslation
        {
            public ColorTranslation(Color destanationColor, float time, Ease ease, float delay)
            {
                DestanationColor = destanationColor;
                Time = time;
                Ease = ease;
                Delay = delay;
            }
            public Color DestanationColor { get; set; }
            public float Time { get; set; }
            public Ease Ease { get; set; }
            public float Delay { get; set; }
        }

        public const string ColorTranslationTweenId = "ColorTranslation";



        public ColorTranslation CurrentColorTranslation { get; private set; }

        private void OnColorTranslationComplete()
        {
            CurrentColorTranslation = null;
            StartNewColorTranslation();
        }
        private void OnColorTranslationInterrupted()
        {
            
        }

        public void StartColorTranslation(ColorTranslation ct)
        {


            if (CurrentColorTranslation != null)
            {
                StopColorTranslation(false);
            }

            CameraSupervisor.MainCamera.DOColor(ct.DestanationColor, ct.Time)
                .SetId(MakeUniqueId(ColorTranslationTweenId))
                .SetEase(ct.Ease)
                .OnComplete(OnColorTranslationComplete)
                .SetDelay(ct.Delay);

        }
        public void StopColorTranslation(bool shouldCompleteTranslation = true)
        {
            if (CurrentColorTranslation == null)
            {
                return;
            }

            DOTween.Kill(MakeUniqueId(ColorTranslationTweenId), shouldCompleteTranslation);
            if (!shouldCompleteTranslation) OnColorTranslationInterrupted();
            CurrentColorTranslation = null;
        }

        public void SetColorWithoutTranslation(Color color)
        {
            if(CurrentColorTranslation != null) StopColorTranslation(false);
            CameraSupervisor.MainCamera.backgroundColor = color;
        }

        #endregion

        #region Color behaviour

        public Range ColorTranslationRange;
        public List<Ease> AllowedColorTranslationsEase;
        public List<Color> AllowedColors;

        public Color GetRandomColor()
        {
            return AllowedColors[Random.Range(0, AllowedColors.Count)];
        }

        public ColorTranslation GetRandomColorTranslation()
        {

            var c = GetRandomColor();
            return new ColorTranslation(c, Random.Range(0.6f, 1.3f), AllowedColorTranslationsEase[Random.Range(0, AllowedColorTranslationsEase.Count)], 4);
        }

        public void SetRandomColorWithoutTranslation()
        {
            SetColorWithoutTranslation(GetRandomColor());
        }

        private void StartNewColorTranslation()
        {
            var ct = GetRandomColorTranslation();
            StartColorTranslation(ct);
        }

        public void StartColorTranslationLoop()
        {
            StartNewColorTranslation();
        }

        #endregion

    }
}
