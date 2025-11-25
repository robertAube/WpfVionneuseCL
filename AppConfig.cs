using MirzaMediaPlayer.MyUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirzaMediaPlayer {
    internal class AppConfig {
        public string mediaListPath { get; } //liste fichier dans un fichier excel A1 à An
        public string defaultVideoFullPath { get; } //vidéo qui joue par défaut si le média n'existe pas. 

        public AppConfig():this(
            @".\A134.xlsm",  
            @".\butiner.mp4"
            ) {
        }
        public AppConfig(string mediaListPath, string defaultVideoFullPath) {
            this.mediaListPath = Util.ConvertToAbsolutePath(mediaListPath);
            this.defaultVideoFullPath = Util.ConvertToAbsolutePath(defaultVideoFullPath);
        }
    }

}
