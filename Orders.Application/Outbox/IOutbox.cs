namespace Orders.Application.Outbox;

public interface IOutbox
{
    void Enqueue(string type, string payloadJson);
}