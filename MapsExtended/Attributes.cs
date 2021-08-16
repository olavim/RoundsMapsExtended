﻿using System;

namespace MapsExt
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MapsExtendedMapObject : Attribute
    {
        public string name;
        public Type dataType;

        public MapsExtendedMapObject(Type dataType)
        {
            this.name = dataType.Name;
            this.dataType = dataType;
        }

        public MapsExtendedMapObject(string name, Type dataType)
        {
            this.name = name;
            this.dataType = dataType;
        }
    }
}
