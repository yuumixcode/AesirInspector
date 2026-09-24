using System;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	public static class SceneValidationWidget
	{
		public enum WidgetAnchor
		{
			BottomLeft,
			TopLeft,
			TopRight,
			BottomRight
		}

		private static Vector2 handleDragStartMousePosition;

		private static Vector2 handleDragStartWidgetOffset;

		private static bool mouseWasInWidgetLastFrame;

		private static GUISpinner spinner;

		private static int warnings;

		private static int errors;

		private static bool isActive;

		private static bool isScanning;

		private static float scanT;

		private static bool showContextMenuNextFrame;

		private static GUIStyle warningText;

		private static GUIStyle errorText;

		private static GUIStyle WarningText
		{
			get
			{
				GUIStyle result = warningText ?? new GUIStyle(SirenixGUIStyles.WhiteLabel)
				{
					alignment = TextAnchor.UpperLeft
				};
				warningText = result;
				return result;
			}
		}

		private static GUIStyle ErrorText
		{
			get
			{
				GUIStyle result = errorText ?? new GUIStyle(SirenixGUIStyles.WhiteLabel)
				{
					alignment = TextAnchor.LowerLeft
				};
				errorText = result;
				return result;
			}
		}

		[InitializeOnLoadMethod]
		private static void Init()
		{
			UnityEditorEventUtility.DuringSceneGUI += OnSceneGUI;
		}

		private static void DrawWidget(ref Rect rect, bool calcSize)
		{
			int padding = 4;
			int height = 25;
			Rect totalRect = rect;
			bool isMouseOver = !calcSize && rect.Contains(Event.current.mousePosition);
			if (isMouseOver && Event.current.rawType == EventType.MouseUp && Event.current.button == 1)
			{
				showContextMenuNextFrame = true;
			}
			if (showContextMenuNextFrame && Event.current.type == EventType.Layout)
			{
				showContextMenuNextFrame = false;
				if (!Application.isPlaying)
				{
					GenericMenu m = new GenericMenu();
					m.AddItem(new GUIContent("Revalidate"), on: false, delegate
					{
						foreach (ValidationSession current in ValidationSession.ActiveValidationSessions)
						{
							if (current.IsValidatingInBackground)
							{
								current.RestartBackgroundValidation();
							}
						}
					});
					if (ValidationSession.ActiveValidationSessions.Any((ValidationSession x) => x.remainingWorkCountSample != 0))
					{
						m.AddItem(new GUIContent("Complete now"), on: false, delegate
						{
							foreach (ValidationSession current in ValidationSession.ActiveValidationSessions)
							{
								current.ValidateQueuedUpWorkNow();
							}
						});
						m.AddItem(new GUIContent("Reset"), on: false, delegate
						{
							ProjectWatcher.RestartWatching(skipLoadEvents: true);
							foreach (ValidationSession current in ValidationSession.ActiveValidationSessions)
							{
								current.Clear(clearResults: true, clearQueue: true);
							}
						});
						m.AddItem(new GUIContent("Stop"), on: false, delegate
						{
							foreach (ValidationSession current in ValidationSession.ActiveValidationSessions)
							{
								current.Clear(clearResults: false, clearQueue: true);
							}
						});
					}
					else
					{
						m.AddDisabledItem(new GUIContent("Complete now"));
						m.AddDisabledItem(new GUIContent("Reset"));
						m.AddDisabledItem(new GUIContent("Stop"));
					}
					if (GlobalConfig<GlobalValidationConfig>.Instance.KeepMainValidationSessionAliveInBackground.Value)
					{
						m.AddItem(new GUIContent("Turn off background validation"), on: false, delegate
						{
							GlobalConfig<GlobalValidationConfig>.Instance.KeepMainValidationSessionAliveInBackground.LocalOverride = true;
							GlobalConfig<GlobalValidationConfig>.Instance.KeepMainValidationSessionAliveInBackground.Value = false;
							foreach (ValidationSession current in ValidationSession.ActiveValidationSessions)
							{
								current.Clear(clearResults: false, clearQueue: true);
							}
						});
					}
					else
					{
						m.AddItem(new GUIContent("Turn on background validation"), on: false, delegate
						{
							GlobalConfig<GlobalValidationConfig>.Instance.KeepMainValidationSessionAliveInBackground.LocalOverride = true;
							GlobalConfig<GlobalValidationConfig>.Instance.KeepMainValidationSessionAliveInBackground.Value = true;
						});
					}
					m.AddItem(new GUIContent("Configure"), on: false, delegate
					{
						ValidationSessionEditor validationSessionEditor = OdinValidatorWindow.OpenWindow(ValidationProfile.MainValidationProfile);
						validationSessionEditor.SelectedMenu = ValidationSessionEditor.MenuOptions.Config;
						validationSessionEditor.MenuVisibility = true;
					});
					if (ValidationSession.ActiveValidationSessions.Count > 1)
					{
						m.AddSeparator("");
						foreach (ValidationSession item in ValidationSession.ActiveValidationSessions)
						{
							m.AddItem(new GUIContent("Show " + item.Name), on: false, delegate
							{
								item.OpenEditor();
							});
						}
					}
					m.ShowAsContext();
				}
			}
			if (calcSize)
			{
				rect.height = height + padding * 2;
				rect.width += padding * 2;
			}
			else
			{
				GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: true, 0f, new Color(0.13f, 0.13f, 0.13f, 1f), 0f, 5f);
				rect.y += padding;
				rect.x += padding;
				rect.width -= padding * 2;
				rect.height -= padding * 2;
			}
			if (calcSize)
			{
				rect.width += height;
			}
			else
			{
				Rect iconRect = rect.TakeFromLeft(rect.height);
				if (errors > 0)
				{
					spinner.DrawSceneWidgetSpinner(iconRect, GUISpinner.IconType.Error, isScanning, isActive, isMouseOver, scanT);
				}
				else if (warnings > 0)
				{
					spinner.DrawSceneWidgetSpinner(iconRect, GUISpinner.IconType.Warning, isScanning, isActive, isMouseOver, scanT);
				}
				else
				{
					spinner.DrawSceneWidgetSpinner(iconRect, GUISpinner.IconType.Valid, isScanning, isActive, isMouseOver, scanT);
				}
			}
			if (errors <= 0 && warnings <= 0)
			{
				return;
			}
			float textWidth = Math.Max(WarningText.CalcSize(GUIHelper.TempContent(errors.ToString())).x + (float)padding, WarningText.CalcSize(GUIHelper.TempContent(warnings.ToString())).x + (float)padding);
			if (calcSize)
			{
				rect.width += textWidth;
				return;
			}
			Rect r = rect.TakeFromLeft(textWidth);
			r.x += padding;
			GUIHelper.PushColor(ValidatorGui.DarkSkinRedErrorColor);
			if (UnityVersion.IsVersionOrGreater(2019, 3))
			{
				GUI.Label(r.SplitVertical(0, 2), errors.ToString(), ErrorText);
			}
			else
			{
				GUI.Label(r.SplitVertical(0, 2).AddYMax(2f), errors.ToString(), ErrorText);
			}
			GUIHelper.PopColor();
			GUIHelper.PushColor(ValidatorGui.DarkSkinYellowWarningColor);
			if (UnityVersion.IsVersionOrGreater(2019, 3))
			{
				GUI.Label(r.SplitVertical(1, 2), warnings.ToString(), WarningText);
			}
			else
			{
				GUI.Label(r.SplitVertical(1, 2).AddYMin(-2f), warnings.ToString(), WarningText);
			}
			GUIHelper.PopColor();
		}

		private static void OnSceneGUI(SceneView sceneView)
		{
			if (!GlobalConfig<GlobalValidationConfig>.Instance.ShowWidget || Application.isPlaying)
			{
				return;
			}
			if (!sceneView.wantsMouseMove)
			{
				sceneView.wantsMouseMove = true;
			}
			GetSessionsState(out warnings, out errors, out isActive, out isScanning, out scanT);
			if (warnings == 0 && errors == 0 && (bool)GlobalConfig<GlobalValidationConfig>.Instance.ShowWidgetOnlyWhenErrorOrWarnings)
			{
				return;
			}
			if (scanT != 1f && scanT != 0f)
			{
				GUIHelper.RequestRepaint();
			}
			Handles.BeginGUI();
			Rect windowArea = sceneView.position;
			windowArea.position = Vector2.zero;
			windowArea.height -= 18f;
			EditorPrefFloat widgetOffsetX = GlobalValidationConfig.WidgetOffsetX;
			EditorPrefFloat widgetOffsetY = GlobalValidationConfig.WidgetOffsetY;
			EditorPrefEnum<WidgetAnchor> anchor = GlobalValidationConfig.WidgetAnchor;
			Rect rect = new Rect(widgetOffsetX, widgetOffsetY, 0f, 0f);
			DrawWidget(ref rect, calcSize: true);
			bool mouseInWidget = rect.Contains(Event.current.mousePosition);
			rect = TransformToAnchor(windowArea, rect, anchor);
			Rect widget = rect;
			DrawWidget(ref rect, calcSize: false);
			int dragHandleId = GUIUtility.GetControlID(FocusType.Passive);
			if (Event.current.OnMouseDown(widget, 0))
			{
				GUIUtility.hotControl = dragHandleId;
				handleDragStartMousePosition = Event.current.mousePosition;
				handleDragStartWidgetOffset.x = widgetOffsetX;
				handleDragStartWidgetOffset.y = widgetOffsetY;
				GUIHelper.RequestRepaint();
			}
			else if (GUIUtility.hotControl == dragHandleId)
			{
				WidgetAnchor closestAnchor = GetClosestAnchor(windowArea, Event.current.mousePosition);
				Rect anchorRect = TransformToAnchor(windowArea, new Rect(0f, 0f, 15f, 15f), closestAnchor);
				if (handleDragStartMousePosition != Event.current.mousePosition)
				{
					SirenixEditorGUI.DrawSolidRect(anchorRect, new Color(1f, 1f, 1f, 0.3f));
				}
				bool onMouseUp = Event.current.OnMouseUp(0);
				if (onMouseUp || Event.current.OnMouseDown(0, useEvent: false) || EditorWindow.focusedWindow != sceneView)
				{
					GUIUtility.hotControl = 0;
					if (onMouseUp && handleDragStartMousePosition == Event.current.mousePosition)
					{
						OdinValidatorWindow.OpenWindow(ValidationProfile.MainValidationProfile);
					}
					else if ((WidgetAnchor)anchor != closestAnchor)
					{
						Vector2 newOffset = GetOffsetForAnchorOfWidgetRectInArea(windowArea, widget, closestAnchor);
						widgetOffsetX.Value = newOffset.x;
						widgetOffsetY.Value = newOffset.y;
						anchor.Value = closestAnchor;
					}
				}
				else if (Event.current.type == EventType.MouseDrag)
				{
					Vector2 delta = Event.current.mousePosition - handleDragStartMousePosition;
					delta = TransformToAnchor(delta, anchor);
					Vector2 offset = handleDragStartWidgetOffset + delta;
					offset.x = Mathf.Clamp(offset.x, 0f, windowArea.width - widget.width);
					offset.y = Mathf.Clamp(offset.y, 0f, windowArea.height - widget.height);
					widgetOffsetX.Value = offset.x;
					widgetOffsetY.Value = offset.y;
					Event.current.Use();
					GUIHelper.RequestRepaint();
				}
			}
			if (Event.current.isMouse)
			{
				if (mouseInWidget || mouseWasInWidgetLastFrame)
				{
					sceneView.Repaint();
				}
				mouseWasInWidgetLastFrame = mouseInWidget;
			}
			sceneView.RepaintIfRequested();
			Handles.EndGUI();
		}

		private static void GetSessionsState(out int warnings, out int errors, out bool isActive, out bool isScanning, out float t)
		{
			warnings = 0;
			errors = 0;
			isActive = false;
			isScanning = false;
			t = 1f;
			foreach (ValidationSession session in ValidationSession.ActiveValidationSessions)
			{
				if (session.remainingWorkCountSample != 0 && session.IsValidatingInBackground)
				{
					isActive = true;
					t = Mathf.Min(session.CalculateCurrentValidationProgress(), t);
				}
				if (session.ShouldDisplayProgressBar)
				{
					isScanning = true;
				}
				if (session.Results != null)
				{
					warnings += session.CurrentWarningCount;
					errors += session.CurrentErrorCount;
				}
			}
			if (GlobalConfig<GlobalValidationConfig>.Instance.KeepMainValidationSessionAliveInBackground.Value)
			{
				isActive = true;
			}
			isActive = isActive || errors > 0 || warnings > 0;
		}

		public static WidgetAnchor GetClosestAnchor(Rect area, Vector2 position)
		{
			position -= area.position;
			area.position = Vector2.zero;
			position.x /= area.width;
			position.y /= area.height;
			if (position.x <= 0.5f)
			{
				if (position.y <= 0.5f)
				{
					return WidgetAnchor.TopLeft;
				}
				return WidgetAnchor.BottomLeft;
			}
			if (position.y <= 0.5f)
			{
				return WidgetAnchor.TopRight;
			}
			return WidgetAnchor.BottomRight;
		}

		public static Rect TransformFromToAnchor(Rect area, Rect rect, WidgetAnchor from, WidgetAnchor to)
		{
			switch (from)
			{
			case WidgetAnchor.BottomLeft:
				rect.y = area.height - (rect.y + rect.height);
				break;
			case WidgetAnchor.TopRight:
				rect.x = area.width - (rect.x + rect.width);
				break;
			case WidgetAnchor.BottomRight:
				rect.x = area.width - (rect.x + rect.width);
				rect.y = area.height - (rect.y + rect.height);
				break;
			default:
				throw new NotImplementedException();
			case WidgetAnchor.TopLeft:
				break;
			}
			return TransformToAnchor(area, rect, to);
		}

		public static Vector2 TransformToAnchor(Vector2 vector, WidgetAnchor anchor)
		{
			return anchor switch
			{
				WidgetAnchor.TopLeft => vector, 
				WidgetAnchor.TopRight => new Vector2(0f - vector.x, vector.y), 
				WidgetAnchor.BottomRight => new Vector2(0f - vector.x, 0f - vector.y), 
				WidgetAnchor.BottomLeft => new Vector2(vector.x, 0f - vector.y), 
				_ => throw new NotImplementedException(), 
			};
		}

		public static Rect TransformToAnchor(Rect area, Rect rect, WidgetAnchor anchor)
		{
			switch (anchor)
			{
			case WidgetAnchor.BottomLeft:
				rect.y = area.height - (rect.y + rect.height);
				rect.position += area.position;
				return rect;
			case WidgetAnchor.BottomRight:
				rect.x = area.width - (rect.x + rect.width);
				rect.y = area.height - (rect.y + rect.height);
				rect.position += area.position;
				return rect;
			case WidgetAnchor.TopRight:
				rect.x = area.width - (rect.x + rect.width);
				rect.position += area.position;
				return rect;
			case WidgetAnchor.TopLeft:
				rect.position += area.position;
				return rect;
			default:
				throw new NotImplementedException();
			}
		}

		public static Vector2 GetOffsetForAnchorOfWidgetRectInArea(Rect area, Rect widget, WidgetAnchor anchor)
		{
			return anchor switch
			{
				WidgetAnchor.BottomLeft => new Vector2(widget.x, area.height - widget.yMax), 
				WidgetAnchor.TopLeft => widget.position, 
				WidgetAnchor.TopRight => new Vector2(area.width - widget.xMax, widget.y), 
				WidgetAnchor.BottomRight => new Vector2(area.width - widget.xMax, area.height - widget.yMax), 
				_ => throw new NotImplementedException(), 
			};
		}

		public static bool IsAnchorLeft(WidgetAnchor anchor)
		{
			if (anchor != WidgetAnchor.BottomLeft)
			{
				return anchor == WidgetAnchor.TopLeft;
			}
			return true;
		}

		public static WidgetAnchor InvertAnchor(WidgetAnchor anchor)
		{
			return (WidgetAnchor)((int)(anchor + 2) % 4);
		}
	}
}
