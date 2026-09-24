using System.Diagnostics;
using System.IO;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Not yet documented.
	/// </summary>
	public sealed class FilePathAttributeDrawer : OdinAttributeDrawer<FilePathAttribute, string>, IDefinesGenericMenuItems
	{
		private ValueResolver<string> parentResolver;

		private ValueResolver<string> extensionsResolver;

		/// <summary>
		/// Initializes the drawer.
		/// </summary>
		protected override void Initialize()
		{
			parentResolver = ValueResolver.GetForString(base.Property, base.Attribute.ParentFolder);
			extensionsResolver = ValueResolver.GetForString(base.Property, base.Attribute.Extensions);
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			ValueResolver.DrawErrors(parentResolver, extensionsResolver);
			EditorGUI.BeginChangeCheck();
			base.ValueEntry.SmartValue = SirenixEditorFields.FilePathField(label, base.ValueEntry.SmartValue, parentResolver.GetValue(), extensionsResolver.GetValue(), base.Attribute.AbsolutePath, base.Attribute.UseBackslashes, base.Attribute.IncludeFileExtension);
			if (EditorGUI.EndChangeCheck())
			{
				GUIHelper.ExitGUI(removeFocusControl: false);
			}
		}

		void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			InspectorProperty parentProperty = property.FindParent((InspectorProperty p) => p.Info.HasSingleBackingMember, includeSelf: true);
			IPropertyValueEntry<string> entry = (IPropertyValueEntry<string>)property.ValueEntry;
			string parent = parentResolver.GetValue();
			if (genericMenu.GetItemCount() > 0)
			{
				genericMenu.AddSeparator("");
			}
			string path = entry.SmartValue;
			if (!path.IsNullOrWhitespace())
			{
				if (!Path.IsPathRooted(path))
				{
					if (!parent.IsNullOrWhitespace())
					{
						path = Path.Combine(parent, path);
					}
					path = Path.GetFullPath(path);
				}
			}
			else if (!parent.IsNullOrWhitespace())
			{
				path = Path.GetFullPath(parent);
			}
			else
			{
				path = Path.GetDirectoryName(Application.dataPath);
			}
			if (!path.IsNullOrWhitespace())
			{
				while (!path.IsNullOrWhitespace() && !Directory.Exists(path))
				{
					path = Path.GetDirectoryName(path);
				}
			}
			if (!path.IsNullOrWhitespace())
			{
				genericMenu.AddItem(new GUIContent("Show in explorer"), on: false, delegate
				{
					Process.Start(path);
				});
			}
			else
			{
				genericMenu.AddDisabledItem(new GUIContent("Show in explorer"));
			}
		}
	}
}
