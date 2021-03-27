using HomeCenter.Storage.RavenDB;
using System;

namespace HomeCenter.Actors.Tests.Builders
{
    internal class FakeKeyProvider : LogKeyProvider
    {
        public override string GetKey() => Guid.NewGuid().ToString();
    }
}