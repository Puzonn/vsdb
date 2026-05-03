public interface IFlowJobManager
{
    bool StartClient(string clientId, string projectId, out string error);
    bool Enqueue(string clientId, Seed seed, out string error);
    Task<bool> StopClient(string clientId);
}