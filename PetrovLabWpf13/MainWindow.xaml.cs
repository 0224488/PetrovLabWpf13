using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

//  Доработать проект текстового редактора из задания 9, заменив выбор шрифта и размера привязками данных.

namespace PetrovLabWpf13
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double defaultFontSize = 16;
        private const string defaultFontName = "Arial";

        public MainWindow()
        {
            InitializeComponent();

            lightMenuItem.IsChecked = true;
            DocFontSize = defaultFontSize;
            DocFontName = defaultFontName;
            DataContext = this;
        }

        public static readonly DependencyProperty DocFontSizeProperty =
            DependencyProperty.Register(nameof(DocFontSize), typeof(double), typeof(MainWindow));

        public double DocFontSize
        {
            get { return (double)GetValue(DocFontSizeProperty); }
            set { SetValue(DocFontSizeProperty, value); }
        }

        public static readonly DependencyProperty DocFontNameProperty =
            DependencyProperty.Register(nameof(DocFontName), typeof(string), typeof(MainWindow));

        public string DocFontName
        {
            get { return (string)GetValue(DocFontNameProperty); }
            set { SetValue(DocFontNameProperty, value); }
        }

        /// <summary>
        /// обработка выбора цвета текста
        /// </summary>
        private void RadioButtonColor_Checked(object sender, RoutedEventArgs e)
		{
            if(textbox != null)
            {
                var radioButton = sender as RadioButton;
                if(radioButton != null)
                {
                    textbox.Foreground = radioButton.Foreground;
                }
            }
        }

        /// <summary>
        /// обработка выбора стиля шрифта (жирный, обычный)
        /// </summary>
        private void ToggleButtonBold_CheckedChanged(object sender, RoutedEventArgs e)
		{
            var toggleButton = sender as ToggleButton;
			if(toggleButton != null)
			{
				if(toggleButton.IsChecked == true)
					textbox.FontWeight = FontWeights.Bold;
                else
                    textbox.FontWeight = FontWeights.Normal;
            }
        }

        /// <summary>
        /// обработка выбора стиля шрифта (курсив, обычный)
        /// </summary>
        private void ToggleButtonItalic_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as ToggleButton;
            if(toggleButton != null)
            {
                if(toggleButton.IsChecked == true)
                    textbox.FontStyle = FontStyles.Italic;
                else
                    textbox.FontStyle = FontStyles.Normal;
            }
        }

        /// <summary>
        /// обработка выбора стиля шрифта (подчеркнутый, обычный)
        /// </summary>
        private void ToggleButtonUnderline_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as ToggleButton;
            if(toggleButton != null)
            {
                if(toggleButton.IsChecked == true)
                    textbox.TextDecorations = TextDecorations.Underline;
                else
                    textbox.TextDecorations = null;
            }
        }

        /// <summary>
        /// обработка команды Открыть
        /// </summary>
        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)

        {
            var dlg = new OpenFileDialog();
			if(dlg.ShowDialog() == true)
			{
                // читаем документ из выбранного файла
                var jsonService = new JsonFileService();
                var document = jsonService.Open(dlg.FileName);

                InitializeDocument(document);
            }
        }

        /// <summary>
        /// обработка команды Сохранить
        /// </summary>
        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            if(saveFileDialog.ShowDialog() == true)
            {
                // создаем документ
                var document = new Document()
                {
					Text = textbox.Text,
					FontName = DocFontName,
					FontSize = DocFontSize,
					IsBold = textbox.FontWeight == FontWeights.Bold,
                    IsItalic = textbox.FontStyle == FontStyles.Italic,
                    IsUnderline = textbox.TextDecorations == TextDecorations.Underline,
					Color = (textbox.Foreground as SolidColorBrush).Color
				};
                // сериализуем его в файл
                var jsonService = new JsonFileService();
                jsonService.Save(saveFileDialog.FileName, document);
            }
        }

        /// <summary>
        /// обработка команды Закрыть
        /// </summary>
        private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(MessageBox.Show("Сохранить изменения в документ?", "Сохранение", 
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.OK)
			{
                Save_Executed(null, null);
			}
            ResetValues();
        }

        /// <summary>
        /// выставление свойств документа
        /// </summary>
        private void InitializeDocument(Document document)
		{
            textbox.Text = document.Text;
			DocFontName = document.FontName;
            DocFontSize = document.FontSize;
            toggleButtonBold.IsChecked = document.IsBold;
            toggleButtonItalic.IsChecked = document.IsItalic;
            toggleButtonUnderline.IsChecked = document.IsUnderline;
            if(document.Color == Colors.Black)
            {
                radioButtonBlack.IsChecked = true;
            }
            else if(document.Color == Colors.Red)
            {
                radioButtonRed.IsChecked = true;
            }
        }

        /// <summary>
        /// выставление исходных свойств нового документа
        /// </summary>
        private void ResetValues()
		{
            textbox.Text = string.Empty;
            DocFontName = defaultFontName;
            DocFontSize = defaultFontSize;
            toggleButtonBold.IsChecked = false;
            toggleButtonItalic.IsChecked = false;
            toggleButtonUnderline.IsChecked = false;
            radioButtonBlack.IsChecked = true;
        }

        public class JsonFileService
        {
            /// <summary>
            /// чтение документа из файла с указанным именем
            /// </summary>
            /// <param name="fileName">имя файла</param>
            /// <returns>документ</returns>
            public Document Open(string fileName)
            {
                Document document = null;
                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                    WriteIndented = true
                };
                
                using(StreamReader reader = new StreamReader(fileName))
                {
					var jsonString = reader.ReadToEnd();
					document = JsonSerializer.Deserialize(jsonString, typeof(Document), options) as Document;
				}
                return document;
            }

            /// <summary>
            /// запись документа в файл с указанным именем
            /// </summary>
            /// <param name="fileName">имя файла</param>
            /// <param name="document">документ</param>
            public void Save(string fileName, Document document)
            {
                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                    WriteIndented = true
                };
                var jsonString = JsonSerializer.Serialize(document, options);
                using(StreamWriter writer = new StreamWriter(fileName))
                {
                    writer.Write(jsonString);
                }
            }
        }

        public class Document
        {
            public string Text { get; set; }
            public double FontSize{ get; set; }
            public string FontName { get; set; }
            public bool IsBold { get; set; }
            public bool IsItalic { get; set; }
            public bool IsUnderline { get; set; }
            public Color Color { get; set; }
        }

		private void MenuItem_Checked(object sender, RoutedEventArgs e)
		{
            var menuItem = sender as MenuItem;
            if(menuItem.IsChecked)
            {
                var style = string.Empty;
                if((sender as MenuItem) == lightMenuItem)
                {
                    darkMenuItem.IsChecked = false;
                    style = "light";
                }
                else if((sender as MenuItem) == darkMenuItem)
                {
                    lightMenuItem.IsChecked = false;
                    style = "dark";
                }
                if(!string.IsNullOrEmpty(style))
                    ThemeChange(style);
            }
        }

        private void ThemeChange(string style)
        {
            // определяем путь к файлу ресурсов
            var uri = new Uri($"/Themes/{style}.xaml", UriKind.Relative);
            var dictUri = new Uri("Dictionary1.xaml", UriKind.Relative);
            // загружаем словарь ресурсов
            ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
            resourceDict.Source = uri;
            // очищаем коллекцию ресурсов приложения
            Application.Current.Resources.Clear();
            var dict = Application.Current.Resources.MergedDictionaries.FirstOrDefault(d => d.Source == dictUri);
            Application.Current.Resources.MergedDictionaries.Clear();
            // добавляем загруженный словарь ресурсов
            if(dict != null)
                Application.Current.Resources.MergedDictionaries.Add(dict);
            Application.Current.Resources.MergedDictionaries.Add(resourceDict);
        }
	}
}
