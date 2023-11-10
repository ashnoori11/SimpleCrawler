namespace SimpleCrwaler.Common;

public static class FileExtentions
{
    const string sourcePattern = @"<source\s+(?:[^>]*?\s+)?src=([""'])(.*?)\1";
    const string videosPattern = @"<video\s+(?:[^>]*?\s+)?src=([""'])(.*?)\1";
    const string imagePattern = @"<img\s+(?:[^>]*?\s+)?src=([""'])(.*?)\1";
    const string svgPattern = @"<svg\s+(?:[^>]*?\s+)?src=([""'])(.*?)\1";


    public static string DownloadHtml(this string url)
    {
        using WebClient client = new WebClient();
        return client.DownloadString(url);
    }

    public static void SaveHtmlToFile(this string html, string fileName) 
        => File.WriteAllText(fileName, html);

    public static string[] ExtractUrls(this string html, string domainName)
    {
        //string sourcePattern = @"<source\s+(?:[^>]*?\s+)?src=([""'])(.*?)\1";
        //string videosPattern = @"<video\s+(?:[^>]*?\s+)?src=([""'])(.*?)\1";
        //string imagePattern = @"<img\s+(?:[^>]*?\s+)?src=([""'])(.*?)\1";
        //string svgPattern = @"<svg\s+(?:[^>]*?\s+)?src=([""'])(.*?)\1";

        MatchCollection matches = Regex.Matches(html, imagePattern, RegexOptions.IgnoreCase);
        MatchCollection matchesSources = Regex.Matches(html, sourcePattern, RegexOptions.IgnoreCase);
        MatchCollection matchesVideos = Regex.Matches(html, videosPattern, RegexOptions.IgnoreCase);
        MatchCollection matchesSvgs = Regex.Matches(html, svgPattern, RegexOptions.IgnoreCase);

        List<string> urls = new List<string>();

        if (domainName.EndsWith("/"))
            domainName = domainName.Substring(0, domainName.Length - 1);

        urls = AddMatchesToList(matches, domainName);
        urls.AddRange(AddMatchesToList(matchesSources, domainName));
        urls.AddRange(AddMatchesToList(matchesVideos, domainName));
        urls.AddRange(AddMatchesToList(matchesSvgs, domainName));

        string[] result = urls.ToArray();
        return result;
    }

    public static void DownloadFiles(this string[] urls, string? downloadPath, out int numberOfDownloadedFiles, bool requiredSsl = true)
    {
        if (!Directory.Exists(downloadPath))
            Directory.CreateDirectory(downloadPath?? string.Empty);

        using WebClient client = new WebClient();
        int counter = 0;

        if (requiredSsl)
        {
            urls = urls.Where(a => a.StartsWith("https://")).ToArray();
        }
        else
        {
            urls = urls.Where(a => a.StartsWith("https://") || a.StartsWith("http://") || a.StartsWith("www.")).ToArray();
        }

        for (int i = 0; i < urls.Length; i++)
        {
            string url = urls[i];
            string fileName = Path.GetFileName(url);
            string filePath = Path.Combine(downloadPath, fileName);

            Console.Write($"Downloading: {url} [");

            try
            {
                client.DownloadFile(url, filePath);
                AnimateLoading();
                Console.WriteLine("] Done");
            }
            catch (Exception exp)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"can not download from this source - invalid address : {exp.Message}");
                Console.ResetColor();
                continue;
            }

            counter++;
        }

        numberOfDownloadedFiles = counter;
    }

    public static List<string> ExtractScriptUrls(this string html, string domainName)
    {
        string pattern = @"<script.*?src=[""'](.*?)[""']";
        MatchCollection matches = Regex.Matches(html, pattern, RegexOptions.IgnoreCase);

        if (domainName.StartsWith("/"))
            domainName = domainName.Substring(0, domainName[domainName.Length - 2]);

        List<string> urls = new List<string>();
        foreach (Match match in matches)
        {
            string url = match.Groups[1].Value;
            url = StripQueryString(url);

            if (url.Contains(":text/javascript;"))
                continue;

            if (url.StartsWith("/"))
                urls.Add($"{domainName}{url}");
            else if (url.StartsWith("https://") || url.StartsWith("https://"))
            {
                urls.Add(url);
            }
            else
                urls.Add($"{domainName}/{url}");
        }

        return urls;
    }

    public static List<string> ExtractCssUrls(this string html, string domainName)
    {
        string pattern = @"<link.*?href=[""'](.*?)[""'].*?rel=[""']stylesheet[""']";
        MatchCollection matches = Regex.Matches(html, pattern, RegexOptions.IgnoreCase);

        if (domainName.StartsWith("/"))
            domainName = domainName.Substring(0, domainName[domainName.Length - 2]);

        List<string> urls = new List<string>();
        foreach (Match match in matches)
        {
            string url = match.Groups[1].Value;
            url = StripQueryString(url);
            if (url.StartsWith("/"))
            {
                urls.Add($"{domainName}{url}");
            }
            else if (url.StartsWith("https://") || url.StartsWith("http://"))
            {
                urls.Add(url);
            }
            else
                urls.Add($"{domainName}/{url}");
        }

        return urls;
    }

    public static void DownloadFiles(this List<string> urls, string? downloadPath, out int downloadFileCount, bool requiredSsl = true)
    {
        if (!Directory.Exists(downloadPath))
            Directory.CreateDirectory(downloadPath ?? string.Empty);

        using WebClient client = new WebClient();
        if (requiredSsl)
        {
            urls = urls.Where(a => a.StartsWith("https://")).ToList();
        }
        else
        {
            urls = urls.Where(a => a.StartsWith("https://") || a.StartsWith("http://")).ToList();
        }

        int counter = 0;
        foreach (string url in urls)
        {
            string fileName = Path.GetFileName(url);
            string filePath = Path.Combine(downloadPath, fileName);

            Console.Write($"Downloading: {url} [");

            try
            {
                client.DownloadFile(url, filePath);
                AnimateLoading();
                Console.WriteLine("] Done");
            }
            catch (Exception exp)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"can not download from this source - invalid address : {exp.Message}");
                Console.ResetColor();
                continue;
            }

            counter++;
        }

        downloadFileCount = counter;
    }

    static string StripQueryString(string url)
    {
        int queryIndex = url.IndexOf('?');
        if (queryIndex != -1)
        {
            url = url.Substring(0, queryIndex);
        }
        return url;
    }

    static List<string> AddMatchesToList(MatchCollection matchesVideos, string domainName)
    {
        List<string> urls = new List<string>();
        for (int i = 0; i < matchesVideos.Count; i++)
        {
            if (matchesVideos[i].Groups[2].Value.Contains("."))
            {
                if (matchesVideos[i].Groups[2].Value.StartsWith("/"))
                {
                    urls.Add($"{domainName}{matchesVideos[i].Groups[2].Value}");
                }
                else if (matchesVideos[i].Groups[2].Value.StartsWith("https://") || matchesVideos[i].Groups[2].Value.StartsWith("http://"))
                {
                    urls.Add(matchesVideos[i].Groups[2].Value);
                }
                else
                {
                    urls.Add($"{domainName}/{matchesVideos[i].Groups[2].Value}");
                }
            }
        }

        return urls;
    }

    static void AnimateLoading(int seconds = 3)
    {
        string[] loadingChars = { "|", "/", "-", "\\" };
        int index = 0;

        DateTime until = DateTime.Now.AddSeconds(seconds);
        while (DateTime.Now < until)
        {
            Console.Write(loadingChars[index]);
            Thread.Sleep(100);

            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
            index = (index + 1) % loadingChars.Length;
        }
    }
}
