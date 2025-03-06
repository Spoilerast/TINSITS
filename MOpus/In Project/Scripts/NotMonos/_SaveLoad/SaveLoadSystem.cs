using System;
using System.IO;
using Extensions;
using NotMonos.Databases;
using UnityEngine;

namespace NotMonos.SaveLoad
{
	internal sealed class SaveLoadSystem
	{
		private readonly string _defaultPath = Application.persistentDataPath;
		private const string FileName = "Save000.json";//todo make savefiles manager
		private bool _inProgress = false;

		public SaveLoadSystem()
			=> PeekLogger.Log($"Save Path is {_defaultPath}");

		internal void LoadSavefile()
			=> InvokeOnlyOnceAtTime(LoadJsonData);

		internal void SaveSavefile()
			=> InvokeOnlyOnceAtTime(SaveJsonData);

		private void InvokeOnlyOnceAtTime(Action method)
		{
			if (_inProgress)
				return;

			_inProgress = true;
			method.Invoke();
			_inProgress = false;
		}

		private bool LoadFromFile(out string result)
		{
			result = "";
			var fullPath = Path.Combine(_defaultPath, FileName);

			try
			{
				result = File.ReadAllText(fullPath);//todo make own class for IO
				return true;
			}
			catch (FileNotFoundException fnfex)
			{
				PeekLogger.LogWarning($"Save file not found. {fnfex.Message}");
				return false;
			}
			catch (Exception e)
			{
				PeekLogger.LogError($"Failed to read from {fullPath} with exception {e}");
				return false;
			}
		}

		private void LoadJsonData()
		{
			if (!LoadFromFile(out var json))
				return;

			SaveData sd = new(json);
			bool condition = sd.saveVersion == Constants.CurrentSaveVersion;
			if (PeekLogger.LogWarningForReturn(condition, "Save file version is not compatible. Load failed"))
				return;

			Processors.LoadProcessor adapter = new();
			adapter.EmbodySaveData(sd);
			PeekLogger.LogTabTab("Load successful");
		}

		private void SaveJsonData()
		{
			Processors.SaveProcessor adapter = new();
			SaveData save = adapter.CreateSaveData();
			if (WriteToFile(save.ToJson))
			{
				PeekLogger.LogTabTab("Save successful");
			}
		}

		private bool WriteToFile(string a_FileContents)
		{
			var fullPath = Path.Combine(_defaultPath, FileName);

			try
			{
				File.WriteAllText(fullPath, a_FileContents);
				return true;
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed to write to {fullPath} with exception {e}");
				return false;
			}
		}
	}
}