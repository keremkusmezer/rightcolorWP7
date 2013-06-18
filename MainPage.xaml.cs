using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using RSA;
using System.IO.IsolatedStorage;
using System.Windows.Navigation;

namespace RightColor
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            this.BackKeyPress += new EventHandler<System.ComponentModel.CancelEventArgs>(MainPage_BackKeyPress);
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
            btn1.Click += new RoutedEventHandler(btnColorPick_Click);
            btn2.Click += new RoutedEventHandler(btnColorPick_Click);
            btn3.Click += new RoutedEventHandler(btnColorPick_Click);
            btn4.Click += new RoutedEventHandler(btnColorPick_Click);
            btn5.Click += new RoutedEventHandler(btnColorPick_Click);
            btn6.Click += new RoutedEventHandler(btnColorPick_Click);

            anim_BarIn.Completed += new EventHandler(anim_BarIn_Completed);
            anim_Correct.Completed += new EventHandler(anim_Correct_Completed);
            anim_Correct_Out.Completed += new EventHandler(anim_Correct_Out_Completed);

            GameTimer.Tick += new EventHandler(GameTimer_Tick);

            btnStartGame.Click += new RoutedEventHandler(btnStartGame_Click);
            btnStartTimedGame.Click += new RoutedEventHandler(btnStartTimedGame_Click);

            btnHowTo.Click += new RoutedEventHandler(btnHowTo_Click);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Save the ListBox control's SelectedIndex in page state
            //State["Index"] = LB.SelectedIndex;
            if (GameRunning)
            {
                State["GameRunning"] = true;
                State["GameScore"] = int.Parse(txtScore.Text);
                State["LivesLeft"] = LivesLeft;
                if (GameTimed)
                {
                    State["GameTimed"] = true;
                    State["SecondsLeft"] = SecondsLeft;
                }
            }
            else
            {
                State["GameRunning"] = false;
                State["GameTimed"] = false;
                State["GameScore"] = 0;
                State["LivesLeft"] = 3;
                State["SecondsLeft"] = 120;
            }
           
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (State.ContainsKey("GameRunning"))
            {
                if ((bool)State["GameRunning"] == true)
                {
                    txtScore.Text = State["GameScore"].ToString();
                    LivesLeft = (int)State["LivesLeft"];
                    UpdateLivesLeftText();

                    if (State.ContainsKey("GameTimed"))
                    {
                        SecondsLeft = (int)State["SecondsLeft"];
                        GameTimed = true;
                        GameTimer.Interval = TimeSpan.FromSeconds(1);
                        txtTimeLeft.Text = TimeSpan.FromSeconds(SecondsLeft).Minutes.ToString().PadLeft(2, "0"[0]) + ":" + TimeSpan.FromSeconds(SecondsLeft).Seconds.ToString().PadLeft(2, "0"[0]);
                    }

                    btnStartGame_Click(null, null);
                }
            }
        }

        void GameTimer_Tick(object sender, EventArgs e)
        {
            if (SecondsLeft == 1)
            {
                anim_BarIn_Completed(sender, e);
            }
            else
            {
                SecondsLeft -= 1;
                txtTimeLeft.Text = TimeSpan.FromSeconds(SecondsLeft).Minutes.ToString().PadLeft(2, "0"[0]) + ":" + TimeSpan.FromSeconds(SecondsLeft).Seconds.ToString().PadLeft(2, "0"[0]);
            }
        }

        void btnStartTimedGame_Click(object sender, RoutedEventArgs e)
        {
            GameTimed = true;
            GameTimer.Interval = TimeSpan.FromSeconds(1);
            txtTimeLeft.Text = TimeSpan.FromSeconds(SecondsLeft).Minutes.ToString().PadLeft(2, "0"[0]) + ":" + TimeSpan.FromSeconds(SecondsLeft).Seconds.ToString().PadLeft(2, "0"[0]);
            btnStartGame_Click(sender, e);
        }

        void btnHowTo_Click(object sender, RoutedEventArgs e)
        {
            Screen_HowTo.IsHitTestVisible = true;
            VisualStateManager.GoToState(this, "HowTo", true);
        }

        bool GameRunning = false;
        bool GameTimed = false;
        int SecondsLeft = 120;
        System.Windows.Threading.DispatcherTimer GameTimer = new System.Windows.Threading.DispatcherTimer();
        int LivesLeft = 3;

        void MainPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (GameRunning)
            {
                GotoWelcomeScreen();
                e.Cancel = true;                
            }
            else if (Screen_HowTo.IsHitTestVisible)
            {
                GotoWelcomeScreen();
                e.Cancel = true;
            }
        }

        public void GotoWelcomeScreen()
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains("HighScore") == true)
            {
                long Highest = (long)IsolatedStorageSettings.ApplicationSettings["HighScore"];
                txtHighestScore.Text = IsolatedStorageSettings.ApplicationSettings["HighScore"].ToString();
                if (Highest == 0)
                {
                    txtHighestScore.Text += ". pfff...";
                }
            }
            Screen_Welcome.IsHitTestVisible = true;
            Screen_HowTo.IsHitTestVisible = false;
            VisualStateManager.GoToState(this, "Welcome", true);
            GameRunning = false;
            GameTimed = false;
            GameTimer.Stop();
            SecondsLeft = 120;
            txtScore.Text = "0";
            LivesLeft = 3;
            UpdateLivesLeftText();
        }

        void btnStartGame_Click(object sender, RoutedEventArgs e)
        {
            List<Color> CurrentColors = RandomizeColors();
            btn1.Content = ColorToString(CurrentColors[0]);
            btn2.Content = ColorToString(CurrentColors[1]);
            btn3.Content = ColorToString(CurrentColors[2]);
            btn4.Content = ColorToString(CurrentColors[3]);
            btn5.Content = ColorToString(CurrentColors[4]);
            btn6.Content = ColorToString(CurrentColors[5]);

            btn1.Tag = CurrentColors[0];
            btn2.Tag = CurrentColors[1];
            btn3.Tag = CurrentColors[2];
            btn4.Tag = CurrentColors[3];
            btn5.Tag = CurrentColors[4];
            btn6.Tag = CurrentColors[5];          

            VisualStateManager.GoToState(this, "Game", true);
            Screen_Welcome.IsHitTestVisible = false;

            GameRunning = true;
            anim_BarIn.Begin();
            Randomize(true);

            if (GameTimed)
            {
                GameTimer.Start();
            }
            else
            {
                txtTimeLeft.Text = "";
            }
        }

        void anim_Correct_Out_Completed(object sender, EventArgs e)
        {
            anim_BarIn.Begin();
        }

        void anim_Correct_Completed(object sender, EventArgs e)
        {
            Randomize();
        }

        void UpdateLivesLeftText()
        {
            txtLivesLeft.Text = "";
            for (int i = 0; i < LivesLeft; i++)
            {
                txtLivesLeft.Text += Convert.ToChar(128).ToString();
            }          
        }
        void anim_BarIn_Completed(object sender, EventArgs e)
        {
            if (GameRunning)
            {
                LoosingLive();
            }
        }

        void LoosingLive()
        {
            if (LivesLeft > 0 && SecondsLeft != 0)
            {
                LivesLeft -= 1;
                UpdateLivesLeftText();
                anim_BarIn.Pause();
                txtAnswerStatus.Text = "Ouch!";
                AnswerStatusColor.Fill = new SolidColorBrush(Colors.Red);
                anim_Correct.Begin();
            }
            else
            {
                //THE END
                if (IsolatedStorageSettings.ApplicationSettings.Contains("HighScore") == false)
                {
                    IsolatedStorageSettings.ApplicationSettings.Add("HighScore", long.Parse(txtScore.Text));
                }
                else
                {
                    long Highest = (long)IsolatedStorageSettings.ApplicationSettings["HighScore"];
                    long Current = long.Parse(txtScore.Text);
                    if (Current > Highest)
                    {
                        IsolatedStorageSettings.ApplicationSettings["HighScore"] = Current;
                    }
                }
                GotoWelcomeScreen();
            }      
        }


        bool Change_ButtonBackgrounds = false;
        bool Change_ButtonPositions = false;

        bool EvalInProgress = false;

        void btnColorPick_Click(object sender, RoutedEventArgs e)
        {
            if (EvalInProgress == false)
            {
                EvalInProgress = true;
                Button Current = sender as Button;
                Color SelectedColor = (Color)Current.Tag;
                if (SelectedColor == TargetColor)
                {
                    txtAnswerStatus.Text = "Correct!";
                    AnswerStatusColor.Fill = new SolidColorBrush(Color.FromArgb(255,90,255,0));
                    //SCORE!
                    int CurrentScore = int.Parse(txtScore.Text);
                    txtScore.Text = (CurrentScore + 1).ToString();
                    if(CurrentScore > 15)
                    {
                        Change_ButtonPositions = true;
                    }
                    else
                    {
                        Change_ButtonPositions= false;
                    }
                    if (CurrentScore > 25)
                    {
                        Change_ButtonBackgrounds = true;
                    }
                    else
                    {
                        Change_ButtonBackgrounds = false;
                    }
                    if (CurrentScore > 35)
                    {
                        var Diff = (CurrentScore - 35) *10;
                        if (Diff < 4500)
                        {
                            Anim_TargetPoint.KeyTime = TimeSpan.FromMilliseconds(5000 - Diff);
                        }                  
                    }
                    anim_BarIn.Pause();
                    anim_Correct.Begin();
                }
                else
                {
                    EvalInProgress = false;
                    LoosingLive();
                }
            }
            
        }

        gameservice.rightcolorSoapClient Service = new gameservice.rightcolorSoapClient();

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateLivesLeftText();
            List<Color> CurrentColors = RandomizeColors();
            btn1.Content = ColorToString(CurrentColors[0]);
            btn2.Content = ColorToString(CurrentColors[1]);
            btn3.Content = ColorToString(CurrentColors[2]);
            btn4.Content = ColorToString(CurrentColors[3]);
            btn5.Content = ColorToString(CurrentColors[4]);
            btn6.Content = ColorToString(CurrentColors[5]);

            btn1.Tag = CurrentColors[0];
            btn2.Tag = CurrentColors[1];
            btn3.Tag = CurrentColors[2];
            btn4.Tag = CurrentColors[3];
            btn5.Tag = CurrentColors[4];
            btn6.Tag = CurrentColors[5];

            GotoWelcomeScreen();
            OnNavigatedTo(null);
        }

        public List<Color> ReturnAllColors()
        {
            List<Color> ColorsToPick = new List<Color>();
            ColorsToPick.Add(Colors.Black);
            ColorsToPick.Add(Colors.Red);
            ColorsToPick.Add(Colors.Blue);
            ColorsToPick.Add(Colors.Orange);
            ColorsToPick.Add(Color.FromArgb(255, 188, 188, 0));
            ColorsToPick.Add(Colors.Green);
            return ColorsToPick;
        }

        Color TargetColor = new Color();

        Random RND = new Random();
        
        public List<Color> RandomizeColors()
        {
            return (ReturnAllColors().OrderBy(a => Guid.NewGuid())).ToList();
        }

        Color OldTargetColor;

        public void Randomize(bool FirstRun = false)
        {
            List<Color> ColorsToPick = RandomizeColors();
                        
            do
            {
                TargetColor = ColorsToPick[RND.Next(0, 5)];
            } while (TargetColor == OldTargetColor );

            OldTargetColor = TargetColor;

            txtColorName.Foreground = new SolidColorBrush(TargetColor);
            ColorsToPick.Remove(TargetColor);
            txtColorName.Text = ColorToString(ColorsToPick[RND.Next(0, 4)]);

            if (Change_ButtonBackgrounds)
            {
                //Randomizing button background colors
                List<Color> BackColors = RandomizeColors();

                btn1.Background = new SolidColorBrush(BackColors[0]);
                btn2.Background = new SolidColorBrush(BackColors[1]);
                btn3.Background = new SolidColorBrush(BackColors[2]);
                btn4.Background = new SolidColorBrush(BackColors[3]);
                btn5.Background = new SolidColorBrush(BackColors[4]);
                btn6.Background = new SolidColorBrush(BackColors[5]);

                //Fix the text colors of the buttons.
                if (BackColors[0] == Color.FromArgb(255, 188, 188, 0))
                {
                    btn1.Foreground = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    btn1.Foreground = new SolidColorBrush(Colors.White);
                }
                if (BackColors[1] == Color.FromArgb(255, 188, 188, 0))
                {
                    btn2.Foreground = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    btn2.Foreground = new SolidColorBrush(Colors.White);
                }
                if (BackColors[2] == Color.FromArgb(255, 188, 188, 0))
                {
                    btn3.Foreground = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    btn3.Foreground = new SolidColorBrush(Colors.White);
                }
                if (BackColors[3] == Color.FromArgb(255, 188, 188, 0))
                {
                    btn4.Foreground = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    btn4.Foreground = new SolidColorBrush(Colors.White);
                }
                if (BackColors[4] == Color.FromArgb(255, 188, 188, 0))
                {
                    btn5.Foreground = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    btn5.Foreground = new SolidColorBrush(Colors.White);
                }
                if (BackColors[5] == Color.FromArgb(255, 188, 188, 0))
                {
                    btn6.Foreground = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    btn6.Foreground = new SolidColorBrush(Colors.White);
                }
            }
            else
            {
                btn1.Background = new SolidColorBrush(Colors.Black);
                btn2.Background = new SolidColorBrush(Colors.Black);
                btn3.Background = new SolidColorBrush(Colors.Black);
                btn4.Background = new SolidColorBrush(Colors.Black);
                btn5.Background = new SolidColorBrush(Colors.Black);
                btn6.Background = new SolidColorBrush(Colors.Black);

                btn1.Foreground = new SolidColorBrush(Colors.White);
                btn2.Foreground = new SolidColorBrush(Colors.White);
                btn3.Foreground = new SolidColorBrush(Colors.White);
                btn4.Foreground = new SolidColorBrush(Colors.White);
                btn5.Foreground = new SolidColorBrush(Colors.White);
                btn6.Foreground = new SolidColorBrush(Colors.White);
            }

            if (Change_ButtonPositions)
            {
                //Randomizing Button Colors
                List<Color> CurrentColors = RandomizeColors();
                btn1.Content = ColorToString(CurrentColors[0]);
                btn2.Content = ColorToString(CurrentColors[1]);
                btn3.Content = ColorToString(CurrentColors[2]);
                btn4.Content = ColorToString(CurrentColors[3]);
                btn5.Content = ColorToString(CurrentColors[4]);
                btn6.Content = ColorToString(CurrentColors[5]);

                btn1.Tag = CurrentColors[0];
                btn2.Tag = CurrentColors[1];
                btn3.Tag = CurrentColors[2];
                btn4.Tag = CurrentColors[3];
                btn5.Tag = CurrentColors[4];
                btn6.Tag = CurrentColors[5];            
            }
            if (!FirstRun)
            {
                anim_Correct_Out.Begin();
            }
            EvalInProgress = false;
           
        }

        public string ColorToString(Color C)
        {            
            if (C == Colors.Black)
            {
                return "Black";
            }
            else if (C == Colors.Red)
            {
                return "Red";
            }
            else if (C == Colors.Blue)
            {
                return "Blue";
            }
            else if (C == Colors.Orange)
            {
                return "Orange";
            }
            else if (C == Color.FromArgb(255, 188, 188, 0))
            {
                return "Yellow";
            }
            else if (C == Colors.Green)
            {
                return "Green";
            }
            else
            {
                return "";
            }
        }
    }
}