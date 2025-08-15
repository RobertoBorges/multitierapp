namespace BookShop.Web.Services
{
    public static class ImagePathHelper
    {
        public static string GetBookImagePath(string imageName)
        {
            if (string.IsNullOrEmpty(imageName))
            {
                return "/images/books/no-image.jpg";
            }

            return $"/images/books/{imageName}";
        }
    }
}
