using UnityEngine.UIElements;

namespace UnityEditor;

public sealed class EditorGUIUtility
{
	internal static void PingObject<Behaviour>(Behaviour script) where Behaviour : UnityEngine.Behaviour
	{
		throw new NotImplementedException();
	}
}

internal class Editor
{
}

public sealed class AssetDatabase
{
	internal static string GetAssetPath(VisualTreeAsset assetToAnalyze)
	{
		throw new NotImplementedException();
	}
}