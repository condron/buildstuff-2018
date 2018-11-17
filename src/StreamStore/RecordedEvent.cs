
namespace StreamStore {
    public class RecordedEvent
    {
        public readonly long Position;
        public readonly string Stream;
        public readonly object Event;

        public RecordedEvent(string stream, long position,  object @event) {
            Position = position;
            Stream = stream;
            Event = @event;
        }
    }
}
