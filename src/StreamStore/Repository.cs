using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly Dictionary<string, List<Action<RecordedEvent>>> _subscriptions = new Dictionary<string, List<Action<RecordedEvent>>>();
        private string AccountStreams => Path.Combine(AccountFolder, "streams.stream");
        public void Subscribe(string stream, int position, Action<RecordedEvent> target) {
            if (!_subscriptions.TryGetValue(stream, out var subscriptions)) {
                subscriptions = new List<Action<RecordedEvent>>();
                _subscriptions.Add(stream, subscriptions);
            }
            subscriptions.Add(target);
            var events = ReadStreamToEnd(stream, position);
            foreach (var recordedEvent in events) {
                target(recordedEvent);
            }
        }
        public void Append(string stream, object[] events) {
            if (!File.Exists(GetStreamFile(stream))) {
                RecordNewStream(stream);
            }
            if (!_subscriptions.TryGetValue(stream, out var subscriptions)) {
                subscriptions = new List<Action<RecordedEvent>>(); //if no subscription use an empty list
            }

            var curPos = 0;
            //n.b. realllly inefficient
            if (File.Exists(GetStreamFile(stream))) {
                curPos = File.ReadAllLines(GetStreamFile(stream)).Length;
            }
            using (var streamFile = File.AppendText(GetStreamFile(stream))) {
                foreach (var @event in events) {
                    streamFile.WriteLine(JsonConvert.SerializeObject(@event, _settings));
                    foreach (var target in subscriptions) {
                        var position = curPos;
                        Task.Run(() => target(new RecordedEvent(stream, position, @event)));
                        curPos++;
                    }
                }
            }
        }
        private void RecordNewStream(string accountNumber) {
            using (var stream = File.AppendText(AccountStreams)) {
                stream.WriteLine(JsonConvert.SerializeObject(new AccountStreamAdded(accountNumber), _settings));
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
