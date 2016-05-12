using System;

namespace Shared.Services
{
    public interface IUriBuilderService
    {
        string ApplicationInstance { get; set; }
        string ServiceInstance { get; set; }
        Uri ToUri();
    }
}
