using System.Diagnostics;

namespace SimpleCrwaler.Common;

public class Constants
{
    public const string GetUrlMessage = "Enter the URL:";
    public const string InvalidUrlInputError = "invalid input url - url can not be null or empty";
    public const string GetHtmlFileNameMessage = "Enter the file name to save HTML:";
    public const string GetDownloadPathMessage = "Enter the download path:";
    public const string StartCrwalingMessage = "Crawling and downloading...";
    public const string InvalidSavePathMessage = "invalid address - The entered path to save the html file is not correct and does not exist";
    public const string CrawlingCompletedMessage = "Crawling and downloading completed :";
    public const string DownloadedFilesCountMessage = "{0} files download from html page";
    public const string NoDownloadHappensMessage = "Nothing was downloaded because the download path was not entered";
    public const string ExceptionErrorTemplate = "an error occured - message : {0} - stackTrace : {1}";
    public const string QuitGuide = "Press the Escape (Esc) key to quit";

    internal static void Print(string text) => Console.WriteLine(text);
    internal static void Print(string text, ConsoleColor? color = null)
    {
        if (color is object)
            Console.ForegroundColor = (ConsoleColor)color;

        Console.WriteLine(text);
        Console.ResetColor();
    }
}
