#if !UC_Free

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.CodeDom;
using System.IO;
using System.CodeDom.Compiler;
using System.Reflection;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;

namespace uConstruct.CodeGenerator
{

    public class BuildingTypesCodeGenerator
    {
        public static string fileName = "BuildingTypes.cs";
        public static string filePath = Application.dataPath + "/UConstruct/Scripts/CodeGenerator/" + fileName;

        public static CodeCompileUnit targetUnit;
        public static CodeTypeDeclaration targetClass;
        public static CodeNamespace CodeNamespace = new CodeNamespace();

        public static string CheckForDuplications(List<string> enumFields)
        {
            string fieldA;
            string fieldB;

            for (int i = 0; i < enumFields.Count; i++)
            {
                fieldA = enumFields[i];

                if (fieldA == "")
                    return "One or more of your building types has no name. (COMPILING FAILED)";

                for (int b = 0; b < enumFields.Count; b++)
                {
                    fieldB = enumFields[b];

                    if (i != b)
                    {
                        if (fieldA.ToLower() == fieldB.ToLower())
                            return "One or more of your building types has the same name. (COMPILING FAILED)";
                    }
                }
            }
            return "";
        }

        static List<string> UpdateSpaces(List<string> enumFields)
        {
            for (int i = 0; i < enumFields.Count; i++)
            {
                enumFields[i] = enumFields[i].Replace(" ", "_");
            }

            return enumFields;
        }

        public static void CompileAssembly(List<string> enumFields)
        {
            string duplicationCheck = CheckForDuplications(enumFields);

            if (duplicationCheck != "")
            {
                Debug.LogError("ERROR : " + duplicationCheck);
                return;
            }

            enumFields = UpdateSpaces(enumFields);

            CodeNamespace = new CodeNamespace();
            CodeNamespace.Imports.Add(new CodeNamespaceImport("System"));
            CodeNamespace.Imports.Add(new CodeNamespaceImport("System.Collections"));

            targetUnit = new CodeCompileUnit();
            targetUnit.Namespaces.Add(CodeNamespace);

            targetClass = new CodeTypeDeclaration("BuildingType");
            targetClass.IsEnum = true;
            targetClass.CustomAttributes.Add(new CodeAttributeDeclaration("Flags"));

            string currentEnumField;
            CodeMemberField enumField;

            for (int i = 0; i < enumFields.Count; i++)
            {
                currentEnumField = enumFields[i];

                enumField = new CodeMemberField("BuildingType", currentEnumField);
                enumField.InitExpression = new CodePrimitiveExpression(ReturnBitValue(targetClass.Members, i));
                targetClass.Members.Add(enumField);
            }

            CodeNamespace.Types.Add(targetClass);

            GenerateCSharpCode();
        }

        static int ReturnBitValue(CodeTypeMemberCollection members, int value)
        {
            int current = 0;
            CodeMemberField field;
            for (int i = 0; i < value; i++)
            {
                field = (CodeMemberField)members[i];
                current += (int)(((CodePrimitiveExpression)field.InitExpression).Value);
            }

            return current + 1;
        }

        public static void GenerateCSharpCode()
        {
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CodeGeneratorOptions options = new CodeGeneratorOptions();

            options.BlankLinesBetweenMembers = false;
            options.IndentString = "  ";

            using (var file = new StreamWriter(filePath))
            {
                provider.GenerateCodeFromCompileUnit(targetUnit, file, options);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            Debug.Log("COMPILING BUILDING TYPES SUCCEDED.");
        }

        public static List<string> LoadEnumData()
        {
            var data = Enum.GetNames(typeof(BuildingType)).ToList();

            for (int i = 0; i < data.Count; i++)
            {
                data[i] = data[i].Replace("_", " ");
            }

            return data;
        }

    }

}

#endif