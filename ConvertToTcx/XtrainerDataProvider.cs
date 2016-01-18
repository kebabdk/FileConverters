using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using Microsoft.VisualBasic.FileIO;

namespace ConvertToTcx
{
    public class XtrainerDataProvider : IXtrainerDataProvider
    {
        public XtrainerDataProvider(TextFieldParser parser)
        {
            Parser = parser;
            StartTime = DateTime.Now;
        }
        public XtrainerDataProvider(TextFieldParser parser, DateTime startTime)
        {
            Parser = parser;
            StartTime = startTime;
        }

        public DateTime StartTime { get;  set; }

        public TextFieldParser Parser { get;  set; }

        public static XtrainerDataProvider Create(SourcedStream sourcedStream)
        {
            
            var parser = new TextFieldParser(sourcedStream.Stream)
                {
                    TextFieldType = FieldType.Delimited,
                    Delimiters = new[] {","}
                };
            if (parser.EndOfData)
            {
                throw new Exception(string.Format("The file {0} does not seem to be a valid xtrainer .csvx file because it is empty.", sourcedStream.Source));
            }

            var rows = parser.ReadFields();
            if (!(rows.Length >= 1 && rows[0] == "ver"))
            {
                throw new Exception(string.Format("The file {0} does not seem to be a valid xtrainer .csvx file because it doesn't say 'ver' in the first field.", sourcedStream.Source));
            }
            rows = parser.ReadFields();
            int dummy;
            if (rows.Length == 5 && int.TryParse(rows[0], out dummy))
            {
                var startDateTime = new DateTime(int.Parse(rows[0]), int.Parse(rows[1]), int.Parse(rows[2]),
                                                      int.Parse(rows[3]), int.Parse(rows[4]), 0);
                parser.ReadFields();
                return new XtrainerDataProvider(parser,startDateTime);
            }
            return new XtrainerDataProvider(parser);
        }

        public IEnumerable<XtrainerDataLine> DataLines
        {
            get
            {
                int rowCount = 0;
                string [] row = null;
                while (!Parser.EndOfData)
                {
                    row = Parser.ReadFields();
                    rowCount++;
                    if (row.Length == 6 || row.Length == 7)
                    {
                        var data = new XtrainerDataLine
                        {
                            Time = row[0],
                            HeartRate = row[1],
                            Rpm = row[2],
                            Power = row[3],
                            ClimbPromille = row[4],
                            Speed = row[5],
                            IsLap = row.Length==7
                        };
                        yield return data;
                    }
                }
            }
        }

    }
}
