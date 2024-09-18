namespace GroupScheduleApp.AppCommunication.Interfaces;

public interface ICommunicationClient
{
    Task StartAsync();
    Task StopAsync();
}