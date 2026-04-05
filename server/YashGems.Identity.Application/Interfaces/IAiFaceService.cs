namespace YashGems.Identity.Application.Interfaces;

public interface IAiFaceService
{
    Task<double> CompareFacesAsync(string facePhotoUrl, string idCardFrontUrl);
}
