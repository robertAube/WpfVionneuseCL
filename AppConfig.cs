using MirzaMediaPlayer.MyUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirzaMediaPlayer {
    internal class AppConfig {
        public string excelMediaListPath { get; } //liste de fichiers média dans un fichier excel
        public string defaultVideoFullPath { get; } //vidéo qui joue par défaut si le média n'existe pas. 
        public string hiddenDirName { get; } //répertoire pour mieux cacher les fichier média sur l'ordinateur local. 

        public AppConfig():this(
            @".\A134.xlsm",  //excelMediaListPath
            @".\butiner.mp4", //defaultVideoFullPath
            @"2ikljst!mbhoeujy4a1iqv" //hiddenDirName
            ) {
        }
        public AppConfig(string mediaListPath, string defaultVideoFullPath, string hiddenDir) {
            this.excelMediaListPath = Util.ConvertToAbsolutePath(mediaListPath);
            this.defaultVideoFullPath = Util.ConvertToAbsolutePath(defaultVideoFullPath);
            this.hiddenDirName = hiddenDir;
        }
    }

}
