using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UML_Generator
{
    class UMLManager
    {
        /// <summary>
        /// NOTE:
        /// if a class's father is in a different assembly (such as one we don't use in project) we won't initialiize 
        /// a tree for him.
        /// </summary>
        /// <param name="possibleTypes"></param>
        /// <returns></returns>
        internal static List<UMLTree> GetTrees(List<Type> possibleTypes)
        {
            //we start the trees with types that has object as their father.
            List<UMLTree> trees = new List<UMLTree>();
            possibleTypes.Remove(typeof(Object)); //we don't want to create a UML tree to Object.
            possibleTypes.RemoveAll(x => x.IsValueType || x.IsPrimitive);

            //we start all of the trees.
            trees.AddRange(possibleTypes.Where(x => x.BaseType == typeof(Object))
                                        .Select(x => new UMLTree(new List<UMLType>() { x })));

            possibleTypes.RemoveAll(x => x.BaseType == typeof(Object)); //we don't want to have duplicates.

            for (int i = 0; i < possibleTypes.Count; i++)
            {
                trees.Any(x => x.AddOneElement(UMLNode.GetUMLNode(possibleTypes[i])));
            }

            trees.RemoveAll(x => x.typesInUMLTree.Count == 1);
            trees.ForEach(x => x.BuildUML());

            return trees;
        }

#if DEBUG
        public static void PrintTrees(List<UMLTree> trees)
        {
            for (int i = 0; i < trees.Count; i++)
            {
                Console.WriteLine(string.Join("\n", trees[i].typesInUMLTree));
                Console.WriteLine("______________________________");
            }
        }
#endif
    }
}
