using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;

namespace XECrawler
{
    public static class ExcelExport
    {
        public static string ExportToFile(List<PropertyModel> properties, UrlModel urlModel)
        {
            string filePath = ConfigurationManager.AppSettings["filePath"];
            string dateStamp = DateTime.Now.ToString("yyyyMMddHmmssfff");
            string fileName = @"\XECrawler" + urlModel.Name + dateStamp + @".xlsx";
            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("Πωλήσεις");
                ws.Cells[1, 1].Value = "ID";
                ws.Cells[1, 2].Value = "Τύπος";
                ws.Cells[1, 3].Value = "Τοποθεσία1";
                ws.Cells[1, 4].Value = "Τοποθεσία2";
                ws.Cells[1, 5].Value = "Όροφος";
                ws.Cells[1, 6].Value = "Τετραγωνικά";
                ws.Cells[1, 7].Value = "Τιμή";
                ws.Cells[1, 8].Value = "Έτος κατασκευής";
                ws.Cells[1, 9].Value = "Υπνοδωμάτια";
                ws.Cells[1, 10].Value = "Toilets";
                ws.Cells[1, 11].Value = "Parking";
                ws.Cells[1, 12].Value = "Τζάκι";
                ws.Cells[1, 13].Value = "Όνομα";
                ws.Cells[1, 14].Value = "Τηλέφωνο";
                ws.Cells[1, 15].Value = "Περιγραφή";
                ws.Cells[1, 16].Value = "Αυτόνομη Θέρμανση";
                for (int j = 1; j <= 16; j++)
                {
                    Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#64b446");
                    ws.Cells[1, j].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[1, j].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    ws.Cells[1, j].Style.Font.Color.SetColor(Color.White);
                }

                int i = 2;
                foreach (var property in properties)
                {
                    ws.Cells[i, 1].Value = property.Id;
                    ws.Cells[i, 2].Value = property.PropertyType;
                    ws.Cells[i, 3].Value = property.Locationp1;
                    ws.Cells[i, 4].Value = property.Locationp2;
                    ws.Cells[i, 5].Value = property.Floor;
                    ws.Cells[i, 6].Value = property.SqMeteters;
                    if (property.Price == 0)
                    {
                        ws.Cells[i, 7].Value = "";
                    }
                    else
                    {
                        ws.Cells[i, 7].Value = property.Price;
                    }
                    ws.Cells[i, 7].Style.Numberformat.Format = "###,### €";
                    if (property.Year == 0)
                    {
                        ws.Cells[i, 8].Value = "";
                    }
                    else
                    {
                        ws.Cells[i, 8].Value = property.Year;
                    }
                    if (property.Bedroms == 0)
                    {
                        ws.Cells[i, 9].Value = "";
                    }
                    else
                    {
                        ws.Cells[i, 9].Value = property.Bedroms;
                    }
                    ws.Cells[i, 10].Value = property.Toilets;
                    ws.Cells[i, 11].Value = property.Parking;
                    ws.Cells[i, 12].Value = property.Fireplace;
                    ws.Cells[i, 13].Value = "";
                    ws.Cells[i, 14].Value = property.Phone;
                    ws.Cells[i, 15].Value = property.Description;
                    ws.Cells[i, 16].Value = property.AutonomousHeat;
                    i++;
                }

                ws.Cells.AutoFitColumns(0.00, 50.00);

                string path = filePath + fileName;
                
                try
                {
                    using (Stream stream = File.Create(path))
                    {
                        package.SaveAs(stream);
                        Console.WriteLine("Excel file created.");
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Exception message: {ex.Message}, Inner Exception: {ex.InnerException}");
                }
                return fileName;
            }
        }
    }
}
