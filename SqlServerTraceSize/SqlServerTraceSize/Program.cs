using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System.Data;

namespace SqlServerTraceSize
{

    public class DatabaseFile
    {
        public string LogicalFileName { get; set; }
        public string FileType { get; set; }
        public int TotalSizeMB { get; set; }
        public int FreeSpaceMB { get; set; }
        public int UsedSpaceMB => TotalSizeMB - FreeSpaceMB;

    }


    internal class Program
    {
        static void Main(string[] args)
        {
            int k = 5;

            var configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .Build();

            string sqlConnString = configuration.GetConnectionString("SqlServerConnString");
                       
            string excelFileName = "output.xlsx";

            if (File.Exists(excelFileName))
            {
                File.Delete(excelFileName);
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var excelPackage = new ExcelPackage(excelFileName))
            {
                var sheet = excelPackage.Workbook.Worksheets.Add("My Sheet");

                sheet.Cells["A1"].Value = DateTime.UtcNow;
                sheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet.Cells["A1"].Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";

                // Excel headers
                var firstData = GetData(sqlConnString);
                {
                    for (int i = 0; i < firstData.Count; i++)
                    {
                        sheet.Cells[1, 1 + (i * k) + 1].Value = firstData[i].LogicalFileName;
                        sheet.Cells[2, 1 + (i * k) + 1].Value = "Type";
                        sheet.Cells[2, 1 + (i * k) + 2].Value = "TotalMB";
                        sheet.Cells[2, 1 + (i * k) + 3].Value = "FreeMB";
                        sheet.Cells[2, 1 + (i * k) + 4].Value = "UsedMB";
                        sheet.Cells[2, 1 + (i * k) + 5].Value = "DeltaMB";
                    }

                    for (int i = 1; i <= (1 + 3 * firstData.Count); i++)
                    {
                        //sheet.Column(i).AutoFit();
                    }

                    sheet.Column(1).AutoFit();
                }

                // Data
                int baseRow = 3;
                int excelRowNumber = baseRow;

                while (true)
                {
                    Console.Write("Getting data... ");
                    var data = GetData(sqlConnString);

                    sheet.Cells[excelRowNumber, 1].Value = DateTime.UtcNow;
                    sheet.Cells[excelRowNumber, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells[excelRowNumber, 1].Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";

                    for (int i = 0; i < data.Count; i++)
                    {
                        Console.Write($" {data[i].LogicalFileName}={data[i].UsedSpaceMB} ");                        

                        sheet.Cells[excelRowNumber, 1 + (i * k) + 1].Value = data[i].FileType;
                        sheet.Cells[excelRowNumber, 1 + (i * k) + 2].Value = data[i].TotalSizeMB;
                        sheet.Cells[excelRowNumber, 1 + (i * k) + 3].Value = data[i].FreeSpaceMB;
                        sheet.Cells[excelRowNumber, 1 + (i * k) + 4].Value = data[i].UsedSpaceMB;
                        sheet.Cells[excelRowNumber, 1 + (i * k) + 5].Value = 0;

                        if (excelRowNumber > baseRow)
                        {
                            string columnLetter = OfficeOpenXml.ExcelCellAddress.GetColumnLetter(1 + (i * k) + 4);
                            sheet.Cells[excelRowNumber, 1 + (i * k) + 5].Formula = $"{columnLetter}{excelRowNumber} - ${columnLetter}${baseRow}";
                        }
                    }

                    Console.WriteLine();

                    if (Console.KeyAvailable)
                    {
                        break;
                    }

                    excelRowNumber++;

                    Thread.Sleep(1000);
                }

                excelPackage.Save();
            }

        }



        private static List<DatabaseFile> GetData(string sqlConnString)
        {
            var q = @"SELECT
                        name AS LogicalFileName, 
                        type_desc as FileType,
                        size/128 AS TotalSizeMB,  
                        size/128 - CAST(FILEPROPERTY(name, 'SpaceUsed') AS INT)/128 AS FreeSpaceMB
                      FROM sys.database_files";

            var databaseFiles = new List<DatabaseFile>();

            using (var sqlConn = new SqlConnection(sqlConnString))
            {
                sqlConn.Open();
                var sqlCmd = sqlConn.CreateCommand();
                sqlCmd.CommandText = q;
                var reader = sqlCmd.ExecuteReader();
                while (reader.Read())
                {
                    var databaseFile = new DatabaseFile
                    {
                        LogicalFileName = reader.GetString(0),
                        FileType = reader.GetString(1),
                        TotalSizeMB = reader.GetInt32(2),
                        FreeSpaceMB = reader.GetInt32(3)
                    };
                    databaseFiles.Add(databaseFile);
                }
            }

            return databaseFiles;
        }

    }
}
