using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirzaMediaPlayer.MyUtil {
    internal class Util {
        /// <summary>
        /// Convertit un chemin relatif en chemin absolu en utilisant un chemin de base si nécessaire.
        /// </summary>
        /// <param name="basePath">Chemin de base (optionnel). Si null, utilise le répertoire courant.</param>
        /// <param name="inputPath">Chemin à convertir (relatif ou absolu).</param>
        /// <returns>Chemin absolu.</returns>
        public static string ConvertToAbsolutePath(string inputPath, string basePath = null) {
            if (string.IsNullOrWhiteSpace(inputPath))
                throw new ArgumentException("Le chemin ne peut pas être vide.", nameof(inputPath));

            // Si le chemin est déjà absolu, on le retourne tel quel
            if (Path.IsPathRooted(inputPath))
                return Path.GetFullPath(inputPath);

            // Si aucun basePath n'est fourni, on utilise le répertoire courant
            string effectiveBasePath = string.IsNullOrWhiteSpace(basePath) ? Directory.GetCurrentDirectory() : basePath;

            // Combine et résout le chemin complet
            string combinedPath = Path.Combine(effectiveBasePath, inputPath);
            return Path.GetFullPath(combinedPath);
        }
    }
}
