using CarRental.Core.Interfaces;
using CarRental.Core.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CarRental.Infrastructure.Services
{
    public class PdfGeneratorService : IPdfGeneratorService
    {
        public async Task<byte[]> GenerateRentalContractAsync(Rental rental, CompanySettings companySettings, string contractNumber)
        {
            return await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(30);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                        page.Header().Element(ComposeHeader);
                        page.Content().Element(ComposeContent);
                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                        });
                    });
                });

                return document.GeneratePdf();

                void ComposeHeader(IContainer container)
                {
                    container.Column(column =>
                    {
                        // Logo y datos de la empresa
                        column.Item().Row(row =>
                        {
                            // Logo (si existe)
                            row.ConstantItem(80).Column(logoColumn =>
                            {
                                if (companySettings.Logo != null && companySettings.Logo.Length > 0)
                                {
                                    logoColumn.Item().Image(companySettings.Logo).FitArea();
                                }
                            });

                            // Información de la empresa
                            row.RelativeItem().PaddingLeft(10).Column(infoColumn =>
                            {
                                infoColumn.Item().Text(companySettings.CompanyName)
                                    .FontSize(14).Bold().FontColor(Colors.Blue.Darken2);

                                if (!string.IsNullOrEmpty(companySettings.ActivityDescription))
                                {
                                    infoColumn.Item().Text(companySettings.ActivityDescription)
                                        .FontSize(7).Italic();
                                }

                                infoColumn.Item().PaddingTop(5).Text(text =>
                                {
                                    text.Span("📍 ").FontSize(8);
                                    text.Span(companySettings.Address).FontSize(8);
                                });

                                infoColumn.Item().Text(text =>
                                {
                                    text.Span("☎ ").FontSize(8);
                                    text.Span($"{companySettings.Phone1}").FontSize(8);
                                    if (!string.IsNullOrEmpty(companySettings.Phone2))
                                    {
                                        text.Span($" - {companySettings.Phone2}").FontSize(8);
                                    }
                                });

                                infoColumn.Item().Text(text =>
                                {
                                    text.Span("✉ ").FontSize(8);
                                    text.Span(companySettings.Email).FontSize(8);
                                });

                                if (!string.IsNullOrEmpty(companySettings.Website))
                                {
                                    infoColumn.Item().Text(text =>
                                    {
                                        text.Span("🌐 ").FontSize(8);
                                        text.Span(companySettings.Website).FontSize(8);
                                    });
                                }
                            });
                        });

                        // Línea divisoria
                        column.Item().PaddingTop(10).PaddingBottom(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);

                        // Información del contrato
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text($"{companySettings.City}").FontSize(9);
                                col.Item().Text($"{rental.CreatedDate:dd/MM/yyyy}").FontSize(9);
                            });

                            row.RelativeItem().AlignRight().Column(col =>
                            {
                                col.Item().Text("CONTRATO No:").FontSize(9).Bold();
                                col.Item().Text(contractNumber).FontSize(11).Bold().FontColor(Colors.Red.Darken1);
                            });
                        });
                    });
                }

                void ComposeContent(IContainer container)
                {
                    container.PaddingVertical(10).Column(column =>
                    {
                        // DATOS DEL ARRENDATARIO
                        column.Item().Element(SectionTitle).Text("NOMBRES Y APELLIDOS DEL ARRENDATARIO:");
                        column.Item().PaddingBottom(5).Row(row =>
                        {
                            row.RelativeItem().Element(DataField).Text($"{rental.Customer?.FirstName} {rental.Customer?.LastName}");
                        });

                        // TIPO Y NÚMERO DE DOCUMENTO
                        column.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Element(LabelStyle).Text("TIPO DE DOCUMENTO:");
                                col.Item().Element(DataField).Text(rental.Customer?.TypeOfDocument.ToString() ?? "Cédula");
                            });

                            row.RelativeItem().PaddingLeft(10).Column(col =>
                            {
                                col.Item().Element(LabelStyle).Text("No. DE DOCUMENTO:");
                                col.Item().Element(DataField).Text(rental.Customer?.DocumentNumber ?? "");
                            });
                        });

                        // DIRECCIÓN DOMICILIO Y TRABAJO
                        column.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Element(LabelStyle).Text("DIRECCIÓN DOMICILIO:");
                                col.Item().Element(DataField).Text(rental.Customer?.Address ?? "");
                            });

                            row.RelativeItem().PaddingLeft(10).Column(col =>
                            {
                                col.Item().Element(LabelStyle).Text("DIRECCIÓN TRABAJO:");
                                col.Item().Element(DataField).Text("N/A");
                            });
                        });

                        // TELÉFONOS Y DIRECCIÓN EXTERIOR
                        column.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Element(LabelStyle).Text("TELÉFONO FIJO:");
                                col.Item().Element(DataField).Text(rental.Customer?.PhoneNumber ?? "");
                            });

                            row.RelativeItem().PaddingLeft(10).Column(col =>
                            {
                                col.Item().Element(LabelStyle).Text("TELÉFONO MÓVIL:");
                                col.Item().Element(DataField).Text(rental.Customer?.PhoneNumber ?? "");
                            });

                            row.RelativeItem().PaddingLeft(10).Column(col =>
                            {
                                col.Item().Element(LabelStyle).Text("DIRECCIÓN EXTERIOR:");
                                col.Item().Element(DataField).Text(rental.DestinationType != Rental.RentalDestinationType.Local
                                    ? rental.DestinationCityName ?? "" : "LOCAL");
                            });
                        });

                        // INFORMACIÓN DEL CONDUCTOR
                        column.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Element(LabelStyle).Text("CONDUCTOR:");
                                col.Item().Element(DataField).Text(rental.DriverName ?? "");
                            });

                            row.RelativeItem().PaddingLeft(10).Column(col =>
                            {
                                col.Item().Element(LabelStyle).Text("LICENCIA: TIPO Y FECHA CADUCIDAD:");
                                col.Item().Element(DataField).Text($"Tipo {rental.DriverLicenseType}     {rental.DriverLicenseExpirationDate:dd/MM/yyyy}");
                            });

                            row.RelativeItem().PaddingLeft(10).Column(col =>
                            {
                                col.Item().Element(LabelStyle).Text("CORREO ELECTRÓNICO:");
                                col.Item().Element(DataField).Text(rental.Customer?.Email ?? "");
                            });
                        });

                        // SECCIÓN GARANTÍA - TARIFA
                        column.Item().PaddingTop(15).Element(ComposeGuaranteeSection);

                        // CARACTERÍSTICAS DEL VEHÍCULO
                        column.Item().PaddingTop(10).Element(ComposeVehicleSection);

                        // OBSERVACIONES
                        column.Item().PaddingTop(10).Column(col =>
                        {
                            col.Item().Element(SectionTitle).Text("Observaciones Generales:");
                            col.Item().Border(1).BorderColor(Colors.Grey.Medium).Padding(5)
                                .MinHeight(40).Text(rental.Notes ?? "El vehículo debe ser entregado limpio\ncaso contrario se cobrará la lavada para\nproceder con su revisión");
                        });

                        // ENTREGA Y RECEPCIÓN
                        column.Item().PaddingTop(10).Element(ComposeDeliverySection);

                        // ACCESORIOS
                        column.Item().PaddingTop(10).Element(ComposeAccessoriesSection);

                        // COMBUSTIBLE
                        column.Item().PaddingTop(10).Element(ComposeFuelSection);
                    });
                }

                IContainer SectionTitle(IContainer container)
                {
                    return container
                        .Background(Colors.Grey.Darken2)
                        .Padding(5);
                }

                IContainer LabelStyle(IContainer container)
                {
                    return container;
                }

                IContainer DataField(IContainer container)
                {
                    return container
                        .Border(0.5f)
                        .BorderColor(Colors.Grey.Medium)
                        .Padding(3);
                }

                void ComposeGuaranteeSection(IContainer container)
                {
                    container.Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            // GARANTÍA
                            row.RelativeItem(2).Column(col =>
                            {
                                col.Item().Background(Colors.Grey.Darken2).Padding(5)
                                    .Text("GARANTÍA").FontColor(Colors.White).Bold().FontSize(9);
                                col.Item().Border(1).Padding(40); // Espacio vacío
                            });

                            // TARIFA SIN IMPUESTOS
                            row.RelativeItem(3).PaddingLeft(5).Column(col =>
                            {
                                col.Item().Background(Colors.Grey.Darken1).Padding(5)
                                    .Text("TARIFA SIN IMPUESTOS").FontColor(Colors.White).Bold().FontSize(9);

                                col.Item().Border(1).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                    });

                                    // Encabezados
                                    table.Cell().BorderBottom(0.5f).Padding(3).Text("No. DE DÍAS").FontSize(8).Bold();
                                    table.Cell().BorderBottom(0.5f).Padding(3).Text("VALOR DEL DÍA").FontSize(8).Bold();
                                    table.Cell().BorderBottom(0.5f).Padding(3).Text("VALOR TOTAL").FontSize(8).Bold();

                                    // Valores - Días
                                    var days = Math.Max(1, (rental.EndDate - rental.StartDate).Days);
                                    table.Cell().Padding(3).Text(days.ToString()).FontSize(9);
                                    table.Cell().Padding(3).Text(rental.DailyRate.ToString("C")).FontSize(9);
                                    table.Cell().Padding(3).Text((rental.DailyRate * days).ToString("C")).FontSize(9);

                                    // Fila KMS
                                    table.Cell().BorderTop(0.5f).Padding(3).Text("KMS. EXCEDIDOS").FontSize(8).Bold();
                                    table.Cell().BorderTop(0.5f).Padding(3).Text("VALOR POR KM.").FontSize(8).Bold();
                                    table.Cell().BorderTop(0.5f).Padding(3).Text("VALOR TOTAL").FontSize(8).Bold();

                                    table.Cell().Padding(3).Text("-").FontSize(9);
                                    table.Cell().Padding(3).Text("-").FontSize(9);
                                    table.Cell().Padding(3).Text("-").FontSize(9);

                                    // SUMA TOTAL (Ocupa 2 columnas + 1 columna)
                                    table.Cell().BorderTop(1).Padding(3).Background(Colors.Grey.Darken2)
                                        .Text("SUMA TOTAL $").FontColor(Colors.White).Bold().FontSize(9);
                                    table.Cell().BorderTop(1).Padding(3).Background(Colors.Grey.Darken2)
                                        .Text("").FontColor(Colors.White).Bold().FontSize(9); // Celda vacía
                                    table.Cell().BorderTop(1).Padding(3).Background(Colors.Grey.Lighten3)
                                        .Text(rental.TotalCost.ToString("C")).Bold().FontSize(10);
                                });
                            });
                        });
                    });
                }

                void ComposeVehicleSection(IContainer container)
                {
                    container.Column(column =>
                    {
                        column.Item().Background(Colors.Grey.Darken1).Padding(5)
                            .Text("CARACTERÍSTICAS DEL VEHÍCULO").FontColor(Colors.White).Bold().FontSize(9);

                        column.Item().Border(1).Padding(5).Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Row(r =>
                                {
                                    r.ConstantItem(60).Text("MARCA:").FontSize(8).Bold();
                                    r.RelativeItem().Text(rental.Vehicle?.Make ?? "").FontSize(9);
                                });
                                col.Item().Row(r =>
                                {
                                    r.ConstantItem(60).Text("PLACA:").FontSize(8).Bold();
                                    r.RelativeItem().Text(rental.Vehicle?.LicensePlate ?? "").FontSize(9);
                                });
                            });

                            row.RelativeItem().PaddingLeft(10).Column(col =>
                            {
                                col.Item().Row(r =>
                                {
                                    r.ConstantItem(60).Text("MODELO:").FontSize(8).Bold();
                                    r.RelativeItem().Text(rental.Vehicle?.Model ?? "").FontSize(9);
                                });
                                col.Item().Row(r =>
                                {
                                    r.ConstantItem(60).Text("COLOR:").FontSize(8).Bold();
                                    r.RelativeItem().Text(rental.Vehicle?.Color ?? "").FontSize(9);
                                });
                            });
                        });
                    });
                }

                void ComposeDeliverySection(IContainer container)
                {
                    container.Column(column =>
                    {
                        column.Item().Background(Colors.Grey.Darken1).Padding(5)
                            .Text("CONSTANCIA DE ENTREGA Y RECEPCIÓN").FontColor(Colors.White).Bold().FontSize(9);

                        column.Item().Border(1).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            // Encabezados
                            table.Cell().Background(Colors.Grey.Darken2).Padding(5)
                                .Text("ENTREGA").FontColor(Colors.White).Bold().FontSize(9);
                            table.Cell().Background(Colors.Grey.Darken2).Padding(5)
                                .Text("RECEPCIÓN").FontColor(Colors.White).Bold().FontSize(9);

                            // Contenido
                            table.Cell().Border(0.5f).Padding(5).Column(col =>
                            {
                                col.Item().Row(r =>
                                {
                                    r.RelativeItem().Text("Fecha:").FontSize(8);
                                    r.RelativeItem().Text(rental.StartDate.ToString("dd/MM/yyyy")).FontSize(8);
                                    r.RelativeItem().Text("Hora:").FontSize(8);
                                    r.RelativeItem().Text(rental.StartDate.ToString("HH:mm")).FontSize(8);
                                });
                                col.Item().PaddingTop(5).Row(r =>
                                {
                                    r.RelativeItem().Text("Kilometraje entregado:").FontSize(8);
                                    r.RelativeItem().Text($"{rental.MileageAtDelivery} km").FontSize(8);
                                });
                            });

                            table.Cell().Border(0.5f).Padding(5).Column(col =>
                            {
                                col.Item().Row(r =>
                                {
                                    r.RelativeItem().Text("Fecha:").FontSize(8);
                                    r.RelativeItem().Text(rental.ActualReturnDate?.ToString("dd/MM/yyyy") ?? "Pendiente").FontSize(8);
                                    r.RelativeItem().Text("Hora:").FontSize(8);
                                    r.RelativeItem().Text(rental.ActualReturnDate?.ToString("HH:mm") ?? "").FontSize(8);
                                });
                                col.Item().PaddingTop(5).Row(r =>
                                {
                                    r.RelativeItem().Text("Kilometraje de recepción:").FontSize(8);
                                    r.RelativeItem().Text(rental.MileageAtReturn.HasValue ? $"{rental.MileageAtReturn} km" : "Pendiente").FontSize(8);
                                });
                            });
                        });
                    });
                }

                void ComposeAccessoriesSection(IContainer container)
                {
                    container.Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Background(Colors.Grey.Darken2).Padding(5)
                                .Text("ACCESORIOS ENTREGADOS").FontColor(Colors.White).Bold().FontSize(9);
                            row.RelativeItem().Background(Colors.Grey.Darken2).Padding(5)
                                .Text("ACCESORIOS RECIBIDOS").FontColor(Colors.White).Bold().FontSize(9);
                        });

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Border(1).Padding(8).Column(col =>
                            {
                                ComposeAccessoryCheckbox(col, "Radio (Estuche de Radio)");
                                ComposeAccessoryCheckbox(col, "Cenicero");
                                ComposeAccessoryCheckbox(col, "Encendedor");
                                ComposeAccessoryCheckbox(col, "Luces intensas");
                                ComposeAccessoryCheckbox(col, "Luces Externas");
                                ComposeAccessoryCheckbox(col, "A/C funciona");
                                ComposeAccessoryCheckbox(col, "Antena");
                                ComposeAccessoryCheckbox(col, "Pito");
                                ComposeAccessoryCheckbox(col, "Forros asientos");
                                ComposeAccessoryCheckbox(col, "Moqueta( )");
                                ComposeAccessoryCheckbox(col, "Parabrisas");
                            });

                            row.RelativeItem().Border(1).Padding(8).Column(col =>
                            {
                                ComposeAccessoryCheckbox(col, "Radio (Estuche de Radio)");
                                ComposeAccessoryCheckbox(col, "Cenicero");
                                ComposeAccessoryCheckbox(col, "Encendedor");
                                ComposeAccessoryCheckbox(col, "Luces intensas");
                                ComposeAccessoryCheckbox(col, "Luces Externas");
                                ComposeAccessoryCheckbox(col, "A/C funciona");
                                ComposeAccessoryCheckbox(col, "Antena");
                                ComposeAccessoryCheckbox(col, "Pito");
                                ComposeAccessoryCheckbox(col, "Forros asientos");
                                ComposeAccessoryCheckbox(col, "Moqueta( )");
                                ComposeAccessoryCheckbox(col, "Parabrisas");
                            });
                        });
                    });
                }

                void ComposeAccessoryCheckbox(ColumnDescriptor column, string label)
                {
                    column.Item().PaddingVertical(1).Row(row =>
                    {
                        row.ConstantItem(12).Border(0.5f).Height(10);
                        row.RelativeItem().PaddingLeft(5).Text(label).FontSize(7);
                    });
                }

                void ComposeFuelSection(IContainer container)
                {
                    container.Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Background(Colors.Grey.Darken2).Padding(5)
                                .Text("COMBUSTIBLE ENTREGADO").FontColor(Colors.White).Bold().FontSize(9);
                            col.Item().Border(1).Padding(10).Height(50).Text("E    1/4    1/2    3/4    F").FontSize(8);
                        });

                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Background(Colors.Grey.Darken2).Padding(5)
                                .Text("COMBUSTIBLE RECIBIDO").FontColor(Colors.White).Bold().FontSize(9);
                            col.Item().Border(1).Padding(10).Height(50).Text("E    1/4    1/2    3/4    F").FontSize(8);
                        });
                    });
                }
            });
        }
    }
}