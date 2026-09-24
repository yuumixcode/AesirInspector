using System;
using System.Collections.Generic;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	public sealed class AnimatedFileTreeView
	{
		private struct Node
		{
			public string Name;

			public string NameLower;

			public string FullPath;

			public int ParentId;

			public int FirstChildId;

			public int NextSiblingId;

			public bool IsDirectory;

			public byte SearchFlags;

			public bool TargetOpen;

			public float Open01;
		}

		public struct Row
		{
			public int NodeId;

			public int Depth;

			public string DisplayName;

			public bool IsDirectory;

			public float HeightFactor;
		}

		public const int InvalidId = -1;

		private const byte SearchSelfMask = 1;

		private const byte SearchDescendantMask = 2;

		private static readonly char[] PathSeparators = new char[2] { '/', '\\' };

		private readonly List<Node> nodes = new List<Node>(1024);

		private readonly Dictionary<string, int> pathToId = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

		private readonly List<Row> rows = new List<Row>(1024);

		private readonly List<int> animatingNodes = new List<int>(16);

		private readonly HashSet<int> animatingSet = new HashSet<int>();

		private readonly List<float> rowOffsets = new List<float>(1024);

		private List<int> tempStack;

		private bool structureDirty = true;

		private bool rowsDirty = true;

		private bool searchDirty = true;

		private string searchText = string.Empty;

		private Rect viewportRect;

		private float itemHeight;

		private float scrollPos;

		private float targetScrollPos;

		private float scrollSpeed = 10f;

		private float openAnimSpeed = 4f;

		private float scrollStep = 200f;

		private float totalContentHeight;

		private int firstVisible;

		private int lastVisibleExclusive;

		public int RootId => 0;

		public float ItemHeight
		{
			get
			{
				return itemHeight;
			}
			set
			{
				float v = value;
				if (v <= 0f)
				{
					v = 1f;
				}
				if (Math.Abs(itemHeight - v) > Mathf.Epsilon)
				{
					itemHeight = v;
					rowsDirty = true;
					ClampScroll();
				}
			}
		}

		public string SearchText
		{
			get
			{
				return searchText;
			}
			set
			{
				string newText = ((value != null) ? value.Trim() : string.Empty);
				if (!(newText == searchText))
				{
					searchText = newText;
					structureDirty = true;
					rowsDirty = true;
					searchDirty = true;
				}
			}
		}

		public int VisibleRowCount => rows.Count;

		public int FirstVisibleRowIndex => firstVisible;

		public int LastVisibleRowIndexExclusive => lastVisibleExclusive;

		public AnimatedFileTreeView(float itemHeight)
		{
			this.itemHeight = ((itemHeight > 0f) ? itemHeight : 1f);
			Node root = new Node
			{
				Name = string.Empty,
				NameLower = string.Empty,
				FullPath = string.Empty,
				ParentId = -1,
				FirstChildId = -1,
				NextSiblingId = -1,
				IsDirectory = true,
				SearchFlags = 0,
				TargetOpen = true,
				Open01 = 1f
			};
			nodes.Add(root);
		}

		public void Clear()
		{
			nodes.Clear();
			pathToId.Clear();
			rows.Clear();
			rowOffsets.Clear();
			animatingNodes.Clear();
			animatingSet.Clear();
			if (tempStack != null)
			{
				tempStack.Clear();
			}
			searchText = string.Empty;
			structureDirty = true;
			rowsDirty = true;
			searchDirty = true;
			scrollPos = 0f;
			targetScrollPos = 0f;
			totalContentHeight = 0f;
			Node root = new Node
			{
				Name = string.Empty,
				NameLower = string.Empty,
				FullPath = string.Empty,
				ParentId = -1,
				FirstChildId = -1,
				NextSiblingId = -1,
				IsDirectory = true,
				SearchFlags = 0,
				TargetOpen = true,
				Open01 = 1f
			};
			nodes.Add(root);
		}

		public int AddPath(string fullPath, bool isDirectory)
		{
			if (string.IsNullOrEmpty(fullPath))
			{
				throw new ArgumentException("fullPath must not be null or empty", "fullPath");
			}
			fullPath = PathUtils.GetCrossPlatformPath(fullPath);
			string[] segments = fullPath.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);
			if (segments.Length == 0)
			{
				throw new ArgumentException("Path does not contain any segments", "fullPath");
			}
			int parentId = RootId;
			string currentPath = null;
			for (int i = 0; i < segments.Length; i++)
			{
				string segment = segments[i];
				bool segmentIsDir = i != segments.Length - 1 || isDirectory;
				currentPath = ((currentPath != null) ? (currentPath + "/" + segment) : segment);
				parentId = EnsureNode(currentPath, parentId, segment, segmentIsDir);
			}
			structureDirty = true;
			rowsDirty = true;
			searchDirty = true;
			return parentId;
		}

		public bool RemovePath(string fullPath)
		{
			if (string.IsNullOrEmpty(fullPath))
			{
				return false;
			}
			fullPath = PathUtils.GetCrossPlatformPath(fullPath);
			if (!pathToId.TryGetValue(fullPath, out var nodeId))
			{
				return false;
			}
			if (nodeId == RootId)
			{
				return false;
			}
			Node node = nodes[nodeId];
			int parentId = node.ParentId;
			if (parentId != -1)
			{
				Node parent = nodes[parentId];
				int childId = parent.FirstChildId;
				if (childId == nodeId)
				{
					parent.FirstChildId = node.NextSiblingId;
					nodes[parentId] = parent;
				}
				else
				{
					int prevId = childId;
					Node prev = nodes[prevId];
					while (prev.NextSiblingId != -1)
					{
						if (prev.NextSiblingId == nodeId)
						{
							prev.NextSiblingId = node.NextSiblingId;
							nodes[prevId] = prev;
							break;
						}
						prevId = prev.NextSiblingId;
						prev = nodes[prevId];
					}
				}
			}
			if (tempStack == null)
			{
				tempStack = new List<int>(64);
			}
			tempStack.Clear();
			tempStack.Add(nodeId);
			while (tempStack.Count > 0)
			{
				int currentId = tempStack[tempStack.Count - 1];
				tempStack.RemoveAt(tempStack.Count - 1);
				Node current = nodes[currentId];
				if (!string.IsNullOrEmpty(current.FullPath))
				{
					pathToId.Remove(current.FullPath);
				}
				if (animatingSet.Contains(currentId))
				{
					animatingSet.Remove(currentId);
					for (int i = animatingNodes.Count - 1; i >= 0; i--)
					{
						if (animatingNodes[i] == currentId)
						{
							animatingNodes.RemoveAt(i);
							break;
						}
					}
				}
				for (int childId2 = current.FirstChildId; childId2 != -1; childId2 = nodes[childId2].NextSiblingId)
				{
					tempStack.Add(childId2);
				}
				current.ParentId = -1;
				current.FirstChildId = -1;
				current.NextSiblingId = -1;
				current.FullPath = null;
				current.Name = null;
				current.NameLower = null;
				current.SearchFlags = 0;
				current.TargetOpen = false;
				current.Open01 = 0f;
				nodes[currentId] = current;
			}
			structureDirty = true;
			rowsDirty = true;
			searchDirty = true;
			return true;
		}

		public bool TryGetNodeId(string fullPath, out int nodeId)
		{
			if (string.IsNullOrEmpty(fullPath))
			{
				nodeId = -1;
				return false;
			}
			fullPath = PathUtils.GetCrossPlatformPath(fullPath);
			return pathToId.TryGetValue(fullPath, out nodeId);
		}

		public string GetName(int nodeId)
		{
			if (nodeId < 0 || nodeId >= nodes.Count)
			{
				return string.Empty;
			}
			return nodes[nodeId].Name ?? string.Empty;
		}

		public string GetFullPath(int nodeId)
		{
			if (nodeId < 0 || nodeId >= nodes.Count)
			{
				return string.Empty;
			}
			return nodes[nodeId].FullPath ?? string.Empty;
		}

		public bool IsDirectory(int nodeId)
		{
			if (nodeId < 0 || nodeId >= nodes.Count)
			{
				return false;
			}
			return nodes[nodeId].IsDirectory;
		}

		public bool IsExpanded(int nodeId)
		{
			if (nodeId < 0 || nodeId >= nodes.Count)
			{
				return false;
			}
			return nodes[nodeId].TargetOpen;
		}

		public void ToggleFolder(int nodeId)
		{
			if (nodeId > 0 && nodeId < nodes.Count)
			{
				Node node = nodes[nodeId];
				if (node.IsDirectory)
				{
					node.TargetOpen = !node.TargetOpen;
					nodes[nodeId] = node;
					animatingSet.Add(nodeId);
					animatingNodes.Add(nodeId);
					rowsDirty = true;
				}
			}
		}

		public void SetFolderExpanded(int nodeId, bool expanded, bool instant)
		{
			if (nodeId <= 0 || nodeId >= nodes.Count)
			{
				return;
			}
			Node node = nodes[nodeId];
			if (!node.IsDirectory)
			{
				return;
			}
			node.TargetOpen = expanded;
			if (instant)
			{
				node.Open01 = (expanded ? 1f : 0f);
				nodes[nodeId] = node;
				if (animatingSet.Contains(nodeId))
				{
					animatingSet.Remove(nodeId);
					for (int i = animatingNodes.Count - 1; i >= 0; i--)
					{
						if (animatingNodes[i] == nodeId)
						{
							animatingNodes.RemoveAt(i);
							break;
						}
					}
				}
				rowsDirty = true;
			}
			else
			{
				nodes[nodeId] = node;
				if (!animatingSet.Contains(nodeId))
				{
					animatingSet.Add(nodeId);
					animatingNodes.Add(nodeId);
				}
				rowsDirty = true;
			}
		}

		public void Layout(Rect viewportRect, Event e)
		{
			this.viewportRect = viewportRect;
			float dt = GUITimeHelper.LayoutDeltaTime;
			if (AdvanceAnimation(dt))
			{
				rowsDirty = true;
			}
			if (structureDirty || rowsDirty)
			{
				RebuildRowsAndOffsets();
				structureDirty = false;
				rowsDirty = false;
				ClampScroll();
			}
			HandleScrollInput(e);
			SmoothScroll(dt);
			UpdateVisibleRange();
		}

		public Row GetRow(int index)
		{
			return rows[index];
		}

		public Rect GetRowRect(int index)
		{
			float y = rowOffsets[index] - scrollPos;
			float yNext = rowOffsets[index + 1] - scrollPos;
			float height = yNext - y;
			if (height < 0f)
			{
				height = 0f;
			}
			return new Rect(viewportRect.x, viewportRect.y + y, viewportRect.width, height);
		}

		private int EnsureNode(string fullPath, int parentId, string name, bool isDirectory)
		{
			if (pathToId.TryGetValue(fullPath, out var existingId))
			{
				Node existing = nodes[existingId];
				if (isDirectory && !existing.IsDirectory)
				{
					existing.IsDirectory = true;
					nodes[existingId] = existing;
				}
				return existingId;
			}
			Node node = new Node
			{
				Name = name,
				NameLower = name.ToLowerInvariant(),
				FullPath = fullPath,
				ParentId = parentId,
				FirstChildId = -1,
				NextSiblingId = -1,
				IsDirectory = isDirectory,
				SearchFlags = 0,
				TargetOpen = false,
				Open01 = 0f
			};
			int newId = nodes.Count;
			nodes.Add(node);
			pathToId[fullPath] = newId;
			Node parent = nodes[parentId];
			node.NextSiblingId = parent.FirstChildId;
			parent.FirstChildId = newId;
			nodes[parentId] = parent;
			nodes[newId] = node;
			return newId;
		}

		private void UpdateSearchFlags()
		{
			int count = nodes.Count;
			for (int i = 0; i < count; i++)
			{
				Node n = nodes[i];
				n.SearchFlags = 0;
				nodes[i] = n;
			}
			if (!string.IsNullOrEmpty(searchText))
			{
				string searchLower = searchText.ToLowerInvariant();
				SetSearchFlagsRecursive(RootId, searchLower);
			}
		}

		private bool SetSearchFlagsRecursive(int nodeId, string searchLower)
		{
			Node node = nodes[nodeId];
			bool matchesSelf = false;
			if (!string.IsNullOrEmpty(node.NameLower) && node.NameLower.IndexOf(searchLower, StringComparison.Ordinal) >= 0)
			{
				matchesSelf = true;
			}
			bool anyChildMatch = false;
			for (int childId = node.FirstChildId; childId != -1; childId = nodes[childId].NextSiblingId)
			{
				if (SetSearchFlagsRecursive(childId, searchLower))
				{
					anyChildMatch = true;
				}
			}
			byte flags = 0;
			if (matchesSelf)
			{
				flags |= 1;
			}
			if (anyChildMatch)
			{
				flags |= 2;
			}
			node.SearchFlags = flags;
			nodes[nodeId] = node;
			return matchesSelf || anyChildMatch;
		}

		private bool NodeMatchesSelfOrDescendant(int nodeId)
		{
			if (nodeId < 0 || nodeId >= nodes.Count)
			{
				return false;
			}
			byte flags = nodes[nodeId].SearchFlags;
			return (flags & 3) != 0;
		}

		private void FlattenChain(ref int displayNodeId, ref string displayName)
		{
			int currentId = displayNodeId;
			while (true)
			{
				int childId = nodes[currentId].FirstChildId;
				if (childId != -1)
				{
					Node child = nodes[childId];
					if (child.IsDirectory)
					{
						int siblingId = child.NextSiblingId;
						if (siblingId == -1)
						{
							string childName = child.Name ?? string.Empty;
							displayName = displayName + "/" + childName;
							displayNodeId = childId;
							currentId = childId;
							continue;
						}
						break;
					}
					break;
				}
				break;
			}
		}

		private void RebuildRowsAndOffsets()
		{
			if (searchText.Length > 0 && searchDirty)
			{
				UpdateSearchFlags();
				searchDirty = false;
			}
			rows.Clear();
			Node root = nodes[RootId];
			if (searchText.Length > 0)
			{
				for (int childId = root.FirstChildId; childId != -1; childId = nodes[childId].NextSiblingId)
				{
					BuildRowsWithSearch(childId, 0);
				}
			}
			else
			{
				for (int childId2 = root.FirstChildId; childId2 != -1; childId2 = nodes[childId2].NextSiblingId)
				{
					BuildRowsNoSearch(childId2, 0, 1f);
				}
			}
			int count = rows.Count;
			if (rowOffsets.Capacity < count + 1)
			{
				rowOffsets.Capacity = count + 1;
			}
			while (rowOffsets.Count < count + 1)
			{
				rowOffsets.Add(0f);
			}
			float y = 0f;
			for (int i = 0; i < count; i++)
			{
				rowOffsets[i] = y;
				float factor = Mathf.Clamp01(rows[i].HeightFactor);
				float h = itemHeight * factor;
				y += h;
			}
			rowOffsets[count] = y;
			totalContentHeight = y;
		}

		private void BuildRowsNoSearch(int nodeId, int depth, float parentFactor)
		{
			Node node = nodes[nodeId];
			if (node.IsDirectory)
			{
				int displayNodeId = nodeId;
				string displayName = node.Name ?? string.Empty;
				FlattenChain(ref displayNodeId, ref displayName);
				Node displayNode = nodes[displayNodeId];
				float clampedParent = Mathf.Clamp01(parentFactor);
				Row row = new Row
				{
					NodeId = displayNodeId,
					Depth = depth,
					DisplayName = displayName,
					IsDirectory = true,
					HeightFactor = clampedParent
				};
				rows.Add(row);
				float openValue = Mathf.Clamp01(displayNode.Open01);
				if (!(openValue > 0f) && !displayNode.TargetOpen && !displayNode.TargetOpen)
				{
					return;
				}
				float childFactor = clampedParent * openValue;
				if (!(childFactor <= 0f) || displayNode.TargetOpen)
				{
					for (int childId = displayNode.FirstChildId; childId != -1; childId = nodes[childId].NextSiblingId)
					{
						BuildRowsNoSearch(childId, depth + 1, childFactor);
					}
				}
			}
			else
			{
				float factor = Mathf.Clamp01(parentFactor);
				if (!(factor <= 0f))
				{
					Row row2 = new Row
					{
						NodeId = nodeId,
						Depth = depth,
						DisplayName = (node.Name ?? string.Empty),
						IsDirectory = false,
						HeightFactor = factor
					};
					rows.Add(row2);
				}
			}
		}

		private void BuildRowsWithSearch(int nodeId, int depth)
		{
			if (!NodeMatchesSelfOrDescendant(nodeId))
			{
				return;
			}
			Node node = nodes[nodeId];
			if (node.IsDirectory)
			{
				int displayNodeId = nodeId;
				string displayName = node.Name ?? string.Empty;
				FlattenChain(ref displayNodeId, ref displayName);
				Row row = new Row
				{
					NodeId = displayNodeId,
					Depth = depth,
					DisplayName = displayName,
					IsDirectory = true,
					HeightFactor = 1f
				};
				rows.Add(row);
				for (int childId = nodes[displayNodeId].FirstChildId; childId != -1; childId = nodes[childId].NextSiblingId)
				{
					BuildRowsWithSearch(childId, depth + 1);
				}
			}
			else
			{
				Row row2 = new Row
				{
					NodeId = nodeId,
					Depth = depth,
					DisplayName = (node.Name ?? string.Empty),
					IsDirectory = false,
					HeightFactor = 1f
				};
				rows.Add(row2);
			}
		}

		public float GetOpenFactor(int nodeId)
		{
			if (nodeId < 0 || nodeId >= nodes.Count)
			{
				return 0f;
			}
			return Mathf.Clamp01(nodes[nodeId].Open01);
		}

		private bool AdvanceAnimation(float dt)
		{
			if (animatingNodes.Count == 0)
			{
				return false;
			}
			bool changed = false;
			float speed = openAnimSpeed;
			for (int i = animatingNodes.Count - 1; i >= 0; i--)
			{
				int id = animatingNodes[i];
				Node node = nodes[id];
				float target = (node.TargetOpen ? 1f : 0f);
				float newV = Mathf.MoveTowards(node.Open01, target, speed * dt);
				if (Math.Abs(newV - node.Open01) > 0.0001f)
				{
					changed = true;
					node.Open01 = newV;
					nodes[id] = node;
				}
				if (Math.Abs(newV - target) < 0.0001f)
				{
					animatingNodes.RemoveAt(i);
					animatingSet.Remove(id);
				}
			}
			return changed;
		}

		private void HandleScrollInput(Event e)
		{
			if (e != null && e.type == EventType.ScrollWheel && viewportRect.Contains(e.mousePosition))
			{
				float direction = Mathf.Sign(e.delta.y);
				targetScrollPos += direction * scrollStep;
				ClampScroll();
				e.Use();
			}
		}

		private void SmoothScroll(float dt)
		{
			float maxScroll = GetMaxScroll();
			targetScrollPos = Mathf.Clamp(targetScrollPos, 0f, maxScroll);
			float t = scrollSpeed * dt;
			if (t <= 0f)
			{
				scrollPos = targetScrollPos;
				return;
			}
			scrollPos = Mathf.Lerp(scrollPos, targetScrollPos, Mathf.Clamp01(t));
			if (float.IsNaN(scrollPos))
			{
				scrollPos = 0f;
			}
			if (scrollPos < 0f)
			{
				scrollPos = 0f;
			}
			else if (scrollPos > maxScroll)
			{
				scrollPos = maxScroll;
			}
		}

		private void UpdateVisibleRange()
		{
			int count = rows.Count;
			if (count == 0 || totalContentHeight <= 0f)
			{
				firstVisible = 0;
				lastVisibleExclusive = 0;
				return;
			}
			float viewTop = scrollPos;
			float viewBottom = scrollPos + viewportRect.height;
			int first = FindFirstRowAtOrBelow(viewTop);
			int last = FindLastRowAbove(viewBottom);
			if (first < 0)
			{
				first = 0;
			}
			if (first > count)
			{
				first = count;
			}
			if (last < first)
			{
				last = first;
			}
			if (last > count)
			{
				last = count;
			}
			firstVisible = first;
			lastVisibleExclusive = last;
		}

		private int FindFirstRowAtOrBelow(float y)
		{
			int count = rows.Count;
			int lo = 0;
			int hi = count;
			while (lo < hi)
			{
				int mid = lo + hi >> 1;
				float nextY = rowOffsets[mid + 1];
				if (nextY <= y)
				{
					lo = mid + 1;
				}
				else
				{
					hi = mid;
				}
			}
			return lo;
		}

		private int FindLastRowAbove(float y)
		{
			int count = rows.Count;
			int lo = 0;
			int hi = count;
			while (lo < hi)
			{
				int mid = lo + hi >> 1;
				float startY = rowOffsets[mid];
				if (startY < y)
				{
					lo = mid + 1;
				}
				else
				{
					hi = mid;
				}
			}
			return lo;
		}

		private float GetMaxScroll()
		{
			float max = totalContentHeight - viewportRect.height;
			if (!(max > 0f))
			{
				return 0f;
			}
			return max;
		}

		private void ClampScroll()
		{
			float max = GetMaxScroll();
			if (scrollPos < 0f)
			{
				scrollPos = 0f;
			}
			else if (scrollPos > max)
			{
				scrollPos = max;
			}
			if (targetScrollPos < 0f)
			{
				targetScrollPos = 0f;
			}
			else if (targetScrollPos > max)
			{
				targetScrollPos = max;
			}
		}
	}
}
