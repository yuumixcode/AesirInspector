using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	public class ChildGameObjectsOnlyAttributeDrawer<T> : OdinAttributeDrawer<ChildGameObjectsOnlyAttribute, T> where T : class
	{
		private bool isValidValues;

		private bool rootIsComponent;

		private int rootCount;

		private bool isList;

		protected override void Initialize()
		{
			Transform root = GetRoot(0);
			rootIsComponent = (object)root != null;
			rootCount = base.Property.SerializationRoot.BaseValueEntry.WeakValues.Count;
			base.Property.ValueEntry.OnValueChanged += delegate
			{
				ValidateCurrentValue();
			};
			isList = base.Property.ChildResolver is ICollectionResolver;
			if (rootIsComponent)
			{
				ValidateCurrentValue();
			}
		}

		private Transform GetRoot(int index)
		{
			IPropertyValueCollection parentValues = base.Property.SerializationRoot.BaseValueEntry.WeakValues;
			Component root = parentValues[index] as Component;
			if ((bool)root)
			{
				return root.transform;
			}
			return null;
		}

		private void ValidateCurrentValue()
		{
			IPropertyValueEntry<T> entry = base.ValueEntry;
			isValidValues = true;
			if (!(entry.SmartValue as UnityEngine.Object))
			{
				return;
			}
			for (int i = 0; i < rootCount; i++)
			{
				Transform root = GetRoot(i);
				UnityEngine.Object uObj = base.ValueEntry.Values[i] as UnityEngine.Object;
				if ((bool)uObj)
				{
					Component component = uObj as Component;
					GameObject go = uObj as GameObject;
					if ((bool)go)
					{
						component = go.transform;
					}
					if (!component)
					{
						isValidValues = false;
						break;
					}
					Transform transform = component.transform;
					if (!base.Attribute.IncludeSelf && transform == root)
					{
						isValidValues = false;
						break;
					}
					if (!IsRootOf(root, transform))
					{
						isValidValues = false;
						break;
					}
				}
			}
		}

		private string GetGameObjectPath(Transform root, Transform child)
		{
			if (root == child)
			{
				return root.name;
			}
			string path = "";
			Transform curr = child;
			while ((bool)curr)
			{
				if (!base.Attribute.IncludeSelf && curr == root)
				{
					return path.Trim(new char[1] { '/' });
				}
				path = curr.name + "/" + path;
				if (base.Attribute.IncludeSelf && curr == root)
				{
					return path.Trim(new char[1] { '/' });
				}
				curr = curr.parent;
			}
			return null;
		}

		private static bool IsRootOf(Transform root, Transform child)
		{
			Transform curr = child;
			while ((bool)curr)
			{
				if (curr == root)
				{
					return true;
				}
				curr = curr.parent;
			}
			return false;
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (!rootIsComponent)
			{
				CallNextDrawer(label);
				return;
			}
			if (rootCount > 1)
			{
				CallNextDrawer(label);
				return;
			}
			if (isList)
			{
				Action prev = CollectionDrawerStaticInfo.NextCustomAddFunction;
				CollectionDrawerStaticInfo.NextCustomAddFunction = ListAddButton;
				CallNextDrawer(label);
				CollectionDrawerStaticInfo.NextCustomAddFunction = prev;
				return;
			}
			if (!isValidValues)
			{
				SirenixEditorGUI.MessageBox("The object must be a child of the selected GameObject.", MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
			}
			GUILayout.BeginHorizontal();
			float width = 15f;
			if (label != null)
			{
				width += GUIHelper.BetterLabelWidth;
			}
			IEnumerable<UnityEngine.Object> newResult = OdinSelector<UnityEngine.Object>.DrawSelectorDropdown(label, GUIContent.none, ShowSelector, GUIStyle.none, GUILayoutOptions.Width(width));
			if (newResult != null && newResult.Any())
			{
				base.ValueEntry.SmartValue = newResult.FirstOrDefault() as T;
			}
			if (Event.current.type == EventType.Repaint)
			{
				Rect btnRect = GUILayoutUtility.GetLastRect().AlignRight(15f);
				btnRect.y += 4f;
				SirenixGUIStyles.PaneOptions.Draw(btnRect, GUIContent.none, 0);
			}
			GUILayout.BeginVertical();
			CallNextDrawer(null);
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}

		private void ListAddButton()
		{
			OdinSelector<UnityEngine.Object> selector = ShowSelector(default(Rect));
			selector.SelectionConfirmed += delegate(IEnumerable<UnityEngine.Object> x)
			{
				ICollectionResolver collectionResolver = base.Property.ChildResolver as ICollectionResolver;
				collectionResolver.QueueAdd(new object[1] { x.FirstOrDefault() });
			};
		}

		private OdinSelector<UnityEngine.Object> ShowSelector(Rect rect)
		{
			GenericSelector<UnityEngine.Object> selector = CreateSelector();
			if (rect == default(Rect))
			{
				UnityShims.Rect.Ctor(out rect, Event.current.mousePosition, Vector2.zero);
				rect.x = (int)rect.x;
				rect.y = (int)rect.y;
				rect.width = (int)rect.width;
				rect.height = (int)rect.height;
				if (!isList)
				{
					rect.xMax = GUIHelper.GetCurrentLayoutRect().xMax;
				}
				selector.ShowInPopup(rect, new Vector2(0f, 0f));
			}
			else
			{
				rect.x = (int)rect.x;
				rect.y = (int)rect.y;
				rect.width = (int)rect.width;
				rect.height = (int)rect.height;
				rect.xMax = GUIHelper.GetCurrentLayoutRect().xMax;
				selector.ShowInPopup(rect);
			}
			return selector;
		}

		private GenericSelector<UnityEngine.Object> CreateSelector()
		{
			Type t = (isList ? (base.Property.ChildResolver as ICollectionResolver).ElementType : typeof(T));
			bool isGo = t == typeof(GameObject);
			Transform root = GetRoot(0);
			IEnumerable<UnityEngine.Object> children = (from x in root.GetComponentsInChildren(isGo ? typeof(Transform) : t, base.Attribute.IncludeInactive)
				where base.Attribute.IncludeSelf || x.transform != root
				select x).OfType<UnityEngine.Object>();
			if (isGo)
			{
				children = (from x in children.OfType<Component>()
					select x.gameObject).OfType<UnityEngine.Object>();
			}
			Func<UnityEngine.Object, string> getName = delegate(UnityEngine.Object x)
			{
				Component component = x as Component;
				GameObject gameObject = x as GameObject;
				Transform child = (component ? component.transform : gameObject.transform);
				return GetGameObjectPath(root, child);
			};
			GenericSelector<UnityEngine.Object> selector = new GenericSelector<UnityEngine.Object>(null, supportsMultiSelect: false, getName, children.Where((UnityEngine.Object x) => x.GetType().InheritsFrom(t)));
			selector.SelectionTree.Config.DrawSearchToolbar = true;
			selector.SetSelection(base.ValueEntry.SmartValue as UnityEngine.Object);
			selector.SelectionTree.EnumerateTree().AddThumbnailIcons(preferAssetPreviewAsIcon: true);
			(from x in selector.SelectionTree.EnumerateTree()
				where x.Icon == null
				select x).ForEach(delegate(OdinMenuItem x)
			{
				x.Icon = EditorIcons.UnityGameObjectIcon;
			});
			selector.SelectionTree.EnumerateTree().ForEach(delegate(OdinMenuItem x)
			{
				x.Toggled = true;
			});
			selector.EnableSingleClickToSelect();
			return selector;
		}
	}
}
