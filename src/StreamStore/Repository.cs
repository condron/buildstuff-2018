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


        public void Append(string accountNumber, object[] events) {

            using (var stream = File.AppendText(GetStreamFile(accountNumber))) {
                foreach (var @event in events) {
                    stream.WriteLine(JsonConvert.SerializeObject(@event, _settings));
                }
            }
        }
        public List<RecordedEvent> ReadStreamToEnd(string accountNumber) {
            var recordedEvents = File.ReadLines(GetStreamFile(accountNumber)).ToList();
            var @events = new List<RecordedEvent>();
            for (int i = 0; i < recordedEvents.Count; i++) {
                var recordedEvent = recordedEvents[i];
                @events.Add( new RecordedEvent(
                    accountNumber.ToString(),
                    i,
                    JsonConvert.DeserializeObject(recordedEvent, _settings)));
            }
            return @events;
        }
    }
}
