using MapsExt.Properties;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MapsExt.MapObjects
{
	public abstract class MapObjectData
	{
		private class PropertyMember
		{
			private readonly MapObjectData _instance;
			private readonly MemberInfo _member;

			public IProperty Value
			{
				get => (IProperty) this._member.GetMemberValue(this._instance);
				set => this._member.SetMemberValue(this._instance, value);
			}

			public PropertyMember(MapObjectData instance, MemberInfo member)
			{
				this._instance = instance;
				this._member = member;
			}

			public bool IsAssignableTo(Type type)
			{
				return type.IsAssignableFrom(this._member.GetReturnType());
			}
		}

		public string mapObjectId = Guid.NewGuid().ToString();
		public bool active = true;

		[NonSerialized]
		private readonly List<PropertyMember> _propertyMembers;

		protected MapObjectData()
		{
			this._propertyMembers = new List<PropertyMember>();

			const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
			var props = this.GetType().GetProperties(flags).Where(p => p.GetReturnType() != null && typeof(IProperty).IsAssignableFrom(p.GetReturnType()));
			var fields = this.GetType().GetFields(flags).Where(p => p.GetReturnType() != null && typeof(IProperty).IsAssignableFrom(p.GetReturnType()));
			this._propertyMembers.AddRange(props.Select(member => new PropertyMember(this, member)));
			this._propertyMembers.AddRange(fields.Select(member => new PropertyMember(this, member)));
		}

		public TProp GetProperty<TProp>() where TProp : IProperty
		{
			return (TProp) this.GetProperty(typeof(TProp));
		}

		public IProperty GetProperty(Type propertyType)
		{
			return this._propertyMembers.Find(m => m.IsAssignableTo(propertyType))?.Value;
		}

		public TProp[] GetProperties<TProp>() where TProp : IProperty
		{
			return this.GetProperties(typeof(TProp)).Cast<TProp>().ToArray();
		}

		public IProperty[] GetProperties(Type propertyType)
		{
			return this._propertyMembers.Where(m => m.IsAssignableTo(propertyType)).Select(m => m.Value).ToArray();
		}

		public void SetProperty<TProp>(TProp property) where TProp : IProperty
		{
			this._propertyMembers.Find(m => m.IsAssignableTo(typeof(TProp))).Value = property;
		}

		public bool TrySetProperty<TProp>(TProp property) where TProp : IProperty
		{
			var member = this._propertyMembers.Find(m => m.IsAssignableTo(typeof(TProp)));
			if (member == null)
			{
				return false;
			}

			member.Value = property;
			return true;
		}

		public override string ToString()
		{
			return $"MapObject ({this.GetType()})\nid: {this.mapObjectId}";
		}
	}
}
