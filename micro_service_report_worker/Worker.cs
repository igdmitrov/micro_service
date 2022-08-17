using System;
using ClosedXML.Excel;
using micro_service_shared;
using micro_service_shared.Command;

namespace micro_service_report_worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IBusClient _busClient;

    public Worker(ILogger<Worker> logger, IBusClient busClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _busClient = busClient ?? throw new ArgumentNullException(nameof(busClient));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        _busClient.Subscribe<GenerateReportCommand>(nameof(GenerateReportCommand), GenerateReport<GenerateReportCommand>, stoppingToken);

        await Task.CompletedTask;
    }

    async void GenerateReport<T>(T command) where T : GenerateReportCommand
    {
        //Generate report
        var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("main");

        using (workbook)
        {
            ws.Cell(2, 2).Value = command.ToDate.ToLongDateString();

            string tmpFolder = System.IO.Path.GetTempPath();
            string filePath = tmpFolder + Guid.NewGuid().ToString() + ".xlsx";

            workbook.SaveAs(filePath);

            var url = "SUPABASE_URL";
            var key = "ANON_KEY";

            await Supabase.Client.InitializeAsync(url, key);
            var storage = Supabase.Client.Instance.Storage;
            var bucket = storage.From("test-storage");

            string fileName = $"user_report_{Guid.NewGuid().ToString()}.xlsx";

            _logger.LogInformation(filePath);
            _logger.LogInformation(fileName);

            await bucket.Upload(filePath, fileName);
            string reportUrl = await bucket.CreateSignedUrl(fileName, 3600);

            _busClient.Publish(new SubmitReportCommand(reportUrl), default);
        }
    }

    public override Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Start");
        return base.StartAsync(stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopped");
        await base.StopAsync(cancellationToken);
    }
}

