using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UML_Generator
{
    class UMLType
    {
        protected static List<UMLType> typesCreated = new List<UMLType>();
        public (double left, double height) positionInDocument { get; set; } = (0, 0);
        public Type type { get; init; } //the type.
        public Type[] containsFromCurrent { get; private set; } //if the type is defined in the current object

        /// <summary>
        /// Shows the hirarchy from the type.
        /// We don't include object in the hirarchy.
        /// </summary>
        /// <returns></returns>
        public UMLTree? GetUMLTreeHirarchy()
        {
            //empty list.
            if (type.GetType() == typeof(Object))
                return null;

            Type currentType = type.BaseType;
            List<UMLType> umlTypes = new List<UMLType>();
            umlTypes.Add((UMLType)type);

            while(currentType != typeof(Object))
            {
                umlTypes.Add(new UMLType(currentType));
                currentType = currentType.BaseType;
            }

            umlTypes.Reverse();

            return new UMLTree(umlTypes);
        }

        public static UMLType GetUMLType(Type type)
        {
            //if we already have an instance of that type
            if (typesCreated.Any(x => x.type == type))
            {
                return typesCreated.First(x => x.type == type);
            }

            UMLType instance = new UMLType(type);
            typesCreated.Add(instance);
            return instance;
        }

        private UMLType(Type type)
        {
            this.type = type;
            containsFromCurrent = type.CurrentTypes().Distinct().ToArray();
        }

        public static implicit operator UMLType(Type t)
        {
            return GetUMLType(t);
        }

        public static explicit operator Type(UMLType t)
        {
            return t.type;
        }

        public override string ToString()
        {
            return $"Type: {type.Name}, Contains [{string.Join(",", containsFromCurrent.Select(x => x.Name))}]";
        }
    }
}
