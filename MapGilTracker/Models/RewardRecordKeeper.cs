using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MapGilTracker.Models
{
    [Serializable]
    public class RewardRecordKeeper
    {
        public Hashtable userTable { get; set; }
        public List<RewardRecord> rewardList { get; set; }

        public RewardRecordKeeper(Configuration config)
        {
            // We want our list to persist between crashes, DCs, etc.
            // so we store/serialize in config and load a ref here. :)
            rewardList = config.rewardList;

            // Populate usertable if rewardList already exists
            userTable = new Hashtable();
            foreach (var record in rewardList)
                if (!userTable.ContainsKey(record.player!))
                    userTable.Add(record.player!, true);
        }

        public RewardRecord AddRecord(int value, string name) => AddRecord(value, name, DateTime.Now);
        public RewardRecord AddRecord(int value, string name, DateTime ts)
        {
            // Add user to userTable, if they don't exist
            // Don't really care about the value here, just abusing hashing for keys
            if (!rewardList.Any(e => e.player == name))
                userTable.Add(name, true);

            // Generate record
            var record = new RewardRecord
            {
                timestamp = ts,
                value = value,
                player = name
            };

            // Append record & return
            rewardList.Add(record);
            return record;
        }

        public void RemoveRecord(RewardRecord record) 
        {
            // Remove record
            rewardList.Remove(record);

            // If player is in no more records, remove from user table
            if (userTable.ContainsKey(record.player!) && !rewardList.Any(e => e.player == record.player))
                userTable.Remove(record.player!);
        }

        public void Clear()
        {
            // Just reset everything
            userTable.Clear();
            rewardList.Clear();
        }
    }
}
