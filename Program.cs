using FlaUI.Core.Capturing;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace LGpoc
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = FlaUI.Core.Application.Attach(6624);
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
            // game-canvas position and size on browser (sticking to left size)
            // window width = 990
            // window height = 1080
            #region big screen
            //int gameXOffset = 396; // top left corner of the game action screen (scoreboard not included)
            //int gameYOffset = 650;

            //int gameXSize = 243; // game screen width
            //int gameYSize = 248; // game screen height (player position not included)

            //int playerLine = 894; // y coordinate that passes thru the blue | of the players ship when in bottom of the screen
            //int enemyEndLine = 900;
            //int enemyLine = 862; // some line close in front of the player 

            //int gameOverLine = 760; // coordinate of the game over phrase (big red text after death)
            //int blueWarning = 735; // blue alert coordinate after confirming signature
            #endregion

            #region smoll screen

            int gameXOffset = 295; // top left corner of the game action screen (scoreboard not included)
            int gameYOffset = 492;

            int gameXSize = 169; // game screen width
            int gameYSize = 154; // game screen height (player position not included)

            int playerLine = 673; // y coordinate that passes thru the blue | of the players ship when in bottom of the screen
            int enemyEndLine = 678;
            int enemyLine = 650; // some line close in front of the player 

            int gameOverLine = 582; // coordinate of the game over phrase (big red text after death)
            int blueWarning = 562; // blue alert coordinate after confirming signature

            long enemyTime = 0;

            #endregion

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
                        if ((px.R > 230 && px.R <= 255) && (px.G > 100 && px.G < 120) && (px.B > 100 && px.B < 120))
                        {
                            string x = DateTime.Now.ToString();
                            System.Diagnostics.Debug.WriteLine(loops +": Game over screen found: " + x);

                            i = 9999;
                            Thread.Sleep(8000);

                            // red death screen
                            Keyboard.Press(VirtualKeyShort.ENTER);
                            Thread.Sleep(8000);
                            Keyboard.Release(VirtualKeyShort.ENTER);

                            Debug.WriteLine("out of game over screen");

                            Keyboard.Release(VirtualKeyShort.LEFT);
                            Keyboard.Release(VirtualKeyShort.RIGHT);
                            Thread.Sleep(4000);
                            Keyboard.Press(VirtualKeyShort.ENTER);
                            Thread.Sleep(40);
                            Keyboard.Release(VirtualKeyShort.ENTER);
                            Thread.Sleep(5000);

                            //herephoto

                            bool gotApp = false;
                            while(!gotApp)
                            {
                                CaptureImage appPhoto = Capture.Rectangle(new Rectangle(gameXOffset, gameYOffset, gameXSize, 1));

                                Bitmap appBit = new Bitmap(appPhoto.Bitmap);
                                Color appBitSample = appBit.GetPixel(gameXSize - 1, 0);

                                if ((appBitSample.R == Color.White.R)
                                    && (appBitSample.B == Color.White.B)
                                    && (appBitSample.G == Color.White.G))
                                {
                                    gotApp = true;
                                }

                                else
                                {
                                    Debug.WriteLine("got no app here");
                                    Keyboard.Press(VirtualKeyShort.ENTER);
                                    Thread.Sleep(40);
                                    Keyboard.Release(VirtualKeyShort.ENTER);
                                    Thread.Sleep(5000);
                                }
                            }
                            
                            // app interaction
                            Keyboard.Type(VirtualKeyShort.TAB);
                            Thread.Sleep(1000);
                            Keyboard.Type(VirtualKeyShort.TAB);
                            Thread.Sleep(1000);
                            Keyboard.Type(VirtualKeyShort.SPACE);
                            


                            //blue warning loop
                            bool waitingWarning = true;
                            long blueEnterTime = DateTimeOffset.Now.AddSeconds(40).ToUnixTimeSeconds();
                            int fails = 0;
                            while (waitingWarning)
                            {
                                Debug.WriteLine("Waiting for blue");
                                if (DateTimeOffset.Now.ToUnixTimeSeconds() > blueEnterTime)
                                {
                                    Debug.WriteLine("stuck on blue, retrying");
                                    blueEnterTime = DateTimeOffset.Now.AddSeconds(40).ToUnixTimeSeconds();
                                    Keyboard.Press(VirtualKeyShort.ENTER);
                                    Thread.Sleep(20);
                                    Keyboard.Release(VirtualKeyShort.ENTER);
                                    Thread.Sleep(5000);

                                    // app interaction
                                    Keyboard.Type(VirtualKeyShort.TAB);
                                    Thread.Sleep(1000);
                                    Keyboard.Type(VirtualKeyShort.TAB);
                                    Thread.Sleep(1000);
                                    Keyboard.Type(VirtualKeyShort.SPACE);
                                    Thread.Sleep(2000);
                                }
                                Thread.Sleep(4000);
                                //307 575
                                CaptureImage blueWarn = Capture.Rectangle(new Rectangle(gameXOffset, blueWarning, gameXSize, 1));
                                Bitmap warn = new Bitmap(blueWarn.Bitmap);
                                Color sample = warn.GetPixel(0, 0);
                                
                                if ((sample.R > 30 && sample.R < 40)
                                    && (sample.G > 20 && sample.G < 30)
                                    && (sample.B > 120 && sample.B < 140))
                                {
                                    CaptureImage errorMsg = Capture.Rectangle(new Rectangle(gameXOffset, 575, gameXSize, 1));
                                    Bitmap errorSample = new Bitmap(errorMsg.Bitmap);
                                    Color errorColorSample = errorSample.GetPixel(307- gameXOffset, 0);
                                    Debug.WriteLine("got blue!");
                                    waitingWarning = false;
                                    if ((errorColorSample.R == 255)
                                    && (errorColorSample.G == 255)
                                    && (errorColorSample.B == 255))
                                    {
                                        Debug.WriteLine("Error, try again");
                                        waitingWarning = true;
                                        Keyboard.Press(VirtualKeyShort.ENTER);
                                        Thread.Sleep(20);
                                        Keyboard.Release(VirtualKeyShort.ENTER);
                                        Thread.Sleep(20);
                                        Keyboard.Press(VirtualKeyShort.ENTER);
                                        Thread.Sleep(20);
                                        Keyboard.Release(VirtualKeyShort.ENTER);
                                        Thread.Sleep(4000);
                                        Keyboard.Press(VirtualKeyShort.ENTER);
                                        Thread.Sleep(20);
                                        Keyboard.Release(VirtualKeyShort.ENTER);
                                        Thread.Sleep(5000);

                                        // app interaction
                                        Keyboard.Type(VirtualKeyShort.TAB);
                                        Thread.Sleep(1000);
                                        Keyboard.Type(VirtualKeyShort.TAB);
                                        Thread.Sleep(1000);
                                        Keyboard.Type(VirtualKeyShort.SPACE);
                                        Thread.Sleep(4000);
                                    }
                                        
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
                            long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                            if (enemyTime != 0 && enemyTime + 500 < now) 
                            { 
                                dangerLine = -1;
                                enemyTime = 0;
                            }
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
                            enemyTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
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
                                System.Diagnostics.Debug.WriteLine(loops + ": Power Up Skip");

                            }
                            // "align with enemy ship" loop
                            if ((px.R > 220 && px.R < 230) && (px.G > 240 && px.G < 250) && (px.B > 240 && px.B < 250))
                             {
                                int distx = Math.Abs(j - playerPos);
                                if (i < playerLine-50)
                                {
                                    if ((j > playerPos+20) // far right
                                        && (dangerLine == -1 || dangerLine < playerPos + 15)) // danger line on the left
                                    {
                                        Keyboard.Release(VirtualKeyShort.LEFT);
                                        Keyboard.Press(VirtualKeyShort.RIGHT); 
                                    }
                                    else if ((j < playerPos-20) //far left
                                        && (dangerLine == -1 || dangerLine > playerPos - 15)) // danger line on the right
                                    {
                                        Keyboard.Release(VirtualKeyShort.RIGHT);
                                        Keyboard.Press(VirtualKeyShort.LEFT);
                                    }
                                    else
                                    {
                                        Keyboard.Release(VirtualKeyShort.LEFT);
                                        Keyboard.Release(VirtualKeyShort.RIGHT);
                                        int wait = 20;
                                        if ((j > playerPos+4)
                                            && (dangerLine == -1 || dangerLine < playerPos + 12))
                                        {
                                            
                                            if (j > playerPos + 8) wait = 50;
                                            Keyboard.Press(VirtualKeyShort.RIGHT);
                                            Thread.Sleep(wait);
                                            Keyboard.Release(VirtualKeyShort.RIGHT);
                                        }
                                        if ((j < playerPos-4)
                                            && (dangerLine == -1 || dangerLine > playerPos - 12))
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
                                        if ((playerPos < dangerLine+15) && (playerPos > dangerLine-6))
                                        {
                                            Keyboard.Press(VirtualKeyShort.LEFT);
                                            Thread.Sleep(30);
                                            Keyboard.Release(VirtualKeyShort.LEFT);
                                        } else if ((playerPos > dangerLine - 15) && (playerPos < dangerLine + 6))
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
