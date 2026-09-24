using System;
using System.Text;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// <para>
	/// DrawerPriority is used in conjunction with <see cref="T:Sirenix.OdinInspector.Editor.DrawerPriorityAttribute" />
	/// to specify the priority of any given drawer. It consists of 3 components:
	/// Super, Wrapper, Value, where Super is the most significant component,
	/// and Standard is the least significant component.
	/// </para>
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.DrawerPriorityLevel" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.DrawerPriorityAttribute" />
	public struct DrawerPriority : IEquatable<DrawerPriority>, IComparable<DrawerPriority>
	{
		/// <summary>
		/// Auto priority is defined by setting all of the components to zero.
		/// If no <see cref="T:Sirenix.OdinInspector.Editor.DrawerPriorityAttribute" /> is defined on a drawer, it will default to AutoPriority.
		/// </summary>
		public static readonly DrawerPriority AutoPriority = new DrawerPriority(0.0, 0.0, 0.0);

		/// <summary>
		/// The standard priority. Mostly used by <see cref="T:Sirenix.OdinInspector.Editor.OdinValueDrawer`1" />s.
		/// </summary>
		public static readonly DrawerPriority ValuePriority = new DrawerPriority(0.0, 0.0, 1.0);

		/// <summary>
		/// The attribute priority. Mostly used by <see cref="T:Sirenix.OdinInspector.Editor.OdinAttributeDrawer`2" />s.
		/// </summary>
		public static readonly DrawerPriority AttributePriority = new DrawerPriority(0.0, 0.0, 1000.0);

		/// <summary>
		/// The wrapper priority. Mostly used by drawers used to decorate properties.
		/// </summary>
		public static readonly DrawerPriority WrapperPriority = new DrawerPriority(0.0, 1.0);

		/// <summary>
		/// The super priority. Mostly used by drawers that wants to wrap the entire property but don't draw the actual property.
		/// These drawers typically don't draw the property itself, and calls CallNextDrawer.
		/// </summary>
		public static readonly DrawerPriority SuperPriority = new DrawerPriority(1.0);

		/// <summary>
		/// The value priority. Mostly used by <see cref="T:Sirenix.OdinInspector.Editor.OdinValueDrawer`1" />s and <see cref="T:Sirenix.OdinInspector.Editor.OdinAttributeDrawer`2" />s.
		/// </summary>
		public double Value;

		/// <summary>
		/// The wrapper priority. Mostly used by drawers used to decorate properties.
		/// </summary>
		public double Wrapper;

		/// <summary>
		/// The super priority. Mostly used by drawers that wants to wrap the entire property but don't draw the actual property.
		/// These drawers typically don't draw the property itself, and calls CallNextDrawer.
		/// </summary>
		public double Super;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.DrawerPriority" /> struct.
		/// </summary>
		/// <param name="priority">The priority.</param>
		public DrawerPriority(DrawerPriorityLevel priority)
		{
			DrawerPriority set = priority switch
			{
				DrawerPriorityLevel.AutoPriority => AutoPriority, 
				DrawerPriorityLevel.ValuePriority => ValuePriority, 
				DrawerPriorityLevel.AttributePriority => AttributePriority, 
				DrawerPriorityLevel.WrapperPriority => WrapperPriority, 
				DrawerPriorityLevel.SuperPriority => SuperPriority, 
				_ => throw new NotImplementedException(priority.ToString()), 
			};
			Value = set.Value;
			Wrapper = set.Wrapper;
			Super = set.Super;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.DrawerPriority" /> struct.
		/// </summary>
		/// <param name="super">
		/// The super priority. Mostly used by drawers that wants to wrap the entire property but don't draw the actual property.
		/// These drawers typically don't draw the property itself, and calls CallNextDrawer.</param>
		/// <param name="wrapper">The wrapper priority. Mostly used by drawers used to decorate properties.</param>
		/// <param name="value">The value priority. Mostly used by <see cref="T:Sirenix.OdinInspector.Editor.OdinValueDrawer`1" />s and <see cref="T:Sirenix.OdinInspector.Editor.OdinAttributeDrawer`2" />s.</param>
		public DrawerPriority(double super = 0.0, double wrapper = 0.0, double value = 0.0)
		{
			Super = super;
			Wrapper = wrapper;
			Value = value;
		}

		/// <summary>
		/// Implements the operator &gt;.
		/// </summary>
		/// <param name="lhs">The LHS.</param>
		/// <param name="rhs">The RHS.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator >(DrawerPriority lhs, DrawerPriority rhs)
		{
			if (lhs == rhs)
			{
				return false;
			}
			if (lhs.Super > rhs.Super)
			{
				return true;
			}
			if (lhs.Super != rhs.Super)
			{
				return false;
			}
			if (lhs.Wrapper > rhs.Wrapper)
			{
				return true;
			}
			if (lhs.Wrapper != rhs.Wrapper)
			{
				return false;
			}
			if (lhs.Value > rhs.Value)
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Implements the operator &lt;.
		/// </summary>
		/// <param name="lhs">The LHS.</param>
		/// <param name="rhs">The RHS.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator <(DrawerPriority lhs, DrawerPriority rhs)
		{
			if (lhs == rhs)
			{
				return false;
			}
			if (lhs.Super < rhs.Super)
			{
				return true;
			}
			if (lhs.Super != rhs.Super)
			{
				return false;
			}
			if (lhs.Wrapper < rhs.Wrapper)
			{
				return true;
			}
			if (lhs.Wrapper != rhs.Wrapper)
			{
				return false;
			}
			if (lhs.Value < rhs.Value)
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Implements the operator &lt;=.
		/// </summary>
		/// <param name="lhs">The LHS.</param>
		/// <param name="rhs">The RHS.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator <=(DrawerPriority lhs, DrawerPriority rhs)
		{
			if (!(lhs < rhs))
			{
				return lhs == rhs;
			}
			return true;
		}

		/// <summary>
		/// Implements the operator &gt;=.
		/// </summary>
		/// <param name="lhs">The LHS.</param>
		/// <param name="rhs">The RHS.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator >=(DrawerPriority lhs, DrawerPriority rhs)
		{
			if (!(lhs > rhs))
			{
				return lhs == rhs;
			}
			return true;
		}

		/// <summary>
		/// Implements the operator +.
		/// </summary>
		/// <param name="lhs">The LHS.</param>
		/// <param name="rhs">The RHS.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static DrawerPriority operator +(DrawerPriority lhs, DrawerPriority rhs)
		{
			lhs.Super += rhs.Super;
			lhs.Wrapper += rhs.Wrapper;
			lhs.Value += rhs.Value;
			return lhs;
		}

		/// <summary>
		/// Implements the operator -.
		/// </summary>
		/// <param name="lhs">The LHS.</param>
		/// <param name="rhs">The RHS.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static DrawerPriority operator -(DrawerPriority lhs, DrawerPriority rhs)
		{
			lhs.Super -= rhs.Super;
			lhs.Wrapper -= rhs.Wrapper;
			lhs.Value -= rhs.Value;
			return lhs;
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="lhs">The LHS.</param>
		/// <param name="rhs">The RHS.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator ==(DrawerPriority lhs, DrawerPriority rhs)
		{
			if (lhs.Super == rhs.Super && lhs.Wrapper == rhs.Wrapper)
			{
				return lhs.Value == rhs.Value;
			}
			return false;
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="lhs">The LHS.</param>
		/// <param name="rhs">The RHS.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator !=(DrawerPriority lhs, DrawerPriority rhs)
		{
			if (lhs.Super == rhs.Super && lhs.Wrapper == rhs.Wrapper)
			{
				return lhs.Value != rhs.Value;
			}
			return true;
		}

		/// <summary>
		/// Gets the priority level.
		/// </summary>
		public DrawerPriorityLevel GetPriorityLevel()
		{
			if (Super > 0.0)
			{
				return DrawerPriorityLevel.SuperPriority;
			}
			if (Wrapper > 0.0)
			{
				return DrawerPriorityLevel.WrapperPriority;
			}
			if (Value >= AttributePriority.Value)
			{
				return DrawerPriorityLevel.AttributePriority;
			}
			if (Value > 0.0)
			{
				return DrawerPriorityLevel.ValuePriority;
			}
			return DrawerPriorityLevel.AutoPriority;
		}

		/// <summary>
		/// Returns a <see cref="T:System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return new StringBuilder(GetPriorityLevel().ToString()).Append(" (").Append(Super).Append(", ")
				.Append(Wrapper)
				.Append(", ")
				.Append(Value)
				.Append(')')
				.ToString();
		}

		/// <summary>
		/// Returns a <see cref="T:System.String" /> that represents this instance.
		/// </summary>
		/// <param name="format">The format.</param>
		/// <returns>
		/// A <see cref="T:System.String" /> that represents this instance.
		/// </returns>
		public string ToString(string format)
		{
			return new StringBuilder(GetPriorityLevel().ToString()).Append(" (").Append(Super.ToString(format)).Append(", ")
				.Append(Wrapper.ToString(format))
				.Append(", ")
				.Append(Value.ToString(format))
				.Append(')')
				.ToString();
		}

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object" />, is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="T:System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj is DrawerPriority b)
			{
				return this == b;
			}
			return false;
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode()
		{
			int hash = 1417;
			hash = hash * 219 + Super.GetHashCode();
			hash = hash * 219 + Wrapper.GetHashCode();
			return hash * 219 + Value.GetHashCode();
		}

		/// <summary>
		/// Equals the specified other.
		/// </summary>
		/// <param name="other">The other.</param>
		/// <returns></returns>
		public bool Equals(DrawerPriority other)
		{
			return this == other;
		}

		/// <summary>
		/// Compares to.
		/// </summary>
		/// <param name="other">The other.</param>
		/// <returns></returns>
		public int CompareTo(DrawerPriority other)
		{
			if (this > other)
			{
				return 1;
			}
			if (this < other)
			{
				return -1;
			}
			return 0;
		}
	}
}
