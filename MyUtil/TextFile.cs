using System.Collections.Generic;
using System.IO;

namespace MyUtil {
    internal class TextFile {
        public static List<string> readLineFromFile(string filePath) {
            string[] lignes;

            try {
                // Utiliser FileStream avec FileShare.Read pour permettre à d'autres processus d'accéder au fichier en même temps
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                   lignes = File.ReadAllLines(filePath);
                }
            }
            catch (IOException e) {
                throw new IOException("TextFile.readLineFromFile : listPaths est vide");
            }
            var logList = new List<string>(lignes);
            return logList;
        }

        public static List<string> readLineFromFileSansModePartage(string filePath) {
            string[] lignes;
            try {
                lignes = File.ReadAllLines(filePath);
            } catch {
                if (File.Exists(filePath)) {
                    File.Move(filePath, "temp");
                    File.Move("temp", filePath);
                    lignes = File.ReadAllLines(filePath);
                }
                else { 
                    lignes = new string[0];
                }
            }
            
            var logList = new List<string>(lignes);

            return logList;
        }
        public static List<string> readSignificativeLinesFromTextFile(string filePath) {
            List<string> lineFromFiles = readLineFromFile(filePath);
            List<string> significativeLinesFromTextFile = new List<string>();

            //  var replacedNames = logList.Select(x => x.Trim());
            foreach (string line in lineFromFiles) {
                var lineTrim = line.Trim();
                if (lineTrim.Length > 0) {
                    if (lineTrim[0] != '#') {
                        significativeLinesFromTextFile.Add(lineTrim);
                    }
                }
            }

            return significativeLinesFromTextFile;
        }
    }
}
