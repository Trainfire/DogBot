using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using Google.GData.Spreadsheets;
using System.Collections;
using Google.Apis.Requests;
using System.Threading.Tasks;

namespace Extensions.GoogleSpreadsheets
{
    class GoogleSpreadsheet
    {
        readonly SheetsService service;
        readonly string spreadSheetID;
        readonly UserCredential credential;

        public GoogleSpreadsheet(string spreadSheetID)
        {
            this.spreadSheetID = spreadSheetID;

            string[] scopes = { SheetsService.Scope.Spreadsheets };
            string applicationName = "Test";

            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
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
            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName,
            });

            // Get the sheet.
            Initialize();
        }

        async void Initialize()
        {
            var spreadSheet = await GetSheet(spreadSheetID, "A2:B");

            Console.WriteLine("Got sheet...");

            foreach (var sheet in spreadSheet.Sheets)
            {
                Console.WriteLine("Sheet {0}", sheet.Properties.Title);

                if (sheet.Data != null)
                    Console.WriteLine("Data Count: {0}", sheet.Data.Count);

                Console.WriteLine("Rows: {0}, Columns: {1}", sheet.Properties.GridProperties.RowCount, sheet.Properties.GridProperties.ColumnCount);
            }
        }

        public void UpdateCells(List<string> cellData)
        {
            var batchRequest = new BatchUpdateRequestWrapper(spreadSheetID);
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
                    Rows = new List<RowData>()
                    {
                        new RowData()
                        {
                            Values = ConvertToCellData(cellData),
                        },
                    },
                    Fields = "*",
                };
            });
            batchRequest.Execute(service);
        }

        List<CellData> ConvertToCellData(List<string> data)
        {
            var cells = new List<CellData>();
            data.ForEach(value =>
            {
                var cellData = new CellData();
                cellData.UserEnteredValue = new ExtendedValue()
                {
                    StringValue = value,
                };
                cells.Add(cellData);
            });
            return cells;
        }

        public void Write(SheetData data)
        {
            // Update the sheet.
            var updateRequest = service.Spreadsheets.Values.Update(data.ValueRange, spreadSheetID, data.Range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
            updateRequest.Execute();
        }

        public void Create(string name)
        {
            MakeBatchUpdateRequest((request) =>
            {
                request.AddSheet = new AddSheetRequest()
                {
                    Properties = new SheetProperties()
                    {
                        Title = name,
                    }
                };
            });
        }

        async Task<Spreadsheet> GetSheet(string spreadsheetID, string ranges = "")
        {
            var request = service.Spreadsheets.Get(spreadSheetID);
            request.Ranges = ranges;
            return await request.ExecuteAsync();
        }

        async Task<BatchUpdateSpreadsheetResponse> MakeBatchUpdateRequestAsync(Action<Request> onCreate)
        {
            var batchRequest = new BatchUpdateSpreadsheetRequest();
            batchRequest.Requests = new List<Request>();

            var request = new Request();
            onCreate(request);
            batchRequest.Requests.Add(request);

            var batchUpdate = service.Spreadsheets.BatchUpdate(batchRequest, spreadSheetID);
            return await batchUpdate.ExecuteAsync();
        }

        void MakeBatchUpdateRequest(Action<Request> onCreate)
        {
            var batchRequest = new BatchUpdateSpreadsheetRequest();
            batchRequest.Requests = new List<Request>();

            var request = new Request();
            onCreate(request);
            batchRequest.Requests.Add(request);

            var batchUpdate = service.Spreadsheets.BatchUpdate(batchRequest, spreadSheetID);
            batchUpdate.Execute();
        }

        public async void Clear()
        {
            var getRequest = service.Spreadsheets.Get(spreadSheetID);
            var response = await getRequest.ExecuteAsync();

            //if (response.Values != null && response.Values.Count != 0)
            //{
            //    for (int i = 0; i < response.Values.Count; i++)
            //    {
            //        response.Values[i] = new List<object>() { "", "" };
            //    }
            //}

            //var clearRequest = service.Spreadsheets.Values.Update(response, spreadSheetID, response.Range);
            //clearRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
            //clearRequest.Execute();
        }
    }

    public class SheetData
    {
        private List<IList<object>> data;

        public ValueRange ValueRange
        {
            get
            {
                var valueRange = new ValueRange();
                valueRange.Values = data;
                valueRange.Range = Range;
                return valueRange;
            }
        }

        public string Range { get; set; }

        public SheetData()
        {
            data = new List<IList<object>>();
        }

        public void AddRow(List<object> values)
        {
            data.Add(values);
        }
    }

    public class BatchUpdateRequestWrapper
    {
        private BatchUpdateSpreadsheetRequest batchUpdateRequest;
        private string spreadsheetID;

        public BatchUpdateRequestWrapper(string spreadsheetID)
        {
            this.spreadsheetID = spreadsheetID;
            batchUpdateRequest = new BatchUpdateSpreadsheetRequest();
            batchUpdateRequest.Requests = new List<Request>();
        }

        public void Add(Request request)
        {
            batchUpdateRequest.Requests.Add(request);
        }

        public void Add(Action<Request> onAdd)
        {
            var request = new Request();
            onAdd(request);
            Add(request);
        }

        public void Execute(SheetsService service)
        {
            service.Spreadsheets.BatchUpdate(batchUpdateRequest, spreadsheetID).Execute();
        }
    }
}
