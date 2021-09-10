using FlaUI.Core.Capturing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace spauto
{
    public class EnvironmentData
    {
        public int GameXOffset { get; } // top left corner of the game action screen (scoreboard not included)
        public int GameYOffset { get; }

        public int GameXSize { get; } // game screen width
        public int GameYSize { get; } // game screen height (player position not included)

        public int PlayerLine { get; } // y coordinate that passes thru the blue | of the players ship when in bottom of the screen
        public int EnemyEndLine { get; }
        public int EnemyLine { get; } // some line close in front of the player 

        public int GameOverLine { get; } // coordinate of the game over phrase (big red text after death)
        public int BlueWarning { get; } // blue alert coordinate after confirming signature

        public long EnemyTime { get; }

        public int BombArea { get; }
        public int MeteorLeft { get; }
        public int MeteorRight { get; }

        public EnvironmentData()
        {
            CaptureImage screen = Capture.MainScreen();

            screen.ToFile(@"c:\temp\screen.png");
            Point p = new();
            Thread.Sleep(2000);
            using (Bitmap screenBmp = new Bitmap(screen.Bitmap))
            {
                Bitmap processedCopy = new Bitmap(screenBmp.Width, screenBmp.Height);
                for (int i = 0; i < screenBmp.Width; i++)
                {
                    for (int j = 0; j < screenBmp.Height; j++)
                    {
                        Color px = screenBmp.GetPixel(i, j);
                        processedCopy.SetPixel(i, j, px);
                        if (((px.R == 0) && (px.G >= 90 && px.G <= 100) && (px.B >= 125 && px.B <= 135)))
                        {
                            Color screenPx = screenBmp.GetPixel(i + 40, j + 40);
                            if (((screenPx.R <= 15) && (screenPx.G <= 12) && (screenPx.B <= 30)) && p.X == 0)
                            {
                                p.X = i + 40;
                                p.Y = j + 40;
                                i = 99999;
                                j = 99999;
                            }
                            else
                            {
                                processedCopy.SetPixel(i, j, Color.Crimson);
                            }
                        }
                    }
                }

                // get corner
                Color pxLimit = screenBmp.GetPixel(p.X, p.Y);
                while (pxLimit.B <= 28)
                {
                    p.X--;
                    pxLimit = screenBmp.GetPixel(p.X, p.Y);
                }
                p.X++;
                pxLimit = screenBmp.GetPixel(p.X, p.Y);
                while (pxLimit.B <= 28)
                {
                    p.Y--;
                    pxLimit = screenBmp.GetPixel(p.X, p.Y);
                }
                p.Y++;

                Point size = new();

                // rect on screen limit
                pxLimit = screenBmp.GetPixel(p.X, p.Y);
                while (pxLimit.B <= 40)
                {
                    size.X++;
                    pxLimit = screenBmp.GetPixel(p.X + size.X, p.Y);
                    processedCopy.SetPixel(size.X, p.Y, Color.Crimson);
                }
                size.X--;
                pxLimit = screenBmp.GetPixel(p.X, p.Y);
                while (pxLimit.B <= 135)
                {
                    size.Y++;
                    pxLimit = screenBmp.GetPixel(p.X, p.Y + size.Y);
                }
                size.Y--;

                Point gameOrigin = new()
                {
                    X = p.X + (int)Math.Round(size.X * 0.275),
                    Y = p.Y + (int)Math.Round(size.Y * 0.05)
                };
                Point gameSize = new()
                {
                    X = size.X - 2 * (int)Math.Round(size.X * 0.275),
                    Y = size.Y - 2 * (int)Math.Round(size.Y * 0.05)
                };



                //      NOTE
                // pX,pY 0,0
                // pX+size.X, py max,0
                // px, py+size.Y 0,max
                // pX+size.X, py+size.Y max, max

                Debug.WriteLine(p.X + " " + size.X + " " + p.Y + " " + size.Y);

                this.GameXOffset = gameOrigin.X;
                this.GameYOffset = gameOrigin.Y;// + (int)Math.Round(gameSize.Y * 0.09);
                this.GameXSize = gameSize.X;
                this.GameYSize = (int)Math.Round(gameSize.Y * 0.85);
                this.PlayerLine = gameOrigin.Y + (int)Math.Round(gameSize.Y * 0.933);
                this.EnemyEndLine = gameOrigin.Y + (int)Math.Round(gameSize.Y * 0.96);
                this.EnemyLine = gameOrigin.Y + (int)Math.Round(gameSize.Y * 0.75);
                this.GameOverLine = gameOrigin.Y + (int)Math.Round(gameSize.Y * 0.47);
                this.BlueWarning = gameOrigin.X + (int)Math.Round(gameSize.X * 0.5);
                this.EnemyTime = 0;
                this.BombArea = gameOrigin.Y + (int)Math.Round(gameSize.Y * 0.8);
                this.MeteorRight = gameOrigin.X + (int)Math.Round(gameSize.X * 0.96);


                for (int i = 0; i < GameYSize; i++)
                {
                    processedCopy.SetPixel(GameXOffset, GameYOffset + i, Color.Cyan);
                    processedCopy.SetPixel(GameXOffset + GameXSize, GameYOffset + i, Color.Cyan);
                    processedCopy.SetPixel(GameXOffset + GameXSize / 2, BlueWarning + i, Color.Blue);
                }

                for (int i = 0; i < GameXSize; i++)
                {
                    processedCopy.SetPixel(GameXOffset + i, GameYOffset, Color.Cyan);
                    processedCopy.SetPixel(GameXOffset + i, GameYOffset + GameYSize, Color.Cyan);
                }
                for (int i = 0; i < GameXSize; i++)
                {
                    processedCopy.SetPixel(GameXOffset + i, PlayerLine, Color.Green);
                    processedCopy.SetPixel(GameXOffset + i, EnemyEndLine, Color.MediumPurple);
                    processedCopy.SetPixel(GameXOffset + i, GameOverLine, Color.Crimson);
                    processedCopy.SetPixel(GameXOffset + i, EnemyLine, Color.MediumPurple);
                }

                processedCopy.Save(@"c:\temp\screen-detected.png", ImageFormat.Bmp);

                            
            }
        }


    }
}
