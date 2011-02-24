using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace FindUsage
{
    class Program
    {
        private static int _count;

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: FindUsage.exe filespec type [member]");
                return;
            }

            var filespec = args[0];
            var targetType = args[1];
            var targetMember = args.Length == 3 ? args[2] : null;

            var assemblies = Directory.EnumerateFiles(Environment.CurrentDirectory, filespec).Select(x => AssemblyDefinition.ReadAssembly(x, new ReaderParameters {ReadSymbols = true}));

            if (targetMember == null)
                FindTypeUsages(assemblies, targetType);
            else
                FindMemberUsages(assemblies, targetType, targetMember);

            Console.WriteLine();
            Console.WriteLine("Found {0} usage(s).", _count);
        }

        static void FindTypeUsages(IEnumerable<AssemblyDefinition> assemblies, string targetType)
        {
            var targetTypes = new HashSet<string>(assemblies.SelectMany(ad => ad.MainModule.Types).Select(td => td.FullName).Where(s => s == targetType));

            Console.WriteLine("Matching target types [{0}]", String.Join(", ", targetTypes));

            Console.WriteLine();
            Console.WriteLine("Scanning assemblies...");
            Console.WriteLine();

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.MainModule.Types)
                {
                    foreach (var property in type.Properties)
                    {
                        CheckType(targetTypes, property.PropertyType, " - Property {0}", property.FullName);
                    }

                    foreach (var field in type.Fields)
                    {
                        CheckType(targetTypes, field.FieldType, " - Field {0}", field.FullName);
                    }

                    foreach (var method in type.Methods)
                    {
                        foreach (var param in method.Parameters)
                        {
                            CheckType(targetTypes, param.ParameterType, " - Method {0}", method.FullName);
                        }

                        CheckType(targetTypes, method.ReturnType, " - Method {0}", method.FullName);

                        if (method.HasBody)
                        {
                            foreach (var var in method.Body.Variables)
                            {
                                var name = var.Name;
                                if (String.IsNullOrEmpty(name))
                                    name = "var" + var.Index;

                                CheckType(targetTypes, var.VariableType, " - Variable {0} in method {1}", name, method.FullName);
                            }
                        }
                    }
                }
            }
        }

        static void FindMemberUsages(IEnumerable<AssemblyDefinition> assemblies, string targetType, string targetMember)
        {
            var targetTypes = assemblies.SelectMany(ad => ad.MainModule.Types).Where(td => td.FullName == targetType);

            var properties = targetTypes.SelectMany(td => td.Properties)
                .Where(pd => pd.Name == targetMember);
            var targetProperties = new HashSet<string>(properties.SelectMany(pd => new[] {pd.GetMethod.FullName, pd.SetMethod.FullName}));

            var targetFields = new HashSet<string>(targetTypes.SelectMany(td => td.Fields)
                                                 .Where(fd => fd.Name == targetMember)
                                                 .Select(fd => fd.FullName));

            if (properties.Count() > 0)
                Console.WriteLine("Matching properties [{0}]", String.Join(", ", properties.Select(pd => pd.FullName)));
            if (targetFields.Count > 0)
                Console.WriteLine("Matching fields [{0}]", String.Join(", ", targetFields));

            Console.WriteLine();
            Console.WriteLine("Scanning assemblies...");
            Console.WriteLine();

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.MainModule.Types)
                {
                    foreach (var method in type.Methods)
                    {
                        if (method.HasBody)
                        {
                            foreach (var instruction in method.Body.Instructions)
                            {
                                if (instruction.Operand is MethodReference)
                                {
                                    var methodDef = (MethodReference)instruction.Operand;
                                    if (targetProperties.Contains(methodDef.FullName))
                                    {
                                        Console.WriteLine(" - Call to {0} at {1}+{2}", methodDef.Name, method.FullName, instruction.Offset);
                                        ++_count;
                                    }
                                }
                                else if (instruction.Operand is FieldReference)
                                {
                                    var fieldDef = (FieldReference)instruction.Operand;
                                    if (targetFields.Contains(fieldDef.FullName))
                                    {
                                        Console.WriteLine(" - Field used at {0}+{1}", method.FullName, instruction.Offset);
                                        ++_count;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        static void CheckType(HashSet<string> targetTypes, TypeReference typeReference, string format, params object[] args)
        {
            if (targetTypes.Contains(typeReference.FullName))
            {
                Console.WriteLine(format, args);
                ++_count;
            }
            else
            {
                if (typeReference is GenericInstanceType)
                {
                    foreach (var genericArg in ((GenericInstanceType)typeReference).GenericArguments)
                    {
                        CheckType(targetTypes, genericArg, format, args);
                    }
                }
                else if (typeReference is ArrayType)
                {
                    CheckType(targetTypes, ((ArrayType) typeReference).ElementType, format, args);
                }
            }
        }
    }
}
