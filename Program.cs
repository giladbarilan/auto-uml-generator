using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Diagnostics;

namespace UML_Generator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            List<Type> typesInProject = PossibleTypesReader.GetAllTypes(); //gets the types in the project.     
            typesInProject.RemoveAll(x => x.IsGenericType);
            List<UMLTree> projectTrees = UMLManager.GetTrees(typesInProject);
            projectTrees.ForEach(x =>
            {
                x.PrintTree();
                Console.WriteLine("_____________");
            }
            );



            Stopwatch f = new Stopwatch();

            RunSync(projectTrees);

            Console.WriteLine(f.ElapsedMilliseconds);
        }

        static void RunSync(List<UMLTree> trees)
        {
            trees.ForEach(x => new UMLWriter().WriteUML(x));
        }

        static async Task RunAsync(List<UMLTree> uml)
        {
            UMLWriter r = new UMLWriter();
            var tasks = uml.Select(x => Task.Run(() => r.WriteUML(x)));
            await Task.WhenAll(tasks);
        }
    }

    #region FirstTree
    public class Person: Human { }
    public class Adult: Person { }
    public class Children: Person { }
    public class Old: Person { }
    public class HealthyOldMan: Old { }
    public class AlmostDead: Old { }
    public class HappyChildren: Children { }
    public class LowSchoolChildren: HappyChildren { }
    public class MiddleSchoolChildren: HappyChildren { }
    public class HighSchoolChildren: HappyChildren { }
    public class HighSchoolGraduated: HighSchoolChildren { }
    public class WorkingAdult: Adult { }
    public class Builder: WorkingAdult { }
    public class Officer: WorkingAdult { }
    public class NotWorkingAdult: Adult { }
    public class Researcher: Officer { }
    public class EvilResearcher: Researcher { }
    public class NiceResearcher: Researcher { }
    public class GoodBuilder: Builder { }
    public class BestBuilder: GoodBuilder { }
    #endregion

    public class BirthDate { }

    public class Organ 
    {
        public BirthDate date;
    }
    public class Animal: Organ { }
    public class Plant: Organ { }
    public class Tree: Plant { }
    public class Brosh: Tree { }
    public class Flower: Plant { }
    public class Rose: Flower { }
    public class Mammals: Animal { }
    public class Cow: Mammals { }
    public class Human: Mammals { }
    public class Dolphin: Mammals { }
    public class CatsFamily: Animal { }
    public class Lion: CatsFamily { }
}
