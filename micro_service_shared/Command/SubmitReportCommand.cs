using System;
namespace micro_service_shared.Command
{
    public record SubmitReportCommand(string ReportUrl, string Command = "SUBMIT_REPORT");
}

