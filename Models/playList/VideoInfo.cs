using MirzaMediaPlayer.MyUtil;


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
            videoFullPath = Util.ConvertToAbsolutePath(path);
        }

    }
}
