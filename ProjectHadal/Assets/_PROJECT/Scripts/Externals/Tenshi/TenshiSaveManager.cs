using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Tenshi.UnitySoku;
using UnityEngine;

namespace Tenshi.SaveHigan
{
    public static class SaveManager
    {
        private static readonly string RootSaveFolderName = "/saves/";
        private static readonly string PersistentSavePath = Application.persistentDataPath + RootSaveFolderName;
        private static readonly string Suffix = ".tenshi";

        #region Saving & Loading

        private static class Formatter
        {
            private static readonly BinaryFormatter Binary = new BinaryFormatter();
            public static bool Serialize(Stream stream, object data)
            {
                bool status = false;
                try
                {
                    Binary.Serialize(stream, data);
                    status = true;
                }
                catch
                {
                    status = false;
                }
                return status;
            }
            public static T Deserialize<T>(Stream stream)
            {
                T value = default(T);
                try
                {
                    value = (T)Binary.Deserialize(stream);
                }
                catch
                {
                }
                return value;
            }
        }
        private static T HandleNoSaveException<T>(string exception = "")
        {
            if (string.IsNullOrWhiteSpace(exception))
                $"The root save path [{PersistentSavePath}] does not exist, please save first.".Msg();
            else
                exception.Msg();
            
            return default(T);
        }

        //! Sync
        public static bool Save<T>(T data, string pathKey) => InternalSave<T>(data, pathKey);
        public static T Load<T>(string pathKey, string customNoSaveException = "") => InternalLoad<T>(pathKey, customNoSaveException);
        private static bool InternalSave<T>(T data, string pathKey, bool emptyFile = false)
        {
            string path = PersistentSavePath;
            Directory.CreateDirectory(path);

            bool status = false;
            using FileStream stream = new FileStream(path + pathKey + Suffix, FileMode.Create, FileAccess.Write);
            if (!emptyFile)
            {
                try
                {
                    Formatter.Serialize(stream, data);
                    status = true;
                    $"Save success for /{pathKey}{Suffix}".Msg();
                }
                catch (SerializationException ex)
                {
                    $"Save fail error: {ex}".Error();
                    status = false;
                }
            }
            else status = true;
            return status;
        }
        private static T InternalLoad<T>(string pathKey, string customNoSaveException = "")
        {
            string rootPath = PersistentSavePath;
            string fullPath = GetFullPathFromKey(pathKey);
            if (!IsFileKeyExistent(pathKey)) return HandleNoSaveException<T>(customNoSaveException);

            T value = default(T);
            using FileStream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            try
            {
                value = Formatter.Deserialize<T>(stream);
                $"Load success for /{pathKey}{Suffix}".Msg();
            }
            catch (SerializationException ex)
            {
                $"Load fail error: {ex.Message}".Error();
            }
            return value;
        }

        //! Async
        public static async Task<bool> SaveAsync<T>(T data, string pathKey) => await InternalSaveAsync<T>(data, pathKey);
        public static async Task<T> LoadAsync<T>(string pathKey, string customNoSaveException = "") => await InternalLoadAsync<T>(pathKey, customNoSaveException);
        private static async Task<bool> InternalSaveAsync<T>(T data, string pathKey)
        {
            string rootPath = PersistentSavePath;
            string fullPath = GetFullPathFromKey(pathKey);
            Directory.CreateDirectory(rootPath);
            return await Task.Run(() =>
            {
                using FileStream stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
                try
                {
                    Formatter.Serialize(stream, data);
                    $"Save Async success for /{fullPath}".Msg();
                    return true;
                }
                catch (SerializationException ex)
                {
                    $"Save Async fail error: {ex}".Error();
                    return false;
                }
            });
        }
        private static async Task<T> InternalLoadAsync<T>(string pathKey, string customNoSaveException = "")
        {
            string rootPath = PersistentSavePath;
            string fullPath = GetFullPathFromKey(pathKey);
            if (!IsFileKeyExistent(pathKey)) return await Task.Run(() => HandleNoSaveException<T>(customNoSaveException));

            return await Task.Run(() =>
            {
                using FileStream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                try
                {
                    T value = Formatter.Deserialize<T>(stream);
                    $"Load Async success for /{pathKey}{Suffix}".Msg();
                    return value;
                }
                catch (SerializationException ex)
                {
                    $"Load Async fail error: {ex.Message}".Error();
                    return default(T);
                }
            });
        }

        #endregion

        #region I/O Query & Mutator

        public static bool IsFileKeyExistent(string pathKey) => File.Exists(GetFullPathFromKey(pathKey));
        public static bool IsFullFilePathExistent(string fullPath) => File.Exists(fullPath);
        private static string GetFullPathFromKey(string pathKey) => PersistentSavePath + pathKey + Suffix;

        public static bool RecreateFileOrDirectory(string pathKey)
        {
            try
            {
                string dirPath = PersistentSavePath + pathKey;
                DirectoryInfo directory = new DirectoryInfo(dirPath);
                if (directory.Exists)
                {
                    directory.Delete(true);
                    Directory.CreateDirectory(dirPath);
                    return true;
                }

                string fullPath = PersistentSavePath + pathKey + Suffix;
                if (IsFullFilePathExistent(fullPath))
                {
                    File.Delete(fullPath);
                    InternalSave<object>(null, pathKey, true);
                    return true;
                }
            }
            catch (IOException ex)
            {
                ex.Error();
            }
            return false;
        }

        public static bool RecreateRootSaveDirectory()
        {
            bool status = false;
            try
            {
                string path = PersistentSavePath;
                if (DeleteRootSaveDirectory())
                    Directory.CreateDirectory(path);
                status = true;
            }
            catch (IOException ex)
            {
                ex.Error();
                status = false;
            }
            return status;
        }

        public static bool DeleteFileOrDirectory(string pathKey)
        {
            try
            {
                string dirPath = PersistentSavePath + pathKey;
                DirectoryInfo directory = new DirectoryInfo(dirPath);
                if (directory.Exists)
                {
                    directory.Delete(true);
                    return true;
                }

                string fullPath = PersistentSavePath + pathKey + Suffix;
                if (IsFullFilePathExistent(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
            }
            catch (IOException ex)
            {
                ex.Error();
            }
            return false;
        }

        public static bool DeleteRootSaveDirectory()
        {
            bool status = false;
            try
            {
                string path = PersistentSavePath;
                DirectoryInfo directory = new DirectoryInfo(path);
                if (directory.Exists)
                {
                    directory.Delete(true);
                    status = true;
                }
            }
            catch (IOException ex)
            {
                ex.Error();
                status = false;
            }
            return status;
        }

        #endregion

        #region Save/Load Events

        public static event Action SaveGameEvent;
        public static event Action LoadGameEvent;
        public static event Action<bool> SetActiveSaveIDsEvent;
        public static void OnSaveGameInvoke() => SaveGameEvent?.Invoke();
        public static void OnLoadGameInvoke() => LoadGameEvent?.Invoke();
        public static void OnSetActiveSaveIDsInvoke(bool state) => SetActiveSaveIDsEvent?.Invoke(state);

        #endregion
    }
}