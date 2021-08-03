using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UML_Generator
{
    static class TypeExtentions
    {

        /// <summary>
        /// returns the number of bases that a class has.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        internal static int GetInheritenceRank(this Type t)
        {
            int numberOfBaseTypes = 0;

            while(t.Name != "Object")
            {
                numberOfBaseTypes++;
                t = t.BaseType;
            }

            return numberOfBaseTypes;
        }

        internal static bool IsBaseType(this Type t, Type other)
        {
            if (t.GetType() == typeof(Object) || other.GetType() == typeof(Object))
                return false;

            while(t != typeof(Object))
            {
                if (t.Name == other.Name)
                    return true;
                t = t.BaseType;
            }

            return false;
        }

        /// <summary>
        /// The types in the next rank.
        /// </summary>
        /// <returns></returns>
        internal static Type[] FatherTypes(this Type t)
        {
            return t.BaseType.CurrentTypes();
        }

        /// <summary>
        /// the types on a specific rank.
        /// </summary>
        /// <param name="rank"></param>
        /// <returns></returns>

        internal static Type[] TypesInRank(this Type t, int rank)
        {
            for (int i = 0; i < rank; i++)
                t = t.BaseType;

            return t.CurrentTypes();
        }


        /// <summary>
        /// The function returns the types of the members (only Fields & properties) in the class (including Non-Public from base types).
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        internal static List<Type> AllContainedTypes(this Type t)
        {
            return GetRelevantMembers(t).Select(x =>
            {
                if (x is FieldInfo)
                    return (x as FieldInfo).FieldType;
                return (x as PropertyInfo)?.PropertyType;
            }).ToList();
        }

        /// <summary>
        /// The function returns all of the types that were defined in the current class.
        /// (Including Non-Public members(only Fields & properties)).
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        internal static Type[] CurrentTypes(this Type t)
        {
            List<MemberInfo> relevantMembers = GetRelevantMembers(t);
            relevantMembers.RemoveAll(x => x.DeclaringType != t); //remove all the types from fathers.

            return relevantMembers.Select(x =>
            {
                if (x is FieldInfo)
                    return (x as FieldInfo).FieldType;
                return (x as PropertyInfo)?.PropertyType;
            }).ToArray();
        }

        /// <summary>
        /// Function returns all the relevant members (only Fields & properties) in the class.
        /// * Including Non-Public members (only Fields & properties) from base types.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static List<MemberInfo> GetRelevantMembers(this Type t)
        {
            List<MemberInfo> members = t.GetFields().ToList<MemberInfo>(); //we add the simple fields.
            //we don't add fields that created on runtime from the properties.
            members.AddRange(t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(x => !x.Name.Contains("k__BackingField"))); //we add the non public fields;
            members.AddRange(t.GetProperties()); //we add the properties.
            members.AddRange(t.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)); //we add the non public.

            t = t.BaseType;

            //adds all of the non public members from the base types.
            while (t != null)
            {
                members.AddRange(t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(x => !x.Name.Contains("k__BackingField")));
                members.AddRange(t.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance));
                t = t.BaseType;
            }

            return members;
        }
    }
}
