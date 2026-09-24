using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	public class ValidationProfileSelector
	{
		public OdinSelector<ValidationProfile> Selector;

		public ValidationProfileSelector(ValidationProfile selected)
		{
			ValidationProfile main = ValidationProfile.MainValidationProfile;
			Selector = new GenericSelector<ValidationProfile>(null, ValidationProfile.FindAll(), supportsMultiSelect: false, (ValidationProfile x) => (!(x == main)) ? x.name : (x.name + " (main)"));
			Selector.SetSelection(selected);
			Selector.SelectionTree.Config.DrawSearchToolbar = false;
			Selector.EnableSingleClickToSelect();
			OdinMenuStyle style = Selector.SelectionTree.DefaultMenuStyle;
			foreach (OdinMenuItem item in Selector.SelectionTree.EnumerateTree())
			{
				ValidationProfile p = item.Value as ValidationProfile;
				if (!p || p.Icon == SdfIconType.None)
				{
					continue;
				}
				OdinMenuItem localItem = item;
				localItem.Icon = EditorIcons.Transparent.Active;
				localItem.IconSelected = EditorIcons.Transparent.Active;
				OdinMenuItem odinMenuItem = localItem;
				odinMenuItem.OnDrawItem = (Action<OdinMenuItem>)Delegate.Combine(odinMenuItem.OnDrawItem, (Action<OdinMenuItem>)delegate
				{
					Rect rect = localItem.LabelRect.Padding(2f, 0f).AlignLeft(localItem.LabelRect.height).AddX(0f - localItem.LabelRect.height - 8f);
					if (localItem.IsSelected)
					{
						SdfIcons.DrawIcon(rect, p.Icon, style.SelectedLabelStyle.normal.textColor);
					}
					else
					{
						SdfIcons.DrawIcon(rect, p.Icon, style.DefaultLabelStyle.normal.textColor);
					}
				});
			}
			Selector.SelectionChanged += delegate(IEnumerable<ValidationProfile> profiles)
			{
				ValidationProfile validationProfile = profiles.FirstOrDefault();
				if ((bool)validationProfile)
				{
					EditorGUIUtility.PingObject(validationProfile);
				}
			};
			Selector.SelectionConfirmed += delegate(IEnumerable<ValidationProfile> profiles)
			{
				ValidationProfile validationProfile = profiles.FirstOrDefault();
				if ((bool)validationProfile)
				{
					EditorGUIUtility.PingObject(validationProfile);
				}
			};
		}

		[OnInspectorGUI]
		[PropertyOrder(-1f)]
		private void OnGUI()
		{
			int h = Selector.SelectionTree.Config.SearchToolbarHeight;
			SirenixEditorGUI.BeginHorizontalToolbar(h);
			GUILayout.Label("Create or select a new validation profile");
			GUILayout.FlexibleSpace();
			Rect r = GUILayoutUtility.GetRect(h, h);
			if (SirenixEditorGUI.SDFIconButton(r, (string)null, SdfIconType.Plus, IconAlignment.LeftOfText, SirenixGUIStyles.ToolbarButton))
			{
				ValidationProfile asset = ScriptableObject.CreateInstance<ValidationProfile>();
				string path = EditorUtility.SaveFilePanelInProject("Create new session config", "My Validation Profile", "asset", null, ValidationProfile.DefaultConfigFolderPath);
				if (!string.IsNullOrEmpty(path))
				{
					path = AssetDatabase.GenerateUniqueAssetPath(path);
					AssetDatabase.CreateAsset(asset, path);
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
					ValidationProfile.MainValidationProfile = asset;
				}
			}
			SirenixEditorGUI.EndHorizontalToolbar();
			GUILayout.Space(-1f);
			Selector.OnInspectorGUI();
		}
	}
}
