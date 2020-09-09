using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HttpServer
{
    
    enum Separator
    {
        Comma = 1,
        Semicolon = 2,
        Tab = 3    
    }

    public static class ProcessCSVFile
    {
        private static List<List<string>> CSVValues = new List<List<string>>();
        private static List<string> CSVHeaders;
        private static int ColumnIndex { get; set; }
        public static char Delimiter {get; set;}

        public static char FindCSVSeparator(string filePath)
        {
            int ccounter = 0, scounter = 0, tcounter = 0;
            using (StreamReader sr = new StreamReader(filePath))
            {
                string line;
                int lineCount = 0;
                while ((line = sr.ReadLine()) != null && lineCount < 1000000)
                {
                    lineCount += 1;
                    var chars = line.ToCharArray();
                    foreach (var ch in chars)
                    {
                        int c = (ch == ',') ? 1 : (ch == ';') ? 2 : (ch == '\t') ? 3 : 4;
                        switch (c)
                        {
                            case (int )Separator.Comma:
                                ccounter += 1;
                                break;
                            case (int) Separator.Semicolon:
                                scounter += 1;
                                break;
                            case (int) Separator.Tab:
                                tcounter += 1;
                                break;
                            default:
                                break;                            
                        }
                    }      
                }
                sr.Close();
                sr.Dispose();
            }
            var separator = (ccounter > scounter) ? ',' : (scounter > tcounter) ? ';' : '\t';
            return separator;
        }



        public static List<List<string>> SortCSVFile(string filePath, string column)
        {
            Delimiter = FindCSVSeparator(filePath);
            FileStream fs = File.Open(filePath, FileMode.Open);
            TextReader sr = new StreamReader(fs);
           
            var csvList = new List<List<string>>();
            string line = string.Empty;
            bool Isfirstline = true;
            while ((line = sr.ReadLine()) != null)
            {
                if (Isfirstline)
                {
                    CSVHeaders = SplitCSVLine(line, Delimiter );
                    Isfirstline = false;
                }
                else
                {
                    List<string> lineDetails = SplitCSVLine(line, Delimiter);
                    CSVValues.Add(lineDetails);
                }
            }
            sr.Dispose();
            fs.Dispose();

            if (AreColumnMembersEqual(column))
            {
                csvList = CSVValues.OrderBy(p => p[ColumnIndex]).ToList();
                csvList.Insert(0, CSVHeaders);
            }

            else
            {
                List<int> indeXList = new List<int>();
                for (int i = 0; i < CSVHeaders.Count(); i++)
                {
                    if (CSVHeaders[i].ToLower().Trim() != column.ToLower())
                        indeXList.Add(i);
                    csvList = CSVValues.OrderBy(p => p[0]).ToList();

                    for (int j = 1; j < indeXList.Count(); j++)
                        csvList = csvList.OrderBy(p => p[j]).ToList();
                    csvList.Insert(0, CSVHeaders);
                }

            }
            return csvList;
        }


        public static string ListToCSVString(List<List<string>> csvLines, char delimiter /*string filePath*/)
        {
            string joinedCSVLine = Environment.NewLine;
            char separator = delimiter; 
            foreach(var csvLine in csvLines)
            {
                joinedCSVLine += String.Join(separator.ToString(), csvLine.ToArray<string>());
                joinedCSVLine += Environment.NewLine;
            }
            return joinedCSVLine.Substring(1);  
        }



        private static List<string> SplitCSVLine(string Row , char delimiter )
        {
            char separator = Delimiter; 
            List<string> csvLine = new List<string>();
            string[] splitRow = Row.Split(delimiter);
            csvLine = splitRow.ToList<string>();
            return csvLine;
        }


        private static bool GetColumnIndex(string column)
        {
            bool check = false;
            for (int i = 0; i < CSVHeaders.Count(); i++)
            {
                if (CSVHeaders[i].ToLower().Trim() == column.ToLower())  
                {
                    ColumnIndex = i;
                    check = true;
                    return check;
                }                
            }
            return check;
        }


        private static bool AreColumnMembersEqual(string column)
        {
            List<string> columnValaues = new List<string>();
            var c = GetColumnIndex(column);
            foreach (var csvline in CSVValues)
            {
             columnValaues.Add(csvline[ColumnIndex]);
            }
            if(columnValaues.Distinct().ToList().Count() > 0)
            {
                return true;
            }

            return false;
        }

    }
}

