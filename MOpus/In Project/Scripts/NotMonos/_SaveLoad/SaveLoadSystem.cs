using System;
using System.IO;
using Extensions;
using NotMonos.Databases;
using NotMonos.Processors;
using UnityEngine;

namespace NotMonos.SaveLoad
{
internal abstract class SaveLoadSystem
{
	private const string FileName = "Save000.json"; //todo make savefiles manager
	private static bool _inProgress;
	public static string DefaultSavePath => Application.persistentDataPath;
	internal static void LoadSavefile() { InvokeOnlyOnceAtTime(LoadJsonData); }

	internal static void SaveSavefile() { InvokeOnlyOnceAtTime(SaveJsonData); }

	private static void InvokeOnlyOnceAtTime(Action method)
	{
		if (_inProgress)
			return;

		_inProgress = true;
		method.Invoke();
		_inProgress = false;
	}

	private static bool LoadFromFile(out string result)
	{
		result = "";
		string fullPath = Path.Combine(DefaultSavePath, FileName);

		try{
			result = File.ReadAllText(fullPath); //todo make own class for IO
			return true;
		}
		catch (FileNotFoundException fileNotFoundException){
			PeekLogger.LogWarning($"Save file not found. {fileNotFoundException.Message}");
		}
		catch (Exception e){
			PeekLogger.LogError($"Failed to read from {fullPath} with exception {e}");
		}

		return false;
	}

	private static void LoadJsonData()
	{
		if (!LoadFromFile(out string json))
			return;

		SaveData sd = new(json);
		bool condition = sd.saveVersion == Constants.CurrentSaveVersion;
		if (PeekLogger.LogWarningForReturn(condition, "Save file version is not compatible. Load failed"))
			return;

		LoadProcessor.EmbodySaveData(sd);
		//PeekLogger.ClearLog();
		PeekLogger.LogTabTab("Load successful");
	}

	private static void SaveJsonData()
	{
		SaveData save = SaveProcessor.CreateSaveData();
		if (WriteToFile(save.ToJson))
			PeekLogger.LogTabTab("Save successful");
	}

	private static bool WriteToFile(string fileContents)
	{
		string fullPath = Path.Combine(DefaultSavePath, FileName);

		try{
			File.WriteAllText(fullPath, fileContents);
			return true;
		}
		catch (Exception e){
			Debug.LogError($"Failed to write to {fullPath} with exception {e}");
			return false;
		}
	}
}
}