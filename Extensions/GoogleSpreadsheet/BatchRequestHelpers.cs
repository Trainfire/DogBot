using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;

namespace Extensions.GoogleSpreadsheets
{
    /// <summary>
    /// Wraps a BatchUpdateSpreadsheetRequest for convenience.
    /// </summary>
    public class BatchUpdateSpreadsheetHelper
    {
        private BatchUpdateSpreadsheetRequest batchUpdateRequest;
        private string spreadsheetID;

        public BatchUpdateSpreadsheetHelper(string spreadsheetID)
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

        public async Task<BatchUpdateSpreadsheetResponse> ExecuteAsync(SheetsService service)
        {
            return await service.Spreadsheets.BatchUpdate(batchUpdateRequest, spreadsheetID).ExecuteAsync();
        }
    }
}
