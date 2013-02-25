using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DraftAdmin.Output
{
    public class XmlDataRow
    {

        #region Private Members

        private string _startRow;
        private string _endRow;
        private string _dataString;

        #endregion

        #region Constructor

        public XmlDataRow()
        {
            _startRow = "<?xml version=\"1.0\" standalone=\"yes\" ?><DataSet><Row>";
            _dataString = "";
            _endRow = "</Row></DataSet>";
        }

        #endregion

        #region Public Methods

        public void Clear()
        {
            _dataString = "";
        }

        public string GetXMLString()
        {
            return _startRow + _dataString + _endRow;
        }

        public void Add(string name, string value)
        {
            _dataString += "<" + name + "><![CDATA[" + value + "]]> </" + name + ">";
        }

        #endregion

    }
}
