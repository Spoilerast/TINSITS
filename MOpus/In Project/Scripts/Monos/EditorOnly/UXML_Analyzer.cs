using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Monos.EditorSystems
{
	internal sealed class UXML_Analyzer : EditorSystem
	{
		private const string EnumsFolderName = "Enums";
		private const string EnumFileStructureStart = "namespace NotMonos.UI.Enums{\n\tpublic enum ";
		private const string FileExtensionUXML = ".uxml";
		private const string FileExtensionClass = ".cs";
		private const string NameAttribute = "name";

		[field: SerializeField,
			ContextMenuItem("Generate file from all Visual Element names in UXML", nameof(GenerateNamesFile))]
		internal VisualTreeAsset AssetToAnalyze { get; private set; }

		private void OnValidate()
		{
			_ = this.IsFieldValueNull(AssetToAnalyze);
		}

		[ContextMenu("Generate file from all Visual Element names in UXML")]
		private void GenerateNamesFile()
		{
			if (PeekLogger.LogWarningForReturn(AssetToAnalyze, "Asset was not setted"))			
				return;

			FindAssetFolder(out var assetPath, out var assetFolder);
			string enumsPath = GetPathForEnums(ref assetFolder);

			IEnumerable<string> names = AnalyzeUXML(ref assetPath);
			if (PeekLogger.LogWarningForReturn(names.Any(),
				"Selected file does not contains any VisualElement with setted 'name' property!"))
				return;			

			ConvertNamesToEnumFile(ref assetPath, ref enumsPath, names);
		}

		private static void ConvertNamesToEnumFile(ref string assetPath, ref string enumsPath, IEnumerable<string> names)
		{
			string enumName = Path.GetFileName(assetPath).Replace(FileExtensionUXML, "");
			string content = CreateEnumFileContent(names, ref enumName);
			string enumsFilePath = Path.Combine(enumsPath, enumName + FileExtensionClass);
			try
			{
				if (!Directory.Exists(enumsPath))
					_ = Directory.CreateDirectory(enumsPath);
				File.WriteAllText(enumsFilePath, content);
			}
			catch (UnauthorizedAccessException uaex)
			{
				PeekLogger.LogError(uaex.Message);
			}

			PeekLogger.Log($"File created: {enumsFilePath}");
		}

		private static string CreateEnumFileContent(IEnumerable<string> names, ref string enumName)
		{
			const string separator = ", ";
			string inheritance = names.Count() < 255
				? "byte"
				: "ushort";
			StringBuilder builder = new();
			_ = builder.Append(EnumFileStructureStart)
					.Append(enumName)
					.Append(" : ")
					.Append(inheritance)
					.Append("{ ");
			foreach (var item in names)
			{
				_ = builder.Append(item)
					.Append(separator);
			}
			_ = builder.Remove(builder.Length - separator.Length, separator.Length)
					.Append(" }\n}");
			return builder.ToString();
		}

		private static string GetPathForEnums(ref string assetFolder)
			=> Path.Combine(assetFolder, EnumsFolderName);

		private static IEnumerable<string> AnalyzeUXML(ref string assetPath)
		{
			XmlDocument xmlDoc = new();
			xmlDoc.Load(assetPath);

			return AnalyzeNode(xmlDoc.DocumentElement);
		}

		private static IEnumerable<string> AnalyzeNode(XmlElement node)
		{
			if (node.Attributes?[NameAttribute] != null)
				yield return node.Attributes[NameAttribute].Value;

			foreach (var name in from XmlElement childNode in node.ChildNodes
								 from name in AnalyzeNode(childNode)
								 select name)
				yield return name;
		}

		private void FindAssetFolder(out string assetPath, out string assetFolder)
		{
			assetPath = AssetDatabase.GetAssetPath(AssetToAnalyze);
			assetFolder = Path.GetDirectoryName(assetPath);
		}
	}
}