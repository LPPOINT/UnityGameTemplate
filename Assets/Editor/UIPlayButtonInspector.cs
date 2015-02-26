using Assets.Classes.Core;
using Assets.Classes.Implementation;
using UnityEditor;

namespace Assets.Editor
{
    [CustomEditor(typeof(UIPlayButton), true)]
    public class UIPlayButtonInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}
