using System;
using System.IO;

namespace ConvertToTcx
{
    public class TcxDataFactory
    {
        private readonly Func<SourcedStream, ITcxData> _lemond;
        private readonly Func<SourcedStream, ITcxData> _computrainer;
        private readonly Func<SourcedStream, ITcxData> _xtrainer;

        public TcxDataFactory(Func<SourcedStream, ITcxData> lemond, Func<SourcedStream, ITcxData> computrainer, Func<SourcedStream, ITcxData> xtrainer)
        {
            _lemond = lemond;
            _computrainer = computrainer;
            _xtrainer = xtrainer;
        }

        public static TcxDataFactory CreateDefault()
        {
            Func<SourcedStream, ITcxData> lemond = r =>
                {
                    var provider = LeMondCsvDataProvider.Create(r);
                    var reader = new LeMondDataReader(provider);
                    return new LeMondTcxData(reader);
                };

            Func<SourcedStream, ITcxData> computrainer = r =>
                {
                    var provider = new CompuTrainer3DPFileProvider(r);
                    return new CompuTrainerTcxData(provider);
                };

            Func<SourcedStream, ITcxData> xtrainer = r =>
            {
                var provider = XtrainerDataProvider.Create(r);
                var reader = new XtrainerDataReader(provider);
                return new XtrainerTcxData(reader);
            };

            return new TcxDataFactory(lemond, computrainer, xtrainer);
        }
        
        public ITcxData Create(SourcedStream reader)
        {
            string extension = Path.GetExtension(reader.Source);
            if (extension == null) throw new ArgumentNullException("extension");
            if (extension.Equals(".csv", StringComparison.OrdinalIgnoreCase))
            {
                // LeMond
                return _lemond(reader);
            }
            else if (extension.Equals(".3dp", StringComparison.OrdinalIgnoreCase))
            {
                // CompuTrainer
                return _computrainer(reader);
            }
            else if (extension.Equals(".csvx", StringComparison.OrdinalIgnoreCase))
            {
                // XTrainer
                return _xtrainer(reader);
            }

            throw new Exception(string.Format("The extension '{0}' is not a supported file type", extension));
        }
    }
}
