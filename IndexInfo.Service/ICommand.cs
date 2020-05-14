namespace IndexInfo.Service
{
    public interface ICommand
    {
        string QueueName {get;set;}
        object Message {get;}
    }
}