using Assets.Classes.Core;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Classes.Implementation
{
    public class UIPlayButton : Button
    {



        public string ClickAnimationStateName = "Click";
        public string DisabledStateName = "Disabled";


        public UnityEvent onClickAnimationComplete;

        public bool IsOnTheScreen
        {
            get { return enabled && gameObject.activeInHierarchy && transform.localScale != Vector3.zero; }
        }


        public void Show()
        {
            gameObject.SetActive(true);
        }
        public void Hide()
        {
            gameObject.SetActive(false);
        }


        public Color PointerDownColor;
        public Color IdleColor = Color.white;

        private Image Image;
        private Animator Animator;

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            Image.DOColor(PointerDownColor, 0.1f);
        }
        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            Image.DOColor(IdleColor, 0.1f);
        }

        private void OnClickAnimationComplete()
        {
            onClickAnimationComplete.Invoke();
        }
        private void OnClick()
        {
            Animator.Play(ClickAnimationStateName);
        }

        protected override void Awake()
        {
            base.Awake();
            onClick.AddListener(OnClick);
            Image = GetComponent<Image>();
            Animator = GetComponent<Animator>();
        }

    }
}
