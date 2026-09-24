using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	internal class SerializationInfoMenuItem : OdinMenuItem
	{
		private MemberSerializationInfo info;

		private string typeName;

		public const int IconSize = 20;

		public const int IconSpacing = 4;

		public SerializationInfoMenuItem(OdinMenuTree tree, string name, MemberSerializationInfo instance)
			: base(tree, name, instance)
		{
			info = instance;
			typeName = instance.MemberInfo.GetReturnType().GetNiceName();
		}

		protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
		{
			if (Event.current.type == EventType.Repaint)
			{
				labelRect.width -= 10f;
				float widthOfMemberName = SirenixGUIStyles.Label.CalcSize(GUIHelper.TempContent(base.Name)).x;
				float widthOfTypeName = SirenixGUIStyles.RightAlignedGreyMiniLabel.CalcSize(GUIHelper.TempContent(typeName)).x;
				GUI.Label(labelRect.SetX(Mathf.Max(labelRect.xMin + widthOfMemberName, labelRect.xMax - widthOfTypeName)).SetXMax(labelRect.xMax), typeName, base.IsSelected ? SirenixGUIStyles.LeftAlignedWhiteMiniLabel : SirenixGUIStyles.LeftAlignedGreyMiniLabel);
				rect.x += 4f;
				rect.x += 4f;
				rect = rect.AlignLeft(20f);
				rect = rect.AlignMiddle(20f);
				DrawTheIcon(rect, info.Info.HasAll(SerializationFlags.SerializedByOdin), info.OdinMessageType);
				rect.x += 28f;
				DrawTheIcon(rect, info.Info.HasAll(SerializationFlags.SerializedByUnity), info.UnityMessageType);
			}
		}

		private void DrawTheIcon(Rect rect, bool serialized, InfoMessageType messageType)
		{
			switch (messageType)
			{
			case InfoMessageType.Error:
				GUI.DrawTexture(rect.AlignCenterXY(22f), EditorIcons.ConsoleErroricon, ScaleMode.ScaleToFit);
				return;
			case InfoMessageType.Warning:
				GUI.DrawTexture(rect.AlignCenterXY(20f), EditorIcons.ConsoleWarnicon, ScaleMode.ScaleToFit);
				return;
			case InfoMessageType.Info:
				GUI.DrawTexture(rect.AlignCenterXY(20f), EditorIcons.ConsoleInfoIcon, ScaleMode.ScaleToFit);
				return;
			}
			if (serialized)
			{
				GUI.DrawTexture(rect.AlignCenterXY(EditorIcons.TestPassed.width), EditorIcons.TestPassed, ScaleMode.ScaleToFit);
				return;
			}
			GUI.color = (EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.2f) : new Color(0.15f, 0.15f, 0.15f, 0.2f));
			EditorIcons.X.Draw(rect.Padding(2f));
			GUI.color = Color.white;
		}
	}
}
