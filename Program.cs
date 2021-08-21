using FlaUI.Core.Capturing;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using System;
using System.Drawing;
using System.Threading;

namespace LGpoc
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = FlaUI.Core.Application.Attach(10748);
            //var app = FlaUI.Core.Application.Launch(@"C:\Program Files\Google\Chrome\Application\chrome.exe");
            using (var automation = new UIA3Automation())
            {
                var window = app.GetMainWindow(automation);
                window.Focus();
                Thread.Sleep(1000);
                Keyboard.Type(VirtualKeyShort.TAB);
                Thread.Sleep(1000);
                Keyboard.Type(VirtualKeyShort.TAB);
                Thread.Sleep(1000);
                Keyboard.Type(VirtualKeyShort.TAB);
                Thread.Sleep(1000);
                Keyboard.Type(VirtualKeyShort.TAB);
                Keyboard.Type(VirtualKeyShort.ENTER);
                Thread.Sleep(5000);
                Game();
            }
            //‪@"C:\Program Files\Google\Chrome\Application\chrome.exe"

        }

        public static void Game()
        {
            int gameXOffset = 347;
            int gameYOffset = 566;
            int gameXSize = 207;
            int gameYSize = 174;
            int playerLine = 788;
            int enemyLine = 760;
            int gameOverLine = 681;
            int blueWarning = 657;

            bool lockk = true;
            int loops = 0;
            int playerPos = 0;
            int dangerLine = 0;
            while (lockk)
            {
                //if (loops > 10000) lockk = false;
                CaptureImage image = Capture.Rectangle(new Rectangle(gameXOffset, gameYOffset, gameXSize, gameYSize));
                CaptureImage player = Capture.Rectangle(new Rectangle(gameXOffset, playerLine, gameXSize, 1));
                CaptureImage gameOver = Capture.Rectangle(new Rectangle(gameXOffset, gameOverLine, gameXSize, 1));

                if (loops % 100 == 0)
                {

                    player.ToFile(@"c:\temp\player" + loops + @".png");
                    image.ToFile(@"c:\temp\game" + loops + @".png");
                }

                loops++;
                using (Bitmap gameOverBmp = new Bitmap(gameOver.Bitmap))
                {
                    for (int i = 0; i < gameOverBmp.Width; i++)
                    {
                        Color px = gameOverBmp.GetPixel(i, 0);

                        //found red text
                        if ((px.R > 230 && px.R < 240) && (px.G > 100 && px.G < 110) && (px.B > 100 && px.B < 110))
                        {
                            i = 9999;
                            Thread.Sleep(5000);
                            Keyboard.Type(VirtualKeyShort.ENTER);

                            Keyboard.Release(VirtualKeyShort.LEFT);
                            Keyboard.Release(VirtualKeyShort.RIGHT);
                            Thread.Sleep(4000);
                            Keyboard.Type(VirtualKeyShort.ENTER);
                            Thread.Sleep(5000);

                            Keyboard.Type(VirtualKeyShort.TAB);
                            Thread.Sleep(1000);
                            Keyboard.Type(VirtualKeyShort.TAB);
                            Thread.Sleep(1000);
                            Keyboard.Type(VirtualKeyShort.SPACE);
                            Thread.Sleep(4000);


                            //aviso azul
                            bool waitingWarning = true;
                            while (waitingWarning)
                            {
                                CaptureImage blueWarn = Capture.Rectangle(new Rectangle(gameXOffset, blueWarning, gameXSize, 1));
                                Bitmap warn = new Bitmap(blueWarn.Bitmap);
                                Color sample = warn.GetPixel(0, 0);
                                
                                if ((sample.R > 30 && sample.R < 40)
                                    && (sample.G > 20 && sample.G < 30)
                                    && (sample.B > 120 && sample.B < 140))
                                {
                                    waitingWarning = false;
                                }
                            }

                            Thread.Sleep(3000);
                            Keyboard.Type(VirtualKeyShort.ENTER);

                            waitingWarning = true;
                            while (waitingWarning)
                            {
                                CaptureImage blueWarn = Capture.Rectangle(new Rectangle(gameXOffset, blueWarning, gameXSize, 1));
                                Bitmap warn = new Bitmap(blueWarn.Bitmap);
                                Color sample = warn.GetPixel(0, 0);

                                if (!((sample.R > 30 && sample.R < 40)
                                    && (sample.G > 20 && sample.G < 30)
                                    && (sample.B > 120 && sample.B < 140)))
                                {
                                    waitingWarning = false;
                                }
                            }

                            Thread.Sleep(3000);
                            Keyboard.Press(VirtualKeyShort.DOWN);
                            Thread.Sleep(100);
                            Keyboard.Release(VirtualKeyShort.DOWN);

                            Thread.Sleep(1000);
                            Keyboard.Type(VirtualKeyShort.ENTER);
                            playerPos = 0;
                            Thread.Sleep(5000);
                        }
                    }
                }

                if (playerPos == 0)
                {
                    Keyboard.Press(VirtualKeyShort.DOWN);
                    Thread.Sleep(1000);
                    Keyboard.Release(VirtualKeyShort.DOWN);
                    player = Capture.Rectangle(new Rectangle(gameXOffset, playerLine, gameXSize, 1));
                }

                using (Bitmap playerBmp = new Bitmap(player.Bitmap))
                {

                    for (int i = 0; i < playerBmp.Width; i++)
                    {
                        Color px = playerBmp.GetPixel(i, 0);
                        if ((px.R > 30 && px.R < 40) && (px.G > 80 && px.G < 90) && (px.B > 190 && px.B < 200))
                        {
                            playerPos = i;
                        }
                        if ((px.R > 220 && px.R < 230) && (px.G > 240 && px.G < 250) && (px.B > 240 && px.B < 250))
                        {
                            dangerLine = 0;
                        }
                    }
                }

                using (Bitmap bmp = new Bitmap(image.Bitmap))
                {
                    for (int i = bmp.Height-1; i > 0; i--)
                    {
                        for (int j = bmp.Width-1; j > 0; j--)
                        {
                            int meteor = 0;
                            Color px = bmp.GetPixel(j, i);
                            if ((px.R > 120 && px.R < 130) && (px.G > 60 && px.G < 70) && (px.B > 70 && px.B < 80))
                            {
                                meteor = j;
                                if (meteor > playerPos) 
                                {
                                    Keyboard.Release(VirtualKeyShort.RIGHT);
                                    Keyboard.Press(VirtualKeyShort.LEFT);
                                }
                                else
                                {
                                    Keyboard.Release(VirtualKeyShort.LEFT);
                                    Keyboard.Press(VirtualKeyShort.RIGHT);
                                }
                                i = -1;
                                j = -1;
                            }

                            if ((px.R > 220 && px.R < 230) && (px.G > 240 && px.G < 250) && (px.B > 240 && px.B < 250))
                            {
                                int distx = Math.Abs(j - playerPos);
                                if (i < playerLine-50)
                                {
                                    if (j > playerPos+20) 
                                    {
                                        System.Diagnostics.Debug.WriteLine("going right");
                                        Keyboard.Release(VirtualKeyShort.LEFT);
                                        Keyboard.Press(VirtualKeyShort.RIGHT); 
                                    }
                                    else if (j < playerPos-20) 
                                    {
                                        System.Diagnostics.Debug.WriteLine("going left");
                                        Keyboard.Release(VirtualKeyShort.RIGHT);
                                        Keyboard.Press(VirtualKeyShort.LEFT);
                                    }
                                    else
                                    {
                                        System.Diagnostics.Debug.WriteLine("going center");
                                        Keyboard.Release(VirtualKeyShort.LEFT);
                                        Keyboard.Release(VirtualKeyShort.RIGHT);
                                        if (j > playerPos+4)
                                        {
                                            Keyboard.Press(VirtualKeyShort.RIGHT);
                                            Thread.Sleep(1);
                                            Keyboard.Release(VirtualKeyShort.RIGHT);
                                        }
                                        if (j < playerPos-4)
                                        {
                                            Keyboard.Press(VirtualKeyShort.LEFT);
                                            Thread.Sleep(1);
                                            Keyboard.Release(VirtualKeyShort.LEFT);
                                        }
                                        if (loops % 2 == 0)
                                        {
                                            Keyboard.Press(VirtualKeyShort.SPACE);
                                        }
                                        if (loops % 13 == 0)
                                        {
                                            Keyboard.Release(VirtualKeyShort.SPACE);
                                        }
                                    }

                                    i = -1;
                                    j = -1;
                                }
                                else
                                {
                                    if (j > playerPos) dangerLine = j-10;
                                    if (j < playerPos) dangerLine = j+10;
                                    if (j > playerPos - 3 && j < playerPos + 3)
                                    {
                                        Keyboard.Type(VirtualKeyShort.SHIFT);
                                    }
                                    
                                }
                                //get out of loop
                            }
                        }
                    }
                    
                }
                

            }
        }
    }
}
