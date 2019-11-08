using System;
using System.Collections.Generic;
using System.Linq;

namespace TowerDefenseGame
{
    internal class Stat
    {
        public List<Modifier> Modifiers = new List<Modifier>();
        public float baseValue;

        public Stat(float baseValue)
        {
            this.baseValue = baseValue;
        }

        public float Value
        {
            get
            {
                float value = baseValue;
                Modifiers = Modifiers.OrderBy(m => m.type).ToList(); //Order, so that first values are applied, then percentages and the absolute values
                foreach (var modifier in Modifiers)
                {
                    value = modifier.Apply(value); //TODO: What to do when there are two absolute values?
                }
                return value;
            }
        }
    }

    internal enum ModifierType
    { Value = 0, Percentage = 1, Absolute = 2 }

    internal class Modifier
    {
        public ModifierType type;
        public float value;
        public readonly string name;

        public Modifier(ModifierType type, float value, string name = "")
        {
            this.type = type;
            this.value = value;
            this.name = name;
        }

        public float Apply(float _val)
        {
            switch (type)
            {
                case ModifierType.Value:
                    return _val + value;

                case ModifierType.Percentage:
                    return _val * (1 + value / 100);

                case ModifierType.Absolute:
                    return value;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}