using System;
namespace micro_service_shared.Command
{
    public record GenerateReportCommand(DateTime ToDate, string Command = "GENERATE_REPORT");
}

