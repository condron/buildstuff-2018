using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace StreamStore {
    public class Repository {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings {
            TypeNameHandling = TypeNameHandling.Objects
        };
        public string AccountFolder = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.CommonApplicationData), "Workshop/Accounts");
        public string GetStreamFile(uint accountNumber) {
            return Path.Combine(AccountFolder, $"{accountNumber}.stream");
        }
        public void Append(uint accountNumber, object[] events) {

            using (var stream = File.AppendText(GetStreamFile(accountNumber))) {
                foreach (var @event in events) {
                    stream.WriteLine(JsonConvert.SerializeObject(@event, _settings));
                }
            }
        }
        public List<object> ReadAll(uint accountNumber) {

            var recordedEvents = File.ReadLines(GetStreamFile(accountNumber));
            var @events = new List<object>();
            foreach (var recordedEvent in recordedEvents) {
                @events.Add(JsonConvert.DeserializeObject(recordedEvent, _settings));
            }
            return @events;
        }
    }
}
