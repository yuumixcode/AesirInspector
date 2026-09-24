using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public class SelfValidator<T> : ValueValidator<T> where T : ISelfValidator
	{
		public override bool CanValidateProperty(InspectorProperty property)
		{
			if (!property.IsTreeRoot && typeof(UnityEngine.Object).IsAssignableFrom(property.ValueEntry.TypeOfValue))
			{
				return false;
			}
			return true;
		}

		protected override void Validate(ValidationResult result)
		{
			if (base.ValueEntry.SmartValue == null)
			{
				return;
			}
			SelfValidationResult selfResult = new SelfValidationResult();
			base.ValueEntry.SmartValue.Validate(selfResult);
			int count = selfResult.Count;
			for (int i = 0; i < count; i++)
			{
				ref SelfValidationResult.ResultItem selfEntry = ref selfResult[i];
				ResultItem item = new ResultItem
				{
					Message = selfEntry.Message
				};
				switch (selfEntry.ResultType)
				{
				case SelfValidationResult.ResultType.Error:
					item.ResultType = ValidationResultType.Error;
					break;
				case SelfValidationResult.ResultType.Warning:
					item.ResultType = ValidationResultType.Warning;
					break;
				case SelfValidationResult.ResultType.Valid:
					item.ResultType = ValidationResultType.Valid;
					break;
				default:
					throw new NotImplementedException(selfEntry.ResultType.ToString());
				}
				if (selfEntry.MetaData != null)
				{
					ResultItemMetaData[] metaData = new ResultItemMetaData[selfEntry.MetaData.Length];
					for (int j = 0; j < metaData.Length; j++)
					{
						SelfValidationResult.ResultItemMetaData metaDataItem = selfEntry.MetaData[j];
						metaData[j] = new ResultItemMetaData(metaDataItem.Name, metaDataItem.Value, metaDataItem.Attributes);
					}
					item.MetaData = metaData;
				}
				if (selfEntry.Fix.HasValue)
				{
					SelfFix fix = selfEntry.Fix.Value;
					if (fix.Action.GetType() == typeof(Action))
					{
						item.Fix = new Fix(fix.Title, (Action)fix.Action, fix.OfferInInspector);
					}
					else
					{
						if (!fix.Action.GetType().IsGenericType || !(fix.Action.GetType().GetGenericTypeDefinition() == typeof(Action<>)))
						{
							result.AddError("Given fix '" + fix.Title + "' had an invalid delegate type of '" + fix.Action.GetType().GetNiceName() + "', for validation " + selfEntry.ResultType.ToString().ToLower() + " result with message '" + selfEntry.Message + "'; only System.Action and System.Action<T> are allowed.");
							continue;
						}
						item.Fix = new Fix
						{
							Title = fix.Title,
							Action = fix.Action,
							OfferInInspector = fix.OfferInInspector,
							ArgType = fix.Action.GetType().GetGenericArguments()[0]
						};
					}
				}
				if (selfEntry.OnContextClick != null)
				{
					item.OnContextClick = CreateOnContextClickInvoker(selfEntry.OnContextClick);
				}
				item.OnSceneGUI = selfEntry.OnSceneGUI;
				item.SelectionObject = selfEntry.SelectionObject;
				item.RichText = selfEntry.RichText;
				result.Add(item);
			}
		}

		private static Action<GenericMenu> CreateOnContextClickInvoker(Func<IEnumerable<SelfValidationResult.ContextMenuItem>> onContextClick)
		{
			return delegate(GenericMenu menu)
			{
				Delegate[] invocationList = onContextClick.GetInvocationList();
				foreach (Delegate obj in invocationList)
				{
					IEnumerable<SelfValidationResult.ContextMenuItem> enumerable = (IEnumerable<SelfValidationResult.ContextMenuItem>)obj.DynamicInvoke();
					foreach (SelfValidationResult.ContextMenuItem current in enumerable)
					{
						if (current.AddSeparatorBefore)
						{
							int num = current.Path.LastIndexOf('/');
							if (num > 0)
							{
								menu.AddSeparator(current.Path.Substring(0, num));
							}
							else
							{
								menu.AddSeparator("");
							}
						}
						menu.AddItem(new GUIContent(current.Path), current.On, CreateMenuFunction(current.OnClick));
					}
				}
			};
		}

		private static GenericMenu.MenuFunction CreateMenuFunction(Action onClick)
		{
			return (GenericMenu.MenuFunction)Delegate.CreateDelegate(typeof(GenericMenu.MenuFunction), onClick.Target, onClick.Method);
		}
	}
}
