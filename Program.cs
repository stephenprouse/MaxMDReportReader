using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
namespace MaxMDReportReader
{
    class Program
    {
        const string UNIQUE_ID_KEY = "XDR Unique ID";
        const string PATIENT_ID_KEY = "XDR Patient ID";
        const string BEGIN_KEY = "-----BEGIN MESSAGE LOG DATA-----";
        const string END_KEY = "-----END MESSAGE LOG DATA-----";
        const string SPLIT_BY = ",";
        const string LINE_BREAK = "\n";
        const string SEQUENCE_ID_PATTERN = @"2\.16\.840\.1\.113883\.3\.2011\.001\.7\.([0-9]+)\.[0-9]+$";
        const string PATIENT_NUMBER_PATTERN = @"([0-9a-zA-Z\-_\|]+)\^\^\^\&2\.16\.840\.1\.113883\.3\.2011\.001\.2\.1\.1\&ISO";
                                             
        static void Main(string[] args)
        {
            string inputFile = @"C:\Users\sprouse.UCHS_DOM1\Downloads\Message_Status_direct.uchs.org_2015-10-01_2015-10-31.csv";
            string outputFile = @"C:\users\sprouse.UCHS_DOM1\desktop\output1.csv";
            processCSVReport(inputFile, outputFile);
            Console.WriteLine("Report generated to: " + outputFile);
            Console.ReadKey();
        }

        static void processCSVReport(string reportFile, string outputFile)
        {
            string[][] csv = readCSV(reportFile);
            string[] headers = csv[0];
            int uniqueIdIndex = 0;
            int patientIdIndex = 0;
            bool isDataRow = false;
            Regex sequenceReg = new Regex(SEQUENCE_ID_PATTERN, RegexOptions.IgnoreCase);
            Regex patientReg = new Regex(PATIENT_NUMBER_PATTERN, RegexOptions.IgnoreCase);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < headers.Length; i++)
            {
                
                sb.AppendFormat("\"{0}\"{1}", headers[i], SPLIT_BY);
                if (headers[i].Trim().Equals(UNIQUE_ID_KEY))
                {
                    uniqueIdIndex = i; 
                }
                if (headers[i].Trim().Equals(PATIENT_ID_KEY))
                {
                    patientIdIndex = i;
                }
            }
            sb.AppendFormat("\"Sequence ID\"{0}\"Patient Number\"{1}", SPLIT_BY, LINE_BREAK); 

            for (int i = 1; i < csv.Length; i++)
            {
                string uniqueId = "";
                string patientId = "";
                string sequenceId = "";
                string patientNumber = "";
                StringBuilder row = new StringBuilder();
                
                if (isDataRow)
                {
                    if (csv[i].Length > 0 && csv[i][0].Equals(END_KEY))
                    {
                        isDataRow = false;
                    }
                    else
                    {
                        if (csv[i].Length > uniqueIdIndex)
                        {
                            uniqueId = csv[i][uniqueIdIndex];
                            Match match = sequenceReg.Match(uniqueId);
                            if (match.Success)
                            {
                                sequenceId = match.Groups[1].Value;
                            }

                        }
                        if (csv[i].Length > patientIdIndex)
                        {
                            patientId = csv[i][patientIdIndex];
                            Match match = patientReg.Match(patientId);
                            if (match.Success)
                            {
                                patientNumber = match.Groups[1].Value;
                            }
                        }
                    }
                }
                else
                {
                    if (csv[i].Length > 0 && csv[i][0].Equals(BEGIN_KEY))
                    {
                        isDataRow = true;
                    }
                }
                for (int j = 0; j < csv[i].Length; j++)
                {
                    row.AppendFormat("\"{0}\"{1}", csv[i][j], SPLIT_BY);
                    
                }
                row.AppendFormat("\"{0}\"{1}\"{2}\"", sequenceId, SPLIT_BY, patientNumber);
                sb.AppendFormat("{0}{1}", row.ToString(), LINE_BREAK);
            }
            File.WriteAllText(outputFile, sb.ToString());
        }

        static string[][] readCSV(string reportFile)
        {

            string[] lines = File.ReadAllLines(reportFile);
            string[][] elements = new string[lines.Length][];

            for (int i = 0; i < lines.Length; i++)
            {
                elements[i] = lines[i].Split(new string[] { ",", ";" }, StringSplitOptions.None);
                for (int j = 0; j < elements[i].Length; j++)
                {
                    elements[i][j] = elements[i][j].Trim(new char[] { ' ', '"', '\'' });
                }
            }
            return elements;
        }

        
    }
}
