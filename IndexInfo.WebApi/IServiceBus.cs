using System.Threading.Tasks;

namespace IndexInfo.WebApi
{
    public interface IServiceBus
    {
        Task SendCommandAsync(ICommand command);
        void RegisterOnMessageHandlerAndReceiveMessages(params object[] args);
        Task ProcessMessageAsync(params object[] args);
    }
}
