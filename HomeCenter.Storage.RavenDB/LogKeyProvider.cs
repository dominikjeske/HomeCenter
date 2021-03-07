using System;

namespace HomeCenter.Storage.RavenDB
{
    public class LogKeyProvider
    {
        public virtual string GetKey() => $"log_{DateTime.Now:yyyyMMddHHmmssfffffff}";
    }
}