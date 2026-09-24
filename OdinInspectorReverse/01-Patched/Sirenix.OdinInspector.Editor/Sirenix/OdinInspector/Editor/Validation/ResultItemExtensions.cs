using System;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public static class ResultItemExtensions
	{
		public static ref ResultItem WithFix(this ref ResultItem item, string title, Action fix, bool offerInInspector = true)
		{
			item.Fix = Fix.Create(title, fix, offerInInspector);
			return ref item;
		}

		public static ref ResultItem WithFix<T>(this ref ResultItem item, string title, Action<T> fix, bool offerInInspector = true) where T : new()
		{
			item.Fix = Fix.Create(title, fix, offerInInspector);
			return ref item;
		}

		public static ref ResultItem WithFix<T>(this ref ResultItem item, Action<T> fix, bool offerInInspector = true) where T : new()
		{
			item.Fix = Fix.Create(fix, offerInInspector);
			return ref item;
		}

		public static ref ResultItem WithFix(this ref ResultItem item, Action fix, bool offerInInspector = true)
		{
			item.Fix = Fix.Create(fix, offerInInspector);
			return ref item;
		}

		public static ref ResultItem WithFix(this ref ResultItem item, Fix fix)
		{
			item.Fix = fix;
			return ref item;
		}

		public static ref ResultItem WithContextClick(this ref ResultItem item, string path, Action onClick)
		{
			return ref item.WithContextClick(path, on: false, onClick);
		}

		public static ref ResultItem WithContextClick(this ref ResultItem item, string path, bool on, Action onClick)
		{
			ref Action<GenericMenu> onContextClick = ref item.OnContextClick;
			onContextClick = (Action<GenericMenu>)Delegate.Combine(onContextClick, (Action<GenericMenu>)delegate(GenericMenu menu)
			{
				menu.AddItem(new GUIContent(path), on, delegate
				{
					onClick();
				});
			});
			return ref item;
		}

		public static ref ResultItem WithContextClick(this ref ResultItem item, Action<GenericMenu> onContextClick)
		{
			ref Action<GenericMenu> onContextClick2 = ref item.OnContextClick;
			onContextClick2 = (Action<GenericMenu>)Delegate.Combine(onContextClick2, onContextClick);
			return ref item;
		}

		public static ref ResultItem WithSceneGUI(this ref ResultItem item, Action onSceneGUI)
		{
			item.OnSceneGUI = onSceneGUI;
			return ref item;
		}

		public static ref ResultItem WithMetaData(this ref ResultItem resultItem, string name, object value, params Attribute[] attributes)
		{
			resultItem.MetaData = resultItem.MetaData ?? new ResultItemMetaData[0];
			Array.Resize(ref resultItem.MetaData, resultItem.MetaData.Length + 1);
			resultItem.MetaData[resultItem.MetaData.Length - 1] = new ResultItemMetaData(name, value, attributes);
			return ref resultItem;
		}

		public static ref ResultItem WithMetaData(this ref ResultItem resultItem, object value, params Attribute[] attributes)
		{
			resultItem.MetaData = resultItem.MetaData ?? new ResultItemMetaData[0];
			Array.Resize(ref resultItem.MetaData, resultItem.MetaData.Length + 1);
			resultItem.MetaData[resultItem.MetaData.Length - 1] = new ResultItemMetaData(null, value, attributes);
			return ref resultItem;
		}

		public static ref ResultItem WithButton(this ref ResultItem resultItem, string name, Action onClick)
		{
			resultItem.MetaData = resultItem.MetaData ?? new ResultItemMetaData[0];
			Array.Resize(ref resultItem.MetaData, resultItem.MetaData.Length + 1);
			resultItem.MetaData[resultItem.MetaData.Length - 1] = new ResultItemMetaData(name, onClick);
			return ref resultItem;
		}

		public static ref ResultItem SetSelectionObject(this ref ResultItem resultItem, UnityEngine.Object obj)
		{
			resultItem.SelectionObject = obj;
			return ref resultItem;
		}

		public static ref ResultItem EnableRichText(this ref ResultItem resultItem)
		{
			resultItem.RichText = true;
			return ref resultItem;
		}
	}
}
