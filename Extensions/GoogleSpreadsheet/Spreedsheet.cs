using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Extensions.GoogleSpreadsheets
{
    /// <summary>
    /// Wraps a Google Spreadsheet.
    /// TODO: Support multiple sheets.
    /// </summary>
    class Spreadsheet
    {
        private readonly SpreadsheetService spreadSheet;
        private readonly string ID;

        public List<RowData> Rows { get; private set; }
        public bool Dirty { get; private set; }

        public Spreadsheet(string spreadsheetID)
        {
            ID = spreadsheetID;
            Rows = new List<RowData>();
            spreadSheet = new SpreadsheetService();
        }

        public void AddRow(List<object> values)
        {
            Dirty = true;
            AddRowInternal(values);
        }

        /// <summary>
        /// Pushes the current data to the associated Google spreadsheet.
        /// </summary>
        public void Push()
        {
            CreateUpdateRequest().Execute(spreadSheet.Service);
        }

        /// <summary>
        /// Pushes the current data to the associated Google spreadsheet.
        /// </summary>
        public async Task<BatchUpdateSpreadsheetResponse> PushAsync()
        {
            return await CreateUpdateRequest().ExecuteAsync(spreadSheet.Service);
        }

        /// <summary>
        /// Gets the current data from the associated Google spreadsheet.
        /// </summary>
        /// <param name="force">Set to True to override any local changes.</param>
        public async Task Get(bool force = false)
        {
            if (Dirty && !force)
            {
                Console.WriteLine("The Spreadsheet data has been modified since the last Get. Push your changes first or pass in 'True' to overwrite your changes.");
                return;
            }

            var request = await RequestValues();

            Rows.Clear();
            Dirty = false;

            if (request == null)
            {
                // TODO.
            }
            else
            {
                foreach (var row in request.Values)
                {
                    AddRowInternal(row as List<object>);
                }
            }
        }

        void AddRowInternal(List<object> values)
        {
            var rowData = new RowData();
            rowData.Values = new List<CellData>();

            foreach (var value in values)
            {
                Console.WriteLine("Add value: {0}", value);
                rowData.Values.Add(ProcessCellData(value));
            }

            Rows.Add(rowData);
        }

        private async Task<ValueRange> RequestValues()
        {
            var requestSheet = spreadSheet.Service.Spreadsheets.Get(ID);
            var requestSheetResponse = requestSheet.Execute();

            // Convert column count to full range. Obviously error prone at the moment...
            // TODO: Support more than 26 columns.
            int columnCount = requestSheetResponse.Sheets[0].Properties.GridProperties.ColumnCount.Value;

            var request = spreadSheet.Service.Spreadsheets.Values.Get(ID, "A:" + IntToColumnRange(columnCount));
            return await request.ExecuteAsync();
        }

        private BatchUpdateSpreadsheetHelper CreateUpdateRequest()
        {
            var batchRequest = new BatchUpdateSpreadsheetHelper(ID);
            batchRequest.Add((request) =>
            {
                request.UpdateCells = new UpdateCellsRequest()
                {
                    Start = new GridCoordinate()
                    {
                        SheetId = 0,
                        RowIndex = 0,
                        ColumnIndex = 0,
                    },
                    Rows = this.Rows,
                    Fields = "*",
                };
            });
            return batchRequest;
        }

        /// <summary>
        /// Parses the object into either a StringValue, NumberValue, or BoolValue.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private CellData ProcessCellData(object value)
        {
            var cellData = new CellData();

            cellData.UserEnteredValue = new ExtendedValue();

            if (value as string != null)
                cellData.UserEnteredValue.StringValue = value.ToString();

            if (value is double || value is int || value is float)
                cellData.UserEnteredValue.NumberValue = Convert.ToDouble(value);

            if (value is bool)
                cellData.UserEnteredValue.BoolValue = Convert.ToBoolean(value);

            return cellData;
        }

        /// <summary>
        /// Converts a column count to a range value. For example, a count of 4 would return 'D' and a count of 70 would return 'BR'.
        /// </summary>
        private string IntToColumnRange(int columns)
        {
            // TODO: Currently broken.
            int baseChar = Convert.ToInt32('A') - 1;

            return Convert.ToChar(baseChar + columns).ToString();

            //int remainder = columns % 26;
            //if (remainder != 0)
            //{
            //    int charPosition = (columns - remainder) / 26;

            //    char firstColumn = Convert.ToChar(baseChar + (charPosition - 1));
            //    char secondColumn = Convert.ToChar(baseChar + (remainder - 1));

            //    return string.Join("", firstColumn, secondColumn);
            //}
            //else
            //{
            //    return Convert.ToChar(baseChar + (columns)).ToString();
            //}
        }
    }
}
