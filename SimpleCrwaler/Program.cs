namespace SimpleCrwaler;
class Program : Constants
{
    static void Main(string[] args)
    {
        string error = string.Empty;
        string stacktrace = string.Empty;
        string? downloadPath = string.Empty;
        string? fileName = string.Empty;
        string? url = string.Empty;
        string html = string.Empty;
        ConsoleKeyInfo cki;

        do
        {
            try
            {
                Print(GetUrlMessage);
                url = Console.ReadLine();
                url.HasValue(()=> new ArgumentNullException(InvalidUrlInputError));
                url = url.RemoveParameters();

                Print(GetHtmlFileNameMessage);
                fileName = Console.ReadLine();

                Print(GetDownloadPathMessage);
                downloadPath = Console.ReadLine();

                Print(StartCrwalingMessage);

                // Download HTML
                html = url.DownloadHtml();

                if (!downloadPath.HasValue(() => FileExtentions.SaveHtmlToFile(html,$"{downloadPath}/{fileName}")))
                {
                    throw new DirectoryNotFoundException(InvalidSavePathMessage);
                }

                // Extract URLs of JavaScript and CSS files
                List<string> jsUrls = html.ExtractScriptUrls(url);
                List<string> cssUrls = html.ExtractCssUrls(url);

                // Extract Images, videos, svg urls from HTML
                string[] urls = html.ExtractUrls(url);

                int fileDownloadCount = 0;
                int jsDownloadCount = 0;
                int cssDownloadCount = 0;

                if (downloadPath.HasValue())
                {
                    // Download JavaScript files
                    jsUrls.DownloadFiles(downloadPath, out jsDownloadCount);

                    // Download CSS files
                    cssUrls.DownloadFiles(downloadPath, out cssDownloadCount);

                    // Download files
                    urls.DownloadFiles(downloadPath, out fileDownloadCount);

                    Print(CrawlingCompletedMessage);
                    Print(string.Format(DownloadedFilesCountMessage, fileDownloadCount + cssDownloadCount + jsDownloadCount));
                }
                else
                {
                    Print(NoDownloadHappensMessage, ConsoleColor.DarkBlue);
                }
            }
            catch (Exception exp)
            {
                error = exp.Message;
                stacktrace = exp.StackTrace ?? string.Empty;
            }

            if (!string.IsNullOrEmpty(error))
            {
                Print(string.Format(ExceptionErrorTemplate,error, stacktrace), ConsoleColor.Red);
            }

            Print(QuitGuide);
            cki = Console.ReadKey();

        } while (cki.Key != ConsoleKey.Escape);
    }
}