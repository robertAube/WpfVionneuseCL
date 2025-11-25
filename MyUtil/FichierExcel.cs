using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirzaMediaPlayer.MyUtil {
    internal class FichierExcel {
        
        public static List<string>  lireColonneExcel(string filePath, int noLigneDebut = 1, int noColonne = 1) { // A1 correspond à noLigne 1, noColonne 1
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            List<string> listeValeur = new List<string>();

            // Ouvrir le fichier
            using (var package = new ExcelPackage(new FileInfo(filePath))) {
                var worksheet = package.Workbook.Worksheets[0];

                int row = noLigneDebut; 
                while (!string.IsNullOrEmpty(worksheet.Cells[row, noColonne].Text)) // Colonne A = index 1
                {
                    string value = worksheet.Cells[row, 1].Text;
                    Console.WriteLine($"A{row} : {value}");
                    row++;
                }
            }

            return listeValeur;
        }
    }
}
