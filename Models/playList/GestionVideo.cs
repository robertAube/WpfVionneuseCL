using MirzaMediaPlayer.MyUtil;
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
            chargerMedia();
            _playListContainer = playListContainer;
            ajouterVideosDansListe();
        }

        private void chargerMedia() {
            List<string> listePath = new List<string>();
            List<string> listeNom = new List<string>();

            try {
                listeNom = FichierExcel.lireColonneExcel(MainWindow.AppConfig.mediaListPath, 1, 1);
                listePath = FichierExcel.lireColonneExcel(MainWindow.AppConfig.mediaListPath, 1, 2);
            }
            catch (Exception ex) {
                videoInfos.Add(getVideo("erreur fichier excel", MainWindow.AppConfig.defaultVideoFullPath));
            }
            int i = 0;
            foreach (string nom in listeNom) {
                videoInfos.Add(getVideo(nom, listePath[i++]));
            }

            //    videoInfos.Add(getVideo("question 1", @"..\..\!fichiers\butiner.mp4"));
            //videoInfos.Add(getVideo("question 2", @"..\..\!fichiers\M30-1356.mp4"));
            //videoInfos.Add(getVideo("formatif", @"..\..\!fichiers\DémoFormatif11.mkv"));



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
                else if (fi.Extension.ToLower().Contains("mp4") || fi.Extension.ToLower().Contains("3gp") || fi.Extension.ToLower().Contains("mkv")) {
                    newList.Icon = @"Icons\Video.ico";
                }
                _playListContainer.PlayListData.Add(newList);
            }
        }

        private VideoInfo getVideo(string name, string filePath) {
            VideoInfo videoInfo;
            string defFilePath = MainWindow.AppConfig.defaultVideoFullPath;

            if (File.Exists(filePath)) {
                string[] videoExtensions = { ".mp4", ".avi", ".mkv", ".mov", ".wmv" };
                string extension = Path.GetExtension(filePath).ToLower();

                if (Array.Exists(videoExtensions, ext => ext == extension)) {
                    videoInfo = new VideoInfo(name, filePath);
                }
                else {
                    videoInfo = new VideoInfo("Le fichier n'est pas une vidéo", defFilePath);
                }
            }
            else {
                videoInfo = new VideoInfo("Le fichier n'existe pas", defFilePath);
            }
            return videoInfo;
        }
    }
}

