using System;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Reflection.Editor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	public struct ProjectEvent : IEquatable<ProjectEvent>
	{
		public ProjectEventType Type;

		public string Path;

		public string AssetGuid;

		[Obsolete("Use EntityId instead.")]
		public int InstanceID;

		public OdinEntityId EntityId;

		public ProjectEventSource Source;

		public SceneReference Scene;

		public UnityEngine.Object UnityObject => EntityId.ToObject();

		private string Info
		{
			get
			{
				if (EntityId.IsValid)
				{
					UnityEngine.Object uObj = EntityId.ToObject();
					if ((bool)uObj)
					{
						if (uObj is GameObject)
						{
							GameObject go = uObj as GameObject;
							return go.name;
						}
						if (uObj is Component)
						{
							Component component = uObj as Component;
							return component.gameObject.name + "/" + component.GetType().Name;
						}
						return uObj.name;
					}
					return EntityId.ToString();
				}
				if (Path != null)
				{
					return Path;
				}
				return "Unkonwn Event Info";
			}
		}

		public bool Equals(ProjectEvent other)
		{
			if (other.Path == Path && other.AssetGuid == AssetGuid && other.EntityId == EntityId)
			{
				return other.Type == Type;
			}
			return false;
		}

		public override string ToString()
		{
			return Info;
		}
	}
}
