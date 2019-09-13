using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using System.Speech.Recognition;
using System.Speech.Recognition.SrgsGrammar;

namespace Snake
{
    /// <summary>
    /// No Summary! The summer is yet to come.
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        SpeechRecognitionEngine sre;
        // This list describes the Bonus Red pieces of Food on the Canvas
        private List<Point> bonusPoints = new List<Point>();

        // This list describes the body of the snake on the Canvas
        private List<Point> snakePoints = new List<Point>();

        private Brush snakeColor = Brushes.HotPink;
        private enum SIZE
        {
            THIN = 4,
            NORMAL = 6,
            THICK = 8
        };
        private enum MOVINGDIRECTION
        {
            UPWARDS = 8,
            DOWNWARDS = 2,
            TOLEFT = 4,
            TORIGHT = 6
        };

        private TimeSpan FAST = new TimeSpan(1);
        private TimeSpan MODERATE = new TimeSpan(10000);
        private TimeSpan SLOW = new TimeSpan(200000);
        private TimeSpan DAMNSLOW = new TimeSpan(500000);



        private Point startingPoint = new Point(100, 100);
        private Point currentPosition = new Point();

        // Movement direction initialisation
        private int direction = 0;

        /* Placeholder for the previous movement direction
         * The snake needs this to avoid its own body.  */
        private int previousDirection = 0;


        /* Here user can change the size of the snake. 
         * Possible sizes are THIN, NORMAL and THICK */
        private int headSize = (int)SIZE.NORMAL;



        private int length = 100;
        private int score = 0;
        private Random rnd = new Random();


        public MainWindow()
        {
            InitializeComponent();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += new EventHandler(timer_Tick);

            timer.Interval = SLOW;
            timer.Start();

            sre = new SpeechRecognitionEngine();
            GrammarBuilder gb = new GrammarBuilder(new Choices("game over", "left", "right"));
            sre.LoadGrammar(new Grammar(gb));
            sre.SetInputToDefaultAudioDevice();
            sre.RecognizeAsync(RecognizeMode.Multiple);
            sre.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(RecognizeSpeech);

            paintSnake(startingPoint);
            currentPosition = startingPoint;

            // Instantiate Food Objects
            for (int n = 0; n < 10; n++)
            {
                paintBonus(n);
            }
        }

        private void paintSnake(Point currentposition)
        {

            /* This method is used to paint a frame of the snake´s body
             * each time it is called. */


            Ellipse newEllipse = new Ellipse();
            newEllipse.Fill = snakeColor;
            newEllipse.Width = headSize;
            newEllipse.Height = headSize;

            Canvas.SetTop(newEllipse, currentposition.Y);
            Canvas.SetLeft(newEllipse, currentposition.X);

            int count = paintCanvas.Children.Count;
            paintCanvas.Children.Add(newEllipse);
            snakePoints.Add(currentposition);


            // Restrict the tail of the snake
            if (count > length)
            {
                paintCanvas.Children.RemoveAt(count - length + 9);
                snakePoints.RemoveAt(count - length);
            }
        }


        private void paintBonus(int index)
        {
            Point bonusPoint = new Point(rnd.Next(5, 620), rnd.Next(5, 380));



            Ellipse newEllipse = new Ellipse();
            newEllipse.Fill = Brushes.Green;
            newEllipse.Width = headSize;
            newEllipse.Height = headSize;

            Canvas.SetTop(newEllipse, bonusPoint.Y);
            Canvas.SetLeft(newEllipse, bonusPoint.X);
            paintCanvas.Children.Insert(index, newEllipse);
            bonusPoints.Insert(index, bonusPoint);

        }


