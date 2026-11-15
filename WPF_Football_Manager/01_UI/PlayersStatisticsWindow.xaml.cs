using WPF_Football_Manager.BLL;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WPF_Football_Manager.UI
{
    public partial class PlayersStatisticsWindow : Window
    {
        private readonly PlayerService _playerService = new PlayerService();
        private DataTable? _dtTopScorers;
        private DataTable? _dtTopCards;
        private DataTable? _dtTopFouls;

        private readonly Brush _textColorDark = new SolidColorBrush(Color.FromRgb(43, 48, 58));
        private readonly Brush _textColorMuted = new SolidColorBrush(Color.FromRgb(140, 148, 160));

        public PlayersStatisticsWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _dtTopScorers = _playerService.GetTop3Scorers();
                _dtTopCards = _playerService.GetTopPlayersByCards();
                _dtTopFouls = _playerService.GetTop3ByFouls();

                DrawPodium();
                DrawMostCards();
                DrawMostFouls();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Грешка при зареждане на статистиката: " + ex.Message,
                    "Грешка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // 1. ПОДИУМ ЗА ГОЛОВЕ
        private void DrawPodium()
        {
            canvasPodium.Children.Clear();

            double totalWidth = canvasPodium.Width;
            double totalHeight = canvasPodium.Height;

            double columnWidth = (totalWidth / 3.0) - 20.0;
            double maxBarHeight = totalHeight - 140.0;
            double baseline = totalHeight - 70.0;

            double x2 = 10.0;
            double x1 = columnWidth + 30.0;
            double x3 = (columnWidth * 2.0) + 50.0;

            // Основна линия под подиума
            Line baseLine = new Line();
            baseLine.X1 = 5;
            baseLine.Y1 = baseline;
            baseLine.X2 = totalWidth - 5;
            baseLine.Y2 = baseline;
            baseLine.Stroke = new SolidColorBrush(Color.FromRgb(220, 224, 230));
            baseLine.StrokeThickness = 2;
            canvasPodium.Children.Add(baseLine);

            int rowsCount = _dtTopScorers != null ? _dtTopScorers.Rows.Count : 0;
            int leaderGoals = 1;

            if (rowsCount > 0 && _dtTopScorers!.Rows[0]["goals_count"] != DBNull.Value)
            {
                leaderGoals = Convert.ToInt32(_dtTopScorers.Rows[0]["goals_count"]);
                if (leaderGoals <= 0)
                {
                    leaderGoals = 1;
                }
            }

            // --- 1-во Място (Златен градиент) ---
            double h1 = maxBarHeight;
            LinearGradientBrush goldBrush = new LinearGradientBrush(
                Color.FromRgb(255, 223, 100),
                Color.FromRgb(245, 175, 25),
                90.0);

            Border bar1 = new Border();
            bar1.Width = columnWidth;
            bar1.Height = h1;
            bar1.Background = goldBrush;
            bar1.CornerRadius = new CornerRadius(10, 10, 0, 0);
            Canvas.SetLeft(bar1, x1);
            Canvas.SetTop(bar1, baseline - h1);
            canvasPodium.Children.Add(bar1);

            if (rowsCount > 0)
            {
                DataRow r1 = _dtTopScorers!.Rows[0];

                // Златна корона над 1-во място
                double crownWidth = 24;
                double crownHeight = 16;
                double crownX = x1 + (columnWidth / 2.0) - (crownWidth / 2.0);
                double crownY = baseline - h1 - 50;

                Polygon crown = new Polygon();
                crown.Fill = new SolidColorBrush(Color.FromRgb(245, 175, 25));
                crown.Points = new PointCollection
                {
                    new Point(crownX, crownY + crownHeight),
                    new Point(crownX, crownY),
                    new Point(crownX + 6, crownY + 8),
                    new Point(crownX + 12, crownY),
                    new Point(crownX + 18, crownY + 8),
                    new Point(crownX + crownWidth, crownY),
                    new Point(crownX + crownWidth, crownY + crownHeight)
                };
                canvasPodium.Children.Add(crown);

                AddCenteredText(
                    canvasPodium,
                    $"{r1["goals_count"]} гола",
                    x1,
                    baseline - h1 - 30,
                    columnWidth,
                    19,
                    FontWeights.Bold,
                    FontStyles.Normal,
                    new SolidColorBrush(Color.FromRgb(210, 130, 0)));

                AddCenteredText(
                    canvasPodium,
                    r1["full_name"]?.ToString() ?? "",
                    x1,
                    baseline + 8,
                    columnWidth,
                    16,
                    FontWeights.Bold,
                    FontStyles.Normal,
                    _textColorDark);

                AddCenteredText(
                    canvasPodium,
                    r1["club_name"]?.ToString() ?? "",
                    x1,
                    baseline + 28,
                    columnWidth,
                    13,
                    FontWeights.Normal,
                    FontStyles.Italic,
                    _textColorMuted);
            }

            // --- 2-ро Място (Сребърен градиент) ---
            double h2 = rowsCount > 1
                ? (double)Convert.ToInt32(_dtTopScorers!.Rows[1]["goals_count"]) / leaderGoals * (maxBarHeight * 0.8)
                : maxBarHeight * 0.75;

            if (h2 < 30)
            {
                h2 = 30;
            }

            LinearGradientBrush silverBrush = new LinearGradientBrush(
                Color.FromRgb(230, 235, 240),
                Color.FromRgb(185, 195, 205),
                90.0);

            Border bar2 = new Border();
            bar2.Width = columnWidth;
            bar2.Height = h2;
            bar2.Background = silverBrush;
            bar2.CornerRadius = new CornerRadius(10, 10, 0, 0);
            Canvas.SetLeft(bar2, x2);
            Canvas.SetTop(bar2, baseline - h2);
            canvasPodium.Children.Add(bar2);

            if (rowsCount > 1)
            {
                DataRow r2 = _dtTopScorers!.Rows[1];

                AddCenteredText(
                    canvasPodium,
                    $"{r2["goals_count"]} гола",
                    x2,
                    baseline - h2 - 28,
                    columnWidth,
                    17,
                    FontWeights.Bold,
                    FontStyles.Normal,
                    new SolidColorBrush(Color.FromRgb(110, 125, 140)));

                AddCenteredText(
                    canvasPodium,
                    r2["full_name"]?.ToString() ?? "",
                    x2,
                    baseline + 8,
                    columnWidth,
                    15,
                    FontWeights.Bold,
                    FontStyles.Normal,
                    _textColorDark);

                AddCenteredText(
                    canvasPodium,
                    r2["club_name"]?.ToString() ?? "",
                    x2,
                    baseline + 28,
                    columnWidth,
                    13,
                    FontWeights.Normal,
                    FontStyles.Italic,
                    _textColorMuted);
            }

            // --- 3-то Място (Бронзов градиент) ---
            double h3 = rowsCount > 2
                ? (double)Convert.ToInt32(_dtTopScorers!.Rows[2]["goals_count"]) / leaderGoals * (maxBarHeight * 0.6)
                : maxBarHeight * 0.5;

            if (h3 < 20)
            {
                h3 = 20;
            }

            LinearGradientBrush bronzeBrush = new LinearGradientBrush(
                Color.FromRgb(245, 190, 150),
                Color.FromRgb(200, 120, 70),
                90.0);

            Border bar3 = new Border();
            bar3.Width = columnWidth;
            bar3.Height = h3;
            bar3.Background = bronzeBrush;
            bar3.CornerRadius = new CornerRadius(10, 10, 0, 0);
            Canvas.SetLeft(bar3, x3);
            Canvas.SetTop(bar3, baseline - h3);
            canvasPodium.Children.Add(bar3);

            if (rowsCount > 2)
            {
                DataRow r3 = _dtTopScorers!.Rows[2];

                AddCenteredText(
                    canvasPodium,
                    $"{r3["goals_count"]} гола",
                    x3,
                    baseline - h3 - 28,
                    columnWidth,
                    17,
                    FontWeights.Bold,
                    FontStyles.Normal,
                    new SolidColorBrush(Color.FromRgb(165, 90, 45)));

                AddCenteredText(
                    canvasPodium,
                    r3["full_name"]?.ToString() ?? "",
                    x3,
                    baseline + 8,
                    columnWidth,
                    15,
                    FontWeights.Bold,
                    FontStyles.Normal,
                    _textColorDark);

                AddCenteredText(
                    canvasPodium,
                    r3["club_name"]?.ToString() ?? "",
                    x3,
                    baseline + 28,
                    columnWidth,
                    13,
                    FontWeights.Normal,
                    FontStyles.Italic,
                    _textColorMuted);
            }
        }

        // 2. ХОРИЗОНТАЛНИ ЛЕНТИ ЗА КАРТОНИ
        private void DrawMostCards()
        {
            canvasMostCards.Children.Clear();

            if (_dtTopCards == null || _dtTopCards.Rows.Count == 0)
            {
                AddLeftText(
                    canvasMostCards,
                    "Няма налични данни за картони.",
                    15,
                    15,
                    17,
                    FontWeights.Normal,
                    FontStyles.Italic,
                    _textColorMuted);
                return;
            }

            double totalWidth = canvasMostCards.Width;
            double startY = 20;
            double rowGap = 85;
            double barHeight = 22;
            double barGap = 6;
            int maxRows = Math.Min(5, _dtTopCards.Rows.Count);

            double nameX = 15;
            double barStartX = 210;
            double barAreaWidth = totalWidth - barStartX - 70;

            int maxCount = 1;
            for (int i = 0; i < maxRows; i++)
            {
                int y = Convert.ToInt32(_dtTopCards.Rows[i]["yellow_cards"]);
                int r = Convert.ToInt32(_dtTopCards.Rows[i]["red_cards"]);
                if (y > maxCount)
                {
                    maxCount = y;
                }

                if (r > maxCount)
                {
                    maxCount = r;
                }
            }

            for (int i = 0; i < maxRows; i++)
            {
                DataRow row = _dtTopCards.Rows[i];
                string fullName = row["full_name"]?.ToString() ?? "";
                string clubName = row["club_name"]?.ToString() ?? "";
                int yellow = Convert.ToInt32(row["yellow_cards"]);
                int red = Convert.ToInt32(row["red_cards"]);

                double playerY = startY + (i * rowGap);

                double nameFontSize = i == 0 ? 15 : 14;
                double numberFontSize = i == 0 ? 18 : 17;
                Brush nameColor = i == 0
                    ? new SolidColorBrush(Color.FromRgb(220, 50, 70))
                    : _textColorDark;

                AddLeftText(
                    canvasMostCards,
                    fullName,
                    nameX,
                    playerY + 4,
                    nameFontSize,
                    FontWeights.Bold,
                    FontStyles.Normal,
                    nameColor);

                AddLeftText(
                    canvasMostCards,
                    clubName,
                    nameX,
                    playerY + 26,
                    13,
                    FontWeights.Normal,
                    FontStyles.Italic,
                    _textColorMuted);

                // Червен картон
                double redBarW = red > 0
                    ? Math.Max(16, (double)red / maxCount * barAreaWidth)
                    : 0;
                double redY = playerY;

                if (redBarW > 0)
                {
                    LinearGradientBrush redBrush = new LinearGradientBrush(
                        Color.FromRgb(255, 110, 120),
                        Color.FromRgb(230, 45, 60),
                        0.0);

                    Border redBar = new Border();
                    redBar.Width = redBarW;
                    redBar.Height = barHeight;
                    redBar.Background = redBrush;
                    redBar.CornerRadius = new CornerRadius(7);
                    Canvas.SetLeft(redBar, barStartX);
                    Canvas.SetTop(redBar, redY);
                    canvasMostCards.Children.Add(redBar);
                }

                AddLeftText(
                    canvasMostCards,
                    red.ToString(),
                    barStartX + redBarW + 10,
                    redY - 1,
                    numberFontSize,
                    FontWeights.Bold,
                    FontStyles.Normal,
                    new SolidColorBrush(Color.FromRgb(230, 45, 60)));

                // Жълт картон
                double yellowBarW = yellow > 0
                    ? Math.Max(16, (double)yellow / maxCount * barAreaWidth)
                    : 0;
                double yellowY = playerY + barHeight + barGap;

                if (yellowBarW > 0)
                {
                    LinearGradientBrush yellowBrush = new LinearGradientBrush(
                        Color.FromRgb(255, 225, 60),
                        Color.FromRgb(240, 180, 0),
                        0.0);

                    Border yellowBar = new Border();
                    yellowBar.Width = yellowBarW;
                    yellowBar.Height = barHeight;
                    yellowBar.Background = yellowBrush;
                    yellowBar.CornerRadius = new CornerRadius(7);
                    Canvas.SetLeft(yellowBar, barStartX);
                    Canvas.SetTop(yellowBar, yellowY);
                    canvasMostCards.Children.Add(yellowBar);
                }

                AddLeftText(
                    canvasMostCards,
                    yellow.ToString(),
                    barStartX + yellowBarW + 10,
                    yellowY - 1,
                    numberFontSize,
                    FontWeights.Bold,
                    FontStyles.Normal,
                    new SolidColorBrush(Color.FromRgb(210, 150, 0)));

                // Разделителна линия под играча
                double lineY = yellowY + barHeight + 12;
                Line sep = new Line();
                sep.X1 = nameX;
                sep.Y1 = lineY;
                sep.X2 = totalWidth - 15;
                sep.Y2 = lineY;
                sep.Stroke = new SolidColorBrush(Color.FromRgb(240, 242, 245));
                sep.StrokeThickness = 1;
                canvasMostCards.Children.Add(sep);
            }
        }

        // 3. СТЪЛБОВЕ ЗА ФАЛОВЕ
        private void DrawMostFouls()
        {
            canvasMostFouls.Children.Clear();

            double totalWidth = canvasMostFouls.Width;
            double totalHeight = canvasMostFouls.Height;

            if (_dtTopFouls == null || _dtTopFouls.Rows.Count == 0)
            {
                AddLeftText(
                    canvasMostFouls,
                    "Няма налични данни за фалове.",
                    15,
                    15,
                    17,
                    FontWeights.Normal,
                    FontStyles.Italic,
                    _textColorMuted);
                return;
            }

            int rowsCount = _dtTopFouls.Rows.Count;
            int maxFouls = 1;
            if (rowsCount > 0 && _dtTopFouls.Rows[0]["fouls_count"] != DBNull.Value)
            {
                maxFouls = Convert.ToInt32(_dtTopFouls.Rows[0]["fouls_count"]);
                if (maxFouls <= 0)
                {
                    maxFouls = 1;
                }
            }

            double baseline = totalHeight - 70.0;
            double maxBarHeight = totalHeight - 150.0;
            double columnWidth = totalWidth / 3.0;
            double barWidth = 44.0;

            Line baseLine = new Line();
            baseLine.X1 = 5;
            baseLine.Y1 = baseline;
            baseLine.X2 = totalWidth - 5;
            baseLine.Y2 = baseline;
            baseLine.Stroke = new SolidColorBrush(Color.FromRgb(220, 224, 230));
            baseLine.StrokeThickness = 2;
            canvasMostFouls.Children.Add(baseLine);

            for (int i = 0; i < rowsCount; i++)
            {
                DataRow row = _dtTopFouls.Rows[i];
                string fullName = row["full_name"]?.ToString() ?? "";
                string clubName = row["club_name"]?.ToString() ?? "";
                int fouls = Convert.ToInt32(row["fouls_count"]);

                double barHeight = (double)fouls / maxFouls * maxBarHeight;
                if (barHeight < 25)
                {
                    barHeight = 25;
                }

                double colX = i * columnWidth;
                double barX = colX + (columnWidth - barWidth) / 2.0;
                double barY = baseline - barHeight;

                LinearGradientBrush blueBrush = new LinearGradientBrush(
                    Color.FromRgb(70, 190, 255),
                    Color.FromRgb(30, 130, 230),
                    90.0);

                Border bar = new Border();
                bar.Width = barWidth;
                bar.Height = barHeight;
                bar.Background = blueBrush;
                bar.CornerRadius = new CornerRadius(10, 10, 0, 0);
                Canvas.SetLeft(bar, barX);
                Canvas.SetTop(bar, barY);
                canvasMostFouls.Children.Add(bar);

                double nameFontSize = i == 0 ? 16 : 15;
                double countFontSize = i == 0 ? 19 : 17;

                Brush countColor = i == 0
                    ? new SolidColorBrush(Color.FromRgb(0, 120, 230))
                    : new SolidColorBrush(Color.FromRgb(70, 100, 130));

                Brush nameColor = i == 0
                    ? new SolidColorBrush(Color.FromRgb(0, 100, 200))
                    : _textColorDark;

                // Предупредителен триъгълник за лидера
                if (i == 0)
                {
                    double triSize = 12;
                    double triX = colX + (columnWidth / 2.0) - 34;
                    double triY = barY - 22;

                    Polygon triangle = new Polygon();
                    triangle.Fill = new SolidColorBrush(Color.FromRgb(240, 150, 0));
                    triangle.Points = new PointCollection
                    {
                        new Point(triX + (triSize / 2.0), triY),
                        new Point(triX, triY + triSize),
                        new Point(triX + triSize, triY + triSize)
                    };
                    canvasMostFouls.Children.Add(triangle);
                }

                AddCenteredText(
                    canvasMostFouls,
                    fouls.ToString(),
                    colX,
                    barY - 28,
                    columnWidth,
                    countFontSize,
                    FontWeights.Bold,
                    FontStyles.Normal,
                    countColor);

                AddCenteredText(
                    canvasMostFouls,
                    fullName,
                    colX,
                    baseline + 8,
                    columnWidth,
                    nameFontSize,
                    FontWeights.Bold,
                    FontStyles.Normal,
                    nameColor);

                AddCenteredText(
                    canvasMostFouls,
                    clubName,
                    colX,
                    baseline + 28,
                    columnWidth,
                    13,
                    FontWeights.Normal,
                    FontStyles.Italic,
                    _textColorMuted);
            }
        }

        // Помощни методи за добавяне на текст
        private void AddCenteredText(
            Canvas canvas,
            string text,
            double left,
            double top,
            double width,
            double fontSize,
            FontWeight fontWeight,
            FontStyle fontStyle,
            Brush foreground)
        {
            TextBlock tb = new TextBlock();
            tb.Text = text;
            tb.Width = width;
            tb.TextAlignment = TextAlignment.Center;
            tb.TextTrimming = TextTrimming.CharacterEllipsis;
            tb.FontFamily = new FontFamily("Segoe UI");
            tb.FontSize = fontSize;
            tb.FontWeight = fontWeight;
            tb.FontStyle = fontStyle;
            tb.Foreground = foreground;

            Canvas.SetLeft(tb, left);
            Canvas.SetTop(tb, top);
            canvas.Children.Add(tb);
        }

        private void AddLeftText(
            Canvas canvas,
            string text,
            double left,
            double top,
            double fontSize,
            FontWeight fontWeight,
            FontStyle fontStyle,
            Brush foreground)
        {
            TextBlock tb = new TextBlock();
            tb.Text = text;
            tb.FontFamily = new FontFamily("Segoe UI");
            tb.FontSize = fontSize;
            tb.FontWeight = fontWeight;
            tb.FontStyle = fontStyle;
            tb.Foreground = foreground;

            Canvas.SetLeft(tb, left);
            Canvas.SetTop(tb, top);
            canvas.Children.Add(tb);
        }
    }
}