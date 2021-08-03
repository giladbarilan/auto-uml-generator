using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMLGraph = UML_Generator.UMLNode;
using System.Threading;
using System.Reflection;
using System.Collections;

namespace UML_Generator
{
    sealed class UMLTree
    {
        public List<UMLType> typesInUMLTree { get; internal set; }
        public UMLGraph UMLGraph_ { get; private set; }

        public UMLTree(List<UMLType> types) => this.typesInUMLTree = types;

        #region Not Used Currently
        /// <summary>
        /// Sorts the tree from the biggest father to the smallest children.
        /// </summary>
        public void BuildGraph()
        {
            //we fill be the biggest in hirarchy

            UMLGraph_ = UMLGraph.GetUMLNode(typesInUMLTree.First(x => x.type.GetInheritenceRank() ==
                                    typesInUMLTree.Min(x => x.type.GetInheritenceRank())));

            BuildGraphRecurse(UMLGraph_, 2);
        }

        /// <summary>
        /// Builds the graph using Recursion.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="i"></param>

        private void BuildGraphRecurse(UMLNode graph, int i)
        {
            List<UMLType> typesInRank = typesInUMLTree.Where(x => x.type.GetInheritenceRank() == i).ToList();
            typesInRank = typesInRank.Where(x => x.type.BaseType.Name == graph.value.type.Name).ToList();

            Console.WriteLine($"Type: {graph.value.type.Name}, Below: { string.Join(",", typesInRank.Select(x => x.type.Name))}");
            Console.WriteLine($"Type: {graph.value.type.Name}, Above: { graph.up?.value.type.Name}");

            if(graph.value.type == typeof(MethodBase))
            {
                Console.WriteLine("Here!");
            }

            if (typesInRank.Count == 0)
                return;

            graph.SetDown(UMLNode.GetUMLNode(typesInRank[0]));
            graph.SetDownLine(typesInRank);

            BuildGraphRecurse(graph.down, i + 1);

            for (int j = 1; j < graph.GetLine().Count; j++)
            {
                BuildGraphRecurse(graph.GetLine()[j], i);
            }
        }

        #endregion

        public void BuildUML()
        {
            for(int i = 0; i < typesInUMLTree.Count; i++)
            {
                UMLType currentType = typesInUMLTree[i].type;

                while (currentType.type.BaseType != typeof(Object))
                {
                    UMLNode typeNode = UMLNode.GetUMLNode(currentType);
                    typeNode.up = UMLNode.GetUMLNode(currentType.type.BaseType);
                    currentType = currentType.type.BaseType;
                    var relatedNode = UMLNode.GetUMLNode(currentType);
                    if (relatedNode.down == null)
                        relatedNode.SetDown(typeNode);
                    else
                    {
                        if(relatedNode.down.GetLine().Contains(UMLNode.GetUMLNode(typeNode.value.type)))
                            relatedNode.down.AddTypeInLine(typeNode.value.type);
                    }
                }
            }
        }
        

        #region Checks if the tree is valid
        /// <summary>
        /// Returns true if the types are related to each other.
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public bool IsTypesRelated(List<UMLType> types)
        {
            UMLType lowestRank = types.First(x => x.type.GetInheritenceRank() == types.Min(x => x.type.GetInheritenceRank()));
            return types.TrueForAll(x => x.type.IsBaseType((Type)lowestRank));
        }

        public bool IsTypesRelated(UMLTree types)
        {
            UMLType lowestRank = types.typesInUMLTree.First(x => x.type.GetInheritenceRank() == types.typesInUMLTree.Min(x => x.type.GetInheritenceRank()));
            return types.typesInUMLTree.TrueForAll(x => x.type.IsBaseType((Type)lowestRank));
        }

        #endregion

        public bool UpgradeTree(UMLTree umlTree)
        {
            //if it's a valid tree
            if (!IsTypesRelated(umlTree.typesInUMLTree))
            {
                throw new InvalidTreeException();
            }

            //if it's a larger version of the same tree (including more childrens).
            if (IsConnectedTrees(umlTree))
            {
                this.typesInUMLTree.AddRange(umlTree.typesInUMLTree);
                this.typesInUMLTree = this.typesInUMLTree.Distinct().ToList(); //we remove duplicates.
                return true;
            }
            return false;
        }

        public bool AddOneElement(UMLNode node)
        {
            UMLType biggestFather = this.typesInUMLTree.First(x => x.type.GetInheritenceRank()
                                    == this.typesInUMLTree.Min(y => y.type.GetInheritenceRank()));

            //they are related if:
            /*
             * node is the base type of the current biggest father.
             * node is a children of the current biggest father.
             */
            if(biggestFather.type.IsBaseType(node.value.type) || 
               node.value.type.IsBaseType(biggestFather.type))
            {
                this.typesInUMLTree.Add(node.value.type);
                return true;
            }
            return false;
        }


        public bool IsConnectedTrees(UMLTree otherTree)
        {
            if (!IsTypesRelated(otherTree.typesInUMLTree))
                return false;

            if (otherTree.typesInUMLTree.Any(x => typesInUMLTree.Select(x => x.type.Name).Contains(x.type.Name)))
            {
                return true;
            }

            if (otherTree.typesInUMLTree.Any(x => typesInUMLTree.Any(y => y.type.IsBaseType((Type)x)))
                || typesInUMLTree.Any(x => otherTree.typesInUMLTree.Any(y => y.type.IsBaseType((Type)x))))
            {
                return true;
            }

            return false;
        }

#if DEBUG
        public void PrintTree()
        {
            for(int i = 0; i < this.typesInUMLTree.Count; i++)
            {
                Console.WriteLine(typesInUMLTree[i].type.Name + " [up: " + UMLNode.GetUMLNode(typesInUMLTree[i]).up?.value.type.Name);
            }

            Console.WriteLine("_________________________");
        }
#endif
    }

    public class InvalidTreeException : Exception
    {
        public override string Message => "Invalid Tree was provided.";
    }

}