        private void timer_Tick(object sender, EventArgs e)
        {
            // Expand the body of the snake to the direction of movement

            switch (direction)
            {
                case (int)MOVINGDIRECTION.DOWNWARDS:
                    currentPosition.Y += 1;
                    paintSnake(currentPosition);
                    break;
                case (int)MOVINGDIRECTION.UPWARDS:
                    currentPosition.Y -= 1;
                    paintSnake(currentPosition);
                    break;
                case (int)MOVINGDIRECTION.TOLEFT:
                    currentPosition.X -= 1;
                    paintSnake(currentPosition);
                    break;
                case (int)MOVINGDIRECTION.TORIGHT:
                    currentPosition.X += 1;
                    paintSnake(currentPosition);
                    break;
            }

            // Restrict to boundaries of the Canvas
            if ((currentPosition.X < 5) || (currentPosition.X > 642) ||
                (currentPosition.Y < 5) || (currentPosition.Y > 422))
                GameOver();

            // Hitting a bonus Point causes the lengthen-Snake Effect
            int n = 0;
            foreach (Point point in bonusPoints)
            {

                if ((Math.Abs(point.X - currentPosition.X) < headSize) &&
                    (Math.Abs(point.Y - currentPosition.Y) < headSize))
                {
                    length += 10;
                    score += 10;

                    // In the case of food consumption, erase the food object
                    // from the list of bonuses as well as from the canvas
                    bonusPoints.RemoveAt(n);
                    paintCanvas.Children.RemoveAt(n);
                    paintBonus(n);
                    break;
                }
                n++;
            }



            for (int q = 0; q < (snakePoints.Count - headSize * 2); q++)
            {
                Point point = new Point(snakePoints[q].X, snakePoints[q].Y);
                if ((Math.Abs(point.X - currentPosition.X) < (headSize)) &&
                     (Math.Abs(point.Y - currentPosition.Y) < (headSize)))
                {
                    GameOver();
                    break;
                }

            }
        }



        private void OnButtonKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down:
                    if (previousDirection != (int)MOVINGDIRECTION.UPWARDS)
                        direction = (int)MOVINGDIRECTION.DOWNWARDS;
                    break;
                case Key.Up:
                    if (previousDirection != (int)MOVINGDIRECTION.DOWNWARDS)
                        direction = (int)MOVINGDIRECTION.UPWARDS;
                    break;
                case Key.Left:
                    if (previousDirection != (int)MOVINGDIRECTION.TORIGHT)
                        direction = (int)MOVINGDIRECTION.TOLEFT;
                    break;
                case Key.Right:
                    if (previousDirection != (int)MOVINGDIRECTION.TOLEFT)
                        direction = (int)MOVINGDIRECTION.TORIGHT;
                    break;
            }
            previousDirection = direction;
        }

        private void GameOver()
        {
            MessageBox.Show("You Lose! Your score is " + score.ToString(), "Game Over", MessageBoxButton.OK, MessageBoxImage.Hand);
            this.Close();
        }

        void RecognizeSpeech(object sender, SpeechRecognizedEventArgs e)
        {
            switch (e.Result.Text)
            {

                case "right":
                    if (previousDirection == (int)MOVINGDIRECTION.TORIGHT)
                        direction = (int)MOVINGDIRECTION.DOWNWARDS;

                    else if ((previousDirection == (int)MOVINGDIRECTION.DOWNWARDS))
                    {
                        direction = (int)MOVINGDIRECTION.TOLEFT;
                    }
                    else if ((previousDirection == (int)MOVINGDIRECTION.TOLEFT))
                    {
                        direction = (int)MOVINGDIRECTION.UPWARDS;
                    }
                    else
                    {
                        direction = (int)MOVINGDIRECTION.TORIGHT;
                    }
                    break;


                case "left":
                    if (previousDirection == (int)MOVINGDIRECTION.TORIGHT)
                        direction = (int)MOVINGDIRECTION.UPWARDS;
                    else if ((previousDirection == (int)MOVINGDIRECTION.DOWNWARDS))
                    {
                        direction = (int)MOVINGDIRECTION.TORIGHT;
                    }
                    else if ((previousDirection == (int)MOVINGDIRECTION.TOLEFT))
                    {
                        direction = (int)MOVINGDIRECTION.DOWNWARDS;
                    }
                    else
                    {
                        direction = (int)MOVINGDIRECTION.TOLEFT;
                    }
                    break;

                case "game over":
                    GameOver();
                    break;

            }
            previousDirection = direction;
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            sre.RecognizeAsyncCancel();
        }
    }
}
