using Assets.Classes.Core;

namespace Assets.Classes.Implementation
{
    public class Helptip : SingletonEntity<Helptip>
    {
        public bool ShouldShow
        {
            get { return true; }
        }
        public bool IsOnTheScreen { get; private set; }
    }
}
