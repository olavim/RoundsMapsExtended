using System;

namespace MapsExtended.Editor
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MapsExtendedEditorMapObject : Attribute
    {
        public Type dataType;

        public MapsExtendedEditorMapObject(Type dataType)
        {
            this.dataType = dataType;
        }
    }
}
