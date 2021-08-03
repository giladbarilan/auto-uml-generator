using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace UML_Generator
{
    class UMLWriter
    {
        double width = 200;
        private static string strokesCode = "";
        private static string arrowsCode = "";


        /// <summary>
        /// Algorithm:
        /// - We start writing from the smallest node.
        /// </summary>
        /// <param name="type"></param>
        public void WriteUML(UMLTree tree)
        {
            //calculates the place where we should start painting.
            double Height = (tree.typesInUMLTree.Max(x => x.type.GetInheritenceRank()) - 1) * 200.0;

            string fileName = tree.typesInUMLTree.Find(x => x.type.GetInheritenceRank() == tree.typesInUMLTree.Min(x => x.type.GetInheritenceRank()))
                              .type.Name + ".html";
            Directory.CreateDirectory("UML-Trees");

            fileName = "UML-Trees\\" + fileName;

            tree.typesInUMLTree = tree.typesInUMLTree.Distinct().ToList();

            int maxCountHeigth = tree.typesInUMLTree.Max(x => x.type.GetInheritenceRank());

            if (maxCountHeigth > 10)
            {
                maxCountHeigth -= 10;
                maxCountHeigth *= 250;
            }
            else
                maxCountHeigth = 0;
            
            //write file structure.
            File.WriteAllText(fileName,
                @$"<html><head><style> .arrow {{width: 0; height: 0;border-left: 5px solid transparent;border-right: 5px solid transparent;border-bottom: 10px solid black;position: absolute;}}.Box1{{font-size:10px;position:absolute;width:{width}px;
                  border: solid black 1px;font-weight:bolder;text-align:center;padding:40px 0px 40px 0px;}}</style></head>
                  <body><canvas id=""myCanvas"" width=""**WIDTH**px"" height=""{3000 + maxCountHeigth}px"">Your browser does not support the HTML5 canvas tag.</canvas>
                  <script>var c = document.getElementById(""myCanvas"");var ctx = c.getContext(""2d"");ctx.beginPath();**SET_STROKES_HERE**</script>**ARROWS**</body></html>");

            var dict = tree.typesInUMLTree.Where(x => x.type.GetInheritenceRank() == tree.typesInUMLTree
                                          .Max(x => x.type.GetInheritenceRank()))
                                          .Select(x => (Type)x)
                                          .GroupBy(x => x.BaseType)
                                          .ToDictionary(x => x.Key);

            IEnumerable<Type> baseTypes = tree.typesInUMLTree.Where(x => x.type.GetInheritenceRank() + 1 == 
                                                                    tree.typesInUMLTree.Max(x => x.type.GetInheritenceRank()))
                                          .Select(x => (Type)x);
            double left = 0.0d;
            double MaxLeft = 0.0d;
            int i = 1;

            /*
            Dictionary<Type, double> next = AddElementsInWidthLastLine(fileName, tree.typesInUMLTree.Where(x => x.type.GetInheritenceRank() == tree.typesInUMLTree
                                                                                                    .Max(x => x.type.GetInheritenceRank()))
                                                                                                    .Select(x => (Type)x).ToList(), Height);
            
            while(Height > 0)
            {
                Height -= 200;
                next = AddElementsInLine(fileName, next, Height);
            }
            */
            
            while(Height > 0)
            {
                AddElementInWidth(fileName, dict, Height, ref left, ref MaxLeft);

                var base_ = dict.Values.Select(x => x.Key);

                base_ = base_.Concat(tree.typesInUMLTree.Where(x => x.type.GetInheritenceRank() + i ==
                        tree.typesInUMLTree.Max(x => x.type.GetInheritenceRank())).Select(x => x.type));

                base_ = base_.Distinct();

                dict = base_.GroupBy(x => x.BaseType).ToDictionary(x => x.Key);

                /*
                dict = tree.typesInUMLTree.Where(x => x.type.GetInheritenceRank() + i ==
                                                                    tree.typesInUMLTree.Max(x => x.type.GetInheritenceRank()))
                                          .Select(x => (Type)x)
                                          .GroupBy(x => x.BaseType)
                                          .ToDictionary(x => x.Key);*/
                Height -= 200;
                i++;

            }

            tree.typesInUMLTree.Find(x => x.type.GetInheritenceRank() == tree.typesInUMLTree.Min(x => x.type.GetInheritenceRank())).SetPosition((left, Height));
            AddSquareElement(fileName, tree.typesInUMLTree.Find(x => x.type.GetInheritenceRank() == tree.typesInUMLTree.Min(x => x.type.GetInheritenceRank())).type.Name, Height, left);

            //create the code for drawing all of the lines.
            tree.typesInUMLTree.Where(x => x.type.BaseType != typeof(Object)).Distinct().ToList()
                               .ForEach(x => DrawLineToFather(UMLType.GetUMLType(x.type), fileName));

            //add the code for drawing all of the lines to the HTML page.
            string fileData = File.ReadAllText(fileName);
            fileData = fileData.Replace(@"**SET_STROKES_HERE**", strokesCode);
            fileData = fileData.Replace($@"**WIDTH**", (MaxLeft + 1500).ToString()); //we use this for setting the bounds of the canvas.
            fileData = fileData.Replace($@"**ARROWS**", arrowsCode);
            File.WriteAllText(fileName, fileData);

            strokesCode = "";
            arrowsCode = "";
        }

        private void DrawLineToFather(UMLType children, string fileName)
        {
            UMLType base_type = UMLType.GetUMLType(children.type.BaseType);
            double randPosition = new Random().Next(70, 130);
            double Angle = GetArrowPictureAngle(children, randPosition); //gets the angle.


            //Adding the angle is optional (I preffer not to do that.)
            AddArrowElement(fileName, base_type.positionInDocument.height + 100, base_type.positionInDocument.left + randPosition, /*Angle*/ 0.0d);


            if (children.type.BaseType == typeof(Object))
                return;

            strokesCode += @$"ctx.moveTo({children.positionInDocument.left} + 200 / 2, {children.positionInDocument.height} - 10);ctx.lineTo({base_type
                .positionInDocument.left }+ {randPosition}, {base_type.positionInDocument.height} + 100);ctx.stroke();{"\n"}";
        }

        private double GetArrowPictureAngle(UMLType children, double randPosition)
        {
            UMLType father_ = children.type.BaseType; //gives the father type.
            double incline;
            try
            {
                incline = ((children.positionInDocument.height - 10) - (father_.positionInDocument.height + 100)) / ((father_.positionInDocument.left + randPosition) - (children.positionInDocument.left + 100));
            }
            catch
            {
                return 0; //straight angle
            }

            //convert it to degrees
            return 180 - (Math.Tanh(incline) * 180 / Math.PI);
        }
        #region NotUsed
        private Dictionary<Type, double> AddElementsInWidthLastLine(string fileName, List<Type> types, double height)
        {
            Dictionary<Type, (int amount, double sum)> avrages = new Dictionary<Type, (int amount, double sum)>();
            
            for(int i = 0; i < types.Count; i++)
            {
                if (!avrages.ContainsKey(types[i].BaseType))
                    avrages.Add(types[i].BaseType, (0, 0));
            }
            
            double left = 0.0d;

            for(int i = 0; i < types.Count; i++)
            {
                ((UMLType)types[i]).SetPosition((left, height));
                avrages[types[i].BaseType] = (avrages[types[i].BaseType].amount + 1, avrages[types[i].BaseType].sum + left);
                AddSquareElement(fileName, types[i].Name, height, left);
                left += width + 50;
            }

            Dictionary<Type, double> avg = new Dictionary<Type, double>();
            types.Select(x => x.BaseType).Distinct().ToList().ForEach(x => avg.Add(x, avrages[x].sum / avrages[x].amount));
            double lastUpRankPos = avg.Max(x => x.Value);
            
            foreach(var item in types.Select(x => x.BaseType).Distinct().Where(x => !avg.Keys.Contains(x)))
            {
                avg.Add(item, lastUpRankPos);
                lastUpRankPos += width + 50;
            }

            return avg;
        }
        private Dictionary<Type, double> AddElementsInLine(string fileName, Dictionary<Type, double> addNow, double height)
        {
            addNow.Keys.ToList().ForEach(x => AddSquareElement(fileName, x.Name, height, addNow[x])); //adds the elements.
            Dictionary<Type, (int num, double sum)> nextLine = new Dictionary<Type, (int num, double sum)>();
            double left = addNow.Values.Min();

            addNow.Keys.Select(x => x.BaseType).Distinct().ToList().ForEach(x => nextLine.Add(x, (0, 0)));

            foreach(var item in addNow.Keys)
            {
                nextLine[item.BaseType] = (nextLine[item.BaseType].num + 1, nextLine[item.BaseType].sum + addNow[item]);
            }

            Dictionary<Type, double> avg = new Dictionary<Type, double>();
            nextLine.Keys.ToList().ForEach(x => avg.Add(x, nextLine[x].sum / nextLine[x].num));

            return avg;
        }

        #endregion

        private void AddElementInWidth(string fileName, Dictionary<Type, IGrouping<Type, Type>> dict, double height, ref double left, ref double MaxLeft)
        {
            double elements = 0;
            double firstLeft = left;

            foreach (var family in dict.Keys)
            {
                var contained_ = ((UMLType)family).containsFromCurrent;

                foreach(var class_ in dict[family].Select(x => UMLNode.GetUMLNode(x)))
                {
                    class_.value.SetPosition((left, height)); //we save the position
                    AddSquareElement(fileName, class_.value.type.Name, height, left);
                    left += width + 50; //next position
                    elements++;
                }

                MaxLeft = Math.Max(left, MaxLeft);
            }

            left = (elements / 2) * (2 * firstLeft + (elements - 1) * (width + 50)); //find middle by mathematic formule
                                                                                     //(n/2)(a1 + an) = Sum of all of the positions.
                                                                                     //then we find the average one.
            left /= elements;
        }

        private void AddSquareElement(string fileName, string typeName, double top, double left)
        {
            string fileText = File.ReadAllText(fileName);
            fileText = fileText.Replace("<body>", "<body>" + $"<div class = \"Box1\" style = \"top: {top}px; left: {left}px;\">{typeName}</div>");
            File.WriteAllText(fileName, fileText);
        }

        private void AddArrowElement(string fileName, double top, double left, double angle)
        {
            arrowsCode += $"<div class = \"arrow\" style = \"left: {left}px; top: {top}px; transform: rotate({angle}deg);\"></div>{Environment.NewLine}";
        }
    }
}
