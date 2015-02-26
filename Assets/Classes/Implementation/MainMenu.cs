using Assets.Classes.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Classes.Implementation
{
    [EntryState]
    public class MainMenu : GameState<MainMenu>
    {

        public string ShowingAnimation = "Showing";
        public string DisposingAnimation = "Disposing";

        public UIPlayButton PlayButton;
        public GameObject UIRoot;

        public override void OnStateEnter(object model)
        {
            Background.Instance.SetRandomColorWithoutTranslation();
            PlayButton.Show();
        }

        public void OnPlayButtonClick()
        {
            GameStates.Instance.EnableState<Run>();
            UIRoot.GetComponent<Animator>().Play(DisposingAnimation);
        }

        public void OnPlayButtomBecameInvisible()
        {
            PlayButton.Hide();
        }
    }
}
