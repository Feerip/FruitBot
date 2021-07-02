using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json.Converters;
using RSAdventurerLogScraper;
using DataTypes;

using Data = Google.Apis.Sheets.v4.Data;
using OpenQA.Selenium;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Discord;
using Discord.WebSocket;

namespace FruitPantry
{
    public sealed class FruitPantry
    {
        private static readonly FruitPantry _instance = new();

        private string[] _scopes = { SheetsService.Scope.Spreadsheets };
        private string _applicationName;
        private string _credentialsFile;
        private string _spreadsheetId;

        private string _dropLogRange;
        private string _classificationRange;
        private string _playerDatabaseRange;
        private string _itemDatabaseRange;
        private string _thresholdValuesRange;

        private SheetsService _service;
        private GoogleCredential _credentials;



        private SortedDictionary<string, DropLogEntry> _dropLog;
        public Dictionary<string, ItemDatabaseEntry> _itemDatabase { get; private set; }
        public Dictionary<string, float> _classificationList { get; set; }
        private Dictionary<string, List<string>> _discordUsers { get; set; }
        private Dictionary<string, List<string>> _runescapePlayers { get; set; }

        public int _thresholdValue { get; set; }
        public float _thresholdMultiplier { get; set; }


        public readonly int RSN = 0;
        public readonly int Fruit = 1;
        public readonly int Drop = 2;
        public readonly int Timestamp = 3;
        public readonly int RuneMetricsID = 4;
        public readonly int PlayerAvatarLink = 5;
        public readonly int DropIconLink = 6;
        public readonly int BossName = 7;
        public readonly int PointValue = 8;
        public readonly int EntryKey = 9;

        public readonly int Classification = 0;
        public readonly int PointsPerDrop = 1;

        public readonly int PlayerDB_RSN = 0;
        public readonly int PlayerDB_Fruit = 1;
        public readonly int PlayerDB_DiscordTag = 2;

        public readonly int ThresholdValue = 0;
        public readonly int ThresholdMultiplier = 1;

        public readonly int ItemDBItemName = 0;
        public readonly int ItemDBClassification = 1;
        public readonly int ItemDBWikiLink = 4;
        public readonly int ItemDBImageURL = 5;
        public readonly int ItemDBActiveFlag = 6;

        public static int NumNewEntries = 0;

        public static class PointsCalculator
        {
            private static FruitPantry thePantry = GetFruitPantry();

            public static float CalculatePoints(DropLogEntry drop)
            {

                //FruitPantry thePantry = GetFruitPantry();



                //float basePoints = thePantry._classificationList[thePantry._itemDatabase[drop._bossName.ToLower()]._classification];
                float basePoints = thePantry._classificationList[drop._bossName];
                float thresholdMultiplier = thePantry._thresholdMultiplier;

                int thresholdLevel = ThresholdLevel(drop._playerName, drop._bossName);

                float pointValue = basePoints * (float)(Math.Pow(thresholdMultiplier, thresholdLevel));


                return pointValue;

            }

            // Takes the drop log of a single player and returns the threshold level they are currently at for any given boss
            public static int ThresholdLevel(string playerName, string bossName)
            {
                int thresholdValue = thePantry._thresholdValue;

                List<DropLogEntry> playerLog = FilterByPlayer(playerName);

                int dropsFromThisBoss = 0;

                foreach (DropLogEntry entry in playerLog)
                {
                    if (!entry._bossName.Equals(bossName))
                        break;
                    dropsFromThisBoss++;
                }

                int thresholdLevel = dropsFromThisBoss / thresholdValue;
                return thresholdLevel;
            }

            // Takes the whole drop log and filter it down to just those of a certain player
            public static List<DropLogEntry> FilterByPlayer(string playerName)
            {
                List<DropLogEntry> playerLog = new();

                foreach (KeyValuePair<string, DropLogEntry> entry in thePantry._dropLog)
                {
                    if (entry.Value._playerName.Equals(playerName))
                    {
                        playerLog.Add(entry.Value);
                    }
                }
                return playerLog;
            }


        }



        //public FruitPantry(string applicationName, string spreadsheetID, string sheet, string credentialsFile)
        //{
        //    _applicationName = applicationName;
        //    _spreadsheetId = spreadsheetID;
        //    _sheet = sheet;

        //    _credentials = GoogleCredential.FromFile(credentialsFile).CreateScoped(_scopes);

        //    _service = new SheetsService(new() 
        //    { 
        //        HttpClientInitializer = _credentials,
        //        ApplicationName = _applicationName 
        //    });

        //    _range = $"{_sheet}!A2:J";

        //    ForceRefresh();
        //}

