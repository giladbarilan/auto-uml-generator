using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace UML_Generator
{
    class PossibleTypesReader
    {
        public static List<Type> GetAllTypes()
        {
            string systemDllPath = typeof(string).Assembly.Location; //gets the dll of the string type.
            systemDllPath = Path.GetDirectoryName(systemDllPath);
            string[] systemLibraries = Directory.GetFiles(systemDllPath); //the list of the system libraies dlls.           
            string[] namespaces_ = Directory.GetFiles(GetSourceCodePath())
                                      .Where(x => x.EndsWith(".cs")).ToArray();
            namespaces_ = GetNamespaces(namespaces_).ToArray();//we remove the semi colon

            List<Type> types = systemLibraries.Where(x => x.EndsWith(".dll")).Select(x =>
            {
                Assembly asm = null;
                try
                {
                    asm = Assembly.LoadFile(x);

                }
                catch
                {
                    asm = null;
                }

                return asm;
            }).SelectMany(x => x != null ? x.GetTypes() : new Type[] { })
                        .Where(x => x != null && !x.Name.Contains("<>c")).Where(x => namespaces_.Contains(x.Namespace))
                        .Where(x => x.IsPublic && (x.IsClass || x.IsValueType)).ToList();

            types.AddRange(AppDomain.CurrentDomain.GetAssemblies()
                           .SelectMany(x => x.GetTypes().Where(x => !x.Name.Contains("<>c")).Where(x => namespaces_.Contains(x.Namespace)))
                           .Where(x => x.IsPublic && (x.IsClass || x.IsValueType)));

            types.AddRange(GetCustomTypes()); //adds our custom created snippets
            types.AddRange(GetUnUsedRefrenecedTypes(namespaces_)); //adds the refrenced libraries we specify but don't use.

            return types;
        }

        //Returns the assembly of the main function.
        public static string GetMainAssemblyLocation()
        {
            StackTrace main = new StackTrace();
            string path = main.GetFrame(main.FrameCount - 1).GetMethod().DeclaringType.Assembly.Location;
            return Path.GetDirectoryName(path);
        }

        public static string GetSourceCodePath()
        {
            string projectPath = GetMainAssemblyLocation();

            try
            {
                while (!Directory.GetFiles(projectPath).Any(x => x.EndsWith(".cs")))
                {
                    projectPath = projectPath.Remove(projectPath.LastIndexOf("\\"));
                }
            }
            catch
            {
                throw new SourceCodeNotFoundException();
            }

            return projectPath;
        }

        public static List<string> GetNamespaces(string[] files)
        {
            List<string> namespaces = new List<string>();

            foreach (string file_ in files)
            {
                StreamReader fileStream = new StreamReader(file_);
                Regex matchUsing = new Regex(@"using([ ]|\t|\n)+\S+;");
                string[] fileNamespaces = matchUsing.Matches(fileStream.ReadToEnd()).Select(x => x.Value)
                                          .Select(x => x.Remove(x.IndexOf("using"), "using".Length).Trim()).ToArray();
                namespaces.AddRange(fileNamespaces);

                fileStream.Close();
            }

            return namespaces.Select(x => x.Remove(x.Length - 1, 1)).Distinct().ToList();
        }

        /// <summary>
        /// This is used in case we have a custom refrenced library (could be a dll we created)
        /// and we specify that in the using, however, we don't use his assemblies.
        /// </summary>
        /// <returns></returns>
        public static List<Type> GetUnUsedRefrenecedTypes(string[] namespaces_)
        {
            string sourceCode = GetMainAssemblyLocation();
            List<Type> otherTypes = new List<Type>();

            string[] fileNames = Directory.GetFiles(Path.GetDirectoryName(sourceCode));

            if (fileNames.Any(x => x.EndsWith(".dll")))
            {
                fileNames = fileNames.Where(x => x.EndsWith(".dll")).ToArray();

                foreach (var file in fileNames)
                {
                    Assembly asm;

                    try
                    {
                        asm = Assembly.LoadFile(file);
                    }
                    catch
                    {
                        continue;
                    }

                    otherTypes.AddRange(asm.GetTypes().Where(x => !x.Name.Contains("<>c"))
                                        .Where(x => x.IsPublic && (x.IsClass || x.IsValueType))
                                        .Where(x => namespaces_.Contains(x.Namespace)).GroupBy(x => x.Name)
                                        .Select(x => x.First()));
                }
            }

            return otherTypes;
        }

        /// <summary>
        /// Returns a list of all the types we created.
        /// </summary>
        /// <returns></returns>
        public static List<Type> GetCustomTypes()
        {
            StackTrace myStackTrace = new StackTrace();
            Type mainType = myStackTrace.GetFrame(myStackTrace.FrameCount - 1).GetMethod().DeclaringType;

            //contains the name of the namesapce of the project
            //using stackframe we get's the first calling method (main - the bottom of the stack) and then we check the namespace of the type where the method is located.
            //in our case main is located under 'Program' then we check what is the namespace of Program.
            string namespace_ = mainType.Namespace;
            Assembly assembly = mainType.Assembly;

            //holds all the custom types.
            //assembly.GetTypes() => returns all the types.
            //then we remove all the types which is not related to our namesapce.
            //then we remove all the compiler created types.

            var types = assembly.GetTypes().Where(x => x.Namespace == namespace_).Where(x => !x.Name.Contains("<>c")); ;
            return types.ToList();
        }
    }


    internal class SourceCodeNotFoundException : Exception
    {
        public override string Message => "Could not load the '.cs' files, please make sure\n" +
                                    "your files located in the default place / you have a valid project type.";
    }

}
