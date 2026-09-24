using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class DesignerFilesOverviewWindow : OdinEditorWindow
	{
		private enum Mode
		{
			[LabelText(SdfIconType.Braces)]
			Programmer,
			[LabelText(SdfIconType.Eyeglasses)]
			Designer
		}

		private const string PrefKey = "DesignerFilesOverviewWindow_SelectedFilePath";

		private const float PanelMinSize = 300f;

		private const float FileTreeItemHeight = 25f;

		private const float IssueItemHeight = 25f;

		private const float StatusBarHeight = 25f;

		private const int FontSizeMin = 10;

		private const int FontSizeMax = 50;

		private const int EditorAreaPadding = 20;

		private float fileTreeWidth = 300f;

		private float issueAreaHeight = 300f;

		private AnimatedFileTreeView fileTree;

		private AnimatedFileTreeView issueFileTree;

		private VirtualizedScrollView editorScrollView = new VirtualizedScrollView(32, 25f, new Color(0.1f, 0.1f, 0.1f));

		private VirtualizedScrollView issuesScrollView = new VirtualizedScrollView(32, 25f, new Color(0.22f, 0.22f, 0.22f));

		private SearchField searchField;

		private string searchTerm;

		private string selectedFilePath;

		private string fileContent;

		private string fileContentHighlighted;

		private string fileContentDesigner;

		private OVDFParser.OVDFFile selectedOVDFFile;

		private List<Action> delayedActions = new List<Action>();

		private GUIStyle editorStyle;

		private GUIStyle issueStyle;

		private GUIStyle rightAlignedMiniLabel;

		private Mode selectedMode;

		private GUIStyle EditorStyle
		{
			get
			{
				GUIStyle obj = editorStyle ?? new GUIStyle(SirenixGUIStyles.MultiLineLabel)
				{
					hover = 
					{
						textColor = Color.white
					},
					normal = 
					{
						textColor = Color.white
					},
					active = 
					{
						textColor = Color.white
					},
					focused = 
					{
						textColor = Color.white
					},
					wordWrap = false
				};
				GUIStyle result = obj;
				editorStyle = obj;
				return result;
			}
		}

		private GUIStyle IssueStyle
		{
			get
			{
				GUIStyle obj = issueStyle ?? new GUIStyle(EditorStyles.label)
				{
					richText = true,
					wordWrap = false,
					alignment = TextAnchor.MiddleLeft
				};
				GUIStyle result = obj;
				issueStyle = obj;
				return result;
			}
		}

		private GUIStyle RightAlignedMiniLabel
		{
			get
			{
				GUIStyle obj = rightAlignedMiniLabel ?? new GUIStyle(SirenixGUIStyles.RightAlignedWhiteMiniLabel)
				{
					normal = 
					{
						textColor = SirenixGUIStyles.Label.normal.textColor
					}
				};
				GUIStyle result = obj;
				rightAlignedMiniLabel = obj;
				return result;
			}
		}

		public static DesignerFilesOverviewWindow ShowWindow()
		{
			return EditorWindow.GetWindow<DesignerFilesOverviewWindow>("Designer Files Overview");
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			fileTree = new AnimatedFileTreeView(25f);
			issueFileTree = new AnimatedFileTreeView(25f);
			OVDFFileWatcher.FileParsed += OnFileParsed;
			OVDFFileWatcher.FileDeleted += OnFileDeleted;
			OVDFFileWatcher.FileIssuesChanged += OnFileIssuesChanged;
			OVDFFileWatcher.RootFolderPathChanged += OnRootFolderPathChanged;
			RebuildTrees();
			base.minSize = new Vector2(600f, 325f);
		}

		protected override void OnDisable()
		{
			OVDFFileWatcher.FileParsed -= OnFileParsed;
			OVDFFileWatcher.FileDeleted -= OnFileDeleted;
			OVDFFileWatcher.FileIssuesChanged -= OnFileIssuesChanged;
			OVDFFileWatcher.RootFolderPathChanged -= OnRootFolderPathChanged;
			base.OnDisable();
		}

		protected override void OnImGUI()
		{
			base.OnImGUI();
			Rect windowRect = base.position.SetPosition(Vector2.zero);
			Rect fileTreeArea = windowRect.TakeFromLeft(fileTreeWidth);
			Rect issueArea = windowRect.TakeFromBottom(issueAreaHeight);
			Rect statusBarArea = windowRect.TakeFromBottom(25f);
			Rect editorArea = windowRect;
			DrawFileTreeArea(fileTreeArea);
			DrawEditorArea(editorArea);
			DrawStatusBarArea(statusBarArea);
			DrawIssueArea(issueArea);
			foreach (Action delayedAction in delayedActions)
			{
				delayedAction?.Invoke();
			}
			delayedActions.Clear();
			if (Event.current.OnMouseDown(0))
			{
				GUIHelper.RemoveFocusControl();
			}
			Repaint();
		}

		private void DrawFileTreeArea(Rect rect)
		{
			Event e = Event.current;
			Rect slideRect = rect.AlignRight(4f);
			Rect separatorRect = rect.TakeFromRight(1f);
			int controlID = GUIUtility.GetControlID(FocusType.Passive);
			EditorGUI.DrawRect(separatorRect, ((e.IsHovering(slideRect) && GUIUtility.hotControl == 0) || GUIUtility.hotControl == controlID) ? Colors.ListItemSelected : Colors.DesignerOverviewWindow.AreaSeparator);
			Vector2 slideDelta = SirenixEditorGUI.SlideRect(slideRect, controlID, MouseCursor.ResizeHorizontal);
			fileTreeWidth = Mathf.Clamp(fileTreeWidth + slideDelta.x, 300f, base.position.width - 300f);
			Rect searchFieldRect = rect.TakeFromTop(30f);
			searchField = searchField ?? new SearchField();
			EditorGUI.BeginChangeCheck();
			searchTerm = searchField.Draw(searchFieldRect.Padding(4f), searchTerm, "Search");
			if (EditorGUI.EndChangeCheck())
			{
				fileTree.SearchText = searchTerm;
				issueFileTree.SearchText = searchTerm;
			}
			if (OVDFFileWatcher.FilesWithIssues.Count > 0)
			{
				Rect r1 = rect.SplitVertical(0, 2);
				Rect r2 = rect.SplitVertical(1, 2);
				DrawFileTree($"Issues ({OVDFFileWatcher.FilesWithIssues.Count})", issueFileTree, r1, showDiagnostics: true);
				DrawFileTree("OVDF Files", fileTree, r2);
			}
			else
			{
				DrawFileTree("OVDF Files", fileTree, rect);
			}
		}

		private void DrawFileTree(string title, AnimatedFileTreeView tree, Rect rect, bool showDiagnostics = false)
		{
			Event e = Event.current;
			Rect titleRect = rect.TakeFromTop(25f);
			EditorGUI.DrawRect(titleRect.TakeFromTop(1f), Colors.DesignerOverviewWindow.AreaSeparator);
			EditorGUI.DrawRect(titleRect.TakeFromBottom(1f), Colors.DesignerOverviewWindow.AreaSeparator);
			EditorGUI.DrawRect(titleRect, Colors.DesignerOverviewWindow.StatusBarBackground);
			GUI.Label(titleRect, title, SirenixGUIStyles.LabelCentered);
			GUI.BeginGroup(rect);
			Rect localRect = new Rect(0f, 0f, rect.width, rect.height);
			tree.Layout(localRect, e);
			int first = tree.FirstVisibleRowIndex;
			int last = tree.LastVisibleRowIndexExclusive;
			for (int i = first; i < last; i++)
			{
				AnimatedFileTreeView.Row node = tree.GetRow(i);
				Rect rowRect = tree.GetRowRect(i);
				string path = tree.GetFullPath(node.NodeId);
				Color bgColor = (Event.current.IsHovering(rowRect) ? ((selectedFilePath == path) ? Colors.ListItemHoverSelected : Colors.ListItemHover) : ((selectedFilePath == path) ? Colors.ListItemSelected : ((i % 2 != 0) ? Colors.ListItemOdd : Colors.ListItemEven)));
				EditorGUI.DrawRect(rowRect, bgColor);
				Color prevColor = GUI.color;
				float alpha = node.HeightFactor;
				GUI.color = new Color(prevColor.r, prevColor.g, prevColor.b, prevColor.a * alpha);
				if (!node.IsDirectory)
				{
					HandleFileContextClick(rowRect, path);
				}
				rowRect = rowRect.AddXMin((float)node.Depth * 10f);
				Rect iconRect = rowRect.TakeFromLeft(25f);
				if (!node.IsDirectory && showDiagnostics)
				{
					OVDFParser.Severity maxSeverity = OVDFParser.Severity.Error;
					OVDFParser.OVDFFile ovdfFile = OVDFFileWatcher.GetOVDFFile(path);
					int j = 0;
					while (ovdfFile != null && j < ovdfFile.Diagnostics.Count)
					{
						OVDFParser.Severity sev = ovdfFile.Diagnostics[j].Severity;
						if (sev > maxSeverity)
						{
							maxSeverity = sev;
						}
						j++;
					}
					SdfIconType severityIcon = maxSeverity switch
					{
						OVDFParser.Severity.Error => SdfIconType.ExclamationOctagonFill, 
						OVDFParser.Severity.Warning => SdfIconType.ExclamationTriangleFill, 
						_ => SdfIconType.InfoCircleFill, 
					};
					Color severityColor = maxSeverity switch
					{
						OVDFParser.Severity.Error => SirenixGUIStyles.RedErrorColor, 
						OVDFParser.Severity.Warning => SirenixGUIStyles.YellowWarningColor, 
						_ => SirenixGUIStyles.LabelCentered.normal.textColor, 
					};
					SdfIcons.DrawIcon(iconRect.Padding(6f), severityIcon, severityColor);
				}
				else if (node.IsDirectory)
				{
					if (tree.SearchText.IsNullOrWhitespace())
					{
						float t = tree.GetOpenFactor(node.NodeId);
						Color iconColor = EditorStyles.label.normal.textColor;
						iconColor.a = t;
						SdfIcons.DrawIcon(iconRect.Padding(6f), SdfIconType.Folder2Open, iconColor);
						iconColor.a = 1f - t;
						SdfIcons.DrawIcon(iconRect.Padding(6f), SdfIconType.FolderFill, iconColor);
					}
					else
					{
						SdfIcons.DrawIcon(iconRect.Padding(6f), SdfIconType.Folder2Open);
					}
				}
				else
				{
					SdfIcons.DrawIcon(iconRect.Padding(6f), SdfIconType.FileEarmarkMedicalFill);
				}
				if (node.IsDirectory)
				{
					if (e.OnMouseDown(rowRect, 0))
					{
						tree.SetFolderExpanded(node.NodeId, !tree.IsExpanded(node.NodeId), instant: false);
						GUIHelper.ExitGUI(removeFocusControl: true);
					}
				}
				else if (e.OnMouseDown(rowRect, 0))
				{
					SelectFile(tree.GetFullPath(node.NodeId));
				}
				rowRect = rowRect.HorizontalPadding(0f, 10f);
				if (!node.IsDirectory && OVDFFileWatcher.TryGetFileRecord(path, out var record))
				{
					Rect statusRect = rowRect.AlignRight(48f);
					rowRect.xMax -= 48f;
					if (record.IsReadOnly)
					{
						Rect lockRect = statusRect.TakeFromRight(20f);
						SdfIcons.DrawIcon(lockRect.Padding(3f), SdfIconType.LockFill, SirenixGUIStyles.YellowWarningColor);
						GUI.Label(lockRect, GUIHelper.TempContent(string.Empty, "Read-only OVDF file"));
					}
					if (!record.IsActive && !string.IsNullOrEmpty(record.OverridingPath))
					{
						Rect shadowRect = statusRect.TakeFromRight(20f);
						SdfIcons.DrawIcon(shadowRect.Padding(3f), SdfIconType.ArrowRightCircleFill, SirenixGUIStyles.LabelCentered.normal.textColor);
						GUI.Label(shadowRect, GUIHelper.TempContent(string.Empty, "Shadowed by a higher-precedence OVDF file. Right-click to select the overriding file."));
					}
				}
				float labelWidth = SirenixGUIStyles.Label.CalcWidth(node.DisplayName);
				if (labelWidth > rowRect.width)
				{
					DrawRightAlignedEllipsisLabel(rowRect, node.DisplayName, bgColor, SirenixGUIStyles.Label);
				}
				else
				{
					GUI.Label(rowRect, node.DisplayName, SirenixGUIStyles.Label);
				}
				GUI.color = prevColor;
			}
			GUI.EndGroup();
		}

		private void DrawEditorArea(Rect rect)
		{
			Event e = Event.current;
			if (e.type == EventType.ScrollWheel && e.control)
			{
				int delta = Math.Sign(e.delta.y);
				int newFontSize = EditorStyle.fontSize - delta;
				ChangeFontSize(newFontSize);
				e.Use();
			}
			EditorGUI.DrawRect(rect, Colors.DesignerOverviewWindow.EditorBackground);
			Rect unpaddedViewRect = rect;
			rect = rect.Padding(20f);
			Rect modeToggleRect = rect.AlignTop(30f).AlignRight(100f);
			selectedMode = (Mode)DrawEnumToggleButtons<Mode>(modeToggleRect, (int)selectedMode, out var _);
			if (selectedMode == Mode.Programmer)
			{
				editorScrollView.Reset();
				float contentHeight = EditorStyle.CalcHeight(fileContentHighlighted, rect.width);
				editorScrollView.AllocateRect(new Rect(rect.x, rect.y, rect.width, contentHeight));
				editorScrollView.Begin();
				List<VirtualizedScrollView.VisibleSlot> visibleSlots = editorScrollView.GetVisibleSlots(rect, unpaddedViewRect);
				foreach (VirtualizedScrollView.VisibleSlot item in visibleSlots)
				{
					CustomSelectableLabel.Draw(item.Rect, fileContentHighlighted, EditorStyle);
				}
				editorScrollView.End();
			}
			else
			{
				if (selectedMode != Mode.Designer)
				{
					return;
				}
				editorScrollView.Reset();
				float contentHeight2 = EditorStyle.CalcHeight(fileContentDesigner, rect.width);
				editorScrollView.AllocateRect(new Rect(rect.x, rect.y, rect.width, contentHeight2));
				editorScrollView.Begin();
				List<VirtualizedScrollView.VisibleSlot> visibleSlots2 = editorScrollView.GetVisibleSlots(rect, unpaddedViewRect);
				foreach (VirtualizedScrollView.VisibleSlot item2 in visibleSlots2)
				{
					CustomSelectableLabel.Draw(item2.Rect, fileContentDesigner, EditorStyle);
				}
				editorScrollView.End();
			}
		}

		private void DrawStatusBarArea(Rect rect)
		{
			Event e = Event.current;
			if (e.OnMouseUp(rect, 0, useEvent: false) && GUIUtility.hotControl == 0)
			{
				issueAreaHeight = ((issueAreaHeight <= 0f) ? 300f : 0f);
			}
			Rect slideRect = rect.AlignTop(4f);
			Rect separatorRect = rect.TakeFromTop(1f);
			EditorGUI.DrawRect(rect, Colors.DesignerOverviewWindow.StatusBarBackground);
			EditorGUI.DrawRect(separatorRect, Colors.DesignerOverviewWindow.AreaSeparator);
			if (issueAreaHeight > 0f)
			{
				EditorGUI.DrawRect(rect.TakeFromBottom(1f), Colors.DesignerOverviewWindow.AreaSeparator);
			}
			int controlID = GUIUtility.GetControlID(FocusType.Passive);
			EditorGUI.DrawRect(separatorRect, ((e.IsHovering(slideRect) && GUIUtility.hotControl == 0) || GUIUtility.hotControl == controlID) ? Colors.ListItemSelected : Colors.DesignerOverviewWindow.AreaSeparator);
			Vector2 slideDelta = SirenixEditorGUI.SlideRect(slideRect, controlID, MouseCursor.ResizeVertical);
			issueAreaHeight = Mathf.Clamp(issueAreaHeight - slideDelta.y, 0f, base.position.height - 300f);
			rect = rect.HorizontalPadding(6f);
			float filePathWidth = RightAlignedMiniLabel.CalcWidth(selectedFilePath);
			Rect filePathRect = rect.TakeFromLeft(filePathWidth).VerticalPadding(2f);
			DrawRightAlignedEllipsisLabel(filePathRect, selectedFilePath, new Color(0.16f, 0.16f, 0.16f), RightAlignedMiniLabel);
			HandleFileContextClick(filePathRect, selectedFilePath);
		}

		private void DrawIssueArea(Rect rect)
		{
			if (selectedOVDFFile == null)
			{
				return;
			}
			if (!selectedOVDFFile.HasDiagnostics)
			{
				GUI.Label(rect, "No issues found", SirenixGUIStyles.CenteredGreyMiniLabel);
				return;
			}
			issuesScrollView.Reset();
			for (int i = 0; i < selectedOVDFFile.Diagnostics.Count; i++)
			{
				issuesScrollView.AllocateRect(new Rect(rect.x, rect.y + (float)i * 25f, rect.width, 25f));
			}
			issuesScrollView.Begin();
			List<VirtualizedScrollView.VisibleSlot> visibleSlots = issuesScrollView.GetVisibleSlots(rect, rect);
			int rowCount = Mathf.CeilToInt(Mathf.Max((float)(selectedOVDFFile.Diagnostics.Count + 1) * 25f, rect.height) / 25f);
			for (int j = 0; j < rowCount; j++)
			{
				Rect bgRect = new Rect(0f, (float)j * 25f, rect.width, 25f);
				Color bgColor = ((j % 2 == 0) ? Colors.ListItemEven : Colors.ListItemOdd);
				EditorGUI.DrawRect(bgRect, bgColor);
			}
			for (int k = 0; k < visibleSlots.Count; k++)
			{
				VirtualizedScrollView.VisibleSlot visibleSlot = visibleSlots[k];
				if (Event.current.IsHovering(visibleSlot.Rect))
				{
					EditorGUI.DrawRect(visibleSlot.Rect, Colors.ListItemHover);
				}
				OVDFParser.Diagnostic diagnostic = selectedOVDFFile.Diagnostics[visibleSlot.Index];
				OVDFParser.Severity severity = diagnostic.Severity;
				SdfIconType severityIcon = severity switch
				{
					OVDFParser.Severity.Error => SdfIconType.ExclamationOctagonFill, 
					OVDFParser.Severity.Warning => SdfIconType.ExclamationTriangleFill, 
					_ => SdfIconType.InfoCircleFill, 
				};
				Color severityColor = severity switch
				{
					OVDFParser.Severity.Error => SirenixGUIStyles.RedErrorColor, 
					OVDFParser.Severity.Warning => SirenixGUIStyles.YellowWarningColor, 
					_ => SirenixGUIStyles.LabelCentered.normal.textColor, 
				};
				Rect rowRect = visibleSlot.Rect.HorizontalPadding(6f);
				Rect iconRect = rowRect.TakeFromLeft(rowRect.height);
				SdfIcons.DrawIcon(iconRect.Padding(0f, 6f, 6f, 6f), severityIcon, severityColor);
				GUI.Label(rowRect, diagnostic.Message, IssueStyle);
			}
			issuesScrollView.End();
		}

		private void DrawRightAlignedEllipsisLabel(Rect rect, string label, Color shadowColor, GUIStyle style)
		{
			TextAnchor prevAlignment = style.alignment;
			style.alignment = TextAnchor.MiddleRight;
			float labelWidth = style.CalcWidth(label);
			bool drawEllipsis = labelWidth > rect.width;
			if (drawEllipsis)
			{
				GUI.Label(rect, GUIHelper.TempContent("", label), style);
				Rect ellipsisRect = rect.TakeFromLeft(11f);
				GUI.Label(ellipsisRect, "...", style);
			}
			GUI.Label(rect, label, style);
			if (drawEllipsis)
			{
				GUI.DrawTexture(rect.AlignLeft(20f), DesignerTextures.FadeMaskLeft, ScaleMode.StretchToFill, alphaBlend: true, 0f, shadowColor, 0f, 0f);
			}
			style.alignment = prevAlignment;
		}

		private void SelectFile(string filePath)
		{
			if (filePath != null)
			{
				GUIHelper.RemoveFocusControl();
				selectedFilePath = filePath;
				if (!File.Exists(filePath))
				{
					ShowToast(ToastPosition.BottomLeft, SdfIconType.ExclamationOctagonFill, "The selected file does not exist or was moved.", Color.red, 3f);
					return;
				}
				selectedOVDFFile = OVDFFileWatcher.GetOVDFFile(filePath);
				fileContent = File.ReadAllText(filePath);
				fileContentHighlighted = OVDFHighlighter.Highlight(fileContent);
				fileContentDesigner = OVDFFriendlyFormatter.Format(selectedOVDFFile);
				EditorPrefs.SetString("DesignerFilesOverviewWindow_SelectedFilePath", filePath);
			}
		}

		private void ClearSelection()
		{
			selectedFilePath = null;
			selectedOVDFFile = null;
			fileContent = null;
			fileContentHighlighted = null;
			fileContentDesigner = null;
			EditorPrefs.DeleteKey("DesignerFilesOverviewWindow_SelectedFilePath");
		}

		private void HandleFileContextClick(Rect rect, string filePath)
		{
			if (!Event.current.OnContextClick(rect) || string.IsNullOrEmpty(filePath))
			{
				return;
			}
			GenericMenu genericMenu = new GenericMenu();
			if (OVDFFileWatcher.TryGetFileRecord(filePath, out var record) && !record.IsActive && !string.IsNullOrEmpty(record.OverridingPath))
			{
				genericMenu.AddItem(new GUIContent("Select Overriding File"), on: false, delegate
				{
					SelectFile(record.OverridingPath);
				});
				genericMenu.AddSeparator(string.Empty);
			}
			if (OVDFFileWatcher.TryGetFileRecord(filePath, out record) && record.Type != null)
			{
				Type type = record.Type;
				genericMenu.AddItem(new GUIContent("Customize in Visual Designer"), on: false, delegate
				{
					OdinVisualDesigner.OpenForType(type);
				});
			}
			else
			{
				genericMenu.AddDisabledItem(new GUIContent("Customize in Visual Designer"));
			}
			genericMenu.AddSeparator(string.Empty);
			if (TryGetProjectAssetPath(filePath, out var assetPath))
			{
				genericMenu.AddItem(new GUIContent("Ping"), on: false, delegate
				{
					UnityEngine.Object obj = AssetDatabase.LoadMainAssetAtPath(assetPath);
					EditorGUIUtility.PingObject(obj);
				});
			}
			else
			{
				genericMenu.AddDisabledItem(new GUIContent("Ping"));
			}
			genericMenu.AddItem(new GUIContent("Open In File Explorer"), on: false, delegate
			{
				EditorUtility.RevealInFinder(filePath);
			});
			if (TryGetProjectAssetPath(filePath, out assetPath))
			{
				genericMenu.AddItem(new GUIContent("Open In External Script Editor"), on: false, delegate
				{
					UnityEngine.Object target = AssetDatabase.LoadMainAssetAtPath(assetPath);
					AssetDatabase.OpenAsset(target);
				});
			}
			else
			{
				genericMenu.AddItem(new GUIContent("Open In External Script Editor"), on: false, delegate
				{
					InternalEditorUtility.OpenFileAtLineExternal(filePath, 1);
				});
			}
			genericMenu.AddSeparator(string.Empty);
			if (OVDFFileWatcher.TryGetFileRecord(filePath, out record))
			{
				genericMenu.AddItem(new GUIContent("Delete File"), on: false, delegate
				{
					DeleteFile(filePath, record);
				});
			}
			else
			{
				genericMenu.AddDisabledItem(new GUIContent("Delete File"));
			}
			genericMenu.ShowAsContext();
		}

		private void DeleteFile(string filePath, OVDFFileWatcher.DesignerFileRecord record)
		{
			if (string.IsNullOrEmpty(filePath))
			{
				return;
			}
			if (record == null)
			{
				ShowToast(ToastPosition.BottomLeft, SdfIconType.ExclamationOctagonFill, "The selected file is no longer known to the Visual Designer.", Color.red, 3f);
				RebuildTrees();
				return;
			}
			if (!File.Exists(filePath))
			{
				OVDFFileWatcher.TryDeleteFile(filePath, out var _);
				RebuildTrees();
				if (string.Equals(selectedFilePath, filePath, StringComparison.OrdinalIgnoreCase))
				{
					ClearSelection();
				}
				ShowToast(ToastPosition.BottomLeft, SdfIconType.ExclamationOctagonFill, "The selected file no longer exists.", Color.red, 3f);
				return;
			}
			string typeName = ((record.Type == null) ? "Unknown type" : record.Type.GetNiceName());
			string readOnlyText = (record.IsReadOnly ? "\n\nThis file is read-only, so the delete may fail if the file system or package source refuses the operation." : string.Empty);
			string activeText = (record.IsActive ? "\n\nThis is the active OVDF file for its type. If a lower-precedence file exists, it will become active after deletion." : "\n\nThis file is currently shadowed by a higher-precedence OVDF file.");
			if (!EditorUtility.DisplayDialog("Odin Visual Designer - Delete OVDF File", "Delete this OVDF file?\n\nType: " + typeName + "\nPath: " + filePath + activeText + readOnlyText + "\n\nThis cannot be undone from the Visual Designer.", "Delete", "Cancel"))
			{
				return;
			}
			Type deletedType = record.Type;
			bool wasSelected = string.Equals(selectedFilePath, filePath, StringComparison.OrdinalIgnoreCase);
			if (!OVDFFileWatcher.TryDeleteFile(filePath, out var errorMessage2))
			{
				EditorUtility.DisplayDialog("Odin Visual Designer - Could Not Delete File", "The OVDF file could not be deleted.\n\nPath: " + filePath + "\n\n" + errorMessage2, "OK");
				return;
			}
			RebuildTrees();
			if (wasSelected)
			{
				if (deletedType != null && OVDFFileWatcher.TryGetActiveFileRecord(deletedType, out var activeRecord))
				{
					SelectFile(activeRecord.File.Path);
				}
				else
				{
					ClearSelection();
				}
			}
			ShowToast(ToastPosition.BottomLeft, SdfIconType.TrashFill, "OVDF file deleted.", new Color(0.8f, 0.45f, 0.45f), 3f);
		}

		private static bool TryGetProjectAssetPath(string filePath, out string assetPath)
		{
			assetPath = FileUtil.GetProjectRelativePath(filePath).Replace('\\', '/');
			if (string.IsNullOrEmpty(assetPath) || assetPath.StartsWith("../") || assetPath.StartsWith("..\\"))
			{
				return false;
			}
			return AssetDatabase.LoadMainAssetAtPath(assetPath) != null;
		}

		private void ChangeFontSize(int newFontSize)
		{
			newFontSize = Mathf.Clamp(newFontSize, 10, 50);
			EditorStyle.fontSize = newFontSize;
		}

		private static int DrawEnumToggleButtons<T>(Rect rect, int selectedButtonIndex, out bool clicked) where T : Enum
		{
			float iconSize = rect.height * 0.85f;
			clicked = false;
			Color backgroundColor = (EditorGUIUtility.isProSkin ? new Color(0.157f, 0.157f, 0.157f, 1f) : new Color(0.647f, 0.647f, 0.647f));
			Color selectedBackgroundColor = (EditorGUIUtility.isProSkin ? new Color(0.302f, 0.302f, 0.302f, 1f) : new Color(0.825f, 0.825f, 0.825f));
			Color borderColor = (EditorGUIUtility.isProSkin ? new Color(0.45f, 0.45f, 0.45f, 1f) : new Color(0.875f, 0.875f, 0.875f));
			EnumTypeUtilities<T>.EnumMember[] visibleEnumMembers = EnumTypeUtilities<T>.VisibleEnumMemberInfos;
			string[] enumMemberNames = visibleEnumMembers.Select((EnumTypeUtilities<T>.EnumMember m) => m.NiceName).ToArray();
			float[] enumMemberNameWidths = enumMemberNames.Select((string n) => SirenixGUIStyles.LabelCentered.CalcSize(GUIHelper.TempContent(n)).x).ToArray();
			SdfIconType[] enumMemberIcons = visibleEnumMembers.Select((EnumTypeUtilities<T>.EnumMember m) => (m.Icon != SdfIconType.None) ? m.Icon : SdfIconType.DiamondFill).ToArray();
			GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: false, 1f, backgroundColor, 0f, 5f);
			float subRectWidth = rect.width / (float)enumMemberNames.Length;
			bool onlyDrawIcons = enumMemberNameWidths.Any((float nameWidth) => subRectWidth <= nameWidth + iconSize + 30f);
			for (int i = 0; i < enumMemberNames.Length; i++)
			{
				Rect subRect = rect.Split(i, enumMemberNames.Length);
				SdfIconType enumMemberIcon = enumMemberIcons[i];
				if (selectedButtonIndex == i)
				{
					GUI.DrawTexture(subRect.Padding(4f), Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: false, 1f, selectedBackgroundColor, 0f, 3f);
				}
				if (onlyDrawIcons)
				{
					SdfIcons.DrawIcon(subRect.AlignCenter(iconSize).Padding(5.28f), enumMemberIcon, (selectedButtonIndex != i) ? (EditorGUIUtility.isProSkin ? EditorStyles.label.normal.textColor : new Color(0.2f, 0.2f, 0.2f)) : (EditorGUIUtility.isProSkin ? Color.white : Color.black));
				}
				else
				{
					string enumMemberName = enumMemberNames[i];
					float enumMemberNameWidth = enumMemberNameWidths[i];
					Rect enumMemberNameRect = subRect.AlignCenter(enumMemberNameWidth + iconSize);
					Rect enumMemberIconRect = enumMemberNameRect.TakeFromLeft(iconSize).Padding(0f, 8f, 8f, 8f);
					SdfIcons.DrawIcon(enumMemberIconRect, enumMemberIcon, (selectedButtonIndex != i) ? (EditorGUIUtility.isProSkin ? EditorStyles.label.normal.textColor : new Color(0.2f, 0.2f, 0.2f)) : (EditorGUIUtility.isProSkin ? Color.white : Color.black));
					GUI.Label(enumMemberNameRect, enumMemberName, SirenixGUIStyles.LabelCentered);
				}
				if (Event.current.OnMouseDown(subRect, 0))
				{
					clicked = true;
					selectedButtonIndex = i;
				}
			}
			GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: false, 1f, borderColor, 1.25f, 5f);
			return selectedButtonIndex;
		}

		private void OnFileParsed(Type type, DesignerFile file, bool hasIssues)
		{
			RebuildTrees();
			if (selectedFilePath == file.Path)
			{
				SelectFile(file.Path);
			}
			Repaint();
		}

		private void OnFileDeleted(string path)
		{
			RebuildTrees();
		}

		private void OnFileIssuesChanged(DesignerFile file, bool hasIssues)
		{
			RebuildTrees();
		}

		private void OnRootFolderPathChanged(string newRootFolderPath)
		{
			RebuildTrees();
		}

		private void RebuildTrees()
		{
			fileTree.Clear();
			issueFileTree.Clear();
			foreach (KeyValuePair<string, OVDFFileWatcher.DesignerFileRecord> kvp in OVDFFileWatcher.FileRecords)
			{
				fileTree.AddPath(kvp.Key, isDirectory: false);
			}
			foreach (DesignerFile filesWithIssue in OVDFFileWatcher.FilesWithIssues)
			{
				string path = filesWithIssue.Path;
				issueFileTree.AddPath(path, isDirectory: false);
			}
		}
	}
}
