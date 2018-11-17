using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace StreamStore {
    public class Repository {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings {
            TypeNameHandling = TypeNameHandling.Objects
        };
        public string AccountFolder = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.CommonApplicationData), "Workshop/Accounts");
        public string GetStreamFile(string accountNumber) {
            return Path.Combine(AccountFolder, $"{accountNumber}.stream");
        }

        private string AccountStreams => Path.Combine(AccountFolder, "streams.stream");

        public void Append(string accountNumber, object[] events) {
            if (!File.Exists(GetStreamFile(accountNumber))) {
                RecordNewStream(accountNumber);
            }
            using (var stream = File.AppendText(GetStreamFile(accountNumber))) {
                foreach (var @event in events) {
                    stream.WriteLine(JsonConvert.SerializeObject(@event, _settings));
                }
            }
        }
        private void RecordNewStream(string accountNumber)
        {
            using (var stream = File.AppendText(AccountStreams)) {
                stream.WriteLine(JsonConvert.SerializeObject(new AccountStreamAdded(accountNumber),_settings));
            }
        }
        public List<RecordedEvent> ReadStreamToEnd(string accountNumber) {
            return ReadStreamToEnd(accountNumber, 0);
        }
        public List<RecordedEvent> ReadStreamToEnd(string accountNumber, int from) {
            //account numbers are always int
            var recordedEvents = !int.TryParse(accountNumber, out _) ? 
                File.ReadLines(AccountStreams).ToList() : 
                File.ReadLines(GetStreamFile(accountNumber)).ToList();
           
            var @events = new List<RecordedEvent>();
            for (int i = from; i < recordedEvents.Count; i++) {
                var recordedEvent = recordedEvents[i];
                @events.Add(new RecordedEvent(
                    accountNumber,
                    i,
                    JsonConvert.DeserializeObject(recordedEvent, _settings)));
            }
            return @events;
        }

        public RecordedEvent ReadStreamEvent(string accountNumber, int position) {
            var recordedEvents = File.ReadLines(GetStreamFile(accountNumber)).ToList();
            if (position < 0 || position > recordedEvents.Count - 1) {
                throw new Exception($"Event Position not found. Stream {accountNumber}, Position {position}");
            }
            return new RecordedEvent(
                    accountNumber,
                    position,
                    JsonConvert.DeserializeObject(recordedEvents[position], _settings));
        }
    }
}
