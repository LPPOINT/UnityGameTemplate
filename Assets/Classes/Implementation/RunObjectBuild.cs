using System;

namespace Assets.Classes.Implementation
{
    public abstract class RunObjectBuild
    {
        public abstract Type GetObjectType();
        public abstract void Build(RunObject obj);

        protected T CastObjectTo<T>(RunObject o) where T : RunObject
        {
            return o as T;
        }

    }


    public class RunObjectGenericBuild<T> : RunObjectBuild where T : RunObject
    {
        public RunObjectGenericBuild(Action<T> buildAction)
        {

            BuildAction = buildAction;
        }

        public RunObjectGenericBuild()
        {
            
        }

        public Action<T> BuildAction { get; set; } 

        public override Type GetObjectType()
        {
            return typeof (T);
        }

        public override void Build(RunObject obj)
        {
            BuildAction(obj as T);
        }
    }

}
