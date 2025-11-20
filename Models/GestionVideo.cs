using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirzaMediaPlayer.Models {
    internal class GestionVideo {
        private List<VideoInfo> videoInfos = new List<VideoInfo>();
        private PlayListContainer _playListContainer;

        public GestionVideo(PlayListContainer playListContainer) {
            videoInfos.Add(getVideo("question 1", @"C:\!ra\204H25- projets\finalV2\ProjetIntegrateur-Equipe1\ProjetIntegrateur_Equipe1\Library\PackageCache\com.unity.timeline@1.7.6\Samples~\Customization\Demo\Videos\M30-1317.mp4"));
            videoInfos.Add(getVideo("question 2", @"C:\!ra\204H25- projets\finalV2\ProjetIntegrateur-Equipe1\ProjetIntegrateur_Equipe1\Library\PackageCache\com.unity.timeline@1.7.6\Samples~\Customization\Demo\Videos\M30-1356.mp4"));
            videoInfos.Add(getVideo("formatif", @"C:\!ra\C#Wpf\MirzaMediaPlayer\!fichiers\DémoFormatif11.mkv"));
            _playListContainer = playListContainer;
            ajouterVideosDansListe();
        }

        private void ajouterVideosDansListe() {
            foreach (VideoInfo videoInfo in videoInfos) {
                FileInfo fi = new FileInfo(videoInfo.VideoFullPath);
                PlayList newList = new PlayList
                {
                    Name = videoInfo.VideoName,
                    FullName = videoInfo.VideoFullPath
                };
                if (fi.Extension.ToLower().Contains("mp3")) {
                    newList.Icon = @"Icons\Music.ico";
                }
                else if (fi.Extension.ToLower().Contains("mp4") || fi.Extension.ToLower().Contains("3gp")) {
                    newList.Icon = @"Icons\Video.ico";
                }
                _playListContainer.PlayListData.Add(newList);
            }
        }

        private VideoInfo getVideo(string name, string filePath) {
            VideoInfo videoInfo;
            string defFilePath = @"C:\!ra\C#Wpf\MirzaMediaPlayer\!fichiers\M09-1317.mp4";

            if (File.Exists(filePath)) {
                string[] videoExtensions = { ".mp4", ".avi", ".mkv", ".mov", ".wmv" };
                string extension = Path.GetExtension(filePath).ToLower();

                if (Array.Exists(videoExtensions, ext => ext == extension)) {
                    videoInfo = new VideoInfo(name, filePath);
                }
                else {
                    videoInfo = new VideoInfo("LE fichier n'est pas une vidéo", defFilePath);
                }
            }
            else {
                videoInfo = new VideoInfo("Le fichier n'existe pas", defFilePath);
            }
            return videoInfo;
        }
    }
}

