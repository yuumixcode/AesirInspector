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
	public sealed class FolderPathAttributeDrawer : OdinAttributeDrawer<FolderPathAttribute, string>, IDefinesGenericMenuItems
	{
		private ValueResolver<string> parentResolver;

		/// <summary>
		/// Initializes the drawer.
		/// </summary>
		protected override void Initialize()
		{
			parentResolver = ValueResolver.GetForString(base.Property, base.Attribute.ParentFolder);
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (parentResolver.HasError)
			{
				SirenixEditorGUI.MessageBox(parentResolver.ErrorMessage, MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
			}
			EditorGUI.BeginChangeCheck();
			base.ValueEntry.SmartValue = SirenixEditorFields.FolderPathField(label, base.ValueEntry.SmartValue, parentResolver.GetValue(), base.Attribute.AbsolutePath, base.Attribute.UseBackslashes);
			if (EditorGUI.EndChangeCheck())
			{
				GUIHelper.ExitGUI(removeFocusControl: false);
			}
		}

		/// <summary>
		/// Adds customs generic menu options.
		/// </summary>
		public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			InspectorProperty parentProperty = property.FindParent((InspectorProperty p) => p.Info.HasSingleBackingMember, includeSelf: true);
			IPropertyValueEntry<string> entry = (IPropertyValueEntry<string>)property.ValueEntry;
			string parent = parentResolver.GetValue();
			if (genericMenu.GetItemCount() > 0)
			{
				genericMenu.AddSeparator("");
			}
			bool exists = false;
			string createDirectoryPath = entry.SmartValue;
			if (!createDirectoryPath.IsNullOrWhitespace())
			{
				if (!Path.IsPathRooted(createDirectoryPath))
				{
					if (!parent.IsNullOrWhitespace())
					{
						createDirectoryPath = Path.Combine(parent, createDirectoryPath);
					}
					createDirectoryPath = Path.GetFullPath(createDirectoryPath);
				}
				exists = Directory.Exists(createDirectoryPath);
			}
			string showInExplorerPath = createDirectoryPath;
			if (showInExplorerPath.IsNullOrWhitespace())
			{
				if (!parent.IsNullOrWhitespace())
				{
					showInExplorerPath = Path.GetFullPath(parent);
				}
				else
				{
					showInExplorerPath = Path.GetDirectoryName(Application.dataPath);
				}
			}
			while (!showInExplorerPath.IsNullOrWhitespace() && !Directory.Exists(showInExplorerPath))
			{
				showInExplorerPath = Path.GetDirectoryName(showInExplorerPath);
			}
			if (!showInExplorerPath.IsNullOrWhitespace())
			{
				genericMenu.AddItem(new GUIContent("Show in explorer"), on: false, delegate
				{
					Application.OpenURL(showInExplorerPath);
				});
			}
			else
			{
				genericMenu.AddDisabledItem(new GUIContent("Show in explorer"));
			}
			if (exists || createDirectoryPath.IsNullOrWhitespace())
			{
				genericMenu.AddDisabledItem(new GUIContent("Create directory"));
				return;
			}
			genericMenu.AddItem(new GUIContent("Create directory"), on: false, delegate
			{
				Directory.CreateDirectory(createDirectoryPath);
				AssetDatabase.Refresh();
			});
		}
	}
}
