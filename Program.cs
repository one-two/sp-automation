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
            // game-canvas position and size on browser
            int gameXOffset = 347; // top left corner of the game action screen (scoreboard not included)
            int gameYOffset = 566;

            int gameXSize = 207; // game screen width
            int gameYSize = 174; // game screen height (player position not included)

            int playerLine = 788; // y coordinate that passes thru the blue | of the players ship when in bottom of the screen
            int enemyEndLine = 799;
            int enemyLine = 760; // some line close in front of the player 

            int gameOverLine = 681; // coordinate of the game over phrase (big red text after death)
            int blueWarning = 657; // blue alert coordinate after confirming signature

            bool lockk = true;
            int loops = 0;
            int playerPos = 0;
            int dangerLine = -1;
            while (lockk)
            {
                //if (loops > 10000) lockk = false;
                CaptureImage image = Capture.Rectangle(new Rectangle(gameXOffset, gameYOffset, gameXSize, gameYSize));
                CaptureImage player = Capture.Rectangle(new Rectangle(gameXOffset, playerLine, gameXSize, 1));
                CaptureImage enemyGone = Capture.Rectangle(new Rectangle(gameXOffset, enemyEndLine, gameXSize, 1));
                CaptureImage closeEnemy = Capture.Rectangle(new Rectangle(gameXOffset, enemyLine, gameXSize, 1));
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

                            // red death screen
                            Keyboard.Type(VirtualKeyShort.ENTER);

                            Keyboard.Release(VirtualKeyShort.LEFT);
                            Keyboard.Release(VirtualKeyShort.RIGHT);
                            Thread.Sleep(4000);
                            Keyboard.Type(VirtualKeyShort.ENTER);
                            Thread.Sleep(5000);

                            // app interaction
                            Keyboard.Type(VirtualKeyShort.TAB);
                            Thread.Sleep(1000);
                            Keyboard.Type(VirtualKeyShort.TAB);
                            Thread.Sleep(1000);
                            Keyboard.Type(VirtualKeyShort.SPACE);
                            Thread.Sleep(4000);


                            //blue warning loop
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

                            //blue warning gone loop
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

                            // go next
                            Thread.Sleep(3000);
                            Keyboard.Press(VirtualKeyShort.DOWN);
                            Thread.Sleep(100);
                            Keyboard.Release(VirtualKeyShort.DOWN);

                            // start next
                            Thread.Sleep(1000);
                            Keyboard.Type(VirtualKeyShort.ENTER);
                            playerPos = 0;
                            dangerLine = -1;
                            Thread.Sleep(5000);
                        }
                    }
                }

                // reset position to the bottom of the screen
                if (playerPos == 0)
                {
                    Keyboard.Press(VirtualKeyShort.DOWN);
                    Thread.Sleep(1000);
                    Keyboard.Release(VirtualKeyShort.DOWN);
                    player = Capture.Rectangle(new Rectangle(gameXOffset, playerLine, gameXSize, 1));
                }

                // get player x position (and enemy in player line)
                using (Bitmap playerBmp = new Bitmap(player.Bitmap))
                {

                    for (int i = 0; i < playerBmp.Width; i++)
                    {
                        Color px = playerBmp.GetPixel(i, 0);
                        if ((px.R > 30 && px.R < 40) && (px.G > 80 && px.G < 90) && (px.B > 190 && px.B < 200))
                        {
                            playerPos = i;
                        }
                    }
                }

                // enemyGone = reset dangerLine
                using (Bitmap enemyBmp = new Bitmap(enemyGone.Bitmap))
                {

                    for (int i = 0; i < enemyBmp.Width; i++)
                    {
                        Color px = enemyBmp.GetPixel(i, 0);
                        
                        if ((px.R > 220 && px.R < 230) && (px.G > 240 && px.G < 250) && (px.B > 240 && px.B < 250))
                        {
                            dangerLine = -1;
                        }
                    }
                }

                // get "enemy in collision course" x coordinate
                using (Bitmap enemyBmp = new Bitmap(closeEnemy.Bitmap))
                {
                    for (int i = 0; i < enemyBmp.Width; i++)
                    {
                        Color px = enemyBmp.GetPixel(i, 0);
                        if ((px.R > 220 && px.R < 230) && (px.G > 240 && px.G < 250) && (px.B > 240 && px.B < 250))
                        {
                            dangerLine = i;
                            i = 9999;
                        }
                        int meteor = 0;
                        if ((px.R > 120 && px.R < 130) && (px.G > 60 && px.G < 70) && (px.B > 70 && px.B < 80))
                        {
                            meteor = i;
                            if (meteor > playerPos)
                            {
                                Keyboard.Release(VirtualKeyShort.LEFT);
                                Keyboard.Press(VirtualKeyShort.RIGHT);

                            }
                            else
                            {
                                Keyboard.Release(VirtualKeyShort.RIGHT);
                                Keyboard.Press(VirtualKeyShort.LEFT);
                            }
                            i = 9999;
                            Thread.Sleep(200);
                        }
                    }
                }

                // main game reaction loop
                using (Bitmap bmp = new Bitmap(image.Bitmap))
                {
                    for (int i = bmp.Height-1; i > 0; i--)
                    {
                        for (int j = bmp.Width-1; j > 0; j--)
                        {
                            // "run away from meteor" loop
                            int meteor = 0;
                            Color px = bmp.GetPixel(j, i);
                            if ((px.R > 120 && px.R < 130) && (px.G > 60 && px.G < 70) && (px.B > 70 && px.B < 80))
                            {
                                meteor = j;
                                if (meteor > playerPos) 
                                {
                                    Keyboard.Release(VirtualKeyShort.LEFT);
                                    Keyboard.Press(VirtualKeyShort.RIGHT);
                                    
                                }
                                else
                                {
                                    Keyboard.Release(VirtualKeyShort.RIGHT);
                                    Keyboard.Press(VirtualKeyShort.LEFT);
                                }
                                i = -1;
                                j = -1;
                                Thread.Sleep(100);
                            }
                            if (meteor != 0) break;

                            // if power up skip 10x10
                            if (((px.R > 225 && px.R < 240) && (px.G > 65 && px.G < 80) && (px.B > 40 && px.B < 50))
                                || ((px.R > 165 && px.R < 175) && (px.G > 55 && px.G < 65) && (px.B > 55 && px.B < 65)))
                            {
                                j -= 12;
                            }
                                // "align with enemy ship" loop
                             if ((px.R > 220 && px.R < 230) && (px.G > 240 && px.G < 250) && (px.B > 240 && px.B < 250))
                             {
                                int distx = Math.Abs(j - playerPos);
                                if (i < playerLine-50)
                                {
                                    if ((j > playerPos+24)
                                        && (dangerLine == -1 || dangerLine < playerPos + 15))
                                    {
                                        Keyboard.Release(VirtualKeyShort.LEFT);
                                        Keyboard.Press(VirtualKeyShort.RIGHT); 
                                    }
                                    else if ((j < playerPos-24)
                                        && (dangerLine == -1 || dangerLine > playerPos - 15))
                                    {
                                        Keyboard.Release(VirtualKeyShort.RIGHT);
                                        Keyboard.Press(VirtualKeyShort.LEFT);
                                    }
                                    else
                                    {
                                        Keyboard.Release(VirtualKeyShort.LEFT);
                                        Keyboard.Release(VirtualKeyShort.RIGHT);
                                        int wait = 20;
                                        if ((j > playerPos+3)
                                            && (dangerLine == -1 || dangerLine < playerPos + 12))
                                        {
                                            
                                            if (j > playerPos + 8) wait = 50;
                                            Keyboard.Press(VirtualKeyShort.RIGHT);
                                            Thread.Sleep(wait);
                                            Keyboard.Release(VirtualKeyShort.RIGHT);
                                        }
                                        if ((j < playerPos-3)
                                            && (dangerLine == -1 || dangerLine > playerPos + 12))
                                        {
                                            if(j < playerPos - 8) wait = 50;
                                            Keyboard.Press(VirtualKeyShort.LEFT);
                                            Thread.Sleep(wait);
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
                                    System.Diagnostics.Debug.WriteLine("Danger: " + dangerLine + ", Player:" + playerPos + ", Enemy:" + j);

                                    if (!((playerPos < dangerLine && dangerLine < j)
                                        || (playerPos > dangerLine && dangerLine > j)) 
                                        || dangerLine == -1)
                                    {
                                        i = -1;
                                        j = -1;
                                    }

                                    if (dangerLine != -1)
                                    {
                                        if ((playerPos < dangerLine+10) && (playerPos > dangerLine-4))
                                        {
                                            Keyboard.Press(VirtualKeyShort.LEFT);
                                            Thread.Sleep(30);
                                            Keyboard.Release(VirtualKeyShort.LEFT);
                                        } else if ((playerPos > dangerLine - 10) && (playerPos < dangerLine + 4))
                                        {
                                            Keyboard.Press(VirtualKeyShort.RIGHT);
                                            Thread.Sleep(30);
                                            Keyboard.Release(VirtualKeyShort.RIGHT);
                                        }
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
