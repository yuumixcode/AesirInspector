using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	internal sealed class DesignerPropertyTree : PropertyTree
	{
		public Type ClosedTargetType;

		protected Type targetTypeBackingField;

		protected InspectorProperty rootProperty;

		protected object[] weakTargets;

		protected ImmutableList<object> immutableWeakTargets;

		private List<OdinPropertyProcessor> processors;

		public override SerializedObject UnitySerializedObject => null;

		public override int UpdateID => 1;

		public override Type TargetType => targetTypeBackingField;

		public override ImmutableList<object> WeakTargets => immutableWeakTargets;

		public override int RootPropertyCount => HasRootPropertyYet ? 1 : 0;

		public override PrefabModificationHandler PrefabModificationHandler => null;

		[Obsolete]
		public override bool IncludesSpeciallySerializedMembers => true;

		protected override bool HasRootPropertyYet => rootProperty != null;

		public override InspectorProperty RootProperty
		{
			get
			{
				if (rootProperty == null)
				{
					InspectorPropertyInfo rootInfo = InspectorPropertyInfo.CreateValue("$ROOT", 0f, base.SerializationBackend, GetterSetterUtility.GetEmptyGetterSetter(typeof(int), ClosedTargetType), (Attribute[])null);
					rootProperty = InspectorProperty.Create(this, null, rootInfo, 0, isRoot: true);
					weakTargets[0] = rootProperty;
					rootProperty.Update(forceUpdate: true);
				}
				return rootProperty;
			}
		}

		public override InspectorProperty SecretRootProperty => RootProperty;

		public bool HasGenericBeenClosedByDesigner => ClosedTargetType != TargetType;

		public DesignerPropertyTree(Type targetType, Type closedTargetType)
		{
			IsDesignerTree = true;
			base.SerializationBackend = SerializationBackend.None;
			targetTypeBackingField = targetType;
			ClosedTargetType = closedTargetType;
			weakTargets = new object[1];
			immutableWeakTargets = new ImmutableList<object>(weakTargets);
		}

		public DesignerPropertyTree(Type targetType)
			: this(targetType, targetType)
		{
		}

		public override void CleanForCachedReuse()
		{
		}

		public override void SetTargets(params object[] newTargets)
		{
		}

		public override void SetSerializedObject(SerializedObject serializedObject)
		{
		}

		public override void RegisterPropertyDirty(InspectorProperty property)
		{
		}

		public override void DelayAction(Action action)
		{
		}

		public override void DelayActionUntilRepaint(Action action)
		{
		}

		public override bool ObjectIsReferenced(object value, out string referencePath)
		{
			referencePath = null;
			return false;
		}

		public override int GetReferenceCount(object reference)
		{
			return 0;
		}

		public override void UpdateTree()
		{
		}

		public override void ReplaceAllReferences(object from, object to)
		{
		}

		public override InspectorProperty GetRootProperty(int index)
		{
			return RootProperty.Children[index];
		}

		public override void InvokeDelayedActions()
		{
		}

		public override bool ApplyChanges()
		{
			return false;
		}

		internal override SerializedObject GetUnitySerializedObjectNoUpdate()
		{
			return null;
		}

		internal override void ForceRegisterObjectReference(object reference, InspectorProperty property)
		{
		}

		protected override void DisposeAndResetRootProperty()
		{
			if (rootProperty != null)
			{
				rootProperty.Dispose();
				rootProperty = null;
			}
		}

		protected override void DisposeInheritedStuff()
		{
		}

		public InspectorPropertyInfo[] CreateAndGetPropertyInfos()
		{
			if (processors == null)
			{
				processors = OdinPropertyProcessorLocator.GetMemberProcessors(RootProperty);
			}
			List<InspectorPropertyInfo> infos = InspectorPropertyInfoUtility.CreateMemberProperties(RootProperty, TargetType, includeSpeciallySerializedMembers: true);
			for (int i = 0; i < processors.Count; i++)
			{
				ProcessedMemberPropertyResolverExtensions.ProcessingOwnerType = TargetType;
				try
				{
					processors[i].ProcessMemberProperties(infos);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
			return InspectorPropertyInfoUtility.BuildPropertyGroupsAndFinalize(RootProperty, TargetType, infos, includeSpeciallySerializedMembers: true);
		}

		protected override void Dispose(bool finalizer)
		{
			base.Dispose(finalizer);
			if (processors == null)
			{
				return;
			}
			for (int i = 0; i < processors.Count; i++)
			{
				OdinPropertyProcessor processor = processors[i];
				if (processor is IDisposable disposable)
				{
					disposable.Dispose();
				}
			}
			processors = null;
		}
	}
}
