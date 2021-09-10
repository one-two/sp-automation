using FlaUI.Core.Capturing;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using spauto;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace LGpoc
{
    
    class Program
    {
        public static string dangerName = "";
        public static bool printMode = false;
        public static BreakConfig randomBreak = new(false, 1000, 15); // (active(true/false), probability 1 in x, time of break)
        public static int tabs = 4; // quantity of tabs from url bar to game canvas
        public static int[] playerColor = new int[3] {231, 9, 1};
        public static int chromePID = 18648;
        public static int ensureGameOver = 10;


        static void Main(string[] args)
        {

            var app = FlaUI.Core.Application.Attach(chromePID);
            using (var automation = new UIA3Automation())
            {
                var window = app.GetMainWindow(automation);
                window.Focus();

                Game();
            }
        }

        public static void Game()
        {


            #region Screen env data

            EnvironmentData Env = new EnvironmentData();
            int gameXOffset = Env.GameXOffset; // top left corner of the game action screen (scoreboard not included)
            int gameYOffset = Env.GameYOffset;

            int gameXSize = Env.GameXSize; // game screen width
            int gameYSize = Env.GameYSize; // game screen height (player position not included)

            int playerLine = Env.PlayerLine; // y coordinate that passes thru the blue | of the players ship when in bottom of the screen
            int enemyEndLine = Env.EnemyEndLine;
            int enemyLine = Env.EnemyLine; // some line close in front of the player 
            int bombArea = Env.BombArea;

            int meteorRight = Env.MeteorRight;

            int gameOverLine = Env.GameOverLine; // coordinate of the game over phrase (big red text after death)
            int blueWarning = Env.BlueWarning; // blue alert coordinate after confirming signature

            long enemyTime = Env.EnemyTime;

            Debug.WriteLine(gameXOffset + " " + gameXSize + " " + meteorRight);

            #endregion

            //Thread.Sleep(1000);
            //Keyboard.Type(VirtualKeyShort.TAB);

            
            //if (gameXSize > 250) tabs = 16;
            for (int i = 0; i < tabs; i++)
            {
                Thread.Sleep(100);
                Keyboard.Type(VirtualKeyShort.TAB);
            }
            Thread.Sleep(500);
            //Keyboard.Type(VirtualKeyShort.ENTER);
            //Thread.Sleep(3000);

            bool lockk = true;
            int loops = 0;
            int playerPos = 999;
            int dangerLine = -1;
            long meteorTime = 0;
            dangerName = "";

            while (lockk)
            {
                CaptureImage mainGame = Capture.Rectangle(new Rectangle(gameXOffset, gameYOffset, gameXSize, gameYSize));
                CaptureImage player = Capture.Rectangle(new Rectangle(gameXOffset, playerLine, gameXSize, 1));
                CaptureImage enemyGone = Capture.Rectangle(new Rectangle(gameXOffset, enemyEndLine, gameXSize, 1));
                CaptureImage closeEnemy = Capture.Rectangle(new Rectangle(gameXOffset, enemyLine, gameXSize, 1));
                CaptureImage closeBomb = Capture.Rectangle(new Rectangle(gameXOffset, bombArea, gameXSize, (int)(gameYSize*0.22)));
                CaptureImage meteorIncRight = Capture.Rectangle(new Rectangle(meteorRight, gameYOffset, 10, (int)(gameYSize * 0.75)));
                CaptureImage gameOver = Capture.Rectangle(new Rectangle(gameXOffset, gameOverLine, gameXSize, 1));

                PrintCapturesToFile(loops, mainGame, player, closeBomb, meteorIncRight, printMode);

                loops++;
                GameOverScreenExec(gameXOffset, gameYOffset, gameXSize, gameYSize, blueWarning, loops, ref playerPos, ref dangerLine, ref enemyTime, gameOver, randomBreak);

                // reset position to the bottom of the screen
                GetPlayerPosition(gameXOffset, gameXSize, playerLine, ref enemyTime, ref playerPos, ref dangerLine, ref player);

                // enemyGone = reset dangerLine
                dangerLine = ResetCollisionColumn(dangerLine, enemyGone, ref enemyTime);

                // get "enemy in collision course" x coordinate
                GetCollisionCourseCoordinates(ref enemyTime, playerPos, ref dangerLine, closeEnemy);

                GetMeteorDanger(ref dangerLine, ref enemyTime, playerPos, meteorIncRight, mainGame);

                GetBombDanger(ref dangerLine, ref enemyTime, playerPos, closeBomb);

                // main game reaction loop
                ReactionLoop(playerLine, loops, playerPos, ref dangerLine, ref meteorTime, mainGame);

            }
        }

        private static void GetMeteorDanger(ref int dangerLine, ref long enemyTime, int playerPos, CaptureImage meteorIncRight, CaptureImage mainGame)
        {
            Bitmap meteorScreenRight = new Bitmap(meteorIncRight.Bitmap);
            Bitmap mg = new Bitmap(mainGame.Bitmap);
            for (int i = 0; i < meteorScreenRight.Width; i++)
            {
                for (int j = 0; j < meteorScreenRight.Height; j++)
                {
                    Color px = meteorScreenRight.GetPixel(i, j);
                    if ((px.R > 118 && px.R < 126) && (px.G > 64 && px.G < 72) && (px.B > 71 && px.B < 79))
                    {
                        //Debug.WriteLine("Meteor Right found: " + j + " Player: " + playerPos);
                        enemyTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() + 1000;
                        dangerName = "Meteor";
                        dangerLine = (int)(mg.Width * 0.35);
                        Keyboard.Press(VirtualKeyShort.RIGHT);
                        Thread.Sleep(100);
                        Keyboard.Release(VirtualKeyShort.RIGHT);

                        i = 99999;
                        j = 99999;
                    }
                }
            }
            
        }

        private static void GetBombDanger(ref int dangerLine, ref long enemyTime, int playerPos, CaptureImage closeBomb)
        {
            using (Bitmap bombScreen = new Bitmap(closeBomb.Bitmap))
            {
                for (int i = 0; i < bombScreen.Height; i++)
                {
                    for (int j = 0; j < bombScreen.Width; j++)
                    {
                        Color px = bombScreen.GetPixel(j, i);
                        if ((px.R > 227 && px.R < 233) && (px.G > 145 && px.G < 151) && (px.B > 15 && px.B < 20))
                        {
                            //Debug.WriteLine("Bomb found: " + j + " Player: " + playerPos);
                            enemyTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() + 2000;
                            dangerName = "Bomb";
                            if (playerPos > j)
                            {
                                dangerLine = j + (int)(bombScreen.Width * 0.11);
                            }
                            else
                            {
                                dangerLine = (j - (int)(bombScreen.Width * 0.11)) <= 0 ? 1: (j - (int)(bombScreen.Width * 0.11));
                            }

                            if (playerPos >= j- (int)(bombScreen.Width * 0.12) && playerPos <= j+ (int)(bombScreen.Width * 0.12))
                            {
                                if (playerPos < bombScreen.Width/2)
                                {
                                    //Debug.WriteLine("Bomb tooclose: " + j + " Player: " + playerPos);
                                    Keyboard.Release(VirtualKeyShort.LEFT);
                                    Keyboard.Press(VirtualKeyShort.RIGHT);
                                    Thread.Sleep(50);
                                    Keyboard.Release(VirtualKeyShort.RIGHT);
                                }
                                else
                                {
                                    Keyboard.Release(VirtualKeyShort.RIGHT);
                                    Keyboard.Press(VirtualKeyShort.LEFT);
                                    Thread.Sleep(50);
                                    Keyboard.Release(VirtualKeyShort.LEFT);
                                }
                            }
                            i = 99999;
                            j = 99999;
                        }
                    }

                }
            }
        }

        private static void ReactionLoop(int playerLine, int loops, int playerPos, ref int dangerLine, ref long meteorTime, CaptureImage mainGame)
        {
            using (Bitmap mainGameScreen = new Bitmap(mainGame.Bitmap))
            {
                for (int i = mainGameScreen.Height - 1; i > 0; i--)
                {
                    for (int j = mainGameScreen.Width - 1; j > 0; j--)
                    {
                        Color px = mainGameScreen.GetPixel(j, i);

                        // if power up skip 10x10 (NOT WORKING PROPERLY)
                        //if (((px.R > 225 && px.R < 240) && (px.G > 65 && px.G < 80) && (px.B > 40 && px.B < 50))
                        //    || ((px.R > 165 && px.R < 175) && (px.G > 55 && px.G < 65) && (px.B > 55 && px.B < 65)))
                        //{
                        //    j -= 12;
                        //    System.Diagnostics.Debug.WriteLine(loops + ": Power Up Skip");

                        //}

                        // "align with enemy ship" loop
                        if (((px.R > 220 && px.R < 230) && (px.G > 240 && px.G < 250) && (px.B > 240 && px.B < 250))
                            || (px.R == 255) && (px.G == 255) && (px.B == 255)
                            )
                        {
                            if (j > dangerLine - (int)(mainGameScreen.Width * 0.05) && j < dangerLine + (int)(mainGameScreen.Width * 0.05))
                            {
                                continue;
                            }
                            if ((j > playerPos + (int)(mainGameScreen.Width * 0.22)) // far right
                                && (dangerLine == -1 || dangerLine < playerPos + (int)(mainGameScreen.Width * 0.03))) // danger line on the left
                            {
                                Keyboard.Release(VirtualKeyShort.LEFT);
                                Keyboard.Press(VirtualKeyShort.RIGHT);
                            }
                            else if ((j < playerPos - (int)(mainGameScreen.Width * 0.22)) //far left
                                && (dangerLine == -1 || dangerLine > playerPos - (int)(mainGameScreen.Width * 0.03))) // danger line on the right
                            {
                                Keyboard.Release(VirtualKeyShort.RIGHT);
                                Keyboard.Press(VirtualKeyShort.LEFT);
                            }
                            else
                            {
                                Keyboard.Release(VirtualKeyShort.LEFT);
                                Keyboard.Release(VirtualKeyShort.RIGHT);
                                int wait = 50;
                                if ((j > playerPos + (int)(mainGameScreen.Width * 0.02))
                                    && (dangerLine == -1 || dangerLine < playerPos + (int)(mainGameScreen.Width * 0.03)))
                                {

                                    if (j > playerPos + (int)(mainGameScreen.Width * 0.06)) wait = 70;
                                    Keyboard.Press(VirtualKeyShort.RIGHT);
                                    Thread.Sleep(wait);
                                    Keyboard.Release(VirtualKeyShort.RIGHT);
                                }
                                if ((j < playerPos - (int)(mainGameScreen.Width * 0.02))
                                    && (dangerLine == -1 || dangerLine > playerPos - (int)(mainGameScreen.Width * 0.03)))
                                {
                                    if (j < playerPos - (int)(mainGameScreen.Width * 0.06)) wait = 70;
                                    Keyboard.Press(VirtualKeyShort.LEFT);
                                    Thread.Sleep(wait);
                                    Keyboard.Release(VirtualKeyShort.LEFT);
                                }
                                if (loops % 2 == 0 && dangerName != "Meteor")
                                {
                                    Keyboard.Press(VirtualKeyShort.SPACE);
                                }
                                if (loops % 13 == 0)
                                {
                                    Keyboard.Release(VirtualKeyShort.SPACE);
                                    if (i < playerLine - (int)(mainGameScreen.Height * 0.20) && dangerName != "Meteor")
                                    {
                                        Keyboard.Type(VirtualKeyShort.LSHIFT);
                                    }
                                }
                                    
                            }

                            if (dangerLine != -1)
                            {
                                //Debug.WriteLine("Danger: " + dangerName + " " + dangerLine + ", Player:" + playerPos + ", Enemy:" + j);

                            }
                            //Debug.WriteLine("Distance%: " + (playerPos-j)/(mainGameScreen.Width+1.0));

                            if (!((playerPos < dangerLine && dangerLine < j)
                                || (playerPos > dangerLine && dangerLine > j))
                                || dangerLine == -1)
                            {
                                i = -1;
                                j = -1;
                            }
                            
                            //get out of loop
                        }
                    }
                }
                if (dangerLine != -1)
                {
                    if ((playerPos < dangerLine + (int)(mainGameScreen.Width * 0.06))
                        && (playerPos > dangerLine - (int)(mainGameScreen.Width * 0.06)))
                    {
                        if (playerPos < mainGameScreen.Width / 2)
                        {
                            Keyboard.Press(VirtualKeyShort.RIGHT);
                            Thread.Sleep(50);
                            Keyboard.Release(VirtualKeyShort.RIGHT);
                        }
                        else
                        {
                            Keyboard.Press(VirtualKeyShort.LEFT);
                            Thread.Sleep(50);
                            Keyboard.Release(VirtualKeyShort.LEFT);
                        }

                    }
                }

            }
        }

        private static void GetCollisionCourseCoordinates(ref long enemyTime, int playerPos, ref int dangerLine, CaptureImage closeEnemy)
        {
            using (Bitmap enemyBmp = new Bitmap(closeEnemy.Bitmap))
            {
                for (int i = 1; i < enemyBmp.Width; i++)
                {
                    Color px = enemyBmp.GetPixel(i, 0);
                    if (((px.R > 220 && px.R < 230) && (px.G > 240 && px.G < 250) && (px.B > 240 && px.B < 250)) 
                        || (px.R == 255) && (px.G == 255) && (px.B == 255)
                        )
                    {
                        dangerName = "Ship";
                        dangerLine = i;
                        enemyTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() + 600;
                        break;
                    }
                }
            }
        }

        private static int ResetCollisionColumn(int dangerLine, CaptureImage enemyGone, ref long enemyTime)
        {
            using (Bitmap enemyBmp = new Bitmap(enemyGone.Bitmap))
            {
                if(enemyTime < DateTimeOffset.Now.ToUnixTimeMilliseconds())
                {
                    dangerLine = -1;
                    enemyTime = 0;
                    dangerName = "";
                } 
                else
                {
                    for (int i = 0; i < enemyBmp.Width; i++)
                    {
                        Color px = enemyBmp.GetPixel(i, 0);

                        if ((px.R > 220 && px.R < 230) && (px.G > 240 && px.G < 250) && (px.B > 240 && px.B < 250))
                        {
                            if (dangerName == "Ship")
                            {
                                dangerLine = -1;
                                enemyTime = 0;
                                dangerName = "";
                            }
                        }
                    }
                }
                
            }

            return dangerLine;
        }

        private static void GetPlayerPosition(int gameXOffset, int gameXSize, int playerLine, ref long enemyTime, ref int playerPos, ref int dangerLine, ref CaptureImage player)
        {
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
                    if ((px.R > playerColor[0]-5 && px.R < playerColor[0]+5) && (px.G > playerColor[1]-5 && px.G < playerColor[1]+5) && (px.B > playerColor[2]-5 && px.B < playerColor[2]+5))
                    {
                        playerPos = i;
                        long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                        if (enemyTime != 0 && enemyTime < now)
                        {
                            dangerLine = -1;
                            enemyTime = 0;
                            dangerName = "";
                        }
                    }
                }
            }
            //Debug.WriteLine("playerpos : " + playerPos);
        }

        private static void GameOverScreenExec(int gameXOffset, int gameYOffset, int gameXSize, int gameYSize, int blueWarning, int loops, ref int playerPos, ref int dangerLine, ref long enemyTime, CaptureImage gameOver, BreakConfig randomBreak)
        {
            using (Bitmap gameOverBmp = new Bitmap(gameOver.Bitmap))
            {
                for (int i = 0; i < gameOverBmp.Width; i++)
                {
                    Color px = gameOverBmp.GetPixel(i, 0);

                    //found red text
                    if ((px.R > 230 && px.R <= 255) && (px.G > 100 && px.G < 120) && (px.B > 100 && px.B < 120))
                    {
                        ensureGameOver--;
                        System.Diagnostics.Debug.WriteLine("Game over screen found: " + ensureGameOver);
                        if (ensureGameOver > 0) break;
                        string x = DateTime.Now.ToString();
                        //System.Diagnostics.Debug.WriteLine(loops + ": Game over screen found: " + playerPos);

                        i = 9999;
                        Thread.Sleep(8000);

                        //lock when in maintenance
                        CheckForMaintenance();

                        if (randomBreak.Active)
                        {
                            Random rd = new Random();
                            int dice = rd.Next(0, randomBreak.Probability);
                            if (dice == 1)
                            {
                                int sd = (int)Math.Round((rd.NextDouble()*10)-5);
                                Thread.Sleep(1000 * 60 * (randomBreak.Minutes+sd));
                            }
                        }


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

                        bool gotApp = false;
                        while (!gotApp)
                        {
                            CaptureImage appPhoto = Capture.Rectangle(new Rectangle(gameXOffset, gameYOffset, gameXSize, 1));

                            Bitmap appBit = new Bitmap(appPhoto.Bitmap);
                            Color appBitSample = appBit.GetPixel(gameXSize - 1, 0);

                            if ((appBitSample.R > 200)
                                && (appBitSample.B > 200)
                                && (appBitSample.G > 200))
                            {
                                Debug.WriteLine("got app!");
                                gotApp = true;
                            }
                            else
                            {
                                //Debug.WriteLine("got no app here");
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
                        while (waitingWarning)
                        {
                            //Debug.WriteLine("Waiting for blue");
                            Thread.Sleep(1000);
                            //307 575
                            CaptureImage blueWarn = Capture.Rectangle(new Rectangle(blueWarning, gameYOffset, 1, gameYSize));
                            Bitmap warn = new Bitmap(blueWarn.Bitmap);

                            for (int j = 0; j < warn.Height; j++)
                            {
                                Color sample = warn.GetPixel(0, j);

                                if ((sample.R > 30 && sample.R < 40)
                                    && (sample.G > 15 && sample.G < 30)
                                    && (sample.B > 120 && sample.B < 155))
                                {
                                    Debug.WriteLine("got blue!");
                                    waitingWarning = false;
                                    break;
                                }
                            }
                            
                        }

                        Thread.Sleep(3000);
                        Keyboard.Type(VirtualKeyShort.ENTER);

                        //blue warning gone loop
                        waitingWarning = true;
                        while (waitingWarning)
                        {
                            CaptureImage blueWarn = Capture.Rectangle(new Rectangle(blueWarning, gameXOffset, 1, gameYSize));
                            Bitmap warn = new Bitmap(blueWarn.Bitmap);
                            

                            for (int j = 0; j < warn.Height; j++)
                            {
                                Color sample = warn.GetPixel(0, j);
                                if (!((sample.R > 30 && sample.R < 40)
                                && (sample.G > 20 && sample.G < 30)
                                && (sample.B > 120 && sample.B < 140)))
                                {
                                    waitingWarning = false;
                                }
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
                        dangerName = "";
                        enemyTime = 0;
                        ensureGameOver = 10;
                        Thread.Sleep(5000);
                    }
                }
            }
        }

        private static void CheckForMaintenance()
        {
            int hourNOW = DateTime.Now.Hour;
            
            bool areWeThereYet = false;

            while (!areWeThereYet)
            {
                if (hourNOW >= 22 && hourNOW < 5)
                {
                    Thread.Sleep(1000 * 60 * 10);
                }
                else
                {
                    areWeThereYet = true;
                }
            }
        }

        private static void PrintCapturesToFile(int loops, CaptureImage image, CaptureImage player, CaptureImage bomb, CaptureImage right, bool printMode)
        {
            if (loops % 100 == 0 && printMode == true)
            {
                player.ToFile(@"c:\temp\player" + loops + @".png");
                image.ToFile(@"c:\temp\game" + loops + @".png");
                bomb.ToFile(@"c:\temp\bomb" + loops + @".png");
                right.ToFile(@"c:\temp\a1" + loops + @".png");
            }
        }

    }
}
