using MapsExt.Properties;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;

namespace MapsExt.MapObjects
{
	public abstract class MapObjectData : ISerializationCallbackReceiver
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

			public Type Type => this._member.GetReturnType();

			public PropertyMember(MapObjectData instance, MemberInfo member)
			{
				this._instance = instance;
				this._member = member;
			}
		}

		private List<PropertyMember> _propertyMembers;

		[SerializeField] private string _mapObjectId = Guid.NewGuid().ToString();

		[SerializeField]
		[FormerlySerializedAs("active")]
		private bool _active = true;

		public string MapObjectId { get => this._mapObjectId; set => this._mapObjectId = value; }
		public bool Active { get => this._active; set => this._active = value; }

		protected MapObjectData()
		{
			this.InitMembers();
		}

		private void InitMembers()
		{
			const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
			var props = this.GetType().GetProperties(flags).Where(p => p.GetReturnType() != null && typeof(IProperty).IsAssignableFrom(p.GetReturnType()));
			var fields = this.GetType().GetFields(flags).Where(p => p.GetReturnType() != null && typeof(IProperty).IsAssignableFrom(p.GetReturnType()));
			this._propertyMembers = new();
			this._propertyMembers.AddRange(props.Select(member => new PropertyMember(this, member)));
			this._propertyMembers.AddRange(fields.Select(member => new PropertyMember(this, member)));
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			this.InitMembers();
			this.OnAfterDeserialize();
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize() => this.OnBeforeSerialize();

		protected virtual void OnAfterDeserialize() { }
		protected virtual void OnBeforeSerialize() { }

		public TProp GetProperty<TProp>() where TProp : IProperty
		{
			return (TProp) this.GetProperty(typeof(TProp));
		}

		public IProperty GetProperty(Type propertyType)
		{
			return this._propertyMembers.Find(m => m.Type == propertyType)?.Value;
		}

		public TProp[] GetProperties<TProp>() where TProp : IProperty
		{
			return this.GetProperties(typeof(TProp)).Cast<TProp>().ToArray();
		}

		public IProperty[] GetProperties(Type propertyType)
		{
			return this._propertyMembers.Where(m => propertyType.IsAssignableFrom(m.Type)).Select(m => m.Value).ToArray();
		}

		public void SetProperty<TProp>(TProp property) where TProp : IProperty
		{
			this._propertyMembers.Find(m => property.GetType() == m.Type).Value = property;
		}

		public bool TrySetProperty<TProp>(TProp property) where TProp : IProperty
		{
			var member = this._propertyMembers.Find(m => property.GetType() == m.Type);
			if (member == null)
			{
				return false;
			}

			member.Value = property;
			return true;
		}

		public override string ToString()
		{
			return $"MapObject ({this.GetType()})\nid: {this.MapObjectId}";
		}
	}
}
