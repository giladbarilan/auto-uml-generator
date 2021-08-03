using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UML_Generator
{
    class UMLNode
    {
        protected static List<UMLNode> createdAlready = new List<UMLNode>();

        public UMLType value { get; set; }
        public UMLNode down { get; private set; }

        private List<UMLNode> typesInLine = new List<UMLNode>();
        public UMLNode up { get; set; } = null;

        public static UMLNode GetUMLNode(UMLType t)
        {
            if (createdAlready.Any(x => x.value == t))
                return createdAlready.First(x => x.value == t);

            UMLNode instance = new UMLNode(t);
            createdAlready.Add(instance);
            return instance;
        }

        private UMLNode(UMLType value)
        {
            this.value = value;
            this.typesInLine = new List<UMLNode>() { this };
        }

        public void AddTypeInLine(UMLType newType)
        {
            if (this.typesInLine.Contains(UMLNode.GetUMLNode(newType.type)))
                return;

            this.typesInLine.Add(UMLNode.GetUMLNode(newType));
            UMLNode.GetUMLNode(newType).up = this.up;
        }

        public void SetDownLine(IEnumerable<UMLType> types) 
        {
            this.down.typesInLine = types.Select(x => UMLNode.GetUMLNode(x)).ToList();
            this.down.typesInLine.ForEach(x => x.typesInLine = this.down.typesInLine);
            this.down.typesInLine.ForEach(x => x.up = this);
        }

        public List<UMLNode> GetLine()
        {
            typesInLine.ForEach(x => 
            {
                x.up = this.up;
                x.typesInLine = this.typesInLine;
            });

            return typesInLine;
        }

        public void SetDown(UMLNode nextLine)
        {
            nextLine.up = this;
            this.down = nextLine;
            this.down.AddTypeInLine(nextLine.value.type);
        }
    }

}
