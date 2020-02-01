using System;
using System.IO;
using System.Linq;
using UnityEngine;
using SongCore;

namespace MassSongRemover
{
    internal static class SongRemover
    {
        private static PlayerData _playerData;
        private static PlayerData PlayerData
        {
            get
            {
                if (_playerData == null)
                {
                    var playerDataModelSO = Resources.FindObjectsOfTypeAll<PlayerDataModelSO>().FirstOrDefault();
                    _playerData = playerDataModelSO?.playerData;
                }

                return _playerData;
            }
        }

        public static readonly string RemovedFolder = Path.Combine(Environment.CurrentDirectory, "RemovedSongs");
        public static readonly string BeatSaberDataFolder = Path.Combine(Environment.CurrentDirectory, "Beat Saber_Data");

        public static void DeleteNonFavouritedSongs()
        {
            var allCustomSongs = Loader.CustomLevelsCollection.beatmapLevels;
            var nonFavouritedSongs = allCustomSongs.Where(level => !PlayerData.IsLevelUserFavorite(level) && level is CustomPreviewBeatmapLevel).Select(level => level as CustomPreviewBeatmapLevel);

            Logger.log.Info($"Moving {nonFavouritedSongs.Count()} songs out of {allCustomSongs.Length} to '{RemovedFolder}'");

            foreach (var song in nonFavouritedSongs)
            {
                DirectoryInfo dirInfo;
                try
                {
                    dirInfo = new DirectoryInfo(song.customLevelPath);

                    if (!dirInfo.Exists)
                    {
                        Logger.log.Warn($"Unable to remove song '{song.songName}' (invalid directory?)");
                        continue;
                    }
                }
                catch (ArgumentException)
                {
                    Logger.log.Warn($"Unable to remove song '{song.songName}' (invalid directory?)");
                    continue;
                }

                if (!dirInfo.FullName.StartsWith(BeatSaberDataFolder))
                {
                    Logger.log.Warn($"Unable to remove song '{song.songName}' (could not find 'Beat Saber_Data' ancestor folder)");
                    continue;
                }

                string locationToMove = dirInfo.Parent.FullName.Replace(BeatSaberDataFolder, RemovedFolder);

                if (!Directory.Exists(locationToMove))
                    Directory.CreateDirectory(locationToMove);

                try
                {
                    dirInfo.MoveTo(Path.Combine(locationToMove, dirInfo.Name));
                }
                catch (IOException)
                {
                    Logger.log.Warn($"Song '{song.songName}' already exists in the removed folder (permanently deleting)");

                    try
                    {
                        dirInfo.Delete(true);
                    }
                    catch (IOException)
                    {
                        Logger.log.Warn($"Unable to permanently delete song '{song.songName}'");
                        continue;
                    }
                }
            }

            Logger.log.Info($"Finished moving songs to '{RemovedFolder}'");
        }
    }
}
