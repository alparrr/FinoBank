using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FinoBankApi.Services
{
    public class PdfService
    {
        public byte[] GenerateTransactionPdf(string title, decimal amount, string date, string sender, string receiver)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Text("FinoBank - Potwierdzenie")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(x =>
                    {
                        x.Item().Text($"Data: {date}");
                        x.Item().Text($"Tytuł: {title}");
                        x.Item().Text($"Kwota: {amount} PLN").Bold();
                        x.Item().Text("--------------------------------");
                        x.Item().Text($"Nadawca: {sender}");
                        x.Item().Text($"Odbiorca: {receiver}");
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(x => x.Span("Wygenerowano elektronicznie").FontSize(10).FontColor(Colors.Grey.Medium));
                });
            })
            .GeneratePdf();
        }
    }
}