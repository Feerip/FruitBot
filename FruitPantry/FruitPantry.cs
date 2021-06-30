using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using RSAdventurerLogScraper;

using Data = Google.Apis.Sheets.v4.Data;

namespace FruitPantry
{
    public class FruitPantry
    {
        private readonly string[] _scopes = { SheetsService.Scope.Spreadsheets };
        private string _applicationName = "FruitBot";
        private string _spreadsheetId = "1iCJHsiC4nEjjFz1Gmw4aTldnMFR5ZAlGSuJfHbP262s";
        private string _sheet = "Drop Log";
        private string _range;
        SheetsService _service;
        GoogleCredential _credentials;

        public SortedDictionary<string, DropLogEntry> _masterList { get; private set; }

        public static readonly int RSN                  = 0;
        public static readonly int Fruit                = 1;
        public static readonly int Drop                 = 2;
        public static readonly int Timestamp            = 3;
        public static readonly int RuneMetricsID        = 4;
        public static readonly int PlayerAvatarLink     = 5;
        public static readonly int DropIconLink         = 6;
        public static readonly int BossName             = 7;
        public static readonly int PointValue           = 8;
        public static readonly int EntryKey             = 9;
        //public enum Indices
        //{
        //    /*0*/ RSN,
        //    /*1*/ Fruit,
        //    /*2*/ Drop,
        //    /*3*/ PointValue, 
        //    /*4*/ Timestamp, 
        //    /*5*/ PlayerAvatarLink, 
        //    /*6*/ DropIconLink, 
        //    /*7*/ BossName,
        //    /*8*/ RunemetricsID
        //}


        public FruitPantry(string applicationName, string spreadsheetID, string sheet, string credentialsFile)
        {
            _applicationName = applicationName;
            _spreadsheetId = spreadsheetID;
            _sheet = sheet;
            
            _credentials = GoogleCredential.FromFile(credentialsFile).CreateScoped(_scopes);

            _service = new SheetsService(new() 
            { 
                HttpClientInitializer = _credentials,
                ApplicationName = _applicationName 
            });

            _range = $"{_sheet}!A2:J";

            ForceRefresh();
        }

        // Refreshes and returns the current drop log as per google sheets.
        public SortedDictionary<string, DropLogEntry> ForceRefresh()
        {
            SortedDictionary<string, DropLogEntry> output = new();

            SpreadsheetsResource.ValuesResource.GetRequest request = _service.Spreadsheets.Values.Get(_spreadsheetId, _range);

            ValueRange response = request.Execute();

            IList<IList<object>> values = response.Values;

            if (values!= null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    //if ()
                    output.Add(row[EntryKey].ToString(), new(
                                                playerName: row[RSN],
                                                fruit: row[Fruit],
                                                dropName: row[Drop], 
                                                timestamp: row[Timestamp],
                                                playerAvatarPNG: row[PlayerAvatarLink], 
                                                dropIconWEBP: row[DropIconLink],
                                                bossName: row[BossName], 
                                                runemetricsDropID: row[RuneMetricsID],
                                                pointValue: row[PointValue], 
                                                entryKey: row[EntryKey]));
                }

            }

            _masterList = output;
            return _masterList;
        }

        public SortedDictionary<string, DropLogEntry> PurgeThePantry()
        {
            Data.ClearValuesRequest requestBody = new Data.ClearValuesRequest();
            SpreadsheetsResource.ValuesResource.ClearRequest request = _service.Spreadsheets.Values.Clear(requestBody, _spreadsheetId, _range);
            Data.ClearValuesResponse response = request.Execute();

            return ForceRefresh();
        }


        // Theoretically should be a very fast way to check uniqueness
        public bool AlreadyExists(DropLogEntry entry)
        {
            return _masterList.ContainsKey(entry._entryKey);
        }


        // Adds an entry to the drop log, sending it to google sheets. Refreshes _masterList and returns it. 
        public async Task<SortedDictionary<string, DropLogEntry>> Add(DropLogEntry entry)
        {
            return await Add(new List<DropLogEntry> { entry });
        }

        public async Task<SortedDictionary<string, DropLogEntry>> Add(List<DropLogEntry> entries)
        {
            List<DropLogEntry> uniqueEntries = new();

            foreach (DropLogEntry entry in entries)
            {
                if (!AlreadyExists(entry))
                {
                    uniqueEntries.Add(entry);
                }
            }

            ProcessNewEntries(uniqueEntries);
            
            return _masterList;
        }

        // Guaranteed unique entries. don't have to worry about checking them, just send them through the API
        private SortedDictionary<string, DropLogEntry> ProcessNewEntries(List<DropLogEntry> entries)
        {
            List<IList<object>> newEntries = new();
            foreach (DropLogEntry entry in entries)
            {

                List<object> rowToAppend = new();
                rowToAppend.Add(entry._playerName);
                rowToAppend.Add(entry._fruit);
                rowToAppend.Add(entry._dropName);
                rowToAppend.Add(entry._timestamp);
                rowToAppend.Add(entry._runemetricsDropID);
                rowToAppend.Add(entry._playerAvatarPNG);
                rowToAppend.Add(entry._dropIconWEBP);
                rowToAppend.Add(entry._bossName);
                rowToAppend.Add(entry._pointValue);
                rowToAppend.Add(entry._entryKey);

                newEntries.Add(rowToAppend);
            }
            ValueRange requestBody = new();
            requestBody.Values = newEntries;

            SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum VIO = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
            SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum IDO = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;

            SpreadsheetsResource.ValuesResource.AppendRequest request = _service.Spreadsheets.Values.Append(requestBody, _spreadsheetId, _range);
            request.ValueInputOption = VIO;
            request.InsertDataOption = IDO;

            Data.AppendValuesResponse response = request.Execute();
            ForceRefresh();

            return _masterList;
        }
    }
}