        private FruitPantry()
        {
            _applicationName = "FruitBot";
            _credentialsFile = "credentials.json";
            _spreadsheetId = "1iCJHsiC4nEjjFz1Gmw4aTldnMFR5ZAlGSuJfHbP262s";


            _dropLogRange = "Drop Log!A2:J";
            _classificationRange = $"Classifications!A2:B19";
            _thresholdValuesRange = $"Classifications!D2:E2";
            _itemDatabaseRange = $"Item Database!A2:G316";
            _playerDatabaseRange = $"Players!A2:C";

            _dropLog = new(comparer: new LogEntryKeyComparer());


            _credentials = GoogleCredential.FromFile(_credentialsFile).CreateScoped(_scopes);

            _service = new SheetsService(new()
            {
                HttpClientInitializer = _credentials,
                ApplicationName = _applicationName
            });

            RefreshDropLog();
            RefreshClassifications();
            RefreshPlayerDatabase();
            RefreshThresholdValues();
            RefreshItemDatabase();
        }

        public async Task<int> ScrapeGameData(IDiscordClient discordClient)
        {
            await Add(DropLogEntry.CreateListFullAuto().Result, (DiscordSocketClient)discordClient);


            return _dropLog.Count;
        }

        public void RefreshClassifications()
        {
            Dictionary<string, float> output = new();
            SpreadsheetsResource.ValuesResource.GetRequest request = _service.Spreadsheets.Values.Get(_spreadsheetId, _classificationRange);
            ValueRange response = request.Execute();

            IList<IList<object>> values = response.Values;

            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    output.Add(row[Classification].ToString(), float.Parse(row[PointsPerDrop].ToString()));
                }
            }
            _classificationList = output;
        }

        public void RefreshItemDatabase()
        {
            Dictionary<string, ItemDatabaseEntry> output = new();
            SpreadsheetsResource.ValuesResource.GetRequest request = _service.Spreadsheets.Values.Get(_spreadsheetId, _itemDatabaseRange);
            ValueRange response = request.Execute();

            IList<IList<object>> values = response.Values;

            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    if (string.Equals(row[ItemDBActiveFlag].ToString(), "TRUE"))
                    {
                        ItemDatabaseEntry newItem = new();
                        newItem._itemName = row[ItemDBItemName].ToString();
                        newItem._classification = row[ItemDBClassification].ToString();
                        newItem._wikiLink = row[ItemDBWikiLink].ToString();
                        newItem._imageURL = row[ItemDBImageURL].ToString();

                        output.Add(row[ItemDBItemName].ToString().ToLower(), newItem);
                    }
                }
            }
            _itemDatabase = output;
            Console.WriteLine();
        }
        public void RefreshThresholdValues()
        {
            int thresholdValue = new();
            float thresholdMultiplier = new();
            SpreadsheetsResource.ValuesResource.GetRequest request = _service.Spreadsheets.Values.Get(_spreadsheetId, _thresholdValuesRange);
            ValueRange response = request.Execute();

            IList<IList<object>> values = response.Values;

            if (values != null && values.Count > 0)
            {
                thresholdValue = int.Parse(values[0][ThresholdValue].ToString());
                thresholdMultiplier = float.Parse(values[0][ThresholdMultiplier].ToString());
            }
            _thresholdValue = thresholdValue;
            _thresholdMultiplier = thresholdMultiplier;

        }
        public void RefreshPlayerDatabase()
        {
            Dictionary<string, List<string>> discordUsersOutput = new();
            Dictionary<string, List<string>> runescapePlayersOutput = new();
            SpreadsheetsResource.ValuesResource.GetRequest request = _service.Spreadsheets.Values.Get(_spreadsheetId, _playerDatabaseRange);
            ValueRange response = request.Execute();

            IList<IList<object>> values = response.Values;

            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    discordUsersOutput.Add(row[PlayerDB_DiscordTag].ToString(), new() { row[PlayerDB_Fruit].ToString(), row[PlayerDB_RSN].ToString() });
                    runescapePlayersOutput.Add(row[PlayerDB_RSN].ToString(), new() { row[PlayerDB_Fruit].ToString(), row[PlayerDB_DiscordTag].ToString() });
                }
            }
            _discordUsers = discordUsersOutput;
            _runescapePlayers = runescapePlayersOutput;
        }

        public static FruitPantry GetFruitPantry()
        {
            return _instance;
        }

        // Refreshes and returns the current drop log as per google sheets.
        public SortedDictionary<string, DropLogEntry> RefreshDropLog()
        {
            SortedDictionary<string, DropLogEntry> output = new(new LogEntryKeyComparer());

            SpreadsheetsResource.ValuesResource.GetRequest request = _service.Spreadsheets.Values.Get(_spreadsheetId, _dropLogRange);

            // Add() function is too stupid fast, we need time for google to process the new entry before we can pull it. 
            Thread.Sleep(1000);
            ValueRange response = request.Execute();

            IList<IList<object>> values = response.Values;

            if (values != null && values.Count > 0)
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

            _dropLog = output;
            return _dropLog;
        }

        public SortedDictionary<string, DropLogEntry> PurgeThePantry()
        {
            Data.ClearValuesRequest requestBody = new Data.ClearValuesRequest();
            SpreadsheetsResource.ValuesResource.ClearRequest request = _service.Spreadsheets.Values.Clear(requestBody, _spreadsheetId, _dropLogRange);
            Data.ClearValuesResponse response = request.Execute();

            return RefreshDropLog();
        }


        // Theoretically should be a very fast way to check uniqueness
        public bool AlreadyExists(DropLogEntry entry)
        {
            return _dropLog.ContainsKey(entry._entryKey);
        }

        public bool IsBeingMonitored(DropLogEntry entry)
        {
            return _itemDatabase.ContainsKey(entry._dropName.ToLower());
        }


        // Adds an entry to the drop log, sending it to google sheets. Refreshes _masterList and returns it. 
        public async Task<SortedDictionary<string, DropLogEntry>> Add(DropLogEntry entry)
        {
            List<IList<object>> newEntries = new();


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

            ValueRange requestBody = new();
            requestBody.Values = newEntries;

            SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum VIO = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
            SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum IDO = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;

            SpreadsheetsResource.ValuesResource.AppendRequest request = _service.Spreadsheets.Values.Append(requestBody, _spreadsheetId, _dropLogRange);
            request.ValueInputOption = VIO;
            request.InsertDataOption = IDO;

            Data.AppendValuesResponse response = request.Execute();
            RefreshDropLog();

            return _dropLog;
        }

        public async Task<SortedDictionary<string, DropLogEntry>> Add(List<DropLogEntry> entries, DiscordSocketClient discordClient)
        {
            //List<DropLogEntry> uniqueEntries = new();

            foreach (DropLogEntry entry in entries)
            {
                if (!AlreadyExists(entry) && IsBeingMonitored(entry))
                {
                    NumNewEntries++;
                    // _runescapePlayers list index is <KEY>(PlayerName){Fruit, DiscordTag}
                    try
                    {
                        entry._fruit = _runescapePlayers[entry._playerName][0];
                    }
                    catch(KeyNotFoundException e)
                    {
                        //await discordClient.GetGuild(769476224363397140).GetTextChannel(856679881547186196).SendMessageAsync(
                        //    $"Warning: Found a fruitless heathen ({entry._playerName}) in scraped Runepixels data. This drop will not be added to the drop log.");
                        //continue;
                    }
                        entry._bossName = _itemDatabase[entry._dropName.ToLower()]._classification;
                        entry._pointValue = PointsCalculator.CalculatePoints(entry).ToString();
                        await Add(entry);
                    
                }
            }


            return _dropLog;
        }

        // Guaranteed unique entries. don't have to worry about checking them, just send them through the API
        //private SortedDictionary<string, DropLogEntry> ProcessNewEntries(List<DropLogEntry> entries)
        //{
        //    List<IList<object>> newEntries = new();
        //    foreach (DropLogEntry entry in entries)
        //    {

        //        List<object> rowToAppend = new();
        //        rowToAppend.Add(entry._playerName);
        //        rowToAppend.Add(entry._fruit);
        //        rowToAppend.Add(entry._dropName);
        //        rowToAppend.Add(entry._timestamp);
        //        rowToAppend.Add(entry._runemetricsDropID);
        //        rowToAppend.Add(entry._playerAvatarPNG);
        //        rowToAppend.Add(entry._dropIconWEBP);
        //        rowToAppend.Add(entry._bossName);
        //        rowToAppend.Add(entry._pointValue);
        //        rowToAppend.Add(entry._entryKey);

        //        newEntries.Add(rowToAppend);
        //    }
        //    ValueRange requestBody = new();
        //    requestBody.Values = newEntries;

        //    SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum VIO = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
        //    SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum IDO = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;

        //    SpreadsheetsResource.ValuesResource.AppendRequest request = _service.Spreadsheets.Values.Append(requestBody, _spreadsheetId, _dropLogRange);
        //    request.ValueInputOption = VIO;
        //    request.InsertDataOption = IDO;

        //    Data.AppendValuesResponse response = request.Execute();
        //    RefreshDropLog();

        //    return _dropLog;
        //}
        public int PullWikiImages()
        {
            List<string> wikiURLs = new();
            _itemDatabase = DropLogEntry.PullWikiImages(_itemDatabase);

            List<IList<object>> newEntries = new();

            int idx = 0;
            foreach (KeyValuePair<string, ItemDatabaseEntry> entry in _itemDatabase)
            {

                List<object> rowToAppend = new();
                rowToAppend.Add(entry.Key);
                rowToAppend.Add(entry.Value._classification);
                rowToAppend.Add("");
                rowToAppend.Add("");
                rowToAppend.Add(entry.Value._wikiLink);
                rowToAppend.Add(entry.Value._imageURL);

                newEntries.Add(rowToAppend);

                idx++;
            }
            ValueRange requestBody = new();
            requestBody.Values = newEntries;

            SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum VIO = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
            SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum IDO = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.OVERWRITE;
            SpreadsheetsResource.ValuesResource.AppendRequest request = _service.Spreadsheets.Values.Append(requestBody, _spreadsheetId, _itemDatabaseRange);
            request.ValueInputOption = VIO;
            request.InsertDataOption = IDO;
            Data.AppendValuesResponse response = request.Execute();
            RefreshItemDatabase();
            return idx;
        }

        public SortedDictionary<string, DropLogEntry> GetDropLog()
        {
            return _dropLog;
        }
    }
}
