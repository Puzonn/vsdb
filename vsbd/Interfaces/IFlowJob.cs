public interface IFlowJobManager
{
    bool StartClient(string clientId, Flow flow, out string error);
    bool Enqueue(string clientId, Seed seed, out string error);
    Task<bool> StopClient(string clientId);
}