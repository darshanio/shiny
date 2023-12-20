using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shiny.Stores;
using Shiny.Support.Repositories;

namespace Shiny.Jobs.Infrastructure;

public class DefaultJobManager : IJobManager
{
    
    readonly IServiceProvider services;
    readonly IRepository repository;
    readonly IObjectStoreBinder storeBinder;
    readonly IJobNativeRegister register;
    readonly ILogger logger;


    public DefaultJobManager(
        IServiceProvider services,
        IRepository repository,
        IObjectStoreBinder storeBinder,
        IJobNativeRegister register,
        ILogger<DefaultJobManager> logger
    )
    {
        this.services = services;
        this.repository = repository;
        this.storeBinder = storeBinder;
        this.register = register;
        this.logger = logger;
    }

    public bool IsRunning { get; private set; }

    public IList<JobInfo> GetJobs()
        => this.repository.GetList<JobInfo>();

    public JobInfo? GetJob(string jobIdentifier)
        => this.repository.Get<JobInfo>(jobIdentifier);


    public void Cancel(string jobIdentifier)
    {
        var job = this.repository.Get<JobInfo>(jobIdentifier);
        if (job != null)
        {
            this.register.Cancel(job);
            this.repository.Remove<JobInfo>(jobIdentifier);
        }
    }


    public virtual void CancelAll()
    {
        var jobs = this.repository.GetList<JobInfo>();
        foreach (var job in jobs)
        {
            if (!job.IsSystemJob)
            {
                this.register.Cancel(job);
                this.repository.Remove(job);
            }
        }
    }


    public void Register(JobInfo jobInfo) => throw new NotImplementedException();
    public Task<AccessState> RequestAccess() => throw new NotImplementedException();
    public Task<IEnumerable<JobRunResult>> Run(CancellationToken cancelToken = default) => throw new NotImplementedException();
    public void RunTask(string taskName, Func<CancellationToken, Task> task) => throw new NotImplementedException();
}