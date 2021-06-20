using CsvHelper;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Export
{
    public static class ExportCSV
    {
        public static void Export(List<ChampionStats> championStats, string path)
        {

            using (var writer = new StreamWriter(path))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(championStats);
                writer.Flush();
            }
        }
    }
}
