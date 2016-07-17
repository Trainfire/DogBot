using Google.Apis.Sheets.v4.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace Extensions.GoogleSpreadsheets
{
    /// <summary>
    /// Wraps a Google Spreadsheet.
    /// </summary>
    class Spreadsheet
    {
        readonly UserCredential credential;

        public string ID { get; private set; }
        public bool Dirty { get; private set; }
        public List<Sheet> Sheets { get; private set; }
        public SheetsService Service { get; private set; }

        public Spreadsheet(string spreadsheetID)
        {
            ID = spreadsheetID;
            Sheets = new List<Sheet>();

            string[] scopes = { SheetsService.Scope.Spreadsheets };
            string applicationName = "Test"; // TODO: Read from config.

            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/sheets.googleapis.com-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Sheets API service.
            Service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName,
            });
        }

        /// <summary>
        /// Pulls all sheet data from the API.
        /// </summary>
        /// <returns></returns>
        public async Task Get()
        {
            var requestSheet = Service.Spreadsheets.Get(ID);
            var requestSheetResponse = await requestSheet.ExecuteAsync();

            foreach (var sheet in requestSheetResponse.Sheets)
            {
                var s = new Sheet(this, sheet.Properties.SheetId.Value, sheet.Properties.Title);
                Sheets.Add(s);
            }
        }

        /// <summary>
        /// Pushes all sheet data to the API.
        /// </summary>
        /// <returns></returns>
        public async Task Push()
        {
            foreach (var sheet in Sheets)
            {
                await sheet.PushAsync();
            }
        }

        public async Task<Sheet> GetOrAddSheet(string name)
        {
            var find = Sheets.Find(x => x.Name == name);
            if (find != null)
            {
                return find;
            }
            else
            {
                return await AddSheet(name);
            }
        }

        public async Task<Sheet> AddSheet(string name)
        {
            var sheet = new Sheet(this, Sheets.Count, name);
            Sheets.Add(sheet);

            var batchRequest = new BatchUpdateSpreadsheetHelper(ID);

            batchRequest.Add((request) =>
            {
                // By default, a spreadsheet already has one sheet. 
                // If there is more than one sheet, create a new one.
                // Otherwise, just update the title of the first one.
                if (Sheets.Count > 1)
                {
                    request.AddSheet = new AddSheetRequest()
                    {
                        Properties = new SheetProperties()
                        {
                            SheetId = Sheets.Count - 1,
                            Title = name,
                            GridProperties = new GridProperties()
                            {
                                ColumnCount = 1,
                                RowCount = 1,
                            }
                        },
                    };
                }
                else
                {
                    request.UpdateSheetProperties = new UpdateSheetPropertiesRequest()
                    {
                        Properties = new SheetProperties()
                        {
                            Title = name,
                            GridProperties = new GridProperties()
                            {
                                ColumnCount = 1,
                                RowCount = 1,
                            }
                        },
                        Fields = "*",
                    };
                }
            });

            await batchRequest.ExecuteAsync(Service);

            return sheet;
        }

        // TODO.
        //public async Task RemoveSheet(Sheet sheet)
        //{

        //}
    }

    class Sheet
    {
        private readonly Spreadsheet spreadSheet;
        private readonly int ID;

        public string Name { get; private set; }
        public List<RowData> Rows { get; private set; }
        public bool Dirty { get; private set; }
        public SpreadsheetProperties Properties { get; private set; }

        public Sheet(Spreadsheet spreadSheet, int id, string name)
        {
            this.spreadSheet = spreadSheet;
            ID = id;
            Name = name;
            Rows = new List<RowData>();
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
                rowData.Values.Add(ProcessCellData(value));
            }

            Rows.Add(rowData);
        }

        private async Task<ValueRange> RequestValues()
        {
            var requestSheet = spreadSheet.Service.Spreadsheets.Get(spreadSheet.ID);
            var requestSheetResponse = requestSheet.Execute();

            // Convert column count to full range. Obviously error prone at the moment...
            // TODO: Support more than 26 columns.
            int columnCount = requestSheetResponse.Sheets[ID].Properties.GridProperties.ColumnCount.Value;

            var request = spreadSheet.Service.Spreadsheets.Values.Get(spreadSheet.ID, "A:" + IntToColumnRange(columnCount));
            return await request.ExecuteAsync();
        }

        private BatchUpdateSpreadsheetHelper CreateUpdateRequest()
        {
            var batchRequest = new BatchUpdateSpreadsheetHelper(spreadSheet.ID);

            batchRequest.Add((request) =>
            {
                request.UpdateSheetProperties = new UpdateSheetPropertiesRequest()
                {
                    Properties = new SheetProperties()
                    {
                        GridProperties = new GridProperties()
                        {
                            RowCount = this.Rows.Count,
                            ColumnCount = this.Rows[0].Values.Count, // bad
                        },
                        SheetId = ID,
                        Title = Name,
                    },
                    Fields = "*",
                };
            });

            batchRequest.Add((request) =>
            {
                request.UpdateCells = new UpdateCellsRequest()
                {
                    Start = new GridCoordinate()
                    {
                        SheetId = ID,
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
