using Assets.Classes.Core;

namespace Assets.Classes.Implementation
{
    public class RocketInput : SingletonEntity<RocketInput>
    {
        private void OnTap(TapGesture gesture)
        {
            Rocket.Instance.ResolveInputWasReceived();
        }

        private void OnSwipe(SwipeGesture gesture)
        {
            Rocket.Instance.ResolveInputWasReceived();
        }
    }
}
