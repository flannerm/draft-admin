using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DraftAdmin.Output;

namespace DraftAdmin.Models
{
    public class PlaylistItem : ModelBase
    {
        #region Private Members

        private int _playlistItemId;
        private string _template;
        private string _desc;
        private int _playlistOrder;
        private decimal _duration;
        private string _query;
        private int _maxRows;
        private string _datasource;
        private string _queryType;
        private string _queryParams;
        private string _outputParam;
        private string _panelType;
        private string _pageType;
        private bool _enabled;
        private bool _mergeDataNoTransitions;
        //private int _rowIndex;
        private Dictionary<string, string> _additionalDataFields;
        private List<XmlDataRow> _xmlDataRows;
        private int _currentRow = 0;
        private bool _onAir = false;

        #endregion

        #region Properties

        public int PlaylistItemID
        {
            get { return _playlistItemId; }
            set { _playlistItemId = value; }
        }

        public string Template
        {
            get { return _template; }
            set { _template = value; }
        }

        public string PanelType
        {
            get { return _panelType; }
            set { _panelType = value; }
        }

        public string PageType
        {
            get { return _pageType; }
            set { _pageType = value; }
        }

        public string Description
        {
            get { return _desc; }
            set { _desc = value; }
        }

        public int PlaylistOrder
        {
            get { return _playlistOrder; }
            set { _playlistOrder = value; }
        }

        public decimal Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }

        public string Query
        {
            get { return _query; }
            set { _query = value; }
        }

        public int MaxRows
        {
            get { return _maxRows; }
            set { _maxRows = value; }
        }

        public string Datasource
        {
            get { return _datasource; }
            set { _datasource = value; }
        }

        public string QueryType
        {
            get { return _queryType; }
            set { _queryType = value; }
        }

        public string QueryParameters
        {
            get { return _queryParams; }
            set { _queryParams = value; }
        }

        public string OutputParameter
        {
            get { return _outputParam; }
            set { _outputParam = value; }
        }

        public Dictionary<string, string> AdditionalDataFields
        {
            get { return _additionalDataFields; }
            set { _additionalDataFields = value; }
        }

        public List<XmlDataRow> XmlDataRows
        {
            get { return _xmlDataRows; }
            set { _xmlDataRows = value; }
        }

        public int CurrentRow
        {
            get { return _currentRow; }
            set { _currentRow = value; }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        public bool MergeDataNoTransitions
        {
            get { return _mergeDataNoTransitions; }
            set { _mergeDataNoTransitions = value; }
        }

        public bool OnAir
        {
            get { return _onAir; }
            set { _onAir = value; OnPropertyChanged("OnAir");  }
        }

        //public int RowIndex
        //{
        //    get { return _rowIndex; }
        //    set { _rowIndex = value; }
        //}

        #endregion

    }
}
