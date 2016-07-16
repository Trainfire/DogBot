using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Extensions.GoogleSpreadsheets
{
    class Spreadsheet
    {
        private readonly SpreadsheetService spreadSheet;
        private readonly string ID;

        public List<RowData> Rows { get; private set; }

        public Spreadsheet(string spreadsheetID)
        {
            ID = spreadsheetID;
            Rows = new List<RowData>();
            spreadSheet = new SpreadsheetService();
        }

        public void AddRow(List<object> values)
        {
            var rowData = new RowData();
            rowData.Values = new List<CellData>();

            foreach (var value in values)
            {
                rowData.Values.Add(ProcessCellData(value));
            }

            Rows.Add(rowData);
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

        private BatchUpdateHelper CreateUpdateRequest()
        {
            var batchRequest = new BatchUpdateHelper(ID);
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
    }
}
