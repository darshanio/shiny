namespace Shiny.Jobs.Infrastructure;


public interface IJobNativeRegister
{
    void Register(JobInfo jobInfo);
    void Cancel(JobInfo jobInfo);
}