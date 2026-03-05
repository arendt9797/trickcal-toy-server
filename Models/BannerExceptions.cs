namespace TrickcalServer.Models;

public class BannerNotFoundException(int bannerId)
    : Exception($"배너를 찾을 수 없습니다. (ID: {bannerId})");

public class BannerExpiredException(int bannerId)
    : Exception($"종료되었거나 비활성화된 배너입니다. (ID: {bannerId})");

public class InvalidBannerConfigException(string message)
    : Exception(message);
