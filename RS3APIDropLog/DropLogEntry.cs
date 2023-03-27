using DataTypes;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;

namespace RS3APIDropLog
{
    public class DropLogEntry
    {




        public string _playerName { get; }
        public string _fruit { get; set; }
        public string _dropName { get; }
        public string _timestamp { get; }
        public string _playerAvatarPNG { get; set; }
        public string _dropIconWEBP { get; set; }
        public string _bossName { get; set; }
        public string _runemetricsDropID { get; set; }
        public string _pointValue { get; set; }

        public string _fruitLogo
        {
            get
            {
                if (_fruit.Equals(FruitResources.Text.grape))
                {
                    return FruitResources.Logos.grape;
                }

                if (_fruit.Equals(FruitResources.Text.banana))
                {
                    return FruitResources.Logos.banana;
                }

                if (_fruit.Equals(FruitResources.Text.apple))
                {
                    return FruitResources.Logos.apple;
                }

                if (_fruit.Equals(FruitResources.Text.peach))
                {
                    return FruitResources.Logos.peach;
                }
                else
                {
                    return FruitResources.Logos.fruitlessHeathen;
                }
                //placeholder for "fruitless heathen"
            }
        }


        public string _entryKey { get; }



        // Standard ctor
        public DropLogEntry(object playerName, object fruit, object dropName, object timestamp,
                            object playerAvatarPNG = null, object dropIconWEBP = null,
                            object bossName = null, object runemetricsDropID = null, object pointValue = null, object entryKey = null)
        {
            // Required
            _playerName = playerName.ToString();
            _fruit = fruit.ToString();
            _dropName = SanitizeDropName(dropName.ToString());
            _timestamp = timestamp.ToString();

            // Image links
            if (playerAvatarPNG != null)
            {
                _playerAvatarPNG = playerAvatarPNG.ToString();
            }
            else
            {
                _playerAvatarPNG = null;
            }

            if (dropIconWEBP != null)
            {
                _dropIconWEBP = dropIconWEBP.ToString() ?? null;
            }
            else
            {
                _dropIconWEBP = null;
            }


            // Runemetrics stuff
            if (bossName != null)
            {
                _bossName = bossName.ToString();
            }
            else
            {
                _bossName = null;
            }

            if (runemetricsDropID != null)
            {
                _runemetricsDropID = runemetricsDropID.ToString();
            }
            else
            {
                _runemetricsDropID = null;
            }

            if (pointValue != null)
            {
                _pointValue = pointValue.ToString();
            }
            else
            {
                _pointValue = null;
            }

            // Calculate unique ID to ensure no duplicates
            // playerName + timestamp is an easy way to get a unique ID for each drop
            _entryKey = _timestamp + " " + _playerName + " " + _dropName;


            if (entryKey != null)
            {
                if (!string.Equals((_timestamp + " " + _playerName + " " + _dropName), _entryKey))
                {
                    throw new DataException("DropLogEntry data corrupted: entry key verification error while downloading entry");
                }
            }

            if (EntryKeyCorrupted)
            {
                throw new DataException("DropLogEntry data corrupted: Entry key does not match expected result.");
            }
        }

        // Automatic ctor (pass in an instance of the javascript class "activity" on runepixels)


        public static bool operator ==(DropLogEntry lhs, DropLogEntry rhs)
        {
            if (lhs._entryKey == rhs._entryKey)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool operator !=(DropLogEntry lhs, DropLogEntry rhs)
        {
            if (lhs._entryKey != rhs._entryKey)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public DropLogEntry(string playerName, RSDropLog.SanitizedDrop input)
        {

            _playerName = playerName;
            _dropName = input._dropname;
            _timestamp = input._timestamp.ToString("MM-dd-yyyy HH:mm"); //FIX THIS LATER FOR THE LOVE OF GOD

            // Calculate unique ID to ensure no duplicates
            // playerName + timestamp is an easy way to get a unique ID for each drop
            _entryKey = _timestamp + " " + _playerName + " " + _dropName;

            if (EntryKeyCorrupted)
            {
                throw new DataException("DropLogEntry data corrupted: Entry key does not match expected result.");
            }
        }

        // Automatically generates a full list of the last 50 drops in the clan.
        public static async Task<List<DropLogEntry>> CreateListFullAuto()
        {
            List<DropLogEntry> output = new();

            List<RSDropLog> dropLogs = await RSDropLog.PullParallelFromJagexAPI(RSDropLog.GetAllVoughtPlayerNames().Result);

            foreach (RSDropLog playerLog in dropLogs)
            {
                foreach (RSDropLog.SanitizedDrop sanitizedDrop in playerLog._sanitizedDropLog)
                {
                    output.Add(new(playerLog._name, sanitizedDrop));
                }
            }
            return output;
        }

        public bool EntryKeyCorrupted => !string.Equals((_timestamp + " " + _playerName + " " + _dropName), _entryKey);

        // sanitization taken care of internally, pass DropLogEntry the drop name as is. 
        private string SanitizeDropName(string dropName)
        {
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;

            return ti.ToTitleCase(dropName.Replace("some ", "").Replace("pair of ", ""));
        }
    }
}