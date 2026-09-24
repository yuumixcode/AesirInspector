using System;
using Sirenix.OdinInspector.Editor.Validation;
using UnityEditor;

namespace Sirenix.OdinValidator.Editor
{
	public struct PersistenceData
	{
		[Obsolete("Persistence data is no longer mangled.", false)]
		public struct Entry
		{
			[Obsolete("Persistence data is no longer mangled.", false)]
			public int Id;

			[Obsolete("Persistence data is no longer mangled.", false)]
			public PersistenceDataType Type;

			[Obsolete("Persistence data is no longer mangled.", false)]
			public DynamicObjectAddress UnityObjectReferenceAddress;
		}

		public ResultItemMetaData[] MetaData;

		public Fix Fix;

		public Action<GenericMenu> OnContextClick;

		public DynamicObjectAddress SelectionObjectAddress;

		public bool RichText;

		[Obsolete("Persistence data is no longer mangled.", false)]
		public Fix MangledFix;

		[Obsolete("Persistence data is no longer mangled.", false)]
		public ResultItemMetaData[] MangledMetaData;

		[Obsolete("Persistence data is no longer mangled.", false)]
		public Action<GenericMenu> MangledOnContextClick;

		[Obsolete("Persistence data is no longer mangled.", false)]
		public Action MangledOnSceneGUI;

		[Obsolete("Persistence data is no longer mangled.", false)]
		public Entry[] RestoreEntries;
	}
}
