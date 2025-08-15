using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Hosting;

namespace BookShop.Web.TagHelpers
{
    [HtmlTargetElement("img", Attributes = "book-image")]
    public class BookImageTagHelper : TagHelper
    {
        private readonly IWebHostEnvironment _env;

        public BookImageTagHelper(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HtmlAttributeName("book-image")]
        public string BookImage { get; set; }

        [HtmlAttributeName("alt")]
        public string Alt { get; set; }

        [HtmlAttributeName("class")]
        public string Class { get; set; }

        [HtmlAttributeName("style")]
        public string Style { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            // Check if the image file exists
            string imagePath = Path.Combine(_env.WebRootPath, "images", "books", BookImage ?? "no-image.jpg");
            bool imageExists = File.Exists(imagePath);
            string imageUrl = imageExists ? $"/images/books/{BookImage}" : "/images/books/no-image.jpg";

            // Set the src attribute
            output.Attributes.SetAttribute("src", imageUrl);

            // Set additional attributes if provided
            if (!string.IsNullOrEmpty(Alt))
            {
                output.Attributes.SetAttribute("alt", Alt);
            }

            if (!string.IsNullOrEmpty(Class))
            {
                output.Attributes.SetAttribute("class", Class);
            }

            if (!string.IsNullOrEmpty(Style))
            {
                output.Attributes.SetAttribute("style", Style);
            }

            // Log to debug
            System.Diagnostics.Debug.WriteLine($"Image path: {imagePath}, Exists: {imageExists}, URL: {imageUrl}");
        }
    }
}
