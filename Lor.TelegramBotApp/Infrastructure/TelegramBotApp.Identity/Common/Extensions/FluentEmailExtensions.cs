using System.Dynamic;
using System.Reflection;
using FluentEmail.Core;
using TelegramBotApp.Identity.Services.EmailService.EmailContext.ContentProvider;

namespace TelegramBotApp.Identity.Common.Extensions;

public static class FluentEmailExtensions
{
    public static IFluentEmail ApplyTemplate(
        this IFluentEmail email,
        IEmailContentProvider contentProvider,
        object[] values)
    {
        var keys = contentProvider.GetTemplateKeys.ToArray();

        if (keys.Length != values.Length) throw new ArgumentException("The number of keys and values must match.");

        var expandoObject = new ExpandoObject();
        var collection = expandoObject as ICollection<KeyValuePair<string, object>>;
        var dictionary = keys.Zip(values, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);

        foreach (var kvp in dictionary) collection.Add(kvp);

        dynamic dynamic = expandoObject;

        return email.UsingTemplateFromEmbedded(
            contentProvider.PathToTemplate,
            dynamic,
            Assembly.GetExecutingAssembly());
    }
}