using Raven.Client.Documents.Session;
using System;

namespace HomeCenter.Storage.RavenDB
{
    internal static class DocumentSessionExtensions
    {
        public static void SetExpirationDate(this IDocumentSession documentSession, TimeSpan? expiration, object data)
        {
            if (expiration.HasValue)
            {
                var metaData = documentSession.Advanced.GetMetadataFor(data);
                metaData[Raven.Client.Constants.Documents.Metadata.Expires] = DateTime.UtcNow.Add(expiration.Value);
            }
        }

        public static void SetExpirationDate(this IAsyncDocumentSession documentSession, TimeSpan? expiration, object data)
        {
            if (expiration.HasValue)
            {
                var metaData = documentSession.Advanced.GetMetadataFor(data);
                metaData[Raven.Client.Constants.Documents.Metadata.Expires] = DateTime.UtcNow.Add(expiration.Value);
            }
        }
    }
}