using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sirenix.Reflection.Editor
{
	public struct Panel_Internal : IEquatable<Panel_Internal>
	{
		internal UnityEngine.UIElements.Panel panel;

		public IPanel IPanel => panel;

		public bool duringLayoutPhase
		{
			get
			{
				return ((UnityEngine.UIElements.BaseVisualElementPanel)panel).duringLayoutPhase;
			}
			set
			{
				((UnityEngine.UIElements.BaseVisualElementPanel)panel).duringLayoutPhase = value;
			}
		}

		public Panel_Internal(IPanel panel)
		{
			this.panel = panel as UnityEngine.UIElements.Panel;
		}

		public void ApplyStyles()
		{
			panel.ApplyStyles();
		}

		public void VisualTreeSetSize(Vector2 size)
		{
			panel.visualTree.SetSize(size);
		}

		public void UpdateVisualTreePhaseViewData()
		{
			panel.m_VisualTreeUpdater.UpdateVisualTreePhase(UnityEngine.UIElements.VisualTreeUpdatePhase.ViewData);
		}

		public void UpdateVisualTreePhaseAnimation()
		{
			panel.m_VisualTreeUpdater.UpdateVisualTreePhase(UnityEngine.UIElements.VisualTreeUpdatePhase.Animation);
		}

		public void UpdateVisualTreePhaseBindings()
		{
			panel.m_VisualTreeUpdater.UpdateVisualTreePhase(UnityEngine.UIElements.VisualTreeUpdatePhase.Bindings);
		}

		public void UpdateVisualTreePhaseStyles()
		{
			panel.m_VisualTreeUpdater.UpdateVisualTreePhase(UnityEngine.UIElements.VisualTreeUpdatePhase.Styles);
		}

		public void UpdateVisualTreePhaseLayout()
		{
			panel.m_VisualTreeUpdater.UpdateVisualTreePhase(UnityEngine.UIElements.VisualTreeUpdatePhase.Layout);
		}

		public void UpdateVisualTree()
		{
			panel.m_VisualTreeUpdater.UpdateVisualTree();
		}

		public void UpdateVisualTreePhaseTransformClip()
		{
			panel.m_VisualTreeUpdater.UpdateVisualTreePhase(UnityEngine.UIElements.VisualTreeUpdatePhase.TransformClip);
		}

		public override bool Equals(object obj)
		{
			if (obj is Panel_Internal @internal)
			{
				return Equals(@internal);
			}
			return false;
		}

		public bool Equals(Panel_Internal other)
		{
			return EqualityComparer<UnityEngine.UIElements.Panel>.Default.Equals(panel, other.panel);
		}

		public override int GetHashCode()
		{
			return 742809708 + EqualityComparer<UnityEngine.UIElements.Panel>.Default.GetHashCode(panel);
		}

		public static bool operator ==(Panel_Internal left, Panel_Internal right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Panel_Internal left, Panel_Internal right)
		{
			return !(left == right);
		}
	}
}
