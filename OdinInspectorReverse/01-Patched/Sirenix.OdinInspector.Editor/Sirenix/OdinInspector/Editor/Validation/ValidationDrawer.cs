using System;
using Clipboard = Sirenix.Utilities.Editor.Clipboard;
using System.Collections.Generic;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Validation
{
	[DrawerPriority(0.0, 10000.0, 0.0)]
	public class ValidationDrawer<T> : OdinValueDrawer<T>, IDisposable
	{
		private List<ValidationResult> oldValidationResults;

		private List<ValidationResult> validationResults;

		private bool rerunFullValidation;

		private bool revalidateEveryFrame;

		private bool subscribedToEvents;

		private bool odinValidatorIsInstalled;

		private int uniqueIdHashForThisValidationDrawer;

		private ValidationComponent validationComponent;

		private Dictionary<int, PropertyTree> issueFixerTrees = new Dictionary<int, PropertyTree>();

		private Dictionary<int, PropertyTree> metaDataTrees = new Dictionary<int, PropertyTree>();

		private GUIStyle messageBoxText;

		private GUIStyle fixArgsPadding;

		private Texture2D _defaultIconTexture;

		private static Color HighlightedBgColor
		{
			get
			{
				if (!EditorGUIUtility.isProSkin)
				{
					return new Color(0.9372549f, 0.9372549f, 0.9372549f, 1f);
				}
				return Color.Lerp(EditorWindowBgColor, Color.white, 0.15f);
			}
		}

		private static Color EditorWindowBgColor
		{
			get
			{
				if (!EditorGUIUtility.isProSkin)
				{
					return new Color(0.76f, 0.76f, 0.76f, 1f);
				}
				return DarkSkinEditorWindowBgColor;
			}
		}

		private static Color DarkSkinEditorWindowBgColor => new Color(0.22f, 0.22f, 0.22f, 1f);

		private GUIStyle MessageBoxText
		{
			get
			{
				if (messageBoxText == null)
				{
					messageBoxText = new GUIStyle("label")
					{
						margin = new RectOffset(4, 4, 2, 2),
						fontSize = 10,
						richText = true,
						wordWrap = true
					};
				}
				return messageBoxText;
			}
		}

		private GUIStyle FixArgsPadding
		{
			get
			{
				if (fixArgsPadding == null)
				{
					fixArgsPadding = new GUIStyle
					{
						padding = new RectOffset(5, 5, 5, 5)
					};
				}
				return fixArgsPadding;
			}
		}

		private Texture2D defaultIconTexture
		{
			get
			{
				if (_defaultIconTexture == null)
				{
					_defaultIconTexture = new Texture2D(20, 20)
					{
						hideFlags = HideFlags.HideAndDontSave
					};
					CleanupUtility.DestroyObjectOnAssemblyReload(_defaultIconTexture);
				}
				return _defaultIconTexture;
			}
		}

		protected override bool CanDrawValueProperty(InspectorProperty property)
		{
			ValidationComponent validation = property.GetComponent<ValidationComponent>();
			if (validation == null)
			{
				return false;
			}
			if (property.GetAttribute<DontValidateAttribute>() != null)
			{
				return false;
			}
			return validation.ValidatorLocator.PotentiallyHasValidatorsFor(property);
		}

		protected override void Initialize()
		{
			uniqueIdHashForThisValidationDrawer = base.Property.GetHashCode() * "ValidationDrawer".GetHashCode();
			validationComponent = base.Property.GetComponent<ValidationComponent>();
			validationComponent.ValidateProperty(ref validationResults, explodeMultiResults: true);
			if (validationResults.Count > 0)
			{
				base.Property.Tree.OnUndoRedoPerformed += OnUndoRedoPerformed;
				base.ValueEntry.OnValueChanged += OnValueChanged;
				base.ValueEntry.OnChildValueChanged += OnChildValueChanged;
				subscribedToEvents = true;
				foreach (ValidationResult result in validationResults)
				{
					if (result.Setup.Validator is IValidator { RevalidationCriteria: RevalidationCriteria.Always })
					{
						revalidateEveryFrame = true;
					}
					ValidationEvents.InvokeOnValidationStateChanged(new ValidationStateChangeInfo
					{
						ValidationResult = result
					});
				}
				CreateIssueFixerTrees(validationResults);
			}
			odinValidatorIsInstalled = AssemblyUtilities.GetTypeByCachedFullName("Sirenix.OdinValidator.Editor.OdinValidatorWindow") != null;
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (validationResults.Count == 0)
			{
				GUIUtility.GetControlID(uniqueIdHashForThisValidationDrawer, FocusType.Passive);
				CallNextDrawer(label);
				return;
			}
			GUILayout.BeginVertical();
			SirenixEditorGUI.BeginShakeableGroup();
			bool resultsChanged = false;
			if (Event.current.type == EventType.Layout && (revalidateEveryFrame || rerunFullValidation))
			{
				List<ValidationResult> results = oldValidationResults;
				oldValidationResults = validationResults;
				results?.Clear();
				validationComponent.ValidateProperty(ref results, explodeMultiResults: true);
				validationResults = results;
				if (results.Count != oldValidationResults.Count)
				{
					resultsChanged = true;
					CreateIssueFixerTrees(results);
					for (int i = 0; i < results.Count; i++)
					{
						ValidationEvents.InvokeOnValidationStateChanged(new ValidationStateChangeInfo
						{
							ValidationResult = results[i]
						});
					}
				}
				else
				{
					for (int j = 0; j < results.Count; j++)
					{
						ValidationResult result = results[j];
						if (!result.IsMatch(oldValidationResults[j]))
						{
							if (result.ResultType == ValidationResultType.Error || result.ResultType == ValidationResultType.Warning)
							{
								resultsChanged = true;
								CreateIssueFixerTrees(results);
							}
							ValidationEvents.InvokeOnValidationStateChanged(new ValidationStateChangeInfo
							{
								ValidationResult = result
							});
						}
					}
				}
				if (resultsChanged)
				{
					SirenixEditorGUI.StartShakingGroup();
				}
			}
			for (int k = 0; k < validationResults.Count; k++)
			{
				ValidationResult result2 = validationResults[k];
				MessageType messageType = MessageType.None;
				if (result2.ResultType == ValidationResultType.Error)
				{
					messageType = MessageType.Error;
				}
				else if (result2.ResultType == ValidationResultType.Warning)
				{
					messageType = MessageType.Warning;
				}
				else if (result2.ResultType == ValidationResultType.Valid && !string.IsNullOrEmpty(result2.Message))
				{
					messageType = MessageType.Info;
				}
				if (messageType != MessageType.None)
				{
					DrawMessageBoxWithButton(ref result2[0], messageType, result2[0].OnContextClick, k, resultsChanged);
				}
			}
			if (Event.current.type == EventType.Layout)
			{
				rerunFullValidation = false;
			}
			GUIUtility.GetControlID(uniqueIdHashForThisValidationDrawer, FocusType.Passive);
			CallNextDrawer(label);
			SirenixEditorGUI.EndShakeableGroup();
			GUILayout.EndVertical();
		}

		public void Dispose()
		{
			if (subscribedToEvents)
			{
				base.Property.Tree.OnUndoRedoPerformed -= OnUndoRedoPerformed;
				base.ValueEntry.OnValueChanged -= OnValueChanged;
				base.ValueEntry.OnChildValueChanged -= OnChildValueChanged;
			}
			issueFixerTrees.Values.ForEach(delegate(PropertyTree tree)
			{
				tree?.Dispose();
			});
			metaDataTrees.Values.ForEach(delegate(PropertyTree tree)
			{
				tree?.Dispose();
			});
			issueFixerTrees = null;
			metaDataTrees = null;
			validationResults = null;
			oldValidationResults = null;
		}

		private void OnUndoRedoPerformed()
		{
			rerunFullValidation = true;
		}

		private void OnValueChanged(int index)
		{
			rerunFullValidation = true;
		}

		private void OnChildValueChanged(int index)
		{
			rerunFullValidation = true;
		}

		private void DrawMessageBoxWithButton(ref ResultItem entry, MessageType messageType, Action<GenericMenu> onContextClick, int index, bool resultsChanged)
		{
			Texture icon = defaultIconTexture;
			if (Event.current.type != EventType.Layout)
			{
				icon = messageType switch
				{
					MessageType.Info => EditorIcons.UnityInfoIcon, 
					MessageType.Warning => EditorIcons.UnityWarningIcon, 
					MessageType.Error => EditorIcons.UnityErrorIcon, 
					_ => null, 
				};
			}
			int btnCount = 0;
			PropertyTree metaDataTree = null;
			int firstButtonIndex = -1;
			if (odinValidatorIsInstalled && entry.MetaData != null && entry.MetaData.Length != 0)
			{
				metaDataTrees.TryGetValue(index, out metaDataTree);
				for (int i = 0; i < entry.MetaData.Length; i++)
				{
					if (entry.MetaData[i].Value is Action)
					{
						if (btnCount == 0)
						{
							firstButtonIndex = i;
						}
						btnCount++;
					}
				}
				if (metaDataTree == null && btnCount < entry.MetaData.Length)
				{
					metaDataTree = PropertyTree.Create(new ResultItemMetaDataDrawer(entry.MetaData, btnCount == 1));
					metaDataTrees.Add(index, metaDataTree);
				}
				if (metaDataTree != null && resultsChanged)
				{
					metaDataTree.Dispose();
					metaDataTree = PropertyTree.Create(new ResultItemMetaDataDrawer(entry.MetaData, btnCount == 1));
					metaDataTrees[index] = metaDataTree;
				}
			}
			Rect entireBox = SirenixEditorGUI.BeginVerticalWithoutUsingControlID(SirenixGUIStyles.MessageBox);
			if (btnCount > 0)
			{
				SirenixEditorGUI.BeginHorizontalWithoutUsingControlID(SirenixGUIStyles.None);
			}
			Rect messageRect = SirenixEditorGUI.BeginVerticalWithoutUsingControlID(SirenixGUIStyles.None);
			GUILayout.Label(GUIHelper.TempContent(entry.Message, icon), MessageBoxText);
			EditorGUILayout.EndVertical();
			if (btnCount > 0)
			{
				ref ResultItemMetaData firstButton = ref entry.MetaData[firstButtonIndex];
				float btnWidth = GUI.skin.button.CalcSize(GUIHelper.TempContent(firstButton.Name)).x + 10f;
				SirenixEditorGUI.BeginVerticalWithoutUsingControlID(SirenixGUIStyles.None, GUILayoutOptions.Width(btnWidth));
				GUILayout.FlexibleSpace();
				if (DrawButtonWithoutUsingControlID(firstButton.Name, btnWidth))
				{
					(firstButton.Value as Action)();
				}
				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}
			if (!odinValidatorIsInstalled)
			{
				EditorGUILayout.EndVertical();
				return;
			}
			bool showFix = entry.Fix != null && entry.Fix.OfferInInspector;
			PropertyTree fixTree = null;
			if (showFix)
			{
				GUIContent fixTitle = new GUIContent(entry.Fix.Title ?? "Fix");
				Rect fixRect = EditorGUILayout.GetControlRect(false, 20f).Expand(4f, 4f, 0f, 3f);
				string message = entry.Message;
				SirenixEditorGUI.DrawBorders(fixRect, 0, 0, 1, 1);
				Rect fixTitleRect = fixRect.AlignLeft(fixRect.width - 85f).HorizontalPadding(6f);
				GUI.Label(fixTitleRect, fixTitle, SirenixGUIStyles.Label);
				Rect buttonRect = fixRect.AlignRight(85f);
				Rect paddedButtonRect = buttonRect.HorizontalPadding(6f);
				Event evt = Event.current;
				EditorGUI.DrawRect(buttonRect.Padding(1f), buttonRect.Contains(evt.mousePosition) ? HighlightedBgColor : Color.clear);
				GUI.Label(paddedButtonRect.AlignRight(50f), "Fix now", SirenixGUIStyles.LabelCentered);
				SdfIcons.DrawIcon(paddedButtonRect.AlignLeft(20f).Padding(3f), SdfIconType.Tools);
				EditorGUI.DrawRect(buttonRect.AlignLeft(1f), SirenixGUIStyles.BorderColor);
				bool fixHasArguments = entry.Fix.ArgType != null;
				issueFixerTrees.TryGetValue(index, out fixTree);
				if (fixHasArguments && fixTree == null)
				{
					object editorObject = entry.Fix.CreateEditorObject();
					fixTree = PropertyTree.Create(editorObject);
					issueFixerTrees.Add(index, fixTree);
				}
				if (evt.OnMouseUp(buttonRect, 0))
				{
					base.Property.RecordForUndo(fixTitle.text, forceCompleteObjectUndo: true);
					if (fixHasArguments)
					{
						object args = fixTree.WeakTargets[0];
						entry.Fix.Action.DynamicInvoke(args);
					}
					else
					{
						entry.Fix.Action.DynamicInvoke();
					}
					GUIHelper.ExitGUI(removeFocusControl: true);
				}
			}
			if (Event.current.OnMouseUp(messageRect, 1))
			{
				GenericMenu menu = new GenericMenu();
				string message2 = entry.Message;
				menu.AddItem(new GUIContent("Copy message"), on: false, delegate
				{
					Clipboard.Copy(message2);
				});
				onContextClick?.Invoke(menu);
				menu.ShowAsContext();
				Event.current.Use();
			}
			if (fixTree != null)
			{
				EditorGUILayout.BeginVertical(SirenixGUIStyles.ContentPadding);
				fixTree.Draw(applyUndo: false);
				EditorGUILayout.EndVertical();
			}
			if (metaDataTree != null)
			{
				if (fixTree != null)
				{
					GUILayout.Space(2f);
					Rect header = SirenixEditorGUI.BeginToolbarBoxHeader();
					EditorGUI.DrawRect(header.AlignTop(1f).SetWidth(entireBox.width).SetX(entireBox.x), SirenixGUIStyles.BorderColor);
					GUILayout.Label("Metadata");
					SirenixEditorGUI.EndToolbarBoxHeader();
				}
				EditorGUILayout.BeginVertical(SirenixGUIStyles.ContentPadding);
				metaDataTree.Draw(applyUndo: false);
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndVertical();
		}

		private void CreateIssueFixerTrees(List<ValidationResult> results)
		{
			issueFixerTrees.Values.ForEach(delegate(PropertyTree tree)
			{
				tree?.Dispose();
			});
			issueFixerTrees.Clear();
			for (int i = 0; i < results.Count; i++)
			{
				ValidationResult result = results[i];
				Fix fix = result[0].Fix;
				if (fix?.ArgType != null)
				{
					object editorObject = fix.CreateEditorObject();
					PropertyTree fixTree = PropertyTree.Create(editorObject);
					issueFixerTrees.Add(i, fixTree);
				}
			}
		}

		private bool DrawButtonWithoutUsingControlID(string name, float width)
		{
			Rect btnRect = GUILayoutUtility.GetRect(width, 19f, GUILayoutOptions.ExpandWidth(expand: false).ExpandHeight(expand: false));
			if (Event.current.OnMouseUp(btnRect, 0))
			{
				return true;
			}
			if (Event.current.type == EventType.Repaint)
			{
				SirenixGUIStyles.Button.Draw(btnRect, name, Event.current.IsHovering(btnRect), isActive: false, on: false, hasKeyboardFocus: false);
			}
			return false;
		}
	}
}
