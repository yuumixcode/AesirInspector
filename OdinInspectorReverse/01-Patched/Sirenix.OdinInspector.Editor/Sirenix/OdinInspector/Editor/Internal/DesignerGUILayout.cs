using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class DesignerGUILayout
	{
		public class RectLayout
		{
			private class LayoutEntry
			{
				public LayoutType Type;

				public float Width;

				public float Padding;

				public int Priority;

				public bool CanShrink;

				public LayoutRect Output;

				public float TotalWidth => Width + 2f * Padding;
			}

			private enum LayoutType
			{
				Fixed,
				FixedGap,
				FlexGap
			}

			private Rect masterRect;

			private List<LayoutEntry> entries = new List<LayoutEntry>();

			public RectLayout(Rect masterRect)
			{
				this.masterRect = masterRect;
			}

			public LayoutRect Rect(float width, int priority = 0, float padding = 0f, bool canShrink = false)
			{
				LayoutEntry entry = new LayoutEntry
				{
					Type = LayoutType.Fixed,
					Width = width,
					Priority = priority,
					Padding = padding,
					CanShrink = canShrink,
					Output = new LayoutRect()
				};
				entries.Add(entry);
				return entry.Output;
			}

			public void Gap(float width)
			{
				entries.Add(new LayoutEntry
				{
					Type = LayoutType.FixedGap,
					Width = width
				});
			}

			public void FlexGap()
			{
				entries.Add(new LayoutEntry
				{
					Type = LayoutType.FlexGap
				});
			}

			public void CalculateLayout()
			{
				float totalFlexWeight = 0f;
				List<LayoutEntry> active = (from layoutEntry in entries
					where layoutEntry.Type == LayoutType.Fixed
					orderby layoutEntry.Priority descending
					select layoutEntry).ToList();
				List<LayoutEntry> fixedGaps = entries.Where((LayoutEntry layoutEntry) => layoutEntry.Type == LayoutType.FixedGap).ToList();
				List<LayoutEntry> flexGaps = entries.Where((LayoutEntry layoutEntry) => layoutEntry.Type == LayoutType.FlexGap).ToList();
				float totalFixedWidth = active.Sum((LayoutEntry layoutEntry) => layoutEntry.TotalWidth) + fixedGaps.Sum((LayoutEntry g) => g.Width);
				float available = masterRect.width - totalFixedWidth;
				if (available < 0f)
				{
					foreach (LayoutEntry e in from layoutEntry in active
						where layoutEntry.CanShrink
						orderby layoutEntry.Priority
						select layoutEntry)
					{
						float shrinkAmount = Mathf.Min(0f - available, e.Width - 1f);
						e.Width -= shrinkAmount;
						available += shrinkAmount;
						if (available >= 0f)
						{
							break;
						}
					}
					if (available < 0f)
					{
						for (int i = active.Count - 1; i >= 0; i--)
						{
							LayoutEntry e2 = active[i];
							available += e2.TotalWidth;
							active.RemoveAt(i);
							if (available >= 0f)
							{
								break;
							}
						}
					}
				}
				int flexCount = flexGaps.Count;
				float flexSize = ((flexCount > 0) ? (available / (float)flexCount) : 0f);
				float x = masterRect.x;
				foreach (LayoutEntry entry in entries)
				{
					switch (entry.Type)
					{
					case LayoutType.Fixed:
						if (active.Contains(entry))
						{
							float paddedX = x;
							float paddedWidth = entry.TotalWidth;
							float rawX = paddedX + entry.Padding;
							float rawWidth = entry.Width;
							entry.Output.PaddedRect = new Rect(paddedX, masterRect.y, paddedWidth, masterRect.height);
							entry.Output.Rect = new Rect(rawX, masterRect.y, rawWidth, masterRect.height);
							x += paddedWidth;
						}
						break;
					case LayoutType.FixedGap:
						x += entry.Width;
						break;
					case LayoutType.FlexGap:
						x += flexSize;
						break;
					}
				}
			}
		}

		public class LayoutRect
		{
			public Rect Rect;

			public Rect PaddedRect;

			public static implicit operator Rect(LayoutRect layoutRect)
			{
				return layoutRect.Rect;
			}
		}
	}
}
