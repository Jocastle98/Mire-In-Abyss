using R3;

public interface IEventBus
{
    void Publish<T>(T message);
    Observable<T> Receive<T>();
}