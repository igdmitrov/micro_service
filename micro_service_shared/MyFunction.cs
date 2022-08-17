using System;
namespace micro_service_shared
{
    public delegate void MyFunction<T>(T model) where T : class;
}

