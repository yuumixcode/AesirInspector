using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.Examples.Internal;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(SearchableAttribute), "The Searchable attribute can be applied to individual members in a type, to make only that member searchable.", Order = -2f)]
	[ExampleAsComponentData(Namespaces = new string[] { "System", "System.Collections.Generic" })]
	internal class SearchablePerksExample
	{
		[Serializable]
		public class Perk
		{
			public string Name;

			[TableList]
			public List<Effect> Effects;
		}

		[Serializable]
		public class Effect
		{
			public Skill Skill;

			public float Value;
		}

		public enum Skill
		{
			Strength,
			Dexterity,
			Constitution,
			Intelligence,
			Wisdom,
			Charisma
		}

		[Searchable]
		public List<Perk> Perks = new List<Perk>
		{
			new Perk
			{
				Name = "Old Sage",
				Effects = new List<Effect>
				{
					new Effect
					{
						Skill = Skill.Wisdom,
						Value = 2f
					},
					new Effect
					{
						Skill = Skill.Intelligence,
						Value = 1f
					},
					new Effect
					{
						Skill = Skill.Strength,
						Value = -2f
					}
				}
			},
			new Perk
			{
				Name = "Hardened Criminal",
				Effects = new List<Effect>
				{
					new Effect
					{
						Skill = Skill.Dexterity,
						Value = 2f
					},
					new Effect
					{
						Skill = Skill.Strength,
						Value = 1f
					},
					new Effect
					{
						Skill = Skill.Charisma,
						Value = -2f
					}
				}
			},
			new Perk
			{
				Name = "Born Leader",
				Effects = new List<Effect>
				{
					new Effect
					{
						Skill = Skill.Charisma,
						Value = 2f
					},
					new Effect
					{
						Skill = Skill.Intelligence,
						Value = -3f
					}
				}
			},
			new Perk
			{
				Name = "Village Idiot",
				Effects = new List<Effect>
				{
					new Effect
					{
						Skill = Skill.Charisma,
						Value = 4f
					},
					new Effect
					{
						Skill = Skill.Constitution,
						Value = 2f
					},
					new Effect
					{
						Skill = Skill.Intelligence,
						Value = -3f
					},
					new Effect
					{
						Skill = Skill.Wisdom,
						Value = -3f
					}
				}
			}
		};
	}
}
