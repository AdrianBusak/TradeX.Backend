namespace TradeX.Infrastructure.BlobStorageClient.Interfaces;

public interface IBlobStorageService
{
    /// <summary>
    /// Učitava stream datoteke na Azure Blob Storage.
    /// </summary>
    /// <param name="fileStream">Stream podataka datoteke.</param>
    /// <param name="blobName">Puna putanja unutar kontejnera (npr. Encounters/guid/filename.jpg).</param>
    /// <param name="contentType">MIME tip datoteke (npr. image/jpeg).</param>
    /// <param name="cancellationToken">Token za otkazivanje operacije.</param>
    /// <returns>Relativna putanja (blobPath) koja je spremljena.</returns>
    Task<string> UploadAsync(Stream fileStream, string blobName, string contentType, CancellationToken cancellationToken);

    /// <summary>
    /// Briše datoteku s Azure Blob Storagea.
    /// </summary>
    /// <param name="blobPath">Relativna putanja spremljena u bazi.</param>
    /// <param name="cancellationToken">Token za otkazivanje operacije.</param>
    Task DeleteAsync(string blobPath, CancellationToken cancellationToken);

    /// <summary>
    /// Generira privremeni SAS (Shared Access Signature) URL za pristup datoteci.
    /// </summary>
    /// <param name="blobPath">Relativna putanja spremljena u bazi.</param>
    /// <param name="expiryMinutes">Trajanje linka u minutama.</param>
    /// <returns>Puni URL s tokenom za frontend.</returns>
    string GetSasUrl(string blobPath, int expiryMinutes = 60);
}
