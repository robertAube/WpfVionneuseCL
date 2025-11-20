using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirzaMediaPlayer.Models {
    internal class VideoInfo {
        private string videoName, videoFullPath;

        public string VideoName {
            get { return videoName; }
        }
        public string VideoFullPath {
            get { return videoFullPath; }
        }

        public VideoInfo(string name, string path) {
            videoName = name;
            videoFullPath = path;
        }
    }
}
