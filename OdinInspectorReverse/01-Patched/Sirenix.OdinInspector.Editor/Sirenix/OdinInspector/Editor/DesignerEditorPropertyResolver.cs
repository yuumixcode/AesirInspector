using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	[OdinDontRegister]
	internal class DesignerEditorPropertyResolver<T> : BaseMemberPropertyResolver<T>, IDisposable
	{
		private List<OdinPropertyProcessor> processors;

		protected override bool AllowNullValues => true;

		public void Dispose()
		{
			if (processors == null)
			{
				return;
			}
			for (int i = 0; i < processors.Count; i++)
			{
				if (processors[i] is IDisposable disposable)
				{
					disposable.Dispose();
				}
			}
		}

		protected override InspectorPropertyInfo[] GetPropertyInfos()
		{
			try
			{
				if (processors == null)
				{
					processors = OdinPropertyProcessorLocator.GetMemberProcessors(base.Property);
				}
				DesignerPropertyTree designerTree = (DesignerPropertyTree)base.Property.Tree;
				bool includeSpeciallySerializedMembers = !base.Property.ValueEntry.SerializationBackend.IsUnity;
				List<InspectorPropertyInfo> infos = InspectorPropertyInfoUtility.CreateMemberProperties(base.Property, designerTree.ClosedTargetType, includeSpeciallySerializedMembers);
				for (int i = 0; i < processors.Count; i++)
				{
					ProcessedMemberPropertyResolverExtensions.ProcessingOwnerType = designerTree.ClosedTargetType;
					try
					{
						processors[i].ProcessMemberProperties(infos);
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
					}
				}
				return InspectorPropertyInfoUtility.BuildPropertyGroupsAndFinalize(base.Property, designerTree.ClosedTargetType, infos, includeSpeciallySerializedMembers);
			}
			catch (Exception)
			{
				throw;
			}
		}
	}
}
