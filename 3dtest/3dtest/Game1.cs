using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using gamelib2d;
using gamelib3d;
using System.IO.IsolatedStorage;
using System.IO;

namespace _3dtest
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Resolution
        int displaywidth;
        int displayheight;

        SpriteFont mainfont;        // Font for drawing text on the screen

        Boolean gameover = false;   // Is the game over TRUE or FALSE?      
        float gameruntime = 0;      // Time since game started

        graphic2d background;       // Background image
        Random randomiser = new Random();       // Variable to generate random numbers

        int gamestate = -1;         // Current game state

        GamePadState[] pad = new GamePadState[1];       // Array to hold gamepad states
        KeyboardState keys;                             // Variable to hold keyboard state

        const int numberofoptions = 4;                    // Number of main menu options
        sprite2d[] menuoptions = new sprite2d[numberofoptions]; // Array of sprites to hold the menu options
        int optionselected = 0;                         // Current menu option selected

        const int numberofhighscores = 10;                              // Number of high scores to store
        int[] highscores = new int[numberofhighscores];                 // Array of high scores

        // Main 3D Game Camera
        camera gamecamera;

        staticmesh ground;  // 3D graphic for the ground in-game
        model3d uservehicle;     // Robot model for user control

        // Create an array of trees
        const int numberoftrees = 30;
        staticmesh[] tree = new staticmesh[numberoftrees];

        List<staticmesh> ufos = new List<staticmesh>();

        sprite2d up, down, left, right, left2,right2;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            // Extend battery life under lock.
            InactiveSleepTime = TimeSpan.FromSeconds(1);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            displaywidth = graphics.GraphicsDevice.Viewport.Width;
            displayheight = graphics.GraphicsDevice.Viewport.Height;
            graphics.ToggleFullScreen();

            gamecamera = new camera(new Vector3(0, 0, 0), new Vector3(0, 0, 0), displaywidth, displayheight, 45, Vector3.Up, 1000, 20000);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            mainfont = Content.Load<SpriteFont>("quartz4");  // Load the quartz4 font

            background = new graphic2d(Content, "Background for Menus", displaywidth, displayheight);

            up = new sprite2d(Content, "up", 105, displayheight - 180, 0.3f, Color.White, true);
            down = new sprite2d(Content, "down",105, displayheight - 50, 0.3f, Color.White, true);
            left = new sprite2d(Content, "left", 50, displayheight - 115, 0.3f, Color.White, true);
            right = new sprite2d(Content, "right", 160, displayheight - 115, 0.3f, Color.White, true);
            left2 = new sprite2d(Content, "left", displaywidth-180, displayheight - 100, 0.3f, Color.White, true);
            right2 = new sprite2d(Content, "right", displaywidth-60, displayheight - 100, 0.3f, Color.White, true);

            menuoptions[0] = new sprite2d(Content, "player1", displaywidth / 2, 150, 1, Color.White, true);
            menuoptions[1] = new sprite2d(Content, "options", displaywidth / 2, 220, 1, Color.White, true);
            menuoptions[2] = new sprite2d(Content, "highscore", displaywidth / 2, 290, 1, Color.White, true);
            menuoptions[3] = new sprite2d(Content, "exit", displaywidth / 2, 360, 1, Color.White, true);

            // Load the 3D models for the static objects in the game from the ContentManager
            ground = new staticmesh(Content, "sground", 100f, new Vector3(0, -40, 0), new Vector3(0, 0, 0));

            // Initialise robot1 object
            uservehicle = new model3d(Content, "tiefighter", 2f, new Vector3(1000, 0, 1000), new Vector3(0, 0, 0), 0.002f, 0.06f, 10);

            /*
            tree[50] = new staticmesh(Content, "tank", (float)(randomiser.Next(20) + 1) / 10,
                                    new Vector3(randomiser.Next(6000) - 3000, 200, randomiser.Next(6000) - 3000),
                                        new Vector3(0, randomiser.Next(7), 0));
            tree[51] = new staticmesh(Content, "ship", (float)(randomiser.Next(20) + 1) / 10,
                                    new Vector3(randomiser.Next(6000) - 3000, 200, randomiser.Next(6000) - 3000),
                                        new Vector3(0, randomiser.Next(7), 0));
            tree[52] = new staticmesh(Content, "stars", 10,
                                    new Vector3(randomiser.Next(6000) - 3000, 200, randomiser.Next(6000) - 3000),
                                        new Vector3(0, randomiser.Next(7), 0));
            */

            // Load High Scores in
            using (IsolatedStorageFile savegamestorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (savegamestorage.FileExists("highscores.txt"))
                {
                    using (IsolatedStorageFileStream fs = savegamestorage.OpenFile("highscores.txt", System.IO.FileMode.Open))
                    {
                        using (StreamReader sr = new StreamReader(fs))
                        {
                            string line;
                            for (int i = 0; i < numberofhighscores; i++)
                            {
                                line = sr.ReadLine();
                                highscores[i] = Convert.ToInt32(line);
                            }

                            sr.Close();
                        }
                    }
                }
            }
            // Sort high scores
            Array.Sort(highscores);
            Array.Reverse(highscores);

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here
            pad[0] = GamePad.GetState(PlayerIndex.One);     // Reads gamepad 1
            keys = Keyboard.GetState();                     // Read keyboard

            float timebetweenupdates = (float)gameTime.ElapsedGameTime.TotalMilliseconds; // Time between updates
            gameruntime += timebetweenupdates;  // Count how long the game has been running for


            // TODO: Add your update logic here
            switch (gamestate)
            {
                case -1:
                    // Game is on the main menu
                    updatemenu();
                    break;
                case 0:
                    // Game is being played
                    updategame(timebetweenupdates);
                    break;
                case 1:
                    // Options menu
                    updateoptions();
                    break;
                case 2:
                    // High Score table
                    updatehighscore();
                    break;
                default:
                    // Do something if none of the above are selected
                    // save high scores
                    using (IsolatedStorageFile savegamestorage = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        using (IsolatedStorageFileStream fs = new IsolatedStorageFileStream("highscores.txt", System.IO.FileMode.Create, savegamestorage))
                        {
                            using (StreamWriter writer = new StreamWriter(fs))
                            {
                                for (int i = 0; i < numberofhighscores; i++)
                                {
                                    writer.WriteLine(highscores[i].ToString());
                                }
                                writer.Flush();
                                writer.Close();
                            }
                        }
                    }
                    this.Exit();    // Quit Game
                    break;
            }

            base.Update(gameTime);
        }

        void reset()
        {
            // Reset everything for a new game
            gameover = false;
            uservehicle.position = new Vector3(1000, 0, 1000);
            uservehicle.rotation = new Vector3(0, 0, 0);
            uservehicle.velocity = new Vector3(0, 0, 0);

            // Randomly intialise trees and lampposts
            for (int i = 0; i < numberoftrees; i++)
            {
                if (i % 2 == 0)
                    tree[i] = new staticmesh(Content, "tree", (float)(randomiser.Next(20) + 1) / 10,
                                    new Vector3(randomiser.Next(6000) - 3000, 0, randomiser.Next(6000) - 3000),
                                        new Vector3(0, randomiser.Next(7), 0));
                else
                {
                    tree[i] = new staticmesh(Content, "lamppost", (float)(randomiser.Next(20) + 1) / 3,
                                    new Vector3(randomiser.Next(6000) - 3000, 0, randomiser.Next(6000) - 3000),
                                        new Vector3(0, randomiser.Next(7), 0));
                    tree[i].position.Y = tree[i].size * 40;
                }
            }

            int numbertospawn = 10 + randomiser.Next(100);

            for (int i = 0; i < numbertospawn; i++)
            {
                ufos.Add(new staticmesh(Content, "ship",
                    (float)(1 + randomiser.Next(20) / 10),
                    new Vector3(randomiser.Next(12000) - 6000, 600 + randomiser.Next(3000), randomiser.Next(12000) - 6000),
                    new Vector3(0, 0, 0)));
            }
        }

        public void updatemenu()
        {
            optionselected = -1;

            // Check for touch over a menu option
            TouchCollection tcoll = TouchPanel.GetState();
            Boolean pressed = false;
            BoundingSphere touchsphere = new BoundingSphere(new Vector3(0, 0, 0), 0);
            foreach (TouchLocation t1 in tcoll)
            {
                if (t1.State == TouchLocationState.Pressed || t1.State == TouchLocationState.Moved)
                {
                    pressed = true;
                    touchsphere = new BoundingSphere(new Vector3(t1.Position.X, t1.Position.Y, 0), 1);
                }
            }

            for (int i = 0; i < numberofoptions; i++)
            {
                if (pressed && touchsphere.Intersects(menuoptions[i].bbox))
                {
                    optionselected = i;
                    gamestate = optionselected;
                    if (gamestate == 0) reset(); // If play game has been selected reset positions etc
                }
            }
        }

        public void drawmenu()
        {
            spriteBatch.Begin();
            // Draw menu options
            for (int i = 0; i < numberofoptions; i++)
            {
                    menuoptions[i].drawme(ref spriteBatch);
            }

            spriteBatch.End();
        }

        public void updategame(float gtime)
        {
            // Main game code
            if (!gameover)
            {
                // Game is being played
                if (pad[0].Buttons.Back == ButtonState.Pressed) gameover = true; // Allow user to quit game

                // Check for touch over a menu option
                Vector2 dirtomove = new Vector2(0, 0);
                float turnamount = 0;
                TouchCollection tcoll = TouchPanel.GetState();
                BoundingSphere touchsphere = new BoundingSphere(new Vector3(0, 0, 0), 1);
                foreach (TouchLocation t1 in tcoll)
                {
                    if (t1.State == TouchLocationState.Pressed || t1.State == TouchLocationState.Moved)
                    {
                        touchsphere = new BoundingSphere(new Vector3(t1.Position.X, t1.Position.Y, 0), 1);
                        if (touchsphere.Intersects(up.bbox))
                            dirtomove.Y = 1;
                        if (touchsphere.Intersects(down.bbox))
                            dirtomove.Y = -1;
                        if (touchsphere.Intersects(left.bbox))
                            dirtomove.X = 1;
                        if (touchsphere.Intersects(right.bbox))
                            dirtomove.X = -1;
                        if (touchsphere.Intersects(left2.bbox))
                            turnamount = 1;
                        if (touchsphere.Intersects(right2.bbox))
                            turnamount = -1;
                    }
                }

                // Move Robot based on user input
                uservehicle.moveme(dirtomove, turnamount, gtime, 70);

                // Spin and move UFOS
                for (int i = 0; i < ufos.Count(); i++)
                {
                    // Spin UFOs
                    ufos[i].rotation.Y += 0.5f;

                    // Move UFOs
                    Vector3 ufovelocity = new Vector3(30, 0, 40);
                    ufos[i].position += ufovelocity;
                }

                //  if (pad[0].Buttons.A == ButtonState.Pressed) robot1.jump(100, gtime);

                // Set the camera to first person
                //gamecamera.setFPor3P(robot.position, robot.direction, new Vector3(0, 0, 0), -60, 300, 60, 45);
                // Set side on camera view
                //gamecamera.setsideon(robot.position, robot.rotation, 500, 50);
                // Set overhead camera view
                //gamecamera.setoverhead(robot.position, 1000);

                // Set the camera to third person
                gamecamera.setFPor3P(uservehicle.position, uservehicle.direction, uservehicle.velocity, 300, 100, 100, 60);

                // Allow the camera to look up and down
                //gamecamera.position.Y += (pad[0].ThumbSticks.Right.Y * 140);

                if (pad[0].Buttons.Back == ButtonState.Pressed)
                    gameover = true;    // End Game
            }
            else
            {
                // Game is over, allow game to return to the main menu
                if (pad[0].Buttons.Back == ButtonState.Pressed)
                {
                    // SORT HIGHSCORE TABLE
                    Array.Sort(highscores);
                    Array.Reverse(highscores);

                    gamestate = -1; // Allow user to quit game
                }
            }
        }

        public void drawgame()
        {
            // Draw the in-game graphics
            sfunctions3d.resetgraphics(GraphicsDevice);
            ground.drawme(gamecamera, false);
            uservehicle.drawme(gamecamera, true);

            for (int i = 0; i < numberoftrees; i++)
                tree[i].drawme(gamecamera, true);

            for (int i = 0; i < ufos.Count(); i++)
                ufos[i].drawme(gamecamera, true);

            spriteBatch.Begin();
            up.drawme(ref spriteBatch);
            down.drawme(ref spriteBatch);
            left.drawme(ref spriteBatch);
            right.drawme(ref spriteBatch);
            left2.drawme(ref spriteBatch);
            right2.drawme(ref spriteBatch);
            if (gameover)
            {
                spriteBatch.DrawString(mainfont, "GAME OVER", new Vector2(130, 100),
                Color.White, MathHelper.ToRadians(0), new Vector2(0, 0), 3f, SpriteEffects.None, 0);
            }
            spriteBatch.End();
        }

        public void updateoptions()
        {
            // Update code for the options screen

            // Allow game to return to the main menu
            if (pad[0].Buttons.Back == ButtonState.Pressed) gamestate = -1;
        }

        public void drawoptions()
        {
            // Draw graphics for OPTIONS screen
            spriteBatch.Begin();
            spriteBatch.End();
        }

        public void updatehighscore()
        {
            // Update code for the high score screen
            // Allow game to return to the main menu
            if (pad[0].Buttons.Back == ButtonState.Pressed) gamestate = -1;

        }

        public void drawhighscore()
        {
            // Draw graphics for High Score table
            spriteBatch.Begin();
            // Draw top ten high scores
            for (int i = 0; i < numberofhighscores; i++)
            {
                spriteBatch.DrawString(mainfont, (i + 1).ToString("0") + ". " + highscores[i].ToString("0"), new Vector2(displaywidth / 2 - 80, 100 + (i * 30)),
                    Color.White, MathHelper.ToRadians(0), new Vector2(0, 0), 1f, SpriteEffects.None, 0);
            }

            spriteBatch.End();
        }



        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            background.drawme(ref spriteBatch);
            spriteBatch.DrawString(mainfont, "Resolution " + displaywidth.ToString() + " " + displayheight.ToString(), new Vector2(20, 20), Color.Yellow);
            spriteBatch.End();

            // Draw stuff depending on the game state
            switch (gamestate)
            {
                case -1:
                    // Game is on the main menu
                    drawmenu();
                    break;
                case 0:
                    // Game is being played
                    drawgame();
                    break;
                case 1:
                    // Options menu
                    drawoptions();
                    break;
                case 2:
                    // High Score table
                    drawhighscore();
                    break;
                default:
                    break;
            }


            base.Draw(gameTime);
        }
    }
}
