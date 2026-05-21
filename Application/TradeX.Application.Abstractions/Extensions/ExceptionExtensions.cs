using System.Text;

namespace TradeX.Application.Abstractions.Extensions;

public static class ExceptionExtensions
{
    public static string ToDeepString(this Exception ex, bool appendErrorMessageOnly = false)
    {
        var sb = new StringBuilder();
        AppendException(sb, ex, 0, appendErrorMessageOnly);
        return sb.ToString();
    }

    private static void AppendException(StringBuilder sb, Exception ex, int level, bool appendErrorMessageOnly)
    {
        string indent = new(' ', level * 2);

        sb.AppendLine($"{indent}Exception Type: {ex.GetType().FullName}");
        sb.AppendLine($"{indent}Message: {ex.Message}");

        if (!appendErrorMessageOnly)
        {
            sb.AppendLine($"{indent}HResult: {ex.HResult}");
            sb.AppendLine($"{indent}Source: {ex.Source}");
            sb.AppendLine($"{indent}TargetSite: {ex.TargetSite}");
            sb.AppendLine($"{indent}StackTrace:");
            sb.AppendLine($"{indent}{ex.StackTrace}");


            if (ex.Data?.Count > 0)
            {
                sb.AppendLine($"{indent}Data:");
                foreach (var key in ex.Data.Keys)
                {
                    sb.AppendLine($"{indent}  {key}: {ex.Data[key]}");
                }
            }
        }

        if (ex is AggregateException aggEx && aggEx.InnerExceptions.Count > 0)
        {
            sb.AppendLine($"{indent}Inner Exceptions ({aggEx.InnerExceptions.Count}):");
            foreach (var inner in aggEx.InnerExceptions)
            {
                AppendException(sb, inner, level + 1, appendErrorMessageOnly);
            }
            return;
        }

        if (ex.InnerException != null)
        {
            sb.AppendLine($"{indent}Inner Exception:");
            AppendException(sb, ex.InnerException, level + 1, appendErrorMessageOnly);
        }
    }
}
