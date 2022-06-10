using System.IO;
using UnityEngine;

namespace ParrelSync
{
    /// <summary>
    /// Allows you to leave out some folders of the clone.
    /// It make a local copy of the ignored folders and create a symbolic link for all remaining files/folders
    /// </summary>
    public static class ClonesExcluder
    {
        /// <summary>
        /// Add paths to ignore here
        /// Path from source unity project folder ex: "Assets/Settings/PlayerSpecificSomething"
        /// </summary>
        private static string[] _foldersToIgnore =
        {
            //"Assets/Settings/PlayerSpecificSomething",
            "Sub1"
        };

        /// <summary>
        /// Adds full project path to _foldersToIgnore paths
        /// </summary>
        private static string[] _fullPathsToIgnore;
        
        public static bool IsPathIgnored(string sourcePath, string destinationPath)
        {
            //Fill _fullPathsToIgnore if not set yet
            if (_fullPathsToIgnore == null)
            {
                _fullPathsToIgnore = new string[_foldersToIgnore.Length];
                string projectPath = Application.dataPath.Replace("/Assets", "");
                projectPath = "C:\\Users\\tijme\\Desktop\\VRCadeRandom\\TestToCopyFrom";

                for (int i = 0; i < _foldersToIgnore.Length; i++)
                {
                    _fullPathsToIgnore[i] = Path.Combine(projectPath, _foldersToIgnore[i]);
                }    
            }
            
            //For each ignored path
            for (int i = 0; i < _fullPathsToIgnore.Length; i++)
            {
                //If this is the folder to ignore
                if (_fullPathsToIgnore[i] == sourcePath)
                {
                    //Copy the source folder but don't link it
                    CopyFilesRecursively(sourcePath, destinationPath);
                }
                //Else if sourcePath is part of one of the ignored directories
                //Create a local folder and try to link all sub directories and files
                else if (_fullPathsToIgnore[i].Contains(sourcePath))
                {
                    //Create empty local folder
                    Directory.CreateDirectory(destinationPath);
                    
                    LinkSubDirectories(sourcePath, destinationPath);

                    LinkSubFiles(sourcePath, destinationPath);

                    return true;
                }
            }
            return false;
        }
        
        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*",SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        private static void LinkSubDirectories(string sourcePath, string destinationPath)
        {
            var subDirs = Directory.GetDirectories(sourcePath);

            //For each subDir, link that subDir with the matching path in destinationPath
            for (int j = 0; j < subDirs.Length; j++)
            {
                //Get subDir destination path 
                string subDirDif = subDirs[j].Substring(subDirs[j].LastIndexOf("\\") + 1);
                string newDestPath = Path.Combine(destinationPath, subDirDif);

                //Try to link subDir folders
                ClonesManager.LinkFolders(subDirs[j], newDestPath);
            }
        }
        
        private static void LinkSubFiles(string sourcePath, string destinationPath)
        {
            var dirFiles = Directory.GetFiles(sourcePath);

            //For each file in this now not linked folder, link individualy
            for (int j = 0; j < dirFiles.Length; j++)
            {
                //Get subDir destination path 
                string dirFileDif = dirFiles[j].Substring(dirFiles[j].LastIndexOf("\\") + 1);
                string newDestPath = Path.Combine(destinationPath, dirFileDif);

                ClonesManager.LinkFiles(dirFiles[j], newDestPath);
            }
        }
    }
}