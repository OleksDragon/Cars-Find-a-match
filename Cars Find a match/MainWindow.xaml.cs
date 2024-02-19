using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Path = System.IO.Path;

namespace Cars_Find_a_match
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private List<Card> _cards;
        public List<Card>Cards
        {  get { return _cards; }
            set
            {
                _cards = value;
                OnPropertyChanged("Cards");
            }
        }
        private int _remainingAttempts;
        public int RemainingAttempts
        {
            get { return _remainingAttempts; }
            set
            {
                _remainingAttempts = value;
                OnPropertyChanged("RemainingAttempts");
            }
        }
        private Card _selectedCard;
        public MainWindow()
        {
            InitializeComponent();

            // Получение списка файлов из папки background
            string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            string imagesDirectory = Path.Combine(projectDirectory, "Images", "Background");
            string[] imageFiles = Directory.GetFiles(imagesDirectory, "*.jpg");

            // Добавление изображений в коллекцию
            foreach (string file in imageFiles)
            {
                BitmapImage bitmap = new BitmapImage(new Uri(file));
                images.Add(bitmap);
            }

            // Применение случайного изображения в качестве фона для текстового блока
            gridField.Background = GetRandomImage();

            DataContext = this;

            InitializeGame();
        }

        private void InitializeGame() // Инициализация игры
        {  
            var cardValues = Enumerable.Range(1, 8).ToList();
            cardValues.AddRange(cardValues);

            Cards = cardValues.OrderBy(v => Guid.NewGuid()).Select(value => new Card { Value = value }).ToList();

            RemainingAttempts = 15; // Установка количества попыток
            progressBar.Value = 0;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand CardClickCommand => new RelayCommand<Card>(CardClick);

        private void CardClick(Card clickedCard)
        {
            if (clickedCard.IsOpen || RemainingAttempts <= 0)
                return;

            if (_selectedCard == null)
            {
                _selectedCard = clickedCard;
                _selectedCard.IsOpen = true;
                return;
            }

            clickedCard.IsOpen = true;

            if (_selectedCard != null && _selectedCard.Value == clickedCard.Value)
            {
                // Если пара угадана, устанавливаем Opacity = 0 через задержку времени
                var timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(1); // Задержка в 1 секунду
                timer.Tick += (sender, args) =>
                {
                    _selectedCard.Opacity = 0;
                    clickedCard.Opacity = 0;
                    progressBar.Value++;
                    _selectedCard = null;
                    timer.Stop(); // Остановка таймера после выполнения анимации
                };
                timer.Start();
            }
            else
            {
                // Закрытие карт после задержки
                var timer = new System.Windows.Threading.DispatcherTimer();
                timer.Tick += (sender, args) =>
                {
                    if (_selectedCard != null)
                        _selectedCard.IsOpen = false;
                    clickedCard.IsOpen = false;
                    _selectedCard = null;
                    timer.Stop(); // Остановка таймера после выполнения анимации
                };
                timer.Interval = TimeSpan.FromSeconds(1); // Задержка в 1 секунду
                timer.Start();

                RemainingAttempts--;
            }

            // Проверка, все ли карты открыты. Если да, то игра завершена.
            if (Cards.All(c => c.IsOpen))
            {
                WinMessage winMessage = new WinMessage();
                winMessage.ShowDialog();
            }

            if (RemainingAttempts < 1)
            {
                MessageBoxResult result = MessageBox.Show("К сожалению, у вас закончились попытки. Хотите сыграть еще раз?", "Игра окончена", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.Yes)
                {
                    InitializeGame();
                }
                else
                {
                    Close();
                }
            }
        }

        // Создание коллекции изображений
        private List<BitmapImage> images = new List<BitmapImage>();
        

        // Метод для случайного выбора изображения из коллекции
        private ImageBrush GetRandomImage()
        {
            Random random = new Random();
            BitmapImage randomImage = images[random.Next(images.Count)];

            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = randomImage;

            return imageBrush;
        }

        private void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            InitializeGame();
        }
    }    
}